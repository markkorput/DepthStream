using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FuseTools
{
	/// <summary>
	/// Generates Random Float Number values and Invokes UnityEvents
	/// </summary>
	[AddComponentMenu("FuseTools/RandomFloat")]
	public class RandomFloat : MonoBehaviour
	{
		public float Min = 0.0f;
		public float Max = 1.0f;
		public bool InvokeOnEnable = true;

		[System.Serializable]
		public class FloatEvent : UnityEvent<float> { }

		[System.Serializable]
		public class Evts
		{
			public FloatEvent Value;
		}

		public Evts Events;

#if UNITY_EDITOR
		[System.Serializable]
		public class Dinfo
		{
			public float LastValue = 0.0f;
		}
		public Dinfo DebugInfo;
#endif

		private void OnEnable()
		{
			if (this.InvokeOnEnable) this.InvokeRandomValue(this.Min, this.Max);
		}
      
		private void InvokeRandomValue(float min, float max)
		{
			var val = min + (max - min) * (float)new System.Random().NextDouble();
			this.Events.Value.Invoke(val);
#if UNITY_EDITOR
			this.DebugInfo.LastValue = val;
#endif
		}

		#region Public Action Methods
		public void Invoke() {
			this.InvokeRandomValue(this.Min, this.Max);
		}

		public void InvokeMax(float val) {
			this.InvokeRandomValue(this.Min, val);
		}

		public void InvokeMin(float val)
        {
            this.InvokeRandomValue(val, this.Max);
        }      
		#endregion
	}
}