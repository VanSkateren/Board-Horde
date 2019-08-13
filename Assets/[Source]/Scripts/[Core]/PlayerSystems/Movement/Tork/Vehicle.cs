using UnityEngine;
using CommonGames.Utilities;
using JetBrains.Annotations;

namespace Adrenak.Tork
{
	[RequireComponent(typeof(Motor))]
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(CenterOfMass))]
	[RequireComponent(typeof(AntiRoll))]
	[RequireComponent(typeof(Aerodynamics))]
	[RequireComponent(typeof(Steering))]
	[RequireComponent(typeof(Brakes))]
	public class Vehicle :  Singleton<Vehicle>
	{
		#region Variables
		
		public Vector3 Velocity => Rigidbody.velocity;

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
		
		public Wheel 
			frontLeftWheel,
			frontRightWheel,
			rearLeftWheel,
			rearRightWheel;
		
		[PublicAPI]
		public Rigidbody Rigidbody { get; private set; }
		[PublicAPI]
		public Ackermann Ackermann { get; private set; }
		[PublicAPI]
		public Steering Steering { get; private set; }
		[PublicAPI]
		public Motor Motor { get; private set; }
		[PublicAPI]
		public Brakes Brake { get; private set; }
		[PublicAPI]
		public Aerodynamics Aerodynamics { get; private set; }
		
		#endregion

		#region Methods
		
		private void Start() 
		{
			Rigidbody = GetComponent<Rigidbody>();
			Steering = GetComponent<Steering>();
			Motor = GetComponent<Motor>();
			Aerodynamics = GetComponent<Aerodynamics>();
			Brake = GetComponent<Brakes>();
		}

		public void SetPlayer(Player player)
		{
			Player = player;
		}

		private void Update()
		{
			if (Player == null) return;

			//TODO: Replace 3.6f
			float __speed = Rigidbody.velocity.magnitude * 3.6F;

			Steering.range = GetMaxSteerAtSpeed(__speed);
			Motor.maxTorque = GetMotorTorqueAtSpeed(__speed);
			Aerodynamics.downForce = GetDownForceAtSpeed(__speed);

			VehicleInput __input = Player.GetInput();

			Steering.value = __input.steering;
			Motor.value = __input.acceleration;
			Brake.value = __input.brake;
			Aerodynamics.midAirSteerInput = __input.steering;
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
		
		#endregion
	}
}
