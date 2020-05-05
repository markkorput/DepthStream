using UnityEngine;
using UnityEngine.Events;

namespace FuseTools.Converters
{
    public class Vector3ToFloat : MonoBehaviour
    {
        [System.Serializable]
        public class FloatEvent : UnityEvent<float> { }

        public FloatEvent Value;
      
        public void ConvertX(Vector3 vec)
        {
			this.Value.Invoke(vec.x);
        }

		public void ConvertY(Vector3 vec)
        {
            this.Value.Invoke(vec.y);
        }

		public void ConvertZ(Vector3 vec)
        {
            this.Value.Invoke(vec.z);
        }     
      
		public void ConvertMagnitude(Vector3 vector) {
			this.Value.Invoke(vector.magnitude);
		}
    }
}