using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace FuseTools.Tests
{
	public class AsyncQueueTests
	{
		//[Test]   
		//void SomeTest() {}

		[UnityTest]
		public IEnumerator WaitUntilReady()
		{
			var queueObj = new GameObject();
			var queue = queueObj.AddComponent<FuseTools.AsyncQueue>();         
			queue.Wait = 0.5f; // half a second of delay between queued items
			Assert.True(queue.IsReady);

			int counter = 0;
			queue.WaitUntilReady().Then(() => counter += 1);
			Assert.False(queue.IsReady); // 0.5 sec timeout
			Assert.AreEqual(counter, 1);

			queue.WaitUntilReady().Then(() => counter += 1);
			Assert.AreEqual(counter, 1); // not yet
			Assert.False(queue.IsReady);
			yield return new WaitForSeconds(0.2f);
			Assert.AreEqual(counter, 1); // not yet
			Assert.False(queue.IsReady);
			yield return new WaitForSeconds(0.2f);         
			Assert.AreEqual(counter, 1); // not yet
			Assert.False(queue.IsReady);
			yield return new WaitForSeconds(0.1f);         
			Assert.AreEqual(counter, 2); // ran!
			Assert.False(queue.IsReady);
			yield return new WaitForSeconds(0.4f);
			Assert.False(queue.IsReady);
			yield return new WaitForSeconds(0.1f);
			Assert.True(queue.IsReady);

			// cleanup
			Object.Destroy(queueObj);
   		}

		[UnityTest]
		public IEnumerator WhenReady() {
			var queueObj = new GameObject();
            var queue = queueObj.AddComponent<FuseTools.AsyncQueue>();
            queue.Wait = 0.5f; // half a second of delay between queued items
            Assert.True(queue.IsReady);
         
			int counter = 0;
			var remotePromise = new RSG.Promise();
			queue.WhenReady(() => new RSG.Promise((res,rej) => {
				counter += 1;
				remotePromise.Then(res); 
			}));

			yield return new WaitForSeconds(0.5f);
			remotePromise.Resolve();
            Assert.False(queue.IsReady); // 0.5 sec timeout
			yield return new WaitForSeconds(0.2f);
            Assert.False(queue.IsReady);
            yield return new WaitForSeconds(0.2f);
            Assert.False(queue.IsReady);
            yield return new WaitForSeconds(0.15f);
            Assert.True(queue.IsReady);
         
			// cleanup
			Object.Destroy(queueObj);
		}

		[UnityTest]
        public IEnumerator BlockUntil()
        {
            var queueObj = new GameObject();
            var queue = queueObj.AddComponent<FuseTools.AsyncQueue>();
         
            queue.Wait = 0.5f; // half a second of delay between queued items         
            Assert.True(queue.IsReady);

			var promise = new RSG.Promise((resolve, reject) => {
			});

			queue.BlockUntil(promise);
			Assert.False(queue.IsReady);

			yield return new WaitForSeconds(0.6f);
			Assert.False(queue.IsReady);

			promise.Resolve();
			Assert.False(queue.IsReady);
			yield return new WaitForSeconds(0.4f);
			Assert.False(queue.IsReady);
			yield return new WaitForSeconds(0.11f);
			Assert.True(queue.IsReady);

            // cleanup
            Object.Destroy(queueObj);         
        }

		[UnityTest]
        public IEnumerator MixWaitUntilReadyAndBlockUntil()
        {
            var queueObj = new GameObject();
            var queue = queueObj.AddComponent<FuseTools.AsyncQueue>();

            queue.Wait = 0.5f; // half a second of delay between queued items         
            Assert.True(queue.IsReady);

            var promise = new RSG.Promise((resolve, reject) => {
            });

            queue.BlockUntil(promise);
            Assert.False(queue.IsReady);

			int counter = 0;
            queue.WaitUntilReady().Then(() => counter += 1);
         
            yield return new WaitForSeconds(0.6f);
            Assert.False(queue.IsReady);
			Assert.AreEqual(counter, 0);
            promise.Resolve();         
            Assert.False(queue.IsReady);
			Assert.AreEqual(counter, 0);
            yield return new WaitForSeconds(0.4f);
            Assert.False(queue.IsReady);
			Assert.AreEqual(counter, 0);
            yield return new WaitForSeconds(0.11f);
			Assert.False(queue.IsReady);
			Assert.AreEqual(counter, 1);
			yield return new WaitForSeconds(0.4f);
			Assert.False(queue.IsReady);
			yield return new WaitForSeconds(0.1f);
			Assert.True(queue.IsReady);

			queue.WaitUntilReady().Then(() => counter += 1);
			Assert.False(queue.IsReady);
            Assert.AreEqual(counter, 2);

			yield return new WaitForSeconds(0.2f);
            Assert.False(queue.IsReady);
         
            // Call block until while not ready yet
			promise = new RSG.Promise((resolve, reject) => {
				counter += 10;
            });

			queue.BlockUntil(promise);
			Assert.AreEqual(counter, 12);

			Assert.False(queue.IsReady);
			promise.Resolve();         

			// wait for timeout of last WaitUntilReady call to finish
			Assert.False(queue.IsReady); 
			yield return new WaitForSeconds(0.2f);
			Assert.False(queue.IsReady);
			yield return new WaitForSeconds(0.1f);
			Assert.False(queue.IsReady);
         
			// wait for timeout of last call to BlockUntil to finish
			yield return new WaitForSeconds(0.2f);
			Assert.False(queue.IsReady);
			yield return new WaitForSeconds(0.2f);
            Assert.False(queue.IsReady);
			yield return new WaitForSeconds(0.1f);
            Assert.True(queue.IsReady);
         
            // cleanup
            Object.Destroy(queueObj);
        }
	}
}