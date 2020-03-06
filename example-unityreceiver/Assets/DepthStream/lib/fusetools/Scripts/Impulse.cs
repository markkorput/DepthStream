using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuseTools
{
	public class Impulse : MonoBehaviour
	{
		public Rigidbody Rigidbody;
		public float Force = 1.0f;
      
		public Rigidbody ResolvedRigidbody
		{
			get
			{
				return this.Rigidbody != null
					   ? this.Rigidbody
						   : this.GetComponent<Rigidbody>();
			}
		}

		void FixedUpdate()
		{
			this.ResolvedRigidbody.AddForce(transform.forward * this.Force);
		}
	}
}