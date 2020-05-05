using UnityEngine;
using UnityEngine.Events;

namespace FuseTools
{
	/// <summary>
	/// Simple component that triggers an event when it starts
	/// </summary>
	/// 
	[AddComponentMenu("FuseTools/DestroyEvent")]
	public class DestroyEvent : MonoBehaviour
	{
		public UnityEvent OnDestroyEvent;

		void OnDestroy()
		{
			this.OnDestroyEvent.Invoke();
		}
	}
}