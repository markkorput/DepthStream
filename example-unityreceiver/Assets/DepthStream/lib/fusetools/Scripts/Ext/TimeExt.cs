using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FuseTools.Ext
{
	public class TimeExt : MonoBehaviour
	{
		[System.Serializable]
		public class FloatEvent : UnityEvent<float> { }

		public FloatEvent ValueEvent;

		#region Public Action Methods
		public void InvokeTime() {
			this.ValueEvent.Invoke(Time.time);
		}
		#endregion
	}
}