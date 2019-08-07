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

		/*
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
				// velocity at point of contact axleed into local space

				Vector3 __t = graphic.InverseTransformDirection(__velocityAtTouch);

				// local x and z directions = 0
				__t.z = __t.x = 0;

				// back to world space * -damping
				Vector3 __damping = graphic.TransformDirection(__t) * -damper;
				Vector3 __finalForce = __force + __damping;

				// VERY simple turning - force rigidbody in direction of wheel
				__t = _rigidbody.axle.InverseTransformDirection(__velocityAtTouch);
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

			Transform __axle = graphic.axle;
			Vector3 __localEulerAngles = __axle.localEulerAngles;
			
			__localEulerAngles = new Vector3 (__localEulerAngles.x, wheelAngle, __localEulerAngles.z);
			__axle.localEulerAngles = __localEulerAngles;
			
			graphic.axle.Rotate (360 * (__speed / _circumference) * Time.fixedDeltaTime, 0, 0);
		}
		*/
		
		private void GetGround()
		{
			IsGrounded = false;
			Vector3 __downwards = axle.TransformDirection(-Vector3.up);

			// down = local downwards direction
			Vector3 __down = axle.TransformDirection(Vector3.down);

			if (Raycast(axle.position, __downwards, out RaycastHit __hit, radius + maxSuspension))
			{

				IsGrounded = true;
				// the velocity at point of contact
				Vector3 __velocityAtTouch = Skateboard.Instance.Rigidbody.GetPointVelocity(__hit.point);

				// calculate spring compression
				// difference in positions divided by total suspension range
				float __compression = __hit.distance / (maxSuspension + radius);
				__compression = -__compression + 1;

				// final force
				Vector3 __force = __compression * spring * -__downwards;
				// velocity at point of contact axleed into local space

				Vector3 __t = axle.InverseTransformDirection(__velocityAtTouch);

				// local x and z directions = 0
				__t.z = __t.x = 0;

				// back to world space * -damping
				Vector3 __damping = axle.TransformDirection(__t) * -damper;
				Vector3 __finalForce = __force + __damping;

				// VERY simple turning - force rigidbody in direction of wheel
				__t = Skateboard.Instance.Transform.InverseTransformDirection(__velocityAtTouch);
				__t.y = 0;
				__t.z = 0;

				__t = axle.TransformDirection(__t);

				//_parent.AddForceAtPosition(__finalForce + (__t), __hit.point);

				if (graphic) graphic.position = axle.position + (__down * (__hit.distance - radius));
			}
			else if (graphic)
			{
				graphic.position = axle.position + (__down * maxSuspension);
			}

			float __speed =  Skateboard.Instance.Rigidbody.velocity.magnitude;

			if (graphic)
			{
				//graphic.axle.localEulerAngles = new Vector3 (graphic.axle.localEulerAngles.x, wheelAngle, graphic.axle.localEulerAngles.z); 
				//graphic.axle.Rotate (360 * (speed / circumference) * Time.fixedDeltaTime, 0, 0); 
			}
		}

		private void DrawGizmos()
		{
			Vector3 __direction = axle.TransformDirection(-Vector3.up) * (this.radius);
			Vector3 __position = graphic.position;
			
			CGDebug.DrawRay(__position, __direction).Color(new Color(0, 1, 0, 1));
			
			__direction = graphic.TransformDirection(-Vector3.up) * (this.maxSuspension);
			CGDebug.DrawRay(new Vector3(__position.x, __position.y - radius, __position.z), __direction).Color(new Color(0, 0, 1, 1));
		}
		
		private void OnDrawGizmosSelected()
		{
			Gizmos.color = new Color(0, 1, 0, 1);
			Vector3 __direction = axle.TransformDirection(-Vector3.up) * (this.radius);
			Gizmos.DrawRay(axle.position, __direction);

			Gizmos.color = new Color(0, 0, 1, 1);
			__direction = axle.TransformDirection(-Vector3.up) * (this.maxSuspension);
			Gizmos.DrawRay(new Vector3(axle.position.x, axle.position.y - radius, axle.position.z), __direction);
		}
		
		#endregion
	}
}

/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonGames.Utilities;
using CommonGames.Utilities.Extensions;
using CommonGames.Utilities.CGTK;

namespace Core.PlayerSystems.Movement
{
	public class RaycastWheel : MonoBehaviour
	{

		// Simple Vehicle Raycast wheel

		public Transform graphic;

		public float mass = 1.0f;
		public float radius = 1.0f;
		public float maxSuspension = 0.2f;
		public float spring = 100.0f;
		public float damper = 0.0f;

		public float wheelAngle = 0f;

		private float _circumference;

		private float _contactPatchArea;
		private Rigidbody _parent;
		private bool IsGrounded = false;

		public bool IsGrounded => IsGrounded;

		private void Awake()
		{
			_circumference = (radius * 2) * Mathf.PI;
			_parent = axle.root.GetComponent<Rigidbody>();
		}

		private void FixedUpdate()
		{
			GetGround();
		}

		private void GetGround()
		{
			IsGrounded = false;
			Vector3 __downwards = axle.TransformDirection(-Vector3.up);
			RaycastHit __hit;

			// down = local downwards direction
			Vector3 __down = axle.TransformDirection(Vector3.down);

			if (UnityEngine.Physics.Raycast(axle.position, __downwards, out __hit, radius + maxSuspension))
			{

				IsGrounded = true;
				// the velocity at point of contact
				Vector3 __velocityAtTouch = _parent.GetPointVelocity(__hit.point);

				// calculate spring compression
				// difference in positions divided by total suspension range
				float __compression = __hit.distance / (maxSuspension + radius);
				__compression = -__compression + 1;

				// final force
				Vector3 __force = __compression * spring * -__downwards;
				// velocity at point of contact axleed into local space

				Vector3 __t = axle.InverseTransformDirection(__velocityAtTouch);

				// local x and z directions = 0
				__t.z = __t.x = 0;

				// back to world space * -damping
				Vector3 __damping = axle.TransformDirection(__t) * -damper;
				Vector3 __finalForce = __force + __damping;

				// VERY simple turning - force rigidbody in direction of wheel
				__t = _parent.axle.InverseTransformDirection(__velocityAtTouch);
				__t.y = 0;
				__t.z = 0;

				__t = axle.TransformDirection(__t);

				//_parent.AddForceAtPosition(__finalForce + (__t), __hit.point);

				if (graphic) graphic.position = axle.position + (__down * (__hit.distance - radius));
			}
			else
			{
				if (graphic) graphic.position = axle.position + (__down * maxSuspension);
			}

			float __speed = _parent.velocity.magnitude;

			if (graphic)
			{
				//graphic.axle.localEulerAngles = new Vector3 (graphic.axle.localEulerAngles.x, wheelAngle, graphic.axle.localEulerAngles.z); 
				//graphic.axle.Rotate (360 * (speed / circumference) * Time.fixedDeltaTime, 0, 0); 
			}
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = new Color(0, 1, 0, 1);
			Vector3 __direction = axle.TransformDirection(-Vector3.up) * (this.radius);
			Gizmos.DrawRay(axle.position, __direction);

			Gizmos.color = new Color(0, 0, 1, 1);
			__direction = axle.TransformDirection(-Vector3.up) * (this.maxSuspension);
			Gizmos.DrawRay(new Vector3(axle.position.x, axle.position.y - radius, axle.position.z), __direction);
		}
	}
}
*/