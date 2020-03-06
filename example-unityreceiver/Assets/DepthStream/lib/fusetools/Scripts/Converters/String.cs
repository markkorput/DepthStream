using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FuseTools.Converters
{
	public class String : MonoBehaviour
	{
		public string Value;
      
		public FuseTools.StringEvent ValueEvent;

		#region Public Action Methods
		public void InvokeValue() {
			this.ValueEvent.Invoke(this.Value);
		}
		#endregion

		#region Public Static Methods
		/// Replaces {DesktopPath} with the full path of the current user's desktop folder
		/// Replaces {ApplicationDataPath} with the full path of the current user's application data folder
		/// Replaces {DesktopPath} with the full path of the application's persistent data folder
		public static string ConvertMacros(string input) {
			return input
				.Replace("{DesktopPath}", System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop))
				.Replace("{ApplicationDataPath}", System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData))
				.Replace("{PersistentDataPath}", Application.persistentDataPath)
				;
		}
		#endregion
	}
}