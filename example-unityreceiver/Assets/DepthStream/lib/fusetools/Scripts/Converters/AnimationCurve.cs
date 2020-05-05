using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuseTools.Converters {
    public class AnimationCurve : MonoBehaviour
    {
        public UnityEngine.AnimationCurve Curve;

        public FuseTools.AnimationCurveEvent OnAnimationCurve = new FuseTools.AnimationCurveEvent();
        public FuseTools.FloatEvent OnCurveValue = new FuseTools.FloatEvent();

        public void InvokeCurveValue(int x) {
            float val = this.Curve.Evaluate(x);
            this.OnCurveValue.Invoke(val);
        }

        public void InvokeCurveValue(float x) {
            float val = this.Curve.Evaluate(x);
            this.OnCurveValue.Invoke(val);
        }

        public void InvokeAnimationCurve() {
            this.OnAnimationCurve.Invoke(this.Curve);
        }

        public static void Apply(UnityEngine.AnimationCurve src, UnityEngine.AnimationCurve dest) {
            dest.keys = new Keyframe[]{};
            foreach(var keyf in src.keys) {
                dest.AddKey(keyf);
            }
        }
    }
}