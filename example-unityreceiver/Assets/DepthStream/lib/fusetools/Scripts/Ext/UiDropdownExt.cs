using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace FuseTools
{
	[AddComponentMenu("FuseTools/UiDropdownExt")]
	public class UiDropdownExt : MonoBehaviour
	{
		[Tooltip("Defaults to first PlayableDirector found on this GameObject")]
		public UnityEngine.UI.Dropdown Dropdown;


		//[System.Serializable]
		//public class Evts
		//{
		//	public UnityEvent Played;
		//}
      
		//public Evts Events;

		void Start()
		{
			if (Dropdown == null) this.Dropdown = GetComponent<UnityEngine.UI.Dropdown>();
		}
      
		#region Public Methods
		public void SetOptions(string[] optionTexts) {
			if (this.Dropdown == null) return;         
			this.Dropdown.ClearOptions();         
			this.Dropdown.AddOptions(new List<string>(optionTexts));
			// this.Dropdown.AddOptions((from txt in optionTexts select new UnityEngine.UI.Dropdown.OptionData(txt)).ToList());
		}
		#endregion
	}
}