using UnityEngine;
using JetBrains.Annotations;

using static UnityEditor.Handles;

namespace Adrenak.Tork
{
	/// <summary>
	/// An implementation of Ackermann steering mechanism
	/// </summary>
	public class Ackermann : MonoBehaviour
	{
		[SerializeField] private Wheel frontRightWheel;
		[SerializeField] private Wheel frontLeftWheel;
		[SerializeField] private Wheel rearRightWheel;
		[SerializeField] private Wheel rearLeftWheel;
		
		[PublicAPI]
		public float[,] Radii { get; private set; }

		[PublicAPI]
		public float Angle { get; private set; }

		public float AxleSeparation => (frontLeftWheel.transform.position - rearLeftWheel.transform.position).magnitude;

		public float AxleWidth => (frontLeftWheel.transform.position - frontRightWheel.transform.position).magnitude;

		public float FrontRightRadius => AxleSeparation / Mathf.Sin(Mathf.Abs(frontRightWheel.steerAngle));

		public float FrontLeftRadius => AxleSeparation / Mathf.Sin(Mathf.Abs(frontLeftWheel.steerAngle));

		private void Start()
		{
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
			rearLeftWheel.steerAngle = rearRightWheel.steerAngle = 0;

			if (Mathf.Approximately(Angle, 0)) 
				frontRightWheel.steerAngle = frontLeftWheel.steerAngle = 0;
			else if (Angle > 0)
			{
				frontRightWheel.steerAngle = Angle;
				frontLeftWheel.steerAngle = __farAngle;
			}
			else if (Angle < 0)
			{
				frontLeftWheel.steerAngle = Angle;
				frontRightWheel.steerAngle = -__farAngle;
			}
		}

		[PublicAPI]
		public Vector3 GetPivot()
		{
			if (Angle > 0)
			{
				Transform __frontRight = frontRightWheel.transform;
				return __frontRight.position + Radii[0, 1] * __frontRight.right;
			}
			else
			{
				Transform __frontLeft = frontLeftWheel.transform;
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

            if(frontLeftWheel != null)
            {
				__wheelTransform = frontLeftWheel.transform;
            	
            	__angle = __wheelTransform.localEulerAngles.y;
				__origin = __wheelTransform.position;
            	DrawLine(__origin, __origin + Quaternion.AngleAxis(__angle, Vector3.up) * transform.forward);
            }

			if (frontRightWheel == null)
			{
				__wheelTransform = frontRightWheel.transform;
            	
				__angle = __wheelTransform.localEulerAngles.y;
				__origin = __wheelTransform.position;
				DrawLine(__origin, __origin + Quaternion.AngleAxis(__angle, Vector3.up) * transform.forward);
			}

			#endif
		}

	}
}
