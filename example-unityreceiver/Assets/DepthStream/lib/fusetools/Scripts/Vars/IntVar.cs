using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FuseTools {
    public class IntVar : MonoBehaviour
    {
        public int Value;

        [System.Serializable]
        public class Evts {
            public FuseTools.IntEvent Value;
            public UnityEvent ComparisonEqual;
            public UnityEvent ComparisonUnequal;
        }

        public Evts Events;

        public void InvokeValue() {
            this.Events.Value.Invoke(this.Value);
        }

        public void SetValue(int val) { this.Value = val; }
        public void SetValue(float val) { this.Value = Mathf.FloorToInt(val); }

        public void CompareWith(int otherValue) { 
            bool areEqual = (otherValue == this.Value);
            (areEqual ? this.Events.ComparisonEqual : this.Events.ComparisonUnequal).Invoke();
         }
    }
}