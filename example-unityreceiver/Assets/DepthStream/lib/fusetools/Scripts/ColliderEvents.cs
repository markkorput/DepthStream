using UnityEngine;
using UnityEngine.Events;

namespace FuseTools
{
	/// <summary>
    /// Triggers Collider-related events to which you can attach
	/// UnityActions in the UnityEditor, without any scripting
    /// </summary>
	[System.Obsolete("Use ColliderExt")]
	[AddComponentMenu("FuseTools/ColliderEvents")]
	public class ColliderEvents : MonoBehaviour
	{
		[System.Serializable]
		public class Evts {
			public UnityEvent OnTriggerEnter;
			public UnityEvent OnCollisionEnter;
		}
      
		public Evts Events;
      
		void OnTriggerEnter(Collider collider) {
			this.Events.OnTriggerEnter.Invoke();
		}

		private void OnCollisionEnter()
		{
			this.Events.OnCollisionEnter.Invoke();
		}
	}
}