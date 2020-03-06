using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace FuseTools.Tests
{
	public class DistanceTests
	{
		[Test]
		public void InvokesDistanceAndDeltaOnRequest()
		{
			GameObject origin = new GameObject();
			origin.transform.position = new Vector3(0, -1, 0);
			GameObject target = new GameObject();
			target.transform.position = new Vector3(0, 2, 0);
         
			var entity = new GameObject();
			Distance distance = entity.AddComponent<Distance>();
			distance.Origin = origin.transform;
			distance.Target = target.transform;

			// public events are not automatically initialized in EditMode
			distance.Events = new Distance.Evts();
			distance.Events.Delta = new Vector3Event();
			distance.Events.Distance = new FloatEvent();
         
			Vector3 deltaResult = new Vector3();
			distance.Events.Delta.AddListener((vec) => deltaResult.Set(vec.x, vec.y, vec.z));
			float distanceResult = 0.0f;
            distance.Events.Distance.AddListener((dist) => distanceResult += dist);

			Assert.AreEqual(deltaResult, new Vector3(0, 0, 0));
			Assert.AreEqual(distanceResult, 0.0f);

			distance.InvokeDelta();
			Assert.AreEqual(deltaResult, new Vector3(0, 3, 0));
			Assert.AreEqual(distanceResult, 0.0f);
         
			distance.InvokeDistance();
			Assert.AreEqual(deltaResult, new Vector3(0, 3, 0));
			Assert.AreEqual(distanceResult, 3.0f);
		}
   }
}