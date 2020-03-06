using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FuseTools
{
	[AddComponentMenu("FuseTools/GenericAction")]
	/// <summary>
	/// Minimal component that wraps around a single UnityEvent
	/// </summary>
	public class GenericAction : MonoBehaviour
	{
		public bool InvokeWhenInactive = false;
		public UnityEvent Event;
      
#if UNITY_EDITOR
		[System.Serializable]
		public class Dinfo
		{
			public int InvokeCount = 0;
		}

		public Dinfo DebugInfo;
#endif

#if UNITY_EDITOR
		void Start()
		{
			this.Event.AddListener(() => this.DebugInfo.InvokeCount += 1);
		}
#endif
      
		#region Public Action Method
		public void Invoke() {
			if (this.InvokeWhenInactive || this.isActiveAndEnabled)
			{
				this.Event.Invoke();
			}
        }
        #endregion
	}
}