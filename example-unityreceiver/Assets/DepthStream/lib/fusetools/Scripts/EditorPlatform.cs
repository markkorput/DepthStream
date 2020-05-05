using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FuseTools {
	public class EditorPlatform : MonoBehaviour
	{
		public bool AtStart = true;

        public UnityEvent OnIsEditor;
        public UnityEvent OnIsNotEditor;
        public BoolEvent OnIsEditorValue;

        // Use this for initialization
        void Start()
		{
			if (this.AtStart) this.Invoke();
		}

		public static bool IsEditor { get {
			#if UNITY_EDITOR
			return true;
			#else
			return false;
			#endif
		}}


		#region Public Action Methods
		public void Invoke()
		{
            var isEditor = IsEditor;

            (isEditor ? this.OnIsEditor : this.OnIsNotEditor).Invoke();
            this.OnIsEditorValue.Invoke(isEditor);
		}
	    #endregion
	}
}