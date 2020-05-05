using UnityEngine;
using UnityEngine.Events;

namespace FuseTools.Test
{
	public class EventWait
	{      
		private UnityEvent event_;
		private bool isInvoked_ = false;

		public EventWait(UnityEvent evt)
		{
			this.event_ = evt;
			evt.AddListener(this.OnInvoke);
		}

		private void OnInvoke()
		{
			this.isInvoked_ = true;
			this.event_.RemoveListener(this.OnInvoke);
		}

		public bool IsInvoked { get { return this.isInvoked_; } }

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
	}

	public class EventWait<T>
	{
		private UnityEvent<T> event_;
		private bool isInvoked_ = false;

		public EventWait(UnityEvent<T> evt)
		{
			this.event_ = evt;
			evt.AddListener(this.OnInvoke);
		}

		private void OnInvoke(T arg)
		{
			this.isInvoked_ = true;
			this.event_.RemoveListener(this.OnInvoke);
		}

		public bool IsInvoked { get { return this.isInvoked_; } }

		public static WaitUntil UntilInvoked(UnityEvent<T> evt)
		{
			EventWait<T> waiter = new EventWait<T>(evt);
			return new WaitUntil(() => waiter.IsInvoked);
		}
	}
}