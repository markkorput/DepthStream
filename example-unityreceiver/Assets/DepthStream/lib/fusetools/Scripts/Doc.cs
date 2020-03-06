using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FuseTools
{
	/// <summary>
	/// In-editor documentation (for development purposes only)
	/// </summary>
	[AddComponentMenu("FuseTools/Doc")]
	public class Doc : MonoBehaviour
	{
		[TextArea]
		public string Text;
		public bool IsLocked = false;
	}

#if UNITY_EDITOR
    [CustomEditor(typeof(Doc))]
    [CanEditMultipleObjects]
	public class DocEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			var doc = (Doc)this.target;
			
			if (doc.IsLocked) {
            	EditorGUILayout.HelpBox(doc.Text, MessageType.Info);
				doc.IsLocked = !EditorGUILayout.Toggle("Edit", !doc.IsLocked);
				return;
			}

			this.DrawDefaultInspector();
		}
    }
#endif
}