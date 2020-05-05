using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuseTools
{
	public class Screenshot : MonoBehaviour
	{
		[UnityEngine.SerializeField]
		private Texture2D buffer = null;
		private bool isScheduled = false;

		private List<System.Action<Texture2D>> textureCallbacks = new List<System.Action<Texture2D>>();
		private List<System.Action<byte[]>> pngBytesCallbacks = new List<System.Action<byte[]>>();
		private static Screenshot cachedInstance = null;
      
		private IEnumerator MakeScreenshot()
		{
			if (this.isScheduled) yield break;
			this.isScheduled = true;
         
			// pre-allocate buffer
			if (buffer != null && (this.buffer.width != Screen.width || this.buffer.height != Screen.height))
			{
				this.buffer = null;
			}

			if (this.buffer == null)
			{
				this.buffer = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);
			}
         
			yield return new WaitForEndOfFrame();

			this.buffer.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
			this.buffer.Apply();
			this.isScheduled = false;
         
			foreach (var callback in this.textureCallbacks) callback.Invoke(this.buffer);
			this.textureCallbacks.Clear();

			if (this.pngBytesCallbacks.Count > 0)
			{
				var bytes = this.buffer.EncodeToPNG();

				foreach (var callback in this.pngBytesCallbacks) callback.Invoke(bytes);
				this.pngBytesCallbacks.Clear();
			}         
		}

		#region Public Action Methods
		public void TakeScreenshot(System.Action<Texture2D> callback)
		{
			this.textureCallbacks.Add(callback);
			StartCoroutine(MakeScreenshot());
		}

		public void TakeScreenshotPNG(System.Action<byte[]> callback)
		{
			this.pngBytesCallbacks.Add(callback);
			StartCoroutine(MakeScreenshot());
		}

		public RSG.IPromise<Texture2D> GetScreenshotTexture()
		{
			return new RSG.Promise<Texture2D>((resolve, reject) => this.TakeScreenshot(resolve));
		}

		public RSG.Promise<byte[]> GetScreenshotPNG()
		{
			return new RSG.Promise<byte[]>((resolve, reject) => this.TakeScreenshotPNG(resolve));
		}

		public RSG.IPromise SavePNGTo(string filePath)
		{
			return GetScreenshotPNG()
				.Then((bytes) =>
				{
					var dir = System.IO.Path.GetDirectoryName(filePath);
					if (!System.IO.Directory.Exists(dir))
						System.IO.Directory.CreateDirectory(dir);

					System.IO.File.WriteAllBytes(filePath, bytes);
				});
		}

		public void SavePNG(string filePath) {
			this.SavePNGTo(filePath).Done();
		}
		#endregion

		#region Static Public Methods
		public static RSG.IPromise SavePNGTo(string filePath, bool useCachedInstance = true)
		{
			return WithInstance<RSG.IPromise>(useCachedInstance, (inst) =>
				inst.SavePNGTo(filePath));
		}

		public static RSG.IPromise<Texture2D> GetTexture(bool useCachedInstance = true){
			return WithInstance<RSG.IPromise<Texture2D>>(useCachedInstance, (inst) => 
					inst.GetScreenshotTexture());
		}

		public static RSG.IPromise<byte[]> GetPNG(bool useCachedInstance = true) {
			return WithInstance<RSG.IPromise<byte[]>>(useCachedInstance, (inst) =>
				inst.GetScreenshotPNG());
		}
		#endregion

		/// Creates a (or reuses a previously created) Screenshot instance
		private static T WithInstance<T>(bool cache, System.Func<Screenshot, T> func) {
			Screenshot screenshot = null;
			GameObject entity = null;

			if (cache)
            {
                screenshot = cachedInstance;
                if (screenshot != null && !screenshot.isActiveAndEnabled) screenshot = null;
            }
         
            if (screenshot == null)
            {
                // create and entity to hold our screenshot component
                entity = new GameObject("Screenshot Instance");
                screenshot = entity.AddComponent<Screenshot>();
                if (cache) cachedInstance = screenshot;
            }

			var result = func.Invoke(screenshot);

			if (!cache) Destroy(entity); // cleanup
			return result;
		}
	}
}