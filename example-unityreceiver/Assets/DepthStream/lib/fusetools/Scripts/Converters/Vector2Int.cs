using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuseTools.Converters
{
	public class Vector2Int : MonoBehaviour
	{
		public UnityEngine.Vector2Int Value = new UnityEngine.Vector2Int();
      
        [System.Serializable]
		public class Evts{
			public FuseTools.Vector2IntEvent Value;
		}
      
		public Evts Events;

		#region Public Action Method
		public void InvokeValue() {
			this.Events.Value.Invoke(this.Value);
		}
		#endregion
	}
}