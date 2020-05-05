using UnityEngine;
using UnityEngine.Events;

namespace FuseTools.Ext
{
	public class TransformExt : MonoBehaviour
	{
		[System.Serializable]
		public class Evts
		{
			[System.Serializable]
			public class Vector3Event : UnityEvent<Vector3> {}

			[System.Serializable]
            public class TransformEvent : UnityEvent<Transform> { }

			[System.Serializable]
            public class QuaternionEvent : UnityEvent<Quaternion> { }
         

			public Vector3Event Position;
			public Vector3Event Scale;
			public QuaternionEvent Rotation;
			public Vector3Event RotationEulers;
			public TransformEvent Transform;
			public TransformEvent Parent;
		}

		[Tooltip("Defaults to the first Component found on this GameObject's")]
        public Transform Component;
		public bool Local = false;
		protected Transform resolved { get { return this.Component == null ? this.transform : this.Component; } }

		public Evts Events;

		#region Methods
		public void InvokePosition() {
			Events.Position.Invoke(this.Local ? this.resolved.localPosition : this.resolved.position);
		}

		public void InvokeTransform()
		{
			Events.Transform.Invoke(this.resolved);
		}
      
		public void InvokeRotationEulers() {
			Events.RotationEulers.Invoke(this.Local ? this.resolved.localRotation.eulerAngles : this.resolved.rotation.eulerAngles);
		}

		public void InvokeRotation() {
			Events.Rotation.Invoke(this.Local ? this.resolved.localRotation : this.resolved.rotation);
		}

		public void InvokeScale() {
			Events.Scale.Invoke(this.Local ? this.resolved.localScale : this.resolved.lossyScale);
		}

		public void InvokeParent() {
			Events.Parent.Invoke(this.resolved.parent);
        }      

		public void SetZeroPosition() {
			this.resolved.localPosition = Vector3.zero;
        }      
      
		public void LookAtMe(Transform transform) {
			transform.LookAt(this.transform);
		}

		public void SetZeroScale() { this.resolved.localScale = Vector3.zero; }
		public void SetOneScale() { this.resolved.localScale = Vector3.one; }

		public void AddChild(Transform t) {
			t.SetParent(this.transform);
		}

		public void AddChild(GameObject t) {
			AddChild(t.transform);
		}

		public void DestroyChildren() {
			for (int i=0; i<this.transform.childCount; i++) {
				Destroy(this.transform.GetChild(i).gameObject);
			}
		}

		public void SetRotationEulerX(float angle) {
			var t = this.resolved;
			var q = (this.Local ? t.localRotation : t.rotation);
			var angles = q.eulerAngles; 
			angles = new Vector3(angle, angles.y, angles.z);
			if (this.Local)
				t.localRotation = Quaternion.Euler(angles);
			else
				t.rotation = Quaternion.Euler(angles);
		}

		public void SetRotationEulerY(float angle) {
			var t = this.resolved;
			var q = (this.Local ? t.localRotation : t.rotation);
			var angles = q.eulerAngles; 
			angles = new Vector3(angles.x, angle, angles.z);
			if (this.Local)
				t.localRotation = Quaternion.Euler(angles);
			else
				t.rotation = Quaternion.Euler(angles);
		}

		public void SetRotationEulerZ(float angle) {
			var t = this.resolved;
			var q = (this.Local ? t.localRotation : t.rotation);
			var angles = q.eulerAngles; 
			angles = new Vector3(angles.x, angles.y, angle);
			if (this.Local)
				t.localRotation = Quaternion.Euler(angles);
			else
				t.rotation = Quaternion.Euler(angles);
		}
		#endregion      
	}
}