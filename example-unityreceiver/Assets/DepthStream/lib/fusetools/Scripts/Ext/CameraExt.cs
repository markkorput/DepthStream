using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace FuseTools
{
	[AddComponentMenu("FuseTools/CameraExt")]
	public class CameraExt : MonoBehaviour
	{
		[Tooltip("Defaults to Camera component found on this GameObject")]
		public Camera Subject;

		//[System.Serializable]
		//public class Evts
		//{
        //}

		//public Evts Events;
      
		public Camera ResolvedCamera { get { return this.Subject != null ? this.Subject : this.GetComponent<Camera>(); }}
      
      
		#region Public Methods
		public void ResetRenderTexture() {
            if (this.ResolvedCamera != null)
			{
				this.ResolvedCamera.targetTexture = null;
				this.ResolvedCamera.forceIntoRenderTexture = false;
			}         

		}
      
		public void SetTargetTexture(RenderTexture renderTex) {
			this.ResolvedCamera.targetTexture = renderTex;
		}

		public static Camera[] DisableAll()
		{
			var cams = FindObjectsOfType<Camera>();
         
            List<Camera> disabledCams = new List<Camera>();
            foreach (var cam in cams)
            {
                if (cam.enabled)
                {
                    cam.enabled = false;
                    disabledCams.Add(cam);
                }
            }

			return disabledCams.ToArray();
		}

		public static void DisableAll(System.Action func) {
			Camera[] disabledCams = DisableAll();
			func.Invoke();
			foreach (var cam in disabledCams) cam.enabled = true;         
		}
		#endregion
	}
}