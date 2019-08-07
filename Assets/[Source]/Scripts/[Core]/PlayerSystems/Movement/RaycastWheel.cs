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
		private bool _grounded = false;

		public bool IsGrounded => _grounded;

		private void Awake()
		{
			_circumference = (radius * 2) * Mathf.PI;
			_parent = transform.root.GetComponent<Rigidbody>();
		}

		private void FixedUpdate()
		{
			GetGround();
		}

		private void GetGround()
		{
			_grounded = false;
			Vector3 __downwards = transform.TransformDirection(-Vector3.up);
			RaycastHit __hit;

			// down = local downwards direction
			Vector3 __down = transform.TransformDirection(Vector3.down);

			if (UnityEngine.Physics.Raycast(transform.position, __downwards, out __hit, radius + maxSuspension))
			{

				_grounded = true;
				// the velocity at point of contact
				Vector3 __velocityAtTouch = _parent.GetPointVelocity(__hit.point);

				// calculate spring compression
				// difference in positions divided by total suspension range
				float __compression = __hit.distance / (maxSuspension + radius);
				__compression = -__compression + 1;

				// final force
				Vector3 __force = __compression * spring * -__downwards;
				// velocity at point of contact transformed into local space

				Vector3 __t = transform.InverseTransformDirection(__velocityAtTouch);

				// local x and z directions = 0
				__t.z = __t.x = 0;

				// back to world space * -damping
				Vector3 __damping = transform.TransformDirection(__t) * -damper;
				Vector3 __finalForce = __force + __damping;

				// VERY simple turning - force rigidbody in direction of wheel
				__t = _parent.transform.InverseTransformDirection(__velocityAtTouch);
				__t.y = 0;
				__t.z = 0;

				__t = transform.TransformDirection(__t);

				//_parent.AddForceAtPosition(__finalForce + (__t), __hit.point);

				if (graphic) graphic.position = transform.position + (__down * (__hit.distance - radius));
			}
			else
			{
				if (graphic) graphic.position = transform.position + (__down * maxSuspension);
			}

			float __speed = _parent.velocity.magnitude;

			if (graphic)
			{
				//graphic.transform.localEulerAngles = new Vector3 (graphic.transform.localEulerAngles.x, wheelAngle, graphic.transform.localEulerAngles.z); 
				//graphic.transform.Rotate (360 * (speed / circumference) * Time.fixedDeltaTime, 0, 0); 
			}
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = new Color(0, 1, 0, 1);
			Vector3 __direction = transform.TransformDirection(-Vector3.up) * (this.radius);
			Gizmos.DrawRay(transform.position, __direction);

			Gizmos.color = new Color(0, 0, 1, 1);
			__direction = transform.TransformDirection(-Vector3.up) * (this.maxSuspension);
			Gizmos.DrawRay(new Vector3(transform.position.x, transform.position.y - radius, transform.position.z), __direction);
		}
	}
}
