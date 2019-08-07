using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonGames.Utilities;
using CommonGames.Utilities.Extensions;
using CommonGames.Utilities.CGTK;

using Sirenix.OdinInspector;
using Sirenix.Serialization;

using JetBrains.Annotations;

using EaseType = CommonGames.Utilities.CGTK.Greasy.EaseType;
using Greasy = CommonGames.Utilities.CGTK.Greasy.Greasy;
using static UnityEngine.Physics;

namespace Core.PlayerSystems.Movement
{
	[Serializable]
	public class RaycastWheel
	{

		#region Variables
		
		// Simple Vehicle Raycast wheel

		[BoxGroup] public Transform axle;
		[BoxGroup] public Transform graphic;
		[Space]
		[BoxGroup] public float mass = 1.0f;
		[BoxGroup] public float radius = 1.0f;
		[BoxGroup] public float maxSuspension = 0.2f;
		[BoxGroup] public float spring = 100.0f;
		[BoxGroup] public float damper = 0.0f;
		[BoxGroup] public float wheelAngle = 0f;

		private float _circumference;

		private float _contactPatchArea;
		private Rigidbody _rigidbody;

		[PublicAPI]
		public bool IsGrounded { get; private set; } = false;
		
		#endregion

		#region Methods
		
		private void Setup()
		{
			_circumference = (radius * 2) * Mathf.PI;
			_rigidbody = Skateboard.Instance.Rigidbody;
		}

		public void Update()
		{
			if(!Skateboard.InstanceExists) return;
			
			Setup();
			GetGround();
			DrawGizmos();
		}

		private void GetGround()
		{
			IsGrounded = false;
			Vector3 __downwards = graphic.TransformDirection(-Vector3.up);

			// down = local downwards direction
			Vector3 __down = graphic.TransformDirection(Vector3.down);

			if (Raycast(graphic.position, __downwards, out RaycastHit __hit, radius + maxSuspension))
			{

				IsGrounded = true;
				// the velocity at point of contact
				Vector3 __velocityAtTouch = _rigidbody.GetPointVelocity(__hit.point);

				// calculate spring compression
				// difference in positions divided by total suspension range
				float __compression = __hit.distance / (maxSuspension + radius);
				__compression = -__compression + 1;

				// final force
				Vector3 __force = __compression * spring * -__downwards;
				// velocity at point of contact transformed into local space

				Vector3 __t = graphic.InverseTransformDirection(__velocityAtTouch);

				// local x and z directions = 0
				__t.z = __t.x = 0;

				// back to world space * -damping
				Vector3 __damping = graphic.TransformDirection(__t) * -damper;
				Vector3 __finalForce = __force + __damping;

				// VERY simple turning - force rigidbody in direction of wheel
				__t = _rigidbody.transform.InverseTransformDirection(__velocityAtTouch);
				__t.y = 0;
				__t.z = 0;

				__t = graphic.TransformDirection(__t);

				//_rigidbody.AddForceAtPosition(__finalForce + (__t), __hit.point);

				if (graphic) graphic.position += (__down * (__hit.distance - radius));
			}
			else
			{
				if (graphic) graphic.position += (__down * maxSuspension);
			}

			float __speed = _rigidbody.velocity.magnitude;

			if (!graphic) return;

			Transform __transform = graphic.transform;
			Vector3 __localEulerAngles = __transform.localEulerAngles;
			
			__localEulerAngles = new Vector3 (__localEulerAngles.x, wheelAngle, __localEulerAngles.z);
			__transform.localEulerAngles = __localEulerAngles;
			
			graphic.transform.Rotate (360 * (__speed / _circumference) * Time.fixedDeltaTime, 0, 0);
		}

		private void DrawGizmos()
		{
			Vector3 __direction = graphic.TransformDirection(-Vector3.up) * (this.radius);
			Vector3 __position = graphic.position;
			
			CGDebug.DrawRay(__position, __direction).Color(new Color(0, 1, 0, 1));
			
			__direction = graphic.TransformDirection(-Vector3.up) * (this.maxSuspension);
			CGDebug.DrawRay(new Vector3(__position.x, __position.y - radius, __position.z), __direction).Color(new Color(0, 0, 1, 1));
		}
		
		#endregion
	}
}
