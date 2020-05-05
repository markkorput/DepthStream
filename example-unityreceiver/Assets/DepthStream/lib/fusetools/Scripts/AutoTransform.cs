using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuseTools
{
	/// <summary>
	/// Applies basic transformation to its GameObject's Transform every frame
	/// </summary>
	[AddComponentMenu("FuseTools/AutoTransform")]
	public class AutoTransform : MonoBehaviour
	{
		[Tooltip("Defaults to this object's transform")]
		public Transform Transform;
		public bool LocalTransform = true;
      
		public Vector3 PreRotTranslate;
		public Vector3 RotateEuler;
		public Vector3 PostRotTranslate;
		public Vector3 Scale;


		private void Start()
		{
			if (this.Transform == null) this.Transform = this.transform;

			if (this.LocalTransform != true && Scale.magnitude > float.Epsilon) {
				Debug.LogWarning("Scaling is not supporting for non-local AutoTransform");
			}
		}
      
		void Update()
		{
			var dt = Time.deltaTime;

			if (this.LocalTransform) {
				// Pre-rotate translate
				this.Transform.localPosition += PreRotTranslate;

				// Rotate (Local)
				this.Transform.localRotation *= Quaternion.Euler(this.RotateEuler);

				// Post-rotation translate
				this.Transform.localPosition += PostRotTranslate;

				this.Transform.localScale += this.Scale;
			} else {
				// Pre-rotate translate
				this.Transform.position += PreRotTranslate;

				// Rotate (Local)
				this.Transform.rotation *= Quaternion.Euler(this.RotateEuler);

				// Post-rotation translate
				this.Transform.position += PostRotTranslate;
			}
		}

		#region Public Action Methods
		public void SetPreRotTranslateX(float val) { this.PreRotTranslate.x = val; }
		public void SetPreRotTranslateY(float val) { this.PreRotTranslate.y = val; }
		public void SetPreRotTranslateZ(float val) { this.PreRotTranslate.z = val; }

		public void SetRotateX(float val) { this.RotateEuler.x = val; }
		public void SetRotateY(float val) { this.RotateEuler.y = val; }
		public void SetRotateZ(float val) { this.RotateEuler.z = val; }
      
		public void SetPostRotTranslateX(float val) { this.PostRotTranslate.x = val; }
		public void SetPostRotTranslateY(float val) { this.PostRotTranslate.y = val; }
		public void SetPostRotTranslateZ(float val) { this.PostRotTranslate.z = val; }
		#endregion
	}
}