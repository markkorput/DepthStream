using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FuseTools
{
	/// <summary>
	/// Lets you invoke UnityActions on a configurable GUI event
	/// </summary>
	[AddComponentMenu("FuseTools/KeyAction")]
	public class KeyAction : MonoBehaviour
	{
		public KeyCode Key;
		public bool RequireControl = false;
        public EventType EventType = EventType.KeyDown;
        public UnityEvent Action;
      
		private void OnGUI()
		{
			var evt = Event.current;
			if (this.Key.Equals(evt.keyCode) && this.EventType.Equals(evt.type))
			{
				if ((!RequireControl) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
				{
					this.Action.Invoke();
				}
			}
    }

		#region Public Action Methods
		public void PerformAction() {
			this.Action.Invoke();
		}
		#endregion
	}
}