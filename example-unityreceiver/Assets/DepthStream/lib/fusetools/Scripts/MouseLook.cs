// taken from: https://answers.unity.com/questions/29741/mouse-look-script.html# and modified
using System;
using UnityEngine;

namespace FuseTools
{
	/// <summary>
    /// Lets you rotate the GameObjects using the mouse. Useful for example for
	/// controlling a camera.
    /// </summary>
	[AddComponentMenu("FuseTools/MouseLook")]
	public class MouseLook : MonoBehaviour
	{
		public KeyCode activateKey = KeyCode.None;
		public Boolean dragLook = false;
		public float xAxisMultiplier = 1.0f;
		public float yAxisMultiplier = 1.0f;

		public float mouseSensitivity = 100.0f;
		public float clampAngle = 80.0f;
		public bool Additive = false;

		private float rotY = 0.0f; // rotation around the up/y axis
		private float rotX = 0.0f; // rotation around the right/x axis
		private Vector2 lastMouseValues;

		void Start()
		{
			Vector3 rot = transform.localRotation.eulerAngles;
			rotY = rot.y;
			rotX = rot.x;
		}

		void Update()
		{
			if (this.activateKey != KeyCode.None && !Input.GetKey(this.activateKey))
				return;

			if (dragLook && !Input.GetMouseButton(0))
				return;

			float mouseX = Input.GetAxis("Mouse X") * xAxisMultiplier;
			float mouseY = Input.GetAxis("Mouse Y") * yAxisMultiplier;

			if (this.Additive) {
				Vector2 nextlast = new Vector2(mouseX, mouseY);
				mouseX -= lastMouseValues.x;
				mouseY -= lastMouseValues.y;
				this.lastMouseValues = nextlast;

				rotY += mouseX * mouseSensitivity * Time.deltaTime;
				rotX += mouseY * mouseSensitivity * Time.deltaTime;

				rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

				Vector3 eulers = transform.localRotation.eulerAngles;
				transform.localRotation = Quaternion.Euler(eulers.x+rotX, eulers.y+rotY, eulers.z);
			} else {
				rotY += mouseX * mouseSensitivity * Time.deltaTime;
				rotX += mouseY * mouseSensitivity * Time.deltaTime;

				rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

				transform.rotation = Quaternion.Euler(rotX, rotY, 0.0f);
			}
		}
	}
}