using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using UnityEngine.Events;

namespace FuseTools.Tests
{
	public class GenericActionTests
	{
		[Test]
		public void HasAnArgumentlessActionAndPublicInvokedActionMethod()
		{
            var entity = new GameObject();
			var genAction = entity.AddComponent<GenericAction>();

			// in edit-mode the public Event attribute isn't auto-initialized
			genAction.Event = new UnityEvent();
         
			int counter = 0;
			genAction.Event.AddListener(() => counter += 1);

			Assert.AreEqual(counter, 0);
			genAction.Invoke();
			Assert.AreEqual(counter, 1);
			genAction.Event.Invoke();
			Assert.AreEqual(counter, 2);        
		}

        [Test]
		public void InvokeWhenInactive() {
			var entity = new GameObject();
            var genAction = entity.AddComponent<GenericAction>();
			// Verify default value; false
            Assert.AreEqual(genAction.InvokeWhenInactive, false);
			Assert.AreEqual(entity.activeInHierarchy, true);
         
            // in edit-mode the public Event attribute isn't auto-initialized
            genAction.Event = new UnityEvent();
         
            int counter = 0;
            genAction.Event.AddListener(() => counter += 1);
            Assert.AreEqual(counter, 0);

			// Entity is active (by default)
            genAction.Invoke();
            Assert.AreEqual(counter, 1);
            // Make inactive
			entity.SetActive(false);
			genAction.Invoke();
            Assert.AreEqual(counter, 1);
            // Direct invocation still works though...         
            genAction.Event.Invoke();
            Assert.AreEqual(counter, 2);
			// enable inactive invocation
			genAction.InvokeWhenInactive = true;
			genAction.Invoke();
            Assert.AreEqual(counter, 3);
			// disable inactive invocation again
			genAction.InvokeWhenInactive = false;
			genAction.Invoke();
            Assert.AreEqual(counter, 3);
            // re-enable entity         
			entity.SetActive(true);
			genAction.Invoke();
            Assert.AreEqual(counter, 4);
            // this is also considered inactive...
			genAction.enabled = false;
			genAction.Invoke();
            Assert.AreEqual(counter, 4);
		}
   }
}