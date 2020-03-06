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
	[AddComponentMenu("FuseTools/DestroySelf")]
	public class DestroySelf : MonoBehaviour
	{
		public enum Moment { Start, OnEnable, OnDisable, None };
		public Moment When = Moment.None;

		void Start()
		{
			if (this.When.Equals(Moment.Start)) Destroy(this.gameObject);
		}

		void OnEnable()
		{
			if (this.When.Equals(Moment.OnEnable)) Destroy(this.gameObject);
		}
      
		void OnDisable()
		{
			if (this.When.Equals(Moment.OnDisable)) Destroy(this.gameObject);
		}
      
		public void DestroySelfNow() {
			Destroy(this.gameObject);
		}
	}
}