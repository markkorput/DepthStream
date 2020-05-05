using UnityEngine;
using UnityEngine.Events;

namespace FuseTools.Converters
{
    public class QuaternionConverter : MonoBehaviour
    {
        [System.Serializable]
        public class FloatEvent : UnityEvent<float> { }
		public FloatEvent FloatValue;
        
		public void InvokeEulerX(Quaternion q)
        {
            this.FloatValue.Invoke(q.eulerAngles.x);
        }   

		public void InvokeEulerY(Quaternion q)
        {
            this.FloatValue.Invoke(q.eulerAngles.y);
        }   

		public void InvokeEulerZ(Quaternion q)
        {
			this.FloatValue.Invoke(q.eulerAngles.z);
        }

        public void InvokeEulerZDegreesInverted(Quaternion q)
        {
            var forward = new Vector3(0,0, 1);
            var rotated = q * forward;
            var degrees = Vector3.Angle(new Vector3(rotated.x, 0, rotated.z), forward);
            if (rotated.x < 0.0f) degrees = -degrees;

            // var degrees = q.eulerAngles.z / Mathf.PI * 180;
            // var degrees = q.eulerAngles.z; /// Mathf.PI * 180;
            degrees = -degrees;
            if (degrees < 0.0) degrees = 360 + degrees;
			this.FloatValue.Invoke(degrees);
        }
    }
}