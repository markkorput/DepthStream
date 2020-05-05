using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FuseTools
{
	public class Delta : MonoBehaviour
	{

		[System.Serializable]
        public class VectorEvent : UnityEvent<Vector3> { }


        private class Frame
		{
			public Vector3 Position;
		}
      
		public Transform Subject;        
		public VectorEvent Translate;

#if UNITY_EDITOR
		[System.Serializable]
		public class Dinfo {
			public Vector3 Delta;
		}
      
		public Dinfo DebugInfo;
#endif
      
		private Frame lastFrame = new Frame();
        
		private Transform Resolved { get {
				return Subject == null ? this.transform : this.Subject;
			}}
      
		private void Start()
		{
			lastFrame.Position = this.Resolved.position;
		}
      
		void Update()
		{
			var p = this.Resolved.position;

			var delta = p - this.lastFrame.Position;
			this.Translate.Invoke(delta);
			this.lastFrame.Position = p;

#if UNITY_EDITOR
			this.DebugInfo.Delta = delta;
#endif
		}
	}
}