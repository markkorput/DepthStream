using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace FuseTools {
    public class PointerEnter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public UnityEvent PointerEnterEvent = new UnityEvent();
        public UnityEvent PointerExitEvent = new UnityEvent();

        public void OnPointerEnter(PointerEventData eventData) {
            this.PointerEnterEvent.Invoke();    
        }

        public void OnPointerExit(PointerEventData eventData) {
            this.PointerExitEvent.Invoke();    
        }
    }
}