using System;
using UnityEngine;
using JetBrains.Annotations;
using CommonGames.Utilities;
using CommonGames.Utilities.Extensions;

using static UnityEditor.Handles;

namespace Adrenak.Tork
{
	/// <summary>
	/// An implementation of Ackermann steering mechanism
	/// </summary>
	public class Ackermann : Singleton<Ackermann>
	{
		private Wheel 
			_frontLeftWheel, 
			_frontRightWheel, 
			_rearLeftWheel, 
			_rearRightWheel;

		[PublicAPI]
		public float[,] Radii { get; private set; }

		[PublicAPI]
		public float Angle { get; private set; }

		[PublicAPI]
		public float AxleSeparation => (_frontLeftWheel.transform.position - _rearLeftWheel.transform.position).magnitude;

		[PublicAPI]
		public float AxleWidth => (_frontLeftWheel.transform.position - _frontRightWheel.transform.position).magnitude;

		[PublicAPI]
		public float FrontRightRadius => AxleSeparation / Mathf.Sin(Mathf.Abs(_frontRightWheel.steerAngle));

		[PublicAPI]
		public float FrontLeftRadius => AxleSeparation / Mathf.Sin(Mathf.Abs(_frontLeftWheel.steerAngle));

		private void Start()
		{
			_frontLeftWheel = Vehicle.Instance.frontLeftWheel;
			_frontRightWheel = Vehicle.Instance.frontRightWheel;
			_rearLeftWheel = Vehicle.Instance.rearLeftWheel;
			_rearRightWheel = Vehicle.Instance.rearRightWheel;
			
			Radii = new float[2, 2];
			
		}

		private void Update()
		{
			Radii = GetRadii(Angle, AxleSeparation, AxleWidth);
		}

		[PublicAPI]
		public void SetAngle(float angle)
		{
			Angle = angle;
			float __farAngle = GetSecondaryAngle(angle, AxleWidth, AxleSeparation);

			// The rear wheels are always at 0 steer in Ackermann
			_rearLeftWheel.steerAngle = _rearRightWheel.steerAngle = 0;

			if (Mathf.Approximately(Angle, 0)) 
				_frontRightWheel.steerAngle = _frontLeftWheel.steerAngle = 0;
			else if (Angle > 0)
			{
				_frontRightWheel.steerAngle = Angle;
				_frontLeftWheel.steerAngle = __farAngle;
			}
			else if (Angle < 0)
			{
				_frontLeftWheel.steerAngle = Angle;
				_frontRightWheel.steerAngle = -__farAngle;
			}
		}

		[PublicAPI]
		public Vector3 GetPivot()
		{
			if (Angle > 0)
			{
				Transform __frontRight = _frontRightWheel.transform;
				return __frontRight.position + Radii[0, 1] * __frontRight.right;
			}
			else
			{
				Transform __frontLeft = _frontLeftWheel.transform;
				return __frontLeft.position - Radii[0, 0] * __frontLeft.right;	
			}
		}

		[PublicAPI]
		public static float GetSecondaryAngle(float angle, float width, float separation)
		{
			float __close = separation / Mathf.Tan(Mathf.Abs(angle) * Mathf.Deg2Rad);
			float __far = __close + width;

			return Mathf.Atan(separation / __far) * Mathf.Rad2Deg;
		}

		[PublicAPI]
		public static float[,] GetRadii(float angle, float separation, float width)
		{
			float __secAngle = GetSecondaryAngle(angle, width, separation);
			float[,] __radii = new float[2, 2];

			if (Mathf.Abs(angle) < 1)
			{
				__radii[0, 0] = __radii[1, 0] = __radii[0, 1] = __radii[1, 1] = 1000;
			}

			if (angle < -1)
			{
				__radii[0, 0] = separation / Mathf.Sin(Mathf.Abs(angle * Mathf.Deg2Rad));
				__radii[0, 1] = separation / Mathf.Sin(Mathf.Abs(__secAngle * Mathf.Deg2Rad));
				__radii[1, 0] = separation / Mathf.Tan(Mathf.Abs(angle * Mathf.Deg2Rad));
				__radii[1, 1] = separation / Mathf.Tan(Mathf.Abs(__secAngle * Mathf.Deg2Rad));
			}
			else if (angle > 1)
			{
				__radii[0, 0] = separation / Mathf.Sin(Mathf.Abs(__secAngle * Mathf.Deg2Rad));
				__radii[0, 1] = separation / Mathf.Sin(Mathf.Abs(angle * Mathf.Deg2Rad));
				__radii[1, 0] = separation / Mathf.Tan(Mathf.Abs(__secAngle * Mathf.Deg2Rad));
				__radii[1, 1] = separation / Mathf.Tan(Mathf.Abs(angle * Mathf.Deg2Rad));
			}

			return __radii;
		}

		private void OnDrawGizmos()
		{
			#if UNITY_EDITOR
			
			color = Color.cyan;

			Transform __wheelTransform = default;
			float __angle = default;
			Vector3 __origin = default;

            if(_frontLeftWheel != null)
            {
				__wheelTransform = _frontLeftWheel.transform;
            	
            	__angle = __wheelTransform.localEulerAngles.y;
				__origin = __wheelTransform.position;
            	DrawLine(__origin, __origin + Quaternion.AngleAxis(__angle, Vector3.up) * transform.forward);
            }

			// ReSharper disable once InvertIf
			if (_frontRightWheel != null)
			{
				__wheelTransform = _frontRightWheel.transform;
            	
				__angle = __wheelTransform.localEulerAngles.y;
				__origin = __wheelTransform.position;
				
				DrawLine(__origin, __origin + Quaternion.AngleAxis(__angle, Vector3.up) * transform.forward);
			}

			#endif
		}

	}
}
