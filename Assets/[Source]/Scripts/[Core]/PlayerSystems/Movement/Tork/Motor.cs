// TODO: Add support for All WD, Rear WD, Front WD. Right now it is only All WD
using UnityEngine;
using CommonGames.Utilities;
using JetBrains.Annotations;

namespace Adrenak.Tork 
{
	[RequireComponent(typeof(Ackermann))]
	public class Motor : Singleton<Motor>
	{
		[Tooltip("The maximum torque that the motor generates")]
		public float maxTorque = 20000;

		[Tooltip("Multiplier to the maxTorque")]
		public float value;

		public float maxReverseInput = -.5f;

		private Vehicle skateboard = null;
		
		private void Start()
		{
			skateboard = Vehicle.Instance;
		}

		private void Update()
		{
			ApplyMotorTorque();
		}

		private void ApplyMotorTorque() 
		{
			value = Mathf.Clamp(value, maxReverseInput, 1);

			// If we have Ackermann steering, we apply torque based on the steering radius of each wheel
			float[,] __radii = Ackermann.GetRadii(skateboard.Ackermann.Angle, skateboard.Ackermann.AxleSeparation, skateboard.Ackermann.AxleWidth);
			float __total = __radii[0, 0] + __radii[1, 0] + __radii[0, 1] + __radii[1, 1];
			float __fl = __radii[0, 0] / __total;
			float __fr = __radii[1, 0] / __total;
			float __rl = __radii[0, 1] / __total;
			float __rr = __radii[1, 1] / __total;
			
			Vehicle.Instance.frontLeftWheel.motorTorque = value * maxTorque * __fl;
			Vehicle.Instance.frontRightWheel.motorTorque = value * maxTorque * __fr;

			Vehicle.Instance.rearLeftWheel.motorTorque = value * maxTorque * __rl;
			Vehicle.Instance.rearRightWheel.motorTorque = value * maxTorque * __rr;
		}
	}
}
