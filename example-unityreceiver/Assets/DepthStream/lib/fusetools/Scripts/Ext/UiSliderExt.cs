using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace FuseTools
{
	[AddComponentMenu("FuseTools/UiSliderExt")]
	public class UiSliderExt : MonoBehaviour
	{
		[Tooltip("Defaults to first PlayableDirector found on this GameObject")]
		public UnityEngine.UI.Slider Slider;
		public Color[] Colors;

		private UnityEngine.UI.Slider Resolved { get { return this.Slider != null ? this.Slider : this.GetComponent<UnityEngine.UI.Slider>(); }}
		//[System.Serializable]
		//public class Evts
		//{
		//	public UnityEvent Played;
		//}
      
		//public Evts Events;


		#region Public Methods
		public void SetMaxValue(int val) {
			if (this.Resolved != null) this.Resolved.maxValue = (float)val;
		}
		#endregion
	}
}