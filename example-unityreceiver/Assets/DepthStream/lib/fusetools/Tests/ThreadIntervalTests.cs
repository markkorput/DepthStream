using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace FuseTools.Tests
{
	public class ThreadIntervalTests
	{
		[UnityTest]
		public IEnumerator ThreadedInterval100msAccuracyTest() {
			var entity = new GameObject("ThreadInterval-AccuracyTest");
			var interval = entity.AddComponent<ThreadInterval>();
			int counter = 0;

			interval.StartInterval(0.2f, () => {
				counter += 1;
			});

			// calculate actual elapsed time, because
			// WaitForSeconds might not be very accurate
			var t = Time.time;
			Assert.AreEqual(counter, 0);
			yield return new WaitForSeconds(0.6f);
			var c = counter;
			t = Time.time - t;

			// Allow for 1.0f divergense to correct for time it took to start thread
			Assert.AreEqual(c, t*5.0f, 1.5f); 
			Object.Destroy(entity);
		}

		// [UnityTest]
		// public IEnumerator OnIntervalEventTest() {
		// 	var entity = new GameObject("ThreadInterval-OnIntervalTest");
		// 	var interval = entity.AddComponent<ThreadInterval>();
		// 	int counter = 0;

		// 	interval.OnInterval.AddListener(() => counter += 1);
		// 	Assert.AreEqual(counter, 0);
		// 	yield return new WaitForSeconds(0.11f);
		// 	Assert.AreEqual(counter, 1);
		// 	yield return new WaitForSeconds(0.1f);
		// 	Assert.AreEqual(counter, 2);
		// 	yield return new WaitForSeconds(0.2f);
		// 	Assert.AreEqual(counter, 4);
		// 	yield return new WaitForSeconds(0.3f);
		// 	Assert.AreEqual(counter, 7);

		// 	Object.Destroy(entity);
		// }
	}
}