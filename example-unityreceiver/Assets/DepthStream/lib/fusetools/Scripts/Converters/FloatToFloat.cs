using UnityEngine;
using UnityEngine.Events;

namespace FuseTools.Converters
{
	public class FloatToFloat : MonoBehaviour
	{
		public enum ModeType { Add, Subtract, Multiply, Divide, Min, Max, Modulus };
      
		[System.Serializable]
		public struct Operation {
			public ModeType Type;
			public float Value;
		}
      
		public Operation[] Operations;
      
		[System.Serializable]
		public class ConvertedEvent : UnityEvent<float> { }
		public ConvertedEvent OnConvert;      

      
		public static float Convert(float valA, Operation[] ops) {
			float tmp = valA;
         
			foreach(var op in ops) {
				tmp = Convert(tmp, op.Value, op.Type);
			}
         
			return tmp;
		}
      
		public static float Convert(float valA, float valB, ModeType mode)
		{
			switch (mode)
			{
				case ModeType.Add:
                    return valA + valB;               
				case ModeType.Subtract:
					return valA - valB;
				case ModeType.Multiply:
					return valA * valB;
				case ModeType.Divide:
                    return valA / valB;
				case ModeType.Min:
					return Mathf.Min(valA, valB);
				case ModeType.Max:
                    return Mathf.Max(valA, valB);
				case ModeType.Modulus:
					return valA % valB;
			}

			return valA;
		}

		#region Public Actions Methods
		public void Convert(float v)
		{
			// Debug.Log("FloatToString: " + this.ConvertValue(val));
			var result = Convert(v, Operations);
			this.OnConvert.Invoke(result);
		}
        #endregion
	}
}