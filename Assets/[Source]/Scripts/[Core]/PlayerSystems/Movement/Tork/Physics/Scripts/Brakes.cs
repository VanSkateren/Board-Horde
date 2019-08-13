using UnityEngine;

namespace Adrenak.Tork {
	[RequireComponent(typeof(Ackermann))]
	public class Brakes : MonoBehaviour {
		[Tooltip("The maximum braking torque that can be applied")]
		[SerializeField]
		private float maxTorque = 5000;

		[Tooltip("Multiplier to the maxTorque")]
		public float value;

		private Wheel 
			frontLeft, 
			frontRight, 
			rearLeft, 
			rearRight;

		private Vehicle skateboard = null;
		
		private void Start()
		{
			skateboard = Vehicle.Instance;
		}

		private void FixedUpdate()
		{
			float fr, fl, rr, rl;

			// If we have Ackerman steering, we apply torque based on the steering radius of each wheel
			if (skateboard.Ackermann != null)
			{
				float[,] radii = Ackermann.GetRadii(skateboard.Ackermann.Angle, skateboard.Ackermann.AxleSeparation, skateboard.Ackermann.AxleWidth);
				float total = radii[0, 0] + radii[1, 0] + radii[0, 1] + radii[1, 1];
				fl = radii[0, 0] / total;
				fr = radii[1, 0] / total;
				rl = radii[0, 1] / total;
				rr = radii[1, 1] / total;
			}
			else
				fr = fl = rr = rl = 1;

			skateboard.frontLeftWheel.brakeTorque = value * maxTorque * fl;
			skateboard.frontRightWheel.brakeTorque = value * maxTorque * fr;

			skateboard.rearLeftWheel.brakeTorque = value * maxTorque * rl;
			skateboard.rearRightWheel.brakeTorque = value * maxTorque * rr;
		}
	}
}