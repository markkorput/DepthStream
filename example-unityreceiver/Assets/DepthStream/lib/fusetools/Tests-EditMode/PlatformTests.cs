using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using UnityEngine.Events;

namespace FuseTools.Tests
{
	public class PlatformTests
	{
		private void init(Platform platform) {
			platform.Events = new Platform.Evts();
			platform.Events.IsIOS = new UnityEvent();
			platform.Events.IsWindows = new UnityEvent();
			platform.Events.IsOSX = new UnityEvent();
			platform.Events.IsAndroid = new UnityEvent();
		}
      
		[Test]
		public void IsIOS_Event()
		{

			var entity = new GameObject();
			var platform = entity.AddComponent<Platform>();
			this.init(platform);

			int ios_count = 0;         
			platform.Events.IsIOS.AddListener(() => ios_count+= 1);         
			platform.InvokePlatformEvents();
         
#if UNITY_IOS
			Assert.AreEqual(ios_count, 1);
#else
			Assert.AreEqual(ios_count, 0);
#endif
		}

		[Test]
        public void IsAndroid_Event()
        {

            var entity = new GameObject();
            var platform = entity.AddComponent<Platform>();
			this.init(platform);

            int count = 0;

            platform.Events.IsAndroid.AddListener(() => count += 1);
            Assert.AreEqual(count, 0);
			platform.InvokePlatformEvents();
         
#if UNITY_ANDROID
            Assert.AreEqual(count, 1);
#else
            Assert.AreEqual(count, 0);
#endif
        }

		[Test]
        public void IsOSX_Event()
        {

            var entity = new GameObject();
            var platform = entity.AddComponent<Platform>();
			this.init(platform);

            int count = 0;

            platform.Events.IsOSX.AddListener(() => count += 1);
            Assert.AreEqual(count, 0);
			platform.InvokePlatformEvents();

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
			Assert.AreEqual(count, 1);
#else
            Assert.AreEqual(count, 0);
#endif
        }
      
		[Test]
        public void IsWindows_Event()
        {

            var entity = new GameObject();
            var platform = entity.AddComponent<Platform>();
			this.init(platform);

            int count = 0;

            platform.Events.IsWindows.AddListener(() => count += 1);
            Assert.AreEqual(count, 0);
			platform.InvokePlatformEvents();

#if UNITY_STANDALONE_WIN|| UNITY_EDITOR_WIN
            Assert.AreEqual(count, 1);
#else
            Assert.AreEqual(count, 0);
#endif
        }
   }
}