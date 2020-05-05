using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuseTools
{
	public class RectTransformExt : MonoBehaviour
	{
		public RectTransform RectTransform;

		public RectTransform ResolvedRectTransform { get { return this.RectTransform != null ? this.RectTransform : this.GetComponent<RectTransform>(); } }

		#region Public Action Methods
		public void SetWidth(float w) {
			this.SetSizeDeltaX(w);
		}

		public void SetSizeDeltaX(float x) {
			this.ResolvedRectTransform.sizeDelta = new Vector2(x, this.ResolvedRectTransform.sizeDelta.y);
		}

		public void SetSizeDeltaY(float y) {
			this.ResolvedRectTransform.sizeDelta = new Vector2(this.ResolvedRectTransform.sizeDelta.x, y);
		}

		public void SetPivotX(float x){
			this.ResolvedRectTransform.pivot = new Vector2(x, this.ResolvedRectTransform.pivot.y);
		}

		public void SetPivotY(float y){
			this.ResolvedRectTransform.pivot = new Vector2(this.ResolvedRectTransform.pivot.x, y);
		}
		#endregion

		public static Vector2 GetWorldTranslateToContainWithin(RectTransform child, RectTransform parent) {
			Vector3[] ccorners = new Vector3[4];
			Vector3[] pcorners = new Vector3[4];
			child.GetWorldCorners(ccorners);
			parent.GetWorldCorners(pcorners);
			float dx = 0.0f, dy = 0.0f;
			if (ccorners[0].x < pcorners[0].x) dx = pcorners[0].x - ccorners[0].x;
			if (ccorners[3].x > pcorners[3].x) dx = pcorners[3].x - ccorners[3].x;
			if (ccorners[0].y < pcorners[0].y) dy = pcorners[0].y - ccorners[0].y;
			if (ccorners[1].y > pcorners[1].y) dy = pcorners[1].y - ccorners[1].y;
			return new Vector2(dx,dy);
		}
	}
}