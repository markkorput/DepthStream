using UnityEngine;
using UnityEngine.Events;

namespace FuseTools
{
	/// <summary>
	/// Simple component that triggers an event when it starts
	/// </summary>
	/// 
	[AddComponentMenu("FuseTools/StartEvent")]
	public class StartEvent : MonoBehaviour
	{
		public UnityEvent OnStart;

		void Start()
		{
			this.OnStart.Invoke();
		}
	}
}