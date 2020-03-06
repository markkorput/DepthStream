using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace FuseTools {
    public class DragHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [System.Serializable]
        public class PointerEventDataEvent : UnityEvent<PointerEventData> {}

        public Camera EventCamera;

        [Header("Events")]
        public FuseTools.Vector2Event NormalisedOffset;


        [System.Serializable]
        public class PointerEventDataEvents {
            public PointerEventDataEvent OnPointerDown = new PointerEventDataEvent();
            public PointerEventDataEvent OnPointerUp = new PointerEventDataEvent();
            public PointerEventDataEvent OnDrag = new PointerEventDataEvent();
        }

        public PointerEventDataEvents UnityEvents = new PointerEventDataEvents();
        
        public void OnPointerDown(PointerEventData eventData) {
            this.UnityEvents.OnPointerUp.Invoke(eventData);
        }

        public void OnPointerUp(PointerEventData eventData) {
            this.UnityEvents.OnPointerDown.Invoke(eventData);
        }

        public void OnDrag(PointerEventData eventData) {
            this.UnityEvents.OnDrag.Invoke(eventData);
        }

        #region Public Action Methods
        public void InvokeNormalisedOffset(PointerEventData eventData) {
            this.UpdateDrag(eventData.position, eventData.pressEventCamera);
        }

        public void UpdateDrag(Vector2 position) {
            this.UpdateDrag(position, EventCamera != null ? EventCamera : Camera.main);
        }

        public void UpdateDrag(Vector2 position, Camera pressEventCamera) {
            // Debug.Log("UpdateDrag: "+position.ToString());

            if (this.NormalisedOffset.GetPersistentEventCount() > 0) {
                var rt = GetComponent<RectTransform>();
                if (rt == null) return;
                Vector2 localPoint = new Vector2();
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, position, pressEventCamera, out localPoint)) {
                    Vector2 normpos = NormPos.LocalToNormalised(localPoint, rt);
                    this.NormalisedOffset.Invoke(normpos);
                }
            }
        }
        #endregion
    }
}