using System;
using UnityEngine;

namespace FuseTools
{
  [AddComponentMenu("FuseTools/ApplicationExt")]
	public class ApplicationExt : MonoBehaviour
	{
    [System.Serializable]
    public class Evts {
      public StringEvent Version;
    }

    public Evts Events;
    
		#region Public Action Methods
		public void InvokeVersion()
		{
      this.Events.Version.Invoke(Application.version);
		}
		#endregion
	}
}
