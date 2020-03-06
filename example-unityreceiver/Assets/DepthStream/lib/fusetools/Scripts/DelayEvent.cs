using UnityEngine;
using UnityEngine.Events;

namespace FuseTools
{
	[AddComponentMenu("FuseTools/DelayEvent")]
	public class DelayEvent : MonoBehaviour
	{
		[Tooltip("Default delay in seconds")]
		public float Delay = 1.0f;
		public bool InvokeOnlyWhenActive = true;
		public bool Repeat = false;
		public UnityEvent Event;

		private int RequestCount = 0;

#if UNITY_EDITOR
        [System.Serializable]
        public class Dinfo
        {
            public int InvokeCount = 0;
			public int RequestCount = 0;
        }
      
        public Dinfo DebugInfo;
#endif
      
		private void InvokeNow()
		{
			if (RequestCount == 0) return;
			this.RequestCount -= 1;
#if UNITY_EDITOR
            this.DebugInfo.RequestCount = this.RequestCount;
#endif

			if (this.InvokeOnlyWhenActive && !this.isActiveAndEnabled) return;
			this.Event.Invoke();
			if (this.Repeat) this.Invoke(this.Delay);

#if UNITY_EDITOR
            this.DebugInfo.InvokeCount += 1;
#endif
		}
      
		#region Public Action Methods
		public void Invoke()
		{
			this.Invoke(this.Delay);
		}
      
		public void Invoke(float customDelay)
        {
            this.RequestCount += 1;
#if UNITY_EDITOR
            this.DebugInfo.RequestCount = this.RequestCount;
#endif
            Invoke("InvokeNow", customDelay);
        }
      
		public void SetDelay(float delay)
		{
			this.Delay = delay;
		}
      
		public void SetRepeat(bool v) {
			this.Repeat = v;
		}
      
		public void Reset()
		{
			RequestCount = 0;
#if UNITY_EDITOR
            this.DebugInfo.RequestCount = this.RequestCount;
#endif         
		}
		#endregion
	}
}