using UnityEngine;
using UnityEngine.Events;

namespace FuseTools
{
	/// <summary>
	/// Triggers Collider-related events to which you can attach
	/// UnityActions in the UnityEditor, without any scripting
	/// </summary>
	[AddComponentMenu("FuseTools/ColliderExt")]
	public class ColliderExt : MonoBehaviour
	{
		[System.Serializable]
		public class CollisionEvent : UnityEvent<Collision> {}

		[System.Serializable]
        public class ColliderEvent : UnityEvent<Collider> {}

		[System.Serializable]
		public class Evts
		{
			public UnityEvent OnTriggerEnter;
			public UnityEvent OnTriggerExit;
			public ColliderEvent OnTriggerEnterInfo;
			public ColliderEvent OnTriggerExitInfo;
			public UnityEvent OnCollisionEnter;
			public UnityEvent OnCollisionExit;
			public CollisionEvent OnCollisionEnterInfo;
			public CollisionEvent OnCollisionExitInfo;
		}

		public Evts Events;
      
		void OnTriggerEnter(Collider colliderInfo)
		{
			this.Events.OnTriggerEnter.Invoke();
			this.Events.OnTriggerEnterInfo.Invoke(colliderInfo);
		}

		void OnTriggerExit(Collider colliderInfo)
        {
            this.Events.OnTriggerExit.Invoke();
			this.Events.OnTriggerExitInfo.Invoke(colliderInfo);
        }
      
		void OnCollisionEnter(Collision collision)
		{
			this.Events.OnCollisionEnter.Invoke();
			this.Events.OnCollisionEnterInfo.Invoke(collision);
		}

		void OnCollisionExit(Collision collision)
        {
			this.Events.OnCollisionExit.Invoke();
			this.Events.OnCollisionExitInfo.Invoke(collision);
        }
	}
}