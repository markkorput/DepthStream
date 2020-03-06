using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace FuseTools
{
	[AddComponentMenu("FuseTools/ScrollRectExt")]
	public class ScrollRectExt : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		[Tooltip("Defaults to ScrollRect component found on this GameObject")]
		public ScrollRect ScrollRect;
		private ScrollRect Resolved { get { return this.ScrollRect != null ? this.ScrollRect : this.GetComponent<ScrollRect>(); }}

		[System.Serializable]
		public class PointerEventDataEvent : UnityEvent<PointerEventData> {}

		[System.Serializable]
		public class Evts
		{
			public PointerEventDataEvent BeginDrag;
			public PointerEventDataEvent Drag;
			public PointerEventDataEvent EndDrag;

			public Vector2Event Velocity;
		}

		public Evts Events;

		void Update() {
			this.Events.Velocity.Invoke(this.Resolved.velocity);
		}

		public void OnBeginDrag(PointerEventData data) {
			this.Events.BeginDrag.Invoke(data);
		}

		public void OnDrag(PointerEventData data) {
			this.Events.Drag.Invoke(data);
		}

		public void OnEndDrag(PointerEventData data) {
			this.Events.EndDrag.Invoke(data);
		}

		#region Public Methods
			public void ScrollToChild(GameObject child) {
				var rt = child.GetComponent<RectTransform>();
				if (rt == null) return;
				this.ScrollToChild(rt);
			}

			public void ScrollToChild(RectTransform child) {
				this.Resolved.normalizedPosition = GetNormalizedChildPosition(child);
			}
		#endregion

		private Vector2 GetNormalizedChildPosition(RectTransform child) {
			Vector3[] contentCorners = new Vector3[4];
			var pos = child.transform.position;
			var rt = this.Resolved.content;
			rt.GetWorldCorners(contentCorners); // bottom left, top left, top right, bottom right

			var size = new Vector2(
				contentCorners[2].x - contentCorners[0].x,
				contentCorners[1].y - contentCorners[0].y);

			var norm = new Vector2(
				(pos.x - contentCorners[0].x) / (size.x < 0.001 ? 1.0f : size.x),
				(pos.y - contentCorners[0].y) / (size.y < 0.001 ? 1.0f : size.y));
			
			return norm;
		}
	}
}