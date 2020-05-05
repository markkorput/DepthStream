using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace FuseTools.Tests
{
	public class DestroySelfTests
	{
		//[Test]
		//void SyncTest() {}

		[UnityTest]
		public IEnumerator DontAutoDestroyByDefault()
		{
			var parent = new GameObject("DestroySelfDefault-Parent");
			var obj = new GameObject("DestroySelfDefault-Child");
			obj.transform.SetParent(parent.transform);
			Assert.AreEqual(parent.transform.childCount, 1);

			// Object Has NOT Been Destroyed In the Start and OnDisable Methods
			var destroySelf = obj.AddComponent<FuseTools.DestroySelf>();
			Assert.AreEqual(destroySelf.When, FuseTools.DestroySelf.Moment.None);

			yield return null;
			Assert.AreEqual(parent.transform.childCount, 1);

			// Object NOT auto-destroyed in OnDisable method
			obj.SetActive(false);
			yield return null;         
			Assert.AreEqual(parent.transform.childCount, 1);
         
			// Object can be destroyed using a public action method
			destroySelf.DestroySelfNow();
			yield return null;
			Assert.AreEqual(parent.transform.childCount, 0);

            // cleanup
			GameObject.Destroy(parent);
		}

		[UnityTest]
		public IEnumerator DestroySelfOnEnable()
		{
			var parent = new GameObject("DestroySelfOnEnable-Parent");
			var obj = new GameObject("DestroySelfOnEnable-Child");
			obj.transform.SetParent(parent.transform);
			Assert.True(parent.transform.childCount == 1);

			obj.SetActive(false);
			var destroySelf = obj.AddComponent<FuseTools.DestroySelf>();
			destroySelf.When = FuseTools.DestroySelf.Moment.OnEnable;
			obj.SetActive(true);

			// yield to skip a frame
			yield return null;

			// Object Has Been Destroyed In the Start Method
			Assert.True(parent.transform.childCount == 0);

			// cleanup
            GameObject.Destroy(parent);
		}
      
		[UnityTest]
		public IEnumerator DestroySelfOnDisable()
		{
			var parent = new GameObject("DestroySelfOnDisable-Parent");
			var obj = new GameObject("DestroySelfOnDisable-Child");
			obj.transform.SetParent(parent.transform);
         
			Assert.True(parent.transform.childCount == 1);
			var destroySelf = obj.AddComponent<FuseTools.DestroySelf>();
			destroySelf.When = FuseTools.DestroySelf.Moment.OnDisable;
         
			// yield to skip a frame
			yield return null;
         
			// Object Still Alive
			Assert.True(parent.transform.childCount == 1);
			obj.SetActive(false);

			// yield to skip a frame
			yield return null;

			// Object Has Been Destroyed In its OnDisable Method
			Assert.True(parent.transform.childCount == 0);
         
			// cleanup
            GameObject.Destroy(parent);
		}
	}
}