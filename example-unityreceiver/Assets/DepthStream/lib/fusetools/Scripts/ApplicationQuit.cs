using UnityEngine;
using UnityEngine.Events;

namespace FuseTools
{
	/// <summary>
	/// Simple component that triggers lifecyce (start, enable, disable) events.
	/// </summary>
	/// 
	[AddComponentMenu("FuseTools/ApplicationQuit")]
	public class ApplicationQuit : MonoBehaviour
	{
		[System.Serializable]
		public class Evts
		{
			public UnityEvent OnApplicationQuit;
		}

		public Evts Events;

		void OnApplicationQuit()
		{
			this.Events.OnApplicationQuit.Invoke();
		}
      
		#region Public Methods
		public void Quit() {
			Application.Quit();
		}
		#endregion
	}
}