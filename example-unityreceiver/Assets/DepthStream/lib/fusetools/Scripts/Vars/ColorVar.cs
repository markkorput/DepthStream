using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FuseTools {
    public class ColorVar : MonoBehaviour
    {
        public Color Value = new Color(255,255,255,1);

        [System.Serializable]
        public class Evts {
            public FuseTools.ColorEvent Value;
        }

        public Evts Events;

        public void InvokeValue() {
            this.Events.Value.Invoke(this.Value);
        }

        public void SetValue(Color val) {
            if (this.Value.Equals(val)) return;
            this.Value = val;
            this.InvokeValue();
        }

        public void SetAlpha(float alpha) {
            SetValue(new Color(this.Value.r, this.Value.g, this.Value.b, alpha));
        }
    }
}