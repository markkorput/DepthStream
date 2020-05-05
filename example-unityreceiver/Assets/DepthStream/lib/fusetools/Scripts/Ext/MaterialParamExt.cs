using UnityEngine;
using UnityEngine.Events;

namespace FuseTools
{
	/// <summary>
	/// Simple component that triggers lifecyce (start, enable, disable) events.
	/// </summary>
	/// 
	[AddComponentMenu("FuseTools/MaterialParamExt")]
	public class MaterialParamExt : MonoBehaviour {
		public Material TheMaterial;
		public string ParamName;

		public Material resolved { get {
			return this.TheMaterial != null ? this.TheMaterial : null; }} // TODO

		public void SetFLoat(float v) {
			this.resolved.SetFloat(this.ParamName, v);
		}
	}
}