using System.Linq;
using System.Collections.Generic;
using CommonGames.Utilities.Extensions;
using UnityEngine;

namespace Adrenak.Tork
{
	public class Aerodynamics : MonoBehaviour
	{
		[SerializeField] private new Rigidbody rigidbody;
		[SerializeField] private Wheel[] wheels;

		public float downForce = 1000;
		public float stabilizationTorque = 15000;
		public float midAirSteerTorque = 1500;
		public float midAirSteerInput;

		public bool applyDownForce;
		public bool stabilize;
		public bool steerMidAir;

		private void FixedUpdate()
		{
			if (applyDownForce)
			{
				DoApplyDownForce();
			}

			if (stabilize)
			{
				Stabilize();
			}

			if (steerMidAir)
			{
				SteerMidAir();
			}
		}

		private void DoApplyDownForce()
		{
			if (!downForce.Approximately(0))
			{
				rigidbody.AddForce(-Vector3.up * downForce);
			}
		}

		private void Stabilize()
		{
			//TODO: Replace LINQ
			IEnumerable<Wheel> __inAir = wheels.Where(x => !x.isGrounded);
			
			if (__inAir.Count() != 4) return;

			// Try to keep vehicle parallel to the ground while jumping
			Vector3 __locUp = transform.up;
			Vector3 __wsUp = new Vector3(0.0f, 1.0f, 0.0f);
			Vector3 __axis = Vector3.Cross(__locUp, __wsUp);
			float __force = stabilizationTorque;

			rigidbody.AddTorque(__axis * __force);
		}

		private void SteerMidAir()
		{
			if (midAirSteerInput.Approximately(0)) return;
			
			rigidbody.AddTorque(new Vector3(0, midAirSteerInput * midAirSteerTorque, 0));
		}
	}
}
