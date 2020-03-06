using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace FuseTools
{
	/// <summary>
	/// 
	/// </summary>
	/// 
	[AddComponentMenu("FuseTools/InputFieldExt")]
	public class InputFieldExt: MonoBehaviour
	{
		[Tooltip("Defaults to first InputField found on this GameObject or any of its children")]
		public UnityEngine.UI.InputField InputField;

		private UnityEngine.UI.InputField Resolved { get {
				return this.InputField != null
							   ? this.InputField
							   : this.GetComponentInChildren<UnityEngine.UI.InputField>();
				}}

		[System.Serializable]
		public class StringEvent : UnityEvent<string> {}
      
		[System.Serializable]
		public class Evts
		{
			public StringEvent Text;
		}
      
		public Evts Events;

		#region Public Methods
		public void InvokeText() {
			var inputField = this.Resolved;
			if (inputField != null) this.Events.Text.Invoke(inputField.text);
		}
		#endregion
	}
}