using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuseTools
{
	/// <summary>
    /// Update its GameObject's Transform every Update to look in the direction
	/// of the specified Target (which optionally defaults to the Camera.main.transform)
    /// </summary>
	[AddComponentMenu("FuseTools/LookAt")]
	public class LookAt : MonoBehaviour
	{      
		public Transform Target;
		public bool DefaultToMainCam = true;      
		public Vector3 RotationOffset;

		void Update()
		{
			var t = this.Target;
			if (t == null && this.DefaultToMainCam && Camera.main != null) t = Camera.main.transform;
			if (t == null) return;
         
			this.transform.LookAt(t, Vector3.up);
			this.transform.Rotate(RotationOffset);
		}
	}
}