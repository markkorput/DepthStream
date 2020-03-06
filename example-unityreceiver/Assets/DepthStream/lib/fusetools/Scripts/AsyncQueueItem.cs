using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FuseTools
{
	public class AsyncQueueItem : MonoBehaviour
	{
		public AsyncQueue Queue;        
        public bool InvokeOnAwake = true;

		public UnityEvent InvokeEvent;

#if UNITY_EDITOR
        [System.Serializable]
        public class Dinfo
        {
            public bool IsQueued = false;
        }

        public Dinfo DebugInfo;
#endif
      
		void Start() {
			if (this.Queue == null) this.Queue = GetComponentInParent<AsyncQueue>();
			if (this.InvokeOnAwake) this.InvokeWhenReady();
		}
      
		public void InvokeWhenReady()
		{
			this.WaitUntilReady().Then(() => this.InvokeEvent.Invoke());
		}
      
		public RSG.Promise WaitUntilReady() {
			return new RSG.Promise((resolve, reject) =>
			{
				if (this.Queue == null) {
					resolve();
					return;
				}

				this.Queue.WaitUntilReady().Then(() =>
				{
					#if UNITY_EDITOR
                        this.DebugInfo.IsQueued = false;
                    #endif

					resolve();
				});
            
#if UNITY_EDITOR
				this.DebugInfo.IsQueued = true;
#endif
			});
		}
	}
}