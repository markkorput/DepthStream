using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuseTools
{
	/// <summary>
    /// This component, when attached to a GameObject which also has a Camera Component,
	/// will make that camera render everything in wireframe
    /// </summary>
    public class WireFrameCam : MonoBehaviour
    {
		private bool restoreValue;
		private bool isApplied = false;

        void OnPreRender()
        {
			if (this.isActiveAndEnabled)
			{
				this.restoreValue = GL.wireframe;
				GL.wireframe = true;
				this.isApplied = true;
			}
        }

        void OnPostRender()
        {
			if (this.isApplied) {
				GL.wireframe = this.restoreValue;
				this.isApplied = false;
			}
        }
    }
}