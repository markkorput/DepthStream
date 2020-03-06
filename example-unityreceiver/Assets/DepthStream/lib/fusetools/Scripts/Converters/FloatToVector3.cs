using UnityEngine;
using UnityEngine.Events;

namespace FuseTools.Converters
{
    public class FloatToVector3 : MonoBehaviour
    {
        [System.Serializable]
        public class ConvertedEvent : UnityEvent<Vector3> { }
      
		public float defaultAxisValue = 0.0f;
        public ConvertedEvent OnConvert;
      
        public void Convert(float val)
        {
			this.OnConvert.Invoke(new Vector3(val, val, val));
        }
      
		public void ConvertX(float val)
        {
			this.OnConvert.Invoke(new Vector3(val, defaultAxisValue,defaultAxisValue));
        }

		public void ConvertY(float val)
        {
			this.OnConvert.Invoke(new Vector3(defaultAxisValue, val, defaultAxisValue));
        }

		public void ConvertZ(float val)
        {
			this.OnConvert.Invoke(new Vector3(defaultAxisValue,defaultAxisValue,val));
        }      
    }
}