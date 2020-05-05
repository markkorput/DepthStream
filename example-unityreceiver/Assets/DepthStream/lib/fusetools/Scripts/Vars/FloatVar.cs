using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FuseTools {
    public class FloatVar : MonoBehaviour
    {
        public float Value;

        [System.Serializable]
        public class Evts {
            public FuseTools.FloatEvent Value;
            public UnityEvent ComparisonEqual;
            public UnityEvent ComparisonUnequal;
            public FuseTools.FloatEvent MathResult;
        }

        public Evts Events;

        public void InvokeValue() {
            this.Events.Value.Invoke(this.Value);
        }

        public void SetValue(float val) { this.Value = val; this.InvokeValue(); }

        public void CompareWith(float otherValue) { 
            bool areEqual = (otherValue == this.Value);
            (areEqual ? this.Events.ComparisonEqual : this.Events.ComparisonUnequal).Invoke();
        }

        #region Lerp
        [System.Serializable]
        public class LerpOpts {
            public float LerpFactor = 0.2f;
            public float DoneDifference = 0.0001f;
            public UnityEvent OnDone = new UnityEvent();
        }

        public LerpOpts LerpOptions = new LerpOpts();

        public void LerpTo(float val) {
            StartCoroutine(this.LerpToCoro(val, this.LerpOptions.LerpFactor, this.LerpOptions.DoneDifference, () => this.LerpOptions.OnDone.Invoke()));
        }

        public void LerpToZero() {
            this.LerpTo(0.0f);
        }

        public void Multiply(float v) {
            this.Events.MathResult.Invoke(v * this.Value);
        }

        private IEnumerator LerpToCoro(float val, float lerpFactor, float doneDelta, System.Action doneFunc = null) {
            while (true) {
                float v = this.Value;
                float delta = (v - val);

                if (System.Math.Abs(delta) < doneDelta) {
                    this.SetValue(val);
                    doneFunc.Invoke();
                    yield break;
                }

                this.SetValue(Mathf.Lerp(v, val, lerpFactor));
                yield return null;
            }
        }
        #endregion
    }
}