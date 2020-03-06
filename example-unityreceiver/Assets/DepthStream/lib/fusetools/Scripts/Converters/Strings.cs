using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FuseTools.Converters
{
	public class Strings : MonoBehaviour
	{
		public string[] Values;
      
		public StringsEvent ValuesEvent;

		#region Public Action Methods
		public void InvokeValues() {
			this.ValuesEvent.Invoke(this.Values);
		}
		#endregion
	}
}