using UnityEngine;
using UnityEngine.Events;

namespace FuseTools.Converters
{
	public class FloatToString : MonoBehaviour
	{
		[Tooltip("Replaces \"{}\" with the Float value, or (TODO!) #### with an zero-padded integer value")]
        public string Pattern;
      
		[System.Serializable]
        public class ConvertedEvent : UnityEvent<string> {}
        public ConvertedEvent OnConvert;
      
        public void Convert(float val)
        {
			// Debug.Log("FloatToString: " + this.ConvertValue(val));
			this.OnConvert.Invoke(this.ConvertValue(val));
        }

		public void Convert(double val)
        {
			this.Convert((float)val);
        }

		public string ConvertValue(float val)
		{
			var regex = new System.Text.RegularExpressions.Regex(@"\{(.+)\}");
			var matches = regex.Match(Pattern);
         
			if (matches.Success)
			{
				return ((string)Pattern.Clone()).Replace(matches.Value, val.ToString(matches.Value.Replace("{", "").Replace("}", "")));
			}
         
			string result = ((string)Pattern.Clone()).Replace(
				"{}", val.ToString());
         
			return result;
		}
	}
}