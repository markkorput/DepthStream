using UnityEngine;
using UnityEngine.Events;

namespace FuseTools.Converters
{
	public class IntToString : MonoBehaviour
	{
		[Tooltip("Replaces \"{}\" with the Integer value, or (TODO!) #### with an zero-padded integer value")]
        public string Pattern = "{}";
      
		[System.Serializable]
        public class ConvertedEvent : UnityEvent<string> {}
        public ConvertedEvent OnConvert;

        public void Convert(int val)
        {
			this.OnConvert.Invoke(this.ConvertValue(val));
        }

		public string ConvertValue(int val) {
			return this.Pattern.Replace("{}", val.ToString());
		}
	}
}