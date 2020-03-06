using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace FuseTools
{
	/// <summary>
	/// 
	/// </summary>
	/// 
	[AddComponentMenu("FuseTools/UiTextExt")]
	public class UiTextExt : MonoBehaviour
	{
		[Tooltip("Defaults to first PlayableDirector found on this GameObject")]
		public UnityEngine.UI.Text Text;
		public Color[] Colors;

		//[System.Serializable]
		//public class Evts
		//{
		//	public UnityEvent Played;
		//}
      
		//public Evts Events;

		void Start()
		{
			if (Text == null) this.Text = GetComponent<UnityEngine.UI.Text>();
			// this.Text.alignment = TextAnchor.
		}

		//private void OnDestroy()
		//{
		//}
      
		#region Public Methods
		public void SetColor(int idx) {
			if (this.Text != null) this.Text.color = this.Colors[idx];
		}

		public void SetColorAlpha(float alpha) {
			if (this.Text == null) return;
			var clr = this.Text.color;
			this.Text.color = new Color(clr.r, clr.g, clr.b, alpha);
		}

		public void SetHorizontalAlignLeft() {
			if (this.Text.alignment.ToString().StartsWith("Lower")) this.Text.alignment = TextAnchor.LowerLeft;
			if (this.Text.alignment.ToString().StartsWith("Middle")) this.Text.alignment = TextAnchor.MiddleLeft;
			if (this.Text.alignment.ToString().StartsWith("Upper")) this.Text.alignment = TextAnchor.UpperLeft;
		}

		public void SetHorizontalAlignRight() {
			if (this.Text.alignment.ToString().StartsWith("Lower")) this.Text.alignment = TextAnchor.LowerRight;
			if (this.Text.alignment.ToString().StartsWith("Middle")) this.Text.alignment = TextAnchor.MiddleRight;
			if (this.Text.alignment.ToString().StartsWith("Upper")) this.Text.alignment = TextAnchor.UpperRight;
		}

		public void SetHorizontalAlignCenter() {
			if (this.Text.alignment.ToString().StartsWith("Lower")) this.Text.alignment = TextAnchor.LowerCenter;
			if (this.Text.alignment.ToString().StartsWith("Middle")) this.Text.alignment = TextAnchor.MiddleCenter;
			if (this.Text.alignment.ToString().StartsWith("Upper")) this.Text.alignment = TextAnchor.UpperCenter;
		}

		public void SetVerticalAlignUpper() {
			if (this.Text.alignment.ToString().EndsWith("Center")) this.Text.alignment = TextAnchor.UpperCenter;
			if (this.Text.alignment.ToString().EndsWith("Left")) this.Text.alignment = TextAnchor.UpperLeft;
			if (this.Text.alignment.ToString().EndsWith("Right")) this.Text.alignment = TextAnchor.UpperRight;
		}

		public void SetVerticalAlignLower() {
			if (this.Text.alignment.ToString().EndsWith("Center")) this.Text.alignment = TextAnchor.LowerCenter;
			if (this.Text.alignment.ToString().EndsWith("Left")) this.Text.alignment = TextAnchor.LowerLeft;
			if (this.Text.alignment.ToString().EndsWith("Right")) this.Text.alignment = TextAnchor.LowerRight;
		}

		public void SetVerticalAlignMiddle() {
			if (this.Text.alignment.ToString().EndsWith("Center")) this.Text.alignment = TextAnchor.MiddleCenter;
			if (this.Text.alignment.ToString().EndsWith("Left")) this.Text.alignment = TextAnchor.MiddleLeft;
			if (this.Text.alignment.ToString().EndsWith("Right")) this.Text.alignment = TextAnchor.MiddleRight;
		}
		#endregion
	}
}