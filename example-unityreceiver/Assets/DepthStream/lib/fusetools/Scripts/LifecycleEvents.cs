using UnityEngine;
using UnityEngine.Events;

namespace FuseTools
{
	/// <summary>
	/// Simple component that triggers lifecyce (start, enable, disable) events.
	/// </summary>
	/// 
	[AddComponentMenu("FuseTools/LifecycleEvents")]
	public class LifecycleEvents : MonoBehaviour
	{
		[System.Serializable]
		public class Evts
		{
			public UnityEvent Start;
			public UnityEvent Enable;
			public UnityEvent Disable;
			[System.Serializable]
			public class BoolEvent : UnityEvent<bool> { }
			public BoolEvent EnabledChange;
			public UnityEvent Update;
			public UnityEvent Destroy;
		}
      
		public Evts Events;

		void Start()
		{
			this.Events.Start.Invoke();
		}

		void OnDestroy()
		{
			this.Events.Destroy.Invoke();
		}

		void OnEnable()
		{
			this.Events.Enable.Invoke();
			this.Events.EnabledChange.Invoke(true);
		}

		void OnDisable()
		{
			this.Events.Disable.Invoke();
			this.Events.EnabledChange.Invoke(false);
		}

		void Update()
		{
			this.Events.Update.Invoke();
		}

		#region Public Action Methods
		public void DestroyGameObject() {
			Destroy(this.gameObject);
		}

		public void ToggleActive() {
			this.gameObject.SetActive(!this.gameObject.activeSelf);
		}
		#endregion
	}
}