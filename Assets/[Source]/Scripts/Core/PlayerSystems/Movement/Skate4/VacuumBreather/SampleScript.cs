using System;
using UnityEngine;

namespace VacuumBreather
{
	public class SampleScript : MonoBehaviour
	{
		public Transform TargetOne;

		public Transform TargetTwo;

		public Transform TargetThree;

		public VacuumBreather.ControlledObject ControlledObject;

		private Vector3 _cameraPosition;

		public SampleScript()
		{
		}

		private void Awake()
		{
			this.ControlledObject.GetComponent<Rigidbody>().centerOfMass = Vector3.zero;
			this._cameraPosition = Camera.main.transform.position;
		}

		private void OnGUI()
		{
			GUI.BeginGroup(new Rect(10f, 10f, 175f, 450f));
			GUI.Box(new Rect(10f, 10f, 150f, 400f), "Choose Action");
			if (GUI.Button(new Rect(20f, 40f, 125f, 20f), "Reset") && this.ControlledObject != null)
			{
				this.ControlledObject.transform.position = Vector3.zero;
				this.ControlledObject.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
				this.ControlledObject.DesiredOrientation = this.ControlledObject.transform.rotation;
				this.ControlledObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
				Camera.main.transform.position = this._cameraPosition;
			}
			if (GUI.Button(new Rect(20f, 70f, 125f, 20f), "Match Target 1") && this.ControlledObject != null)
			{
				this.ControlledObject.DesiredOrientation = this.TargetOne.rotation;
			}
			if (GUI.Button(new Rect(20f, 100f, 125f, 20f), "Match Target 2") && this.ControlledObject != null)
			{
				this.ControlledObject.DesiredOrientation = this.TargetTwo.rotation;
			}
			if (GUI.Button(new Rect(20f, 130f, 125f, 20f), "Match Target 3") && this.ControlledObject != null)
			{
				this.ControlledObject.DesiredOrientation = this.TargetThree.rotation;
			}
			if (GUI.Button(new Rect(20f, 160f, 125f, 20f), "Look at Target 1") && this.ControlledObject != null)
			{
				this.ControlledObject.DesiredOrientation = Quaternion.LookRotation(this.TargetOne.position, Vector3.up);
			}
			if (GUI.Button(new Rect(20f, 190f, 125f, 20f), "Look at Target 2") && this.ControlledObject != null)
			{
				this.ControlledObject.DesiredOrientation = Quaternion.LookRotation(this.TargetTwo.position, Vector3.up);
			}
			if (GUI.Button(new Rect(20f, 220f, 125f, 20f), "Look at Target 3") && this.ControlledObject != null)
			{
				this.ControlledObject.DesiredOrientation = Quaternion.LookRotation(this.TargetThree.position, Vector3.up);
			}
			GUIStyle style = GUI.skin.GetStyle("Label");
			style.alignment = TextAnchor.UpperCenter;
			GUI.Label(new Rect(20f, 250f, 125f, 60f), "Use scrollwheel to zoom camera.", style);
			GUI.EndGroup();
		}

		private void Update()
		{
			Vector3 vector3 = Input.mouseScrollDelta.y * Vector3.forward;
			Vector3 vector31 = Camera.main.transform.TransformDirection(vector3);
			Transform transforms = Camera.main.transform;
			transforms.position = transforms.position + vector31;
		}
	}
}