using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuseTools
{
	public class AsyncQueue : MonoBehaviour
	{
		[Tooltip("Time (in seconds) to wait in between items")]
		public float Wait = 0.0f;
		public float ReadyCallbackTimeout = 5.0f;
		public bool Verbose = false;
      
#if UNITY_EDITOR
		[System.Serializable]
		public class Dinfo {
			public bool IsReady = false;
			public int Count = 0;
		}
		public Dinfo DebugInfo = new Dinfo();
#endif

		private Queue<System.Action> queue = new Queue<System.Action>();
		private float readyTime = 0.0f;
		private bool isReady = true;
		private float lastReadyCallbackTime = 0.0f;
		private RSG.IPromise asyncReadyPromise = null;
      
		public bool IsReady { get { return this.isReady; } } //return Time.time >= this.readyTime; }}
      
		void Update()
		{
			if (!this.isReady)
			{
				if (this.asyncReadyPromise != null)
				{
					
					if (Time.time >= this.lastReadyCallbackTime + this.ReadyCallbackTimeout) {
						// abort callback
						this.asyncReadyPromise = null;
						this.isReady = true;
					}               
				}
				else
				{
					this.isReady = (Time.time >= this.readyTime);
				}
			}
         
			if (this.queue.Count > 0 && this.isReady)
			{
				if (this.Verbose) Debug.Log("[AsyncQueue.Update] next item");
				this.isReady = false;
				this.readyTime = Time.time + this.Wait;
				var item = this.queue.Dequeue();
            
#if UNITY_EDITOR
				this.DebugInfo.Count = this.queue.Count;
#endif
				item.Invoke();
			}
         
#if UNITY_EDITOR
			this.DebugInfo.IsReady = this.isReady;
#endif
		}

		public RSG.IPromise WaitUntilReady() {
			if (this.Verbose) Debug.Log("[AsyncQueue.WaitUntilReady]");
         
			return new RSG.Promise((resolve, reject) =>
			{
				if (this.IsReady && this.queue.Count == 0)
				{
					if (this.Verbose) Debug.Log("[AsyncQueue.WaitUntilReady] directly ready");

					this.isReady = false;
					this.readyTime = Time.time + this.Wait;
               
					resolve();
					return;
				}
            
				if (this.Verbose) Debug.Log("[AsyncQueue.WaitUntilReady] enqueue");
				this.queue.Enqueue(resolve);
            
#if UNITY_EDITOR
				this.DebugInfo.Count = this.queue.Count;
				this.DebugInfo.IsReady = this.IsReady;
#endif
			});
		}

		public RSG.IPromise WhenReady(System.Func<RSG.IPromise> func) {
			return this.WaitUntilReady().Then(() =>
			{
				var promise = func.Invoke();
				this.asyncReadyPromise = promise;
				this.isReady = false;
				this.lastReadyCallbackTime = Time.time;

				this.asyncReadyPromise.Finally(() =>
				{
					if (this.asyncReadyPromise != promise) return; // we were aborted?!
                    this.isReady = false;
                    this.readyTime = Time.time + this.Wait;
                    this.asyncReadyPromise = null;
                    this.Update();               
				});
			});
		}
      
		public void BlockUntil(RSG.IPromise promise) {
			this.WaitUntilReady().Then(() =>
			{
				this.asyncReadyPromise = promise;
				this.isReady = false;
				this.lastReadyCallbackTime = Time.time;
            
				promise.Finally(() =>
				{
					if (this.asyncReadyPromise != promise) return; // we were aborted?!
					this.isReady = false;
					this.readyTime = Time.time + this.Wait;
					this.asyncReadyPromise = null;
					this.Update();
				}).Done();
			});
		}
	}
}