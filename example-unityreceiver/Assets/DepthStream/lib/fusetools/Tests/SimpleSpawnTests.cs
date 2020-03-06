using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace FuseTools.Tests
{
	public class SimpleSpawnTests
	{
		public class ValueComp : MonoBehaviour {
			public int Value = 0;
		}

		[UnityTest]
		public IEnumerator EventsTest() {
			
			var entity = new GameObject("SimpleSpawnEvensTest");
			entity.SetActive(false);
			var simpleSpawn = entity.AddComponent<SimpleSpawn>();
			var template = new GameObject("Template", typeof(ValueComp));
			template.transform.SetParent(entity.transform);
			template.SetActive(false);
			simpleSpawn.AtStart = false;
			simpleSpawn.Template = template;
			entity.SetActive(true);

			if (simpleSpawn.BeforeSpawn == null) {
				simpleSpawn.BeforeSpawn = new GameObjectEvent();
				simpleSpawn.BeforeActivateEvent = new GameObjectEvent();
				simpleSpawn.SpawnEvent = new GameObjectEvent();
			}

			simpleSpawn.BeforeSpawn.AddListener((gameObject) => {
				Assert.AreEqual(gameObject.GetComponent<ValueComp>().Value, 0);
				Assert.AreEqual(template.GetComponent<ValueComp>().Value, 0);
				gameObject.GetComponent<ValueComp>().Value = 1;
			});

			simpleSpawn.BeforeActivateEvent.AddListener((gameObject) => {
				Assert.AreEqual(gameObject.GetComponent<ValueComp>().Value, 1);
				Assert.AreEqual(template.GetComponent<ValueComp>().Value, 1);
				gameObject.GetComponent<ValueComp>().Value = 2;
			});

			simpleSpawn.SpawnEvent.AddListener((gameObject) => {
				Assert.AreEqual(gameObject.GetComponent<ValueComp>().Value, 2);
				Assert.AreEqual(template.GetComponent<ValueComp>().Value, 1);
				gameObject.GetComponent<ValueComp>().Value = 3;
			});

			Assert.AreEqual(template.GetComponent<ValueComp>().Value, 0);
			Assert.IsNull(simpleSpawn.LastSpawnedObject);
			simpleSpawn.Spawn();
			
			Assert.AreEqual(template.GetComponent<ValueComp>().Value, 1);
			Assert.IsNotNull(simpleSpawn.LastSpawnedObject);
			Assert.AreEqual(simpleSpawn.LastSpawnedObject.GetComponent<ValueComp>().Value, 3);

			yield return null;
			Object.Destroy(entity);
		}
	}
}