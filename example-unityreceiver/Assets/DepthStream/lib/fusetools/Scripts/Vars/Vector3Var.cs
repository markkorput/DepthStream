using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FuseTools {
    public class Vector3Var : MonoBehaviour
    {
        [System.Serializable]
        public class LerpOpts {
            public float LerpFactor = 0.2f;
            public float DoneDifference = 0.0001f;
            public UnityEvent OnDone = new UnityEvent();
        }

        public Vector3 Value;

        public LerpOpts LerpOptions = new LerpOpts();

        [System.Serializable]
        public class Evts {
            public FuseTools.Vector3Event Value;
            public UnityEvent ComparisonEqual;
            public UnityEvent ComparisonUnequal;
        }

        public Evts Events;

        public void InvokeValue() {
            this.Events.Value.Invoke(this.Value);
        }

        public void SetValue(Vector3 val) { this.Value = val; this.InvokeValue(); }
        public void SetValue(float val) { this.SetValue(new Vector3(val, val, val)); }

        public void CompareWith(Vector3 otherValue) { 
            bool areEqual = (otherValue == this.Value);
            (areEqual ? this.Events.ComparisonEqual : this.Events.ComparisonUnequal).Invoke();
        }

        public void LerpTo(Vector3 val) {
            StartCoroutine(this.LerpToCoro(val, this.LerpOptions.LerpFactor, this.LerpOptions.DoneDifference, () => this.LerpOptions.OnDone.Invoke()));
        }

        public void LerpToZero() {
            StartCoroutine(this.LerpToCoro(Vector3.zero, this.LerpOptions.LerpFactor, this.LerpOptions.DoneDifference, () => this.LerpOptions.OnDone.Invoke()));
        }

        private IEnumerator LerpToCoro(Vector3 val, float lerpFactor, float doneDelta, System.Action doneFunc = null) {
            while (true) {
                Vector3 v = this.Value;
                Vector3 delta = (v - val);
                float diff = delta.magnitude;

                if (diff < doneDelta) {
                    this.SetValue(val);
                    doneFunc.Invoke();
                    yield break;
                }

                this.SetValue(Vector3.Lerp(v, val, lerpFactor));
                yield return null;
            }
        }
    }
}