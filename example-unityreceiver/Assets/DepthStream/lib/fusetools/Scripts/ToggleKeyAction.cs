using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FuseTools
{
	/// <summary>
	/// Lets you alternate between UnityActions during a configurable GUI event
	/// </summary>
	[AddComponentMenu("FuseTools/ToggleKeyAction")]
	public class ToggleKeyAction : MonoBehaviour
	{
		public KeyCode Key;
		public EventType EventType = EventType.KeyDown;
		public UnityEvent ActionA;
		public UnityEvent ActionB;

		private bool isB = false;

		private void OnGUI()
		{
			var evt = Event.current;

			if (this.Key.Equals(evt.keyCode) && this.EventType.Equals(evt.type))
			{
				this.Toggle();
			}
		}
      
		#region Public Action Methods
		public void Toggle() {
			(this.isB ? this.ActionB : this.ActionA).Invoke();
            this.isB = !this.isB;         
		}
		#endregion
	}
}