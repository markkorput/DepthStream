using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FuseTools
{
	/// <summary>
	/// Calculates the distance between two Transforms
	/// </summary>
	public class Distance : MonoBehaviour
	{
		public Transform Origin;
		public Transform Target;
		public bool EveryUpdate = false;
      
		[System.Serializable]
		public class Evts
		{
			public FloatEvent Distance;
			public Vector3Event Delta;
		}

		public Evts Events;

		private void Start()
		{
			if (this.Target == null) this.Target = this.transform;
		}
      
		void Update()
		{
			if (EveryUpdate)
			{
				if (Origin == null || Target == null) return;
                var delta = Target.position - Origin.position;
                this.Events.Delta.Invoke(delta);
            
				var dist = delta.magnitude;
                this.Events.Distance.Invoke(dist);            
			}
		}
      
		#region Public Action Methods
		public void InvokeDelta()
		{
			if (Origin == null || Target == null) return;
			var delta = Target.position - Origin.position;
			this.Events.Delta.Invoke(delta);
		}
      
		public void InvokeDistance()
        {
            if (Origin == null || Target == null) return;
            var delta = Target.position - Origin.position;
			var dist = delta.magnitude;
			this.Events.Distance.Invoke(dist);
        }
		#endregion
	}
}