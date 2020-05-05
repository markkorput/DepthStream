using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FuseTools
{
	/// <summary>
	/// TODO
	/// </summary>
	[AddComponentMenu("FuseTools/SimpleSpawn")]
	public class SimpleSpawn : MonoBehaviour
	{
		public GameObject Template;
		public Transform Parent;
		public bool AtStart = true;
		public bool Activate = true;
		public bool OriginalName = false;

		[Tooltip("Will be invoked with the Spawned GameObject, after activating that object (assuming the template object is de-activated")]
		public GameObjectEvent SpawnEvent;
		[Tooltip("Is invoked with the template object as argument")]

		public GameObjectEvent BeforeSpawn;
		[Tooltip("Will be invoked with the Spawned GameObject, before activating that object (assuming the template object is de-activated")]
		public GameObjectEvent BeforeActivateEvent;

		private List<GameObject> SpawnedObjects = new List<GameObject>();

		public GameObject LastSpawnedObject { get { return SpawnedObjects.Count == 0 ? null : SpawnedObjects[SpawnedObjects.Count - 1]; }}

		private Transform ResolvedParent { get { return this.Parent == null ? this.transform : this.Parent; }}

		void Start()
		{
			if (AtStart) this.Spawn();
		}
	
		/// <summary>
		/// Helper method to keep things DRY. See the various public Spawn methods which use this method
		/// </summary>
		/// <param name="func">Func.</param>
		/// <param name="template">Template.</param>
		protected GameObject CustomInstantiate(System.Func<GameObject, GameObject> func, GameObject template = null){
			if (template == null) template = this.Template;
			this.BeforeSpawn.Invoke(template);
			var obj = func.Invoke(template);
			if (this.OriginalName) obj.name = template.name;
			this.BeforeActivateEvent.Invoke(obj);
			if (this.Activate) obj.SetActive(true);
			
			this.SpawnedObjects.Add(obj);         
			this.SpawnEvent.Invoke(obj);
			return obj;
		}

		#region Public Methods
		public void Spawn()
		{
			this.CustomInstantiate((template) =>
			   Instantiate(template, this.ResolvedParent));             
		}

		public void Spawn(Vector3 position) {
			this.CustomInstantiate((template) =>
               Instantiate(template, position, this.Template.transform.rotation, this.ResolvedParent));
		}

		public void SpawnCustomTemplate(GameObject template) {
			this.CustomInstantiate((t) =>
			   Instantiate(t, this.ResolvedParent), template);
		}

		public void SpawnCustomParent(Transform parent) {
			this.CustomInstantiate((template) =>
               Instantiate(template, parent));
		}

		public void SpawnAtPositions(Vector3[] positions) {
			foreach(var pos in positions) {
				this.Spawn(pos);
			}
		}
      
		public void DestroyAll()
		{
			foreach (var obj in SpawnedObjects) Destroy(obj);
			SpawnedObjects.Clear();
		}

		public void DestroyOldest()
		{
			if (this.SpawnedObjects.Count == 0) return;
			var obj = this.SpawnedObjects[0];
			Destroy(obj);
			this.SpawnedObjects.Remove(obj);
		}

		public void DestroyLast()
		{
			if (this.SpawnedObjects.Count == 0) return;
			var obj = this.SpawnedObjects[this.SpawnedObjects.Count - 1];
			Destroy(obj);
			this.SpawnedObjects.Remove(obj);
		}
      
		public void DestroySpawned(GameObject obj) {
			if(this.SpawnedObjects.Remove(obj)) {
				Destroy(obj);	
			} // else log warning?
		}
        #endregion
	}
}