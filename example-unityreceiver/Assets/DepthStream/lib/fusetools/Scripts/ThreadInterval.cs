using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FuseTools
{
	/// <summary>
	/// Runs a System.Action at a specific interval in a separate thread
	/// </summary>
	[AddComponentMenu("FuseTools/ThreadInterval")]
	public class ThreadInterval : MonoBehaviour {

		#region Constants
		public const long secondsToTicks = 10000000;
		public const double ticksToSeconds = 1.0 / (double)secondsToTicks;
		public const long millisecondsToTicks = 10000;
		public const double ticksToMilliseconds = 1.0 / (double)millisecondsToTicks;
		public const long wakeupTicks = 100 * millisecondsToTicks;
		public const long minSleepTicks = 2 * wakeupTicks;
		#endregion


		public bool StartAtAwake = true;
		public float IntervalInSeconds = 1.0f;


		private Thread intervalThread = null;
		private System.Action intervalFunc = null;
		private bool bContinue = true;
		private long intervalNextTime;
		private long intervalTicks;
		// public UnityEvent OnInterval; // unity can't invoke events from a non-main thread?

		void OnEnable() {
			if (this.StartAtAwake) {
				this.StartInterval(this.IntervalInSeconds, () => {});
			}
		}

		void OnDisable() {
			this.StopInterval(false /* don't join */);
		}

		void OnDestroy() {
			this.StopInterval(true /* join thread */);
		}

		private void IntervalMethod() {
			while (this.bContinue) {
				var t = System.DateTime.Now.Ticks;

				if (t >= this.intervalNextTime) {
					this.intervalFunc.Invoke();
					// if (this.OnInterval != null) this.OnInterval.Invoke();
					this.intervalNextTime = t + this.intervalTicks;
				}

				long restTicks = this.intervalNextTime - t;
				if (restTicks > minSleepTicks) {
					var sleepTicks = restTicks - wakeupTicks;
					Thread.Sleep((int)(sleepTicks * ticksToMilliseconds));
				}
			}
		}

		#region Public Action Methods
		public void SetInterval(float secs) {
			this.IntervalInSeconds = secs;
			this.intervalTicks = (long)((double)this.IntervalInSeconds * (double)secondsToTicks);
		}

		public void SetIntervalFps(float fps) {
			this.SetInterval(1.0f / fps);
		}

		public void StartInterval(float seconds, System.Action func) {
			this.IntervalInSeconds = seconds;
			this.StartInterval(func);
		}

		public void StartInterval(System.Action func) {
			this.StopInterval();
			intervalThread = new Thread(this.IntervalMethod);
			intervalThread.IsBackground = true;
			this.intervalFunc = func;

			this.SetInterval(this.IntervalInSeconds);
			this.intervalNextTime = System.DateTime.Now.Ticks + intervalTicks;
			this.bContinue = true;
			intervalThread.Start();
		}

		public void StopInterval(bool joinThread=false) {
			if (intervalThread != null && intervalThread.IsAlive) {
				this.bContinue = false;
				if (joinThread) this.intervalThread.Join();
				this.intervalThread = null;
			}
		}
		#endregion
	}
}