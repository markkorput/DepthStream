using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FuseTools {
    public class EaseTo : MonoBehaviour
    {
        public Transform Subject;
        public Transform Target;
        public float EaseFactor = 0.01f;
        public float DoneDistance = 0.0001f;

        public UnityEvent DoneEvent;
        public UnityEvent StartEvent;

        private  bool bDone = true;

        private Vector3 Delta { get {
            return this.Target.position - this.Subject.position;
        } }

        private bool IsWithinDoneDistance { get {
            return this.Delta.magnitude <= this.DoneDistance;
        }}

        void Start()
        {
            if (this.Subject == null) this.Subject = this.transform;   
            this.SetTarget(this.Target);
        }

        void Update()
        {
            if (this.Subject == null || this.Target == null) return;

            var within = this.IsWithinDoneDistance;

            if (within && !this.bDone) {
                this.bDone = true;
                this.Subject.position = this.Target.position;
                this.DoneEvent.Invoke();
                return;
            }

            if (this.bDone && !within) {
                this.bDone = false;
                this.Subject.position += this.Delta * this.EaseFactor;
                this.StartEvent.Invoke();
                return;
            }

            this.Subject.position += this.Delta * this.EaseFactor;
        }

        #region Public Action Methods
        public void SetTarget(Transform t) {
            this.Target = t;

            if (this.Subject != null && this.Target != null) {
                this.bDone = this.IsWithinDoneDistance;
                (this.bDone ? this.DoneEvent : this.StartEvent).Invoke();
            }
        }
        #endregion
    }
}