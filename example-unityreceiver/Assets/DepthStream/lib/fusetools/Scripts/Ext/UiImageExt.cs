using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace FuseTools
{
	/// <summary>
	/// 
	/// </summary>
	/// 
	[AddComponentMenu("FuseTools/UiImageExt")]
	public class UiImageExt : MonoBehaviour
	{
		[Tooltip("Defaults to first PlayableDirector found on this GameObject")]
		public UnityEngine.UI.Image Image;
		public Color[] Colors;

		//[System.Serializable]
		//public class Evts
		//{
		//	public UnityEvent Played;
		//}
      
		//public Evts Events;

		void Start()
		{
			if (this.Image == null) this.Image = GetComponent<UnityEngine.UI.Image>();
		}

		//private void OnDestroy()
		//{
		//}
      
		#region Public Methods
		public void SetColor(int idx) {
			if (this.Image != null) this.Image.color = this.Colors[idx];
		}

		public void SetColorAlpha(float alpha) {
			if (this.Image == null) return;
			var clr = this.Image.color;
			this.Image.color = new Color(clr.r, clr.g, clr.b, alpha);
		}
		#endregion
	}
}