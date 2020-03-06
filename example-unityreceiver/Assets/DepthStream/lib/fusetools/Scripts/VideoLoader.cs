using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

namespace FuseTools {
	public class VideoLoader : MonoBehaviour
	{
		public bool Verbose = false;
		private bool bIsLoading = false;
		public bool IsLoading { get { return this.bIsLoading; }}

		public VideoClipEvent OnVideoLoaded;
		public StringEvent OnVideoUrl;

		private IEnumerator LoadResourcePath(string resourcePath, System.Action<VideoClip> callback = null)
		{
				while (this.bIsLoading) yield return null;
				this.bIsLoading = true;

				if (Verbose) Debug.Log("VideoLoader.LoadResourcePath: "+resourcePath);
				var req = Resources.LoadAsync(resourcePath);
				if (req == null) {
						if (Verbose) Debug.Log("Failed to load Resource ("+resourcePath+")");
						yield break;
				}

				yield return new WaitUntil(() => req.isDone);
				if (Verbose) Debug.Log("VideoLoader.LoadResourcePath isDone");
				VideoClip clip = req.asset as VideoClip;

				if (clip == null) {
					if (Verbose) Debug.Log("Loaded resource is null");
				} else {
					if (callback != null) callback.Invoke(clip);
				}

				this.bIsLoading = false;
		}

		#region Public Action Methods
		public void LoadResource(string resourcePath) {
			StartCoroutine(this.LoadResourcePath(resourcePath, (clip) => {
				this.OnVideoLoaded.Invoke(clip);
			}));
		}

		public void Load(string url) {
			if (string.IsNullOrEmpty(url)) return;

			if (url.StartsWith("res://")) {
				this.LoadResource(url.Replace("res://", ""));
				return;
			}

			if (System.IO.File.Exists(url)) {
				InvokeLocalFileUrl(url);
			}
		}

		public void InvokeLocalFileUrl(string path) {
			if (string.IsNullOrEmpty(path)) return;
			if (System.IO.File.Exists(path)) this.OnVideoUrl.Invoke("file://"+path);
		}
		#endregion
	}
}


