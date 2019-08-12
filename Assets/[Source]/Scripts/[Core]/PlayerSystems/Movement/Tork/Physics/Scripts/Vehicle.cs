using UnityEngine;
using JetBrains.Annotations;

namespace Adrenak.Tork
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(CenterOfMassAssigner))]
	[RequireComponent(typeof(AntiRoll))]
	[RequireComponent(typeof(Aerodynamics))]
	[RequireComponent(typeof(Motor))]
	[RequireComponent(typeof(Steering))]
	[RequireComponent(typeof(Brakes))]
	public class Vehicle : MonoBehaviour 
	{
		public Vector3 Velocity => _rigidbody.velocity;

		[Tooltip("The maximum motor torque available based on the speed (KM/H)")]
		[SerializeField]
		private AnimationCurve motorTorqueForSpeed = AnimationCurve.Linear(0, 10000, 250, 0);

		[Tooltip("The steering angle based on the speed (KM/H)")]
		[SerializeField]
		private AnimationCurve maxSteeringAngleForSpeed = AnimationCurve.Linear(0, 35, 250, 5);

		[Tooltip("The down force based on the speed (KM/H)")]
		[SerializeField]
		private AnimationCurve downForceForSpeed = AnimationCurve.Linear(0, 0, 250, 2500);

		public Player Player { get; private set; }

		private Rigidbody _rigidbody;
		private Steering _steering;
		private Motor _motor;
		private Brakes _brake;
		private Aerodynamics _aerodynamics;

		private void Start() 
		{
			_rigidbody = GetComponent<Rigidbody>();
			_steering = GetComponent<Steering>();
			_motor = GetComponent<Motor>();
			_aerodynamics = GetComponent<Aerodynamics>();
			_brake = GetComponent<Brakes>();
		}

		public void SetPlayer(Player player)
		{
			Player = player;
		}

		private void Update()
		{
			if (Player == null) return;

			//TODO: Replace 3.6f
			float __speed = _rigidbody.velocity.magnitude * 3.6F;

			_steering.range = GetMaxSteerAtSpeed(__speed);
			_motor.maxTorque = GetMotorTorqueAtSpeed(__speed);
			_aerodynamics.downForce = GetDownForceAtSpeed(__speed);

			VehicleInput __input = Player.GetInput();

			_steering.value = __input.steering;
			_motor.value = __input.acceleration;
			_brake.value = __input.brake;
			_aerodynamics.midAirSteerInput = __input.steering;
		}

		[PublicAPI]
		public float GetMotorTorqueAtSpeed(float speed)
			=> motorTorqueForSpeed.Evaluate(speed);

		[PublicAPI]
		public float GetMaxSteerAtSpeed(float speed)
			=> maxSteeringAngleForSpeed.Evaluate(speed);

		[PublicAPI]
		public float GetDownForceAtSpeed(float speed) 
			=> downForceForSpeed.Evaluate(speed);
	}
}
