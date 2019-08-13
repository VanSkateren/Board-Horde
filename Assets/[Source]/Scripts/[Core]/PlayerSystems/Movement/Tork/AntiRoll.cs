using System;
using System.Collections.Generic;
using UnityEngine;

namespace Adrenak.Tork {
	public class AntiRoll : MonoBehaviour
	{
		[Serializable]
		public class Axle
		{
			public Wheel left;
			public Wheel right;
			public float force = 36000f;

			public Axle(Wheel left, Wheel right, float force = 36000f)
			{
				this.left = left;
				this.right = right;
				this.force = force;
			}
		}

		public new Rigidbody rigidbody;
		public List<Axle> axles;

		private void Reset()
		{
			if(!Vehicle.InstanceExists) return;

			axles = new List<Axle>()
			{
				new Axle(left: Vehicle.Instance.frontLeftWheel, right: Vehicle.Instance.frontRightWheel),
				new Axle(left: Vehicle.Instance.rearLeftWheel, right: Vehicle.Instance.rearRightWheel)
			};
		}

		private void FixedUpdate()
		{
			foreach(Axle __axle in axles)
			{
				Vector3 __wsDown = transform.TransformDirection(Vector3.down);
				__wsDown.Normalize();

				float __travelL = Mathf.Clamp01(__axle.left.CompressionRatio);
				float __travelR = Mathf.Clamp01(__axle.right.CompressionRatio);
				float __antiRollForce = (__travelL - __travelR) * __axle.force;

				if (__axle.left.isGrounded)
				{
					rigidbody.AddForceAtPosition(__wsDown * -__antiRollForce, __axle.left.Hit.point);
				}

				if (__axle.right.isGrounded)
				{
					rigidbody.AddForceAtPosition(__wsDown * __antiRollForce, __axle.right.Hit.point);
				}
			}
		}
	}
}
