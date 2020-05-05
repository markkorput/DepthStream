using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FuseTools {
	public class Platform : MonoBehaviour
	{

		[System.Serializable]
		public class Evts
		{
			public UnityEvent IsWindows;
			public UnityEvent IsOSX;
			public UnityEvent IsIOS;
			public UnityEvent IsAndroid;
		}

		public bool AtAwake = true;
		[Tooltip("When NOT checked both IsIOS and IsOSX events will be invoked while developeing on a mac for the iOS platform. When checked this scenarion will only invoke the IsIOS event.")]
		public bool OnlyOne = false;
		public Evts Events;

		// Use this for initialization
		void Start()
		{
			if (this.AtAwake) this.InvokePlatformEvents();
		}

		public static bool IsIOS { get {
			#if UNITY_IOS
			return true;
			#else
			return false;
			#endif
		}}

		public static bool IsAndroid { get {
			#if UNITY_ANDROID
			return true;
			#else
			return false;
			#endif
		}}

		public static bool IsOSX { get {
			#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
			return true;
			#else
			return false;
			#endif
		}}

		public static bool IsWindows { get {
			#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN && !(UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX)
			return true;
			#else
			return false;
			#endif
		}}

		#region Public Action Methods
		public void InvokePlatformEvents()
		{
			if (IsIOS) {
				this.Events.IsIOS.Invoke();
				if (this.OnlyOne) return;
			}

			if (IsAndroid) {
				this.Events.IsAndroid.Invoke();
				if (this.OnlyOne) return;
			}

			if (IsOSX) {
				this.Events.IsOSX.Invoke();
				if (this.OnlyOne) return;
			}

			if (IsWindows) {
				this.Events.IsWindows.Invoke();
				if (this.OnlyOne) return;
			}
		}
	#endregion
	}
}