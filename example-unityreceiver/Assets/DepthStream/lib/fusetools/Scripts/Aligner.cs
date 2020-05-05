using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FuseTools
{
	/// <summary>
	/// Aligns one Transform with Another
	/// </summary>
	[AddComponentMenu("FuseTools/Aligner")]
	public class Aligner : MonoBehaviour
	{
		public enum AxisDef { ALL, X, Y, Z, XY, XZ, YZ };
      
		public static int AXIS_X = 1;
		public static int AXIS_Y = 2;
		public static int AXIS_Z = 4;      
		public static int AXIS_XY = AXIS_X | AXIS_Y;
		public static int AXIS_XZ = AXIS_X | AXIS_Z;
		public static int AXIS_YZ = AXIS_Y | AXIS_Z;
		public static int AXIS_ALL = AXIS_Y | AXIS_Y | AXIS_Z;

		[Tooltip("Transform to move (align)")]
		public Transform Subject;
		[Tooltip("Transform who's position will be made to match with target's position")]
		public Transform SubjectAnchor;
		[Tooltip("The target who's position will be aligned with")]
		public Transform TargetAnchor;
		public AxisDef Axis = AxisDef.ALL;
		public bool AtStart = true;
		public bool EachUpdate = true;
      
		private void Start()
        {
            if (AtStart) this.Align();
        }
      
		private void Update()
		{
			if (EachUpdate) this.Align();
		}

		private int AxisDefToInt(AxisDef def) {
			if (def.Equals(AxisDef.ALL)) return AXIS_ALL;
			if (def.Equals(AxisDef.X)) return AXIS_X;
			if (def.Equals(AxisDef.Y)) return AXIS_Y;
			if (def.Equals(AxisDef.Z)) return AXIS_Z;
			if (def.Equals(AxisDef.XY)) return AXIS_XY;
			if (def.Equals(AxisDef.XZ)) return AXIS_XZ;
			if (def.Equals(AxisDef.YZ)) return AXIS_YZ;
			return 0;
		}
      
		#region Public Action Methods
        public void Align()
        {
            Align(this.Subject, this.SubjectAnchor, this.TargetAnchor, this.AxisDefToInt(this.Axis));
        }
      
		public void AlignX(float worldPosX)
        {
			Align(this.Subject, this.SubjectAnchor.position, new Vector3(worldPosX, this.SubjectAnchor.position.y, this.SubjectAnchor.position.z), this.AxisDefToInt(this.Axis));
        }

		public void AlignY(float worldPosY)
        {
			Align(this.Subject, this.SubjectAnchor.position, new Vector3(this.SubjectAnchor.position.x, worldPosY, this.SubjectAnchor.position.z), this.AxisDefToInt(this.Axis));
        }
      
		public void AlignZ(float worldPosZ)
        {
			Align(this.Subject, this.SubjectAnchor.position, new Vector3(this.SubjectAnchor.position.x, this.SubjectAnchor.position.y, worldPosZ), this.AxisDefToInt(this.Axis));
        }
		#endregion
      
		public static void Align(Transform subject, Transform subjectAnchor, Transform targetAnchor, int axisFlags)
		{
			Align(subject, subjectAnchor.position, targetAnchor.position, axisFlags);
		}
      
		public static void Align(Transform subject, Vector3 subjectAnchorPos, Vector3 targetAnchorPos, int axisFlags) {
			Vector3 delta = targetAnchorPos - subjectAnchorPos;
			Vector3 flaggedDelta = new Vector3(
				(axisFlags & AXIS_X) == 0 ? 0 : delta.x,
				(axisFlags & AXIS_Y) == 0 ? 0 : delta.y,
				(axisFlags & AXIS_Z) == 0 ? 0 : delta.z);
			// Debug.Log("Aligner.Align: offset =" + delta.ToString() + ", axis=" + axisFlags.ToString());
			subject.position += flaggedDelta;         
		}
	}
}