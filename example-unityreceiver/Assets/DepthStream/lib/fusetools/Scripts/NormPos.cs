using System;
using UnityEngine;

namespace FuseTools
{
	public class NormPos
	{
		public static Vector3 NormalisedToLocal(Vector2 normalisedPosition, RectTransform rt)
		{
			Vector2 localPos = (normalisedPosition - rt.pivot) * rt.sizeDelta;
			return new Vector3(localPos.x, localPos.y, 0.0f);
		}
      
		public static Vector2 LocalToNormalised(Vector3 localPosition, RectTransform rt, bool clamp = false)
		{
			var vec2 = LocalToNormalised(new Vector2(localPosition.x, localPosition.y), rt, clamp);
			if (clamp) vec2 = Clamp(vec2, new Rect(0, 0, 1, 1));
			return vec2;
		}

		public static Vector2 LocalToNormalised(Vector2 localPosition, RectTransform rt, bool clamp = false)
		{
			var size = new Vector2(
				Mathf.Abs(rt.sizeDelta.x) < float.Epsilon ? rt.rect.width : rt.sizeDelta.x,
				Mathf.Abs(rt.sizeDelta.y) < float.Epsilon ? rt.rect.height : rt.sizeDelta.y);
			var pos = new Vector2(localPosition.x, localPosition.y);
			
			var norm = pos / size + rt.pivot;
			// Debug.Log("LocalToNormalised, size = "+size.ToString()+", pivot: "+rt.pivot.ToString()+", pos: "+pos.ToString()+", norm: "+norm.ToString());
			return norm;
		}

		public static Vector2 Clamp(Vector2 value, Rect rect)
		{
			return new Vector2(Mathf.Clamp(value.x, rect.min.x, rect.max.x), Mathf.Clamp(value.y, rect.min.y, rect.max.y));
		}
	}
}