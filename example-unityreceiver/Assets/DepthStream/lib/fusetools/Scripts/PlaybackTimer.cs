using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FuseTools {

    public class PlaybackTimer : MonoBehaviour {
      public PlaybackTimerData PlaybackTimerData;
      public float Duration = -1;
      public bool AutoStop = true;

      public UnityEvent AutoStopEvent;

      void Update() {
        if (this.PlaybackTimerData.IsRunning) {
          if (AutoStop && this.Duration > 0.0f && PlaybackTimerData.Time >= Duration) {
            PlaybackTimerData.Stop();
            PlaybackTimerData.Time = Duration;
            this.AutoStopEvent.Invoke();
          }
        }
      }

      public void SetState(PlaybackTimerData.StateType state) {
        this.PlaybackTimerData.SetState(state);
      }

      public void SetDuration(float dur) {
        this.Duration = dur;
      }
    }

    [System.Serializable]
    public class PlaybackTimerData {
        public enum StateType { NotStarted, Started, Paused, Stopped }

        public StateType State = StateType.NotStarted;
        public double LastStateChangeAt = 0;
        public double TimeLastStateChange = 0;
        public float Frequency = 60.0f;

        public bool IsRunning { get { return this.State.Equals(StateType.Started); }}
        public bool IsPaused { get { return this.State.Equals(StateType.Paused); }}
        public bool IsStopped { get { return this.State.Equals(StateType.Stopped); }}
        public bool IsNotStarted { get { return this.State.Equals(StateType.NotStarted); }}

        public double Time {
          get {
            double rectime = TimeLastStateChange;
            
            if (State.Equals(StateType.Started))
            {
                rectime += (SystemTimeInSeconds - LastStateChangeAt);
            }
            
            return rectime;
          }

          set {
            TimeLastStateChange = value;
            this.LastStateChangeAt = SystemTimeInSeconds;
          }
        }

        public static double SystemTimeInSeconds { get {
            return FuseTools.ThreadInterval.ticksToSeconds * (double)System.DateTime.Now.Ticks;
        }}

        public uint FrameIndex {
          get {
            return FrameIndexForTimeInSeconds(this.Time);
          }

          set {
            var t = (float)value / this.Frequency;
            this.Time = t;
          }
        }

        private uint FrameIndexForTimeInSeconds(double t) {
          return (uint)Mathf.FloorToInt((float)t * this.Frequency);
        }

        private void SetStateInternal(StateType val) {
          this.LastStateChangeAt = SystemTimeInSeconds;
          this.State = val;         
        }

        public void SetState(StateType val) {
          switch(val) {
            case StateType.Started: this.Start(); break;
            case StateType.Paused: this.Pause(); break;
            case StateType.Stopped: this.Stop(); break;
            case StateType.NotStarted: this.Reset(); break;
            default: return;
          }
        }

        public void Start(bool resumeIfPaused=false)
        {         
          if (!this.State.Equals(StateType.Paused) || !resumeIfPaused)
                {
                    // restart
                    this.TimeLastStateChange = 0.0;
                }

              this.SetStateInternal(StateType.Started);
        }
          
        public void Resume() {
          this.Start(true);
        }

        public void Pause()
            {
          this.TimeLastStateChange = this.Time;
                this.SetStateInternal(StateType.Paused);
            }
            
        public void Stop() {
          if (this.State.Equals(StateType.Started))
          {
              var t = this.Time;
              this.TimeLastStateChange = t;
          }

          this.SetStateInternal(StateType.Stopped);
        }

        public void Reset() {
          // this.Stop();
          this.State = StateType.NotStarted;
          this.LastStateChangeAt = 0;
          this.TimeLastStateChange = 0;
        }
    }
}