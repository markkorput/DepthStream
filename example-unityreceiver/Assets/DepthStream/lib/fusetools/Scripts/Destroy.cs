using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuseTools
{
	/// <summary>
	/// Destroys its GameObject at a specified moment in the GameObject's lifecycle.
	/// This is a hacky solution that allows you to have objects in your scene
	/// for development purposes (for reference), but which should not be present
	/// during execution of the application.
	/// </summary>
	[AddComponentMenu("FuseTools/Destroy")]
	public class Destroy : MonoBehaviour
	{
		public GameObject Object;
		public enum Moment { Start, OnEnable, OnDisable, None };
		public Moment When = Moment.None;

		public GameObject Obj { get { return this.Object == null ? this.gameObject : this.Object; } }

		void Start()
		{
			if (this.When.Equals(Moment.Start)) this.DestroyNow();
		}

		void OnEnable()
		{
			if (this.When.Equals(Moment.OnEnable)) this.DestroyNow();
		}
      
		void OnDisable()
		{
			if (this.When.Equals(Moment.OnDisable)) this.DestroyNow();
		}

		public void DestroyNow() {
			Destroy(this.Obj);
		}

		public void DestroyNow(GameObject go) {
			Destroy(go);
		}
	}
}