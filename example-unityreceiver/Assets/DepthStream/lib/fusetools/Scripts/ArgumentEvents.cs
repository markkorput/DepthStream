using UnityEngine;
using UnityEngine.Events;

namespace FuseTools
{
	// Native Value Arguments
	[System.Serializable]
	public class FloatEvent : UnityEvent<float> { }
	[System.Serializable]   
	public class StringEvent : UnityEvent<string> { }
	[System.Serializable]
	public class IntEvent : UnityEvent<int> { }
	[System.Serializable]
    public class UintEvent : UnityEvent<uint> { }   
	[System.Serializable]
	public class BoolEvent : UnityEvent<bool> { }
	[System.Serializable]
    public class DoubleEvent : UnityEvent<double> { }

	// Native Array Arguments
    [System.Serializable]
    public class FloatsEvent : UnityEvent<float[]> { }
    [System.Serializable]
    public class StringsEvent : UnityEvent<string[]> { }
    [System.Serializable]
    public class IntsEvent : UnityEvent<int[]> { }
	[System.Serializable]
    public class UintsEvent : UnityEvent<uint[]> { }
    [System.Serializable]
    public class BoolsEvent : UnityEvent<bool[]> { }
    [System.Serializable]
    public class DoublesEvent : UnityEvent<double[]> { }

	// Unity Object Arguments
	[System.Serializable]
    public class Vector2Event : UnityEvent<Vector2> { }
    [System.Serializable]
    public class Vector3Event : UnityEvent<Vector3> { }
    [System.Serializable]
    public class QuaternionEvent : UnityEvent<Quaternion> { }
   
	[System.Serializable]
	public class Vector2IntEvent : UnityEvent<UnityEngine.Vector2Int> { }
    [System.Serializable]
	public class Vector3IntEvent : UnityEvent<UnityEngine.Vector3Int> { }

	[System.Serializable]
    public class GameObjectEvent : UnityEvent<GameObject> {}
	[System.Serializable]
	public class TransformEvent : UnityEvent<Transform> {}
	[System.Serializable]
	public class CameraEvent : UnityEvent<Camera> { }
    [System.Serializable]
    public class RayEvent : UnityEvent<Ray> { }
    [System.Serializable]
    public class AudioClipEvent : UnityEvent<AudioClip> { }

    [System.Serializable]
    public class VideoClipEvent : UnityEvent<UnityEngine.Video.VideoClip> { }

    [System.Serializable]
    public class ColorEvent : UnityEvent<Color> { }
    [System.Serializable]
    public class AnimationCurveEvent : UnityEvent<UnityEngine.AnimationCurve> { }
}
