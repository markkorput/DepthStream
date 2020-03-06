using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuseTools
{
	/// <summary>
    /// This component's Public Action Methods can be called in UnityEvents
	/// Useful for debugging.
    /// </summary>
	public class DebugLogger : MonoBehaviour
	{
		private const uint MAXLINES = 10;
		private Queue<string> lines = new Queue<string>((int)MAXLINES);
        
#if UNITY_EDITOR
		[System.Serializable]
		public class Dinfo
		{
			[Multiline]
			public string Log = "";
		}

		public Dinfo DebugInfo;
#endif

		#region Public Action Methods
		public void LogInt(int number) { this.log(number.ToString()); }
		public void LogFloat(float number) { this.log(number.ToString()); }
		public void LogDouble(double number) { this.log(number.ToString()); }
		public void LogLong(long number) { this.log(number.ToString()); }
		public void LogShort(short number) { this.log(number.ToString()); }
		public void LogBool(bool val) { this.log(val.ToString()); }
		public void LogString(string txt) { this.log(txt); }
		public void LogVector2(Vector2 v2) { this.log(v2.ToString()); }
		public void LogVector3(Vector3 v3) { this.log(v3.ToString()); }
		#endregion
      
		private void log(string txt)
		{
			Debug.Log("[DebugLogger] " + txt);
#if UNITY_EDITOR
			lines.Enqueue(txt);
			while (lines.Count > MAXLINES) lines.Dequeue();

			this.DebugInfo.Log = string.Join("\n", lines.ToArray());
#endif
		}
	}
}