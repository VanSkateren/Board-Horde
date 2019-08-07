using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace VacuumBreather
{
	public class ControlledObject : MonoBehaviour
	{
		private readonly PidQuaternionController _pidController = new PidQuaternionController(8f, 0f, 0.05f);

		private Transform _currentTransform;

		private Rigidbody _objectRigidbody;

		public float Kp;

		public float Ki;

		public float Kd;

		public Quaternion DesiredOrientation
		{
			get;
			set;
		}

		public ControlledObject()
		{
		}

		private void Awake()
		{
			this._currentTransform = base.transform;
			this._objectRigidbody = base.GetComponent<Rigidbody>();
		}

		private void FixedUpdate()
		{
			Quaternion desiredOrientation = this.DesiredOrientation;
			if (this._currentTransform == null || this._objectRigidbody == null)
			{
				return;
			}
			this._pidController.Kp = this.Kp;
			this._pidController.Ki = this.Ki;
			this._pidController.Kd = this.Kd;
			Vector3 vector3 = this._pidController.ComputeRequiredAngularAcceleration(this._currentTransform.rotation, this.DesiredOrientation, this._objectRigidbody.angularVelocity, Time.fixedDeltaTime * 0.25f);
			this._objectRigidbody.AddTorque(vector3, ForceMode.Acceleration);
		}
	}
}