using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FuseTools {
    public class TapUnlock : MonoBehaviour
    {
        public int MinTaps = 10;
        public float MinTapsPerSecond = 5;
        public bool ResetOnUnlock = true;

        public UnityEvent OnUnlock;

        #if UNITY_EDITOR
        [System.Serializable]
        public class Dinfo {
            public int TapCount;
            public float Velocity;
        }

        public Dinfo DebugInfo;
        #endif

        private Queue<float> tapTimes = new Queue<float>();

        public void Tap() {
            var t = Time.time;
            tapTimes.Enqueue(t);

            #if UNITY_EDITOR
            this.DebugInfo.TapCount = tapTimes.Count;
            this.DebugInfo.Velocity = this.Velocity;
            #endif

            if (tapTimes.Count < MinTaps) return;
            while (tapTimes.Count > MinTaps) tapTimes.Dequeue();

            var vel = this.Velocity;
            if (vel >= this.MinTapsPerSecond) {
                this.Unlock();
            }
        }

        private float Velocity { get { 
            var times = tapTimes.ToArray();
            if (times.Length < 2) return 0.0f;
            var dur = times[times.Length-1] - times[0];
            return times.Length / dur;
        }}

        private void Unlock() {
            this.OnUnlock.Invoke();
            if (this.ResetOnUnlock) this.tapTimes.Clear();
        }
    }
}