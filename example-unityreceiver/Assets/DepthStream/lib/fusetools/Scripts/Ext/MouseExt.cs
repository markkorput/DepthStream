using UnityEngine;
using UnityEngine.Events;

namespace FuseTools
{
    [AddComponentMenu("Fusetools/MouseExt")]
	public class MouseExt : MonoBehaviour {
        [System.Serializable]
		public class Evts
		{
            public UnityEvent OnMouseDown = new UnityEvent();
            public FloatEvent OnAxisX = new FloatEvent();
            public FloatEvent OnAxisY = new FloatEvent();
            public BoolEvent OnPrimaryMouseButton = new BoolEvent();

        }

        public bool InvokeAxesOnUpdate = false;
        public bool InvokePrimaryMouseButtonOnUpdate = false;
        public Evts Events = new Evts();

        #region Unity Methods
        void Update() {
            if (this.InvokeAxesOnUpdate) this.InvokeAxisXY();
            if (this.InvokePrimaryMouseButtonOnUpdate) this.Events.OnPrimaryMouseButton.Invoke(Input.GetMouseButton(0));
            
        }

        void OnMouseDown(){
            this.Events.OnMouseDown.Invoke();
        }
        #endregion

        #region Public Action Methods
        public void InvokeAxisX() {
            this.Events.OnAxisX.Invoke(Input.GetAxis("Mouse X"));
        }

        public void InvokeAxisY() {
            this.Events.OnAxisY.Invoke(Input.GetAxis("Mouse Y"));
        }

        public void InvokeAxisXY() {
            this.Events.OnAxisX.Invoke(Input.GetAxis("Mouse X"));
            this.Events.OnAxisY.Invoke(Input.GetAxis("Mouse Y"));
        }

        public void SetInvokeAxesOnUpdate(bool v) { this.InvokeAxesOnUpdate = v; }
        #endregion
    }
}