using UnityEngine;
using UnityEngine.Events;

namespace FuseTools.Ext
{
	public class LineRendererExt : MonoBehaviour
	{
		[System.Serializable]
		public class Evts
		{
			//[System.Serializable]
			//public class Vector3Event : UnityEvent<Vector3> {}
			//public Vector3Event Position;
		}

		[Tooltip("Defaults to the first LineRenderer found on this GameObject's")]
		public LineRenderer Component;

		protected LineRenderer resolved { get { return this.Component == null ? this.GetComponent<LineRenderer>() : this.Component; } }

		//public Evts Events;
      
		#region Methods
		public void SetFirstPosition(Vector3 p) {
			if (this.resolved.positionCount == 0)
				this.resolved.SetPositions(new Vector3[] { p });
			else {
				this.resolved.SetPosition(0, p);
			}
		}
      
		public void SetSecondPosition(Vector3 p)
		{
			// TODO; check "UseWorldSpace" flag
			if (this.resolved.positionCount == 0)
				this.resolved.SetPositions(new Vector3[] { new Vector3(0, 0, 0), p });
			else if (this.resolved.positionCount == 1)
				this.resolved.SetPositions(new Vector3[] { this.resolved.GetPosition(0), p });
			else
			{
				this.resolved.SetPosition(1, p);
			}
		}

		public void SetFromPosition(Transform t)
		{
			this.SetFirstPosition(t.position);
		}
      
		public void SetToPosition(Transform t)
		{
			this.SetSecondPosition(t.position);
		}
		#endregion      
	}
}