using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace FuseTools.Test
{
	public class EventCounter : System.IDisposable
	{      
		private UnityEvent event_;
		private bool isInvoked_ = false;
		private int count = 0;
      
		public bool IsInvoked { get { return this.isInvoked_; } }
		public int Count { get { return count; }}
      
		public EventCounter(UnityEvent evt)
		{
			this.event_ = evt;
			evt.AddListener(this.OnInvoke);
		}

		private void OnInvoke()
		{
			this.isInvoked_ = true;
			this.count += 1;
		}



		public static WaitUntil UntilInvoked(UnityEvent evt)
		{
			EventWait waiter = new EventWait(evt);
			return new WaitUntil(() => waiter.IsInvoked);
		}

		public static WaitUntil UntilInvoked<T>(UnityEvent evt)
		{
			EventWait waiter = new EventWait(evt);
			return new WaitUntil(() => waiter.IsInvoked);
		}

		public void Dispose() {
			this.event_.RemoveListener(this.OnInvoke);
		}
	}

	public class EventCounter<T>
	{
		private UnityEvent<T> event_;
		private List<T> argHistory = new List<T>();
      
		public int Count { get { return argHistory.Count; } }
		public T[] ArgHistory { get { return argHistory.ToArray(); } }
		public T LastArg { get { return argHistory.Count == 0 ? default(T) : argHistory[argHistory.Count-1]; } }
		public bool IsInvoked { get { return argHistory.Count > 0; } }

		public EventCounter(UnityEvent<T> evt)
		{
			this.event_ = evt;
			evt.AddListener(this.OnInvoke);
		}

		private void OnInvoke(T arg)
		{
			argHistory.Add(arg);
		}

		public static WaitUntil UntilInvoked(UnityEvent<T> evt)
		{
			EventWait<T> waiter = new EventWait<T>(evt);
			return new WaitUntil(() => waiter.IsInvoked);
		}

		public void Dispose() {
			this.event_.RemoveListener(this.OnInvoke);
		}
	}
}