using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuseTools
{
	public class VirtualParent : MonoBehaviour
	{
		private const int TRANSLATE = 1;
		private const int ROTATE = 2;
		private const int SCALE = 4;
		private const int ALL = TRANSLATE | ROTATE | SCALE;

		[Tooltip("Default to this object's transform")]
		public Transform Child;
		public Transform Parent;
		public bool Translate = true;
		public bool Rotate = true;
		public bool Scale = true;
      
		public bool Ease = false;
		public float EaseFactor = 0.1f;

		private Transform dummy;

		void Start()
		{
			if (this.Child == null) this.Child = this.transform;
         
			if (this.Parent != null)
			{
				this.dummy = CreateDummy(this.Parent, this.Child);
			}
		}

		void Update()
		{
			if (this.dummy == null) {
				if (this.Parent == null) return;
				this.dummy = CreateDummy(this.Parent, this.Child);
			}

			if (this.Ease) AlignWithEasing(this.Child, this.dummy, this.EaseFactor, GetFlags(this.Translate, this.Rotate, this.Scale));
			else Align(this.Child, this.dummy, GetFlags(this.Translate, this.Rotate, this.Scale));
		}

		private int GetFlags(bool t, bool r, bool s) {
			return (t ? TRANSLATE : 0) | (r ? ROTATE : 0) | (s ? SCALE : 0);
		}
      
		private static Transform CreateDummy(Transform parent, Transform positioner) {
			var dummy = new GameObject();
            dummy.name = "VirtualParentDummy";
			dummy.transform.parent = parent;
			//dummy.transform.localPosition = new Vector3(0, 0, 0);
			//Align(dummy.transform, positioner, ALL);
			dummy.transform.localPosition = Vector3.zero;
			dummy.transform.localScale = Vector3.one;
			dummy.transform.localRotation = Quaternion.identity;
			return dummy.transform;
		}

		private static void Align(Transform subject, Transform target, int flags)
		{
			//var originalParent = subject.parent;
			//subject.parent = target;
			//if ((flags & TRANSLATE) != 0) subject.localPosition = Vector3.zero;
			//if ((flags & SCALE) != 0) subject.localScale = Vector3.one;
			//if ((flags & ROTATE) != 0) subject.localRotation = Quaternion.identity;
			//subject.parent = originalParent;
         
			if ((flags & TRANSLATE) != 0) subject.position = target.position;
			if ((flags & ROTATE) != 0) subject.rotation = target.rotation;
			if ((flags & SCALE) != 0)
			{
				//var parentScale = (subject.parent != null ? subject.parent.lossyScale : new Vector3(1, 1, 1));
				//var targetScale = target.lossyScale;            
				//subject.localScale = new Vector3(parentScale.x / targetScale.x, parentScale.y / targetScale.y, parentScale.z / targetScale.z);
				subject.localScale = target.localScale; // L O C A L
			}

		}
      
		private static void AlignWithEasing(Transform subject, Transform target, float easeFactor, int flags)
        {
            var originalParent = subject.parent;
            subject.parent = target;
			if ((flags & TRANSLATE) != 0) subject.localPosition = Vector3.Lerp(subject.localPosition, Vector3.zero, easeFactor);
			if ((flags & SCALE) != 0) subject.localScale = Vector3.Lerp(subject.localScale, Vector3.one, easeFactor);
			if ((flags & ROTATE) != 0) subject.localRotation = Quaternion.Lerp(subject.localRotation, Quaternion.identity, easeFactor);
            subject.parent = originalParent;
        }

		#region Public Action Methods
		public void Reset() {
			if (this.dummy) {
				Destroy(this.dummy.gameObject);
				this.dummy = null;
			}
		}
      
		public void SetParent(Transform p)
		{
			if (this.dummy) Destroy(this.dummy);
			this.Parent = p;
		}
		#endregion
	}
}