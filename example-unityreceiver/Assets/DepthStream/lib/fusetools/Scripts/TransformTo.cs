using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FuseTools
{   
	public class TransformTo : MonoBehaviour
	{
		public const int TRANSLATE = 1;
		public const int ROTATE = 2;
		public const int SCALE = 4;
		public const int ALL = TRANSLATE | ROTATE | SCALE;
  
		public Transform Subject;
		public Transform Origin;
		public Transform Target;
		public bool Translate = true;
		public bool Rotate = true;
		public bool Scale = true;
		public float Velocity = 1.0f;
      
		[System.Serializable]
		public class Evts
		{
			[Tooltip("Invoked when starting transform from current state to target state")]
			public UnityEvent Start;
			[Tooltip("Invoked when finishing transform from current state to target state")]
			public UnityEvent End;
			[Tooltip("Invoked when starting transform from target state to original state")]
			public UnityEvent Return;
			[Tooltip("Invoked when finishing transform from target state to original state")]
			public UnityEvent Returned;
			[Tooltip("Invoked both for Start and Return")]
			public UnityEvent Move;
			[Tooltip("Invoked both for End and Returned")]
			public UnityEvent Idle;         
		}

		public Evts Events;

		private bool bHasOrigin = false;
		private Vector3 originScale, originPos;
		private Quaternion originRotation;
		private float progress = -1;      
		private float direction = 0.0f;
		private List<System.Action> doneCallbacks = new List<System.Action>();
		private bool isMoving { get { return this.direction >= float.Epsilon || this.direction < -float.Epsilon; } }
		private Transform ResolvedSubject { get { return this.Subject == null ? this.transform : this.Subject; }}

		void Update()
		{
			if (!isMoving) return;

			bool finished = false;
			this.progress += this.direction * Time.deltaTime * this.Velocity;

			if (this.direction < 0.0f && this.progress < 0.0f) {
				this.progress = -1.0f;            
				finished = true;
			}

			if (this.direction > 0.0f && this.progress > 1.0f) {
				this.progress = 1.0f;
				finished = true;
			}

			this.ApplyProgress(this.progress);       

			if (finished) {
				bool forward = this.direction > 0.0f;
				this.direction = 0.0f; // stop

				// notify
				var funcs = this.doneCallbacks.ToArray();
				this.doneCallbacks.Clear();
				foreach (var func in funcs) func.Invoke();

				(forward ? this.Events.End : this.Events.Returned).Invoke();
				this.Events.Idle.Invoke();            
			}
		}

		public void ApplyProgress(float progr) {
			if (this.Origin != null) {
				this.originPos = this.Origin.position;
				this.originRotation = this.Origin.rotation;
				this.originScale = this.Origin.lossyScale;
			} else if (!this.bHasOrigin) {
				this.originPos = this.ResolvedSubject.position;
				this.originRotation = this.ResolvedSubject.rotation;
				this.originScale = this.ResolvedSubject.lossyScale;
				this.bHasOrigin = true;
			}

			ApplyTo(this.ResolvedSubject, this.Target, this.originPos, this.originRotation, this.originScale, progr, GetFlags(this.Translate, this.Rotate, this.Scale));
		}

		public static void ApplyTo(Transform subject, Transform target, Vector3 originPosition, Quaternion originRotation, Vector3 originScale, float progress, int flags) {
			if ((flags & TRANSLATE) != 0) subject.position = Vector3.Lerp(originPosition, target.position, progress);
			if ((flags & ROTATE) != 0) subject.rotation = Quaternion.Lerp(originRotation, target.rotation, progress);
			if ((flags & SCALE) != 0) subject.localScale = Vector3.Lerp(originScale, target.lossyScale, progress);
		}

		public static int GetFlags(bool translate, bool rotate, bool scale) {
			return (translate ? TRANSLATE : 0) | (rotate ? ROTATE : 0) | (scale ? SCALE : 0);
		}

		#region Public Action Methods
				public void SetTarget(Transform t) { this.Target = t; }
				public void SetOrigin(Transform t) { this.Origin = t; }

				public void SetSubject(Transform t) { this.Subject = t; }
				public void SetSubject(GameObject gameObject) { this.Subject = gameObject.transform; }
				public void StartTransform() { direction = 1.0f; progress = 0.0f; }
		#endregion
    }
}