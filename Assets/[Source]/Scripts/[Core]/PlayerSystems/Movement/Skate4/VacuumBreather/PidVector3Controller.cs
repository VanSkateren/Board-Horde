using System;
using UnityEngine;

namespace VacuumBreather
{
	public class PidVector3Controller
	{
		private const float MaxOutput = 1000000f;

		private float _integralMax;

		private Vector3 _integral;

		private float _kp;

		private float _ki;

		private float _kd;

		public float Kd
		{
			get
			{
				return this._kd;
			}
			set
			{
				if (value < 0f)
				{
					throw new ArgumentOutOfRangeException("value", "Kd must be a non-negative number.");
				}
				this._kd = value;
			}
		}

		public float Ki
		{
			get
			{
				return this._ki;
			}
			set
			{
				if (value < 0f)
				{
					throw new ArgumentOutOfRangeException("value", "Ki must be a non-negative number.");
				}
				this._ki = value;
				this._integralMax = 1000000f / this.Ki;
				this._integral = new Vector3(Mathf.Clamp(this._integral.x, -this._integralMax, this._integralMax), Mathf.Clamp(this._integral.y, -this._integralMax, this._integralMax), Mathf.Clamp(this._integral.z, -this._integralMax, this._integralMax));
			}
		}

		public float Kp
		{
			get
			{
				return this._kp;
			}
			set
			{
				if (value < 0f)
				{
					throw new ArgumentOutOfRangeException("value", "Kp must be a non-negative number.");
				}
				this._kp = value;
			}
		}

		public PidVector3Controller(float kp, float ki, float kd)
		{
			if (kp < 0f)
			{
				throw new ArgumentOutOfRangeException("kp", "kp must be a non-negative number.");
			}
			if (ki < 0f)
			{
				throw new ArgumentOutOfRangeException("ki", "ki must be a non-negative number.");
			}
			if (kd < 0f)
			{
				throw new ArgumentOutOfRangeException("kd", "kd must be a non-negative number.");
			}
			this.Kp = kp;
			this.Ki = ki;
			this.Kd = kd;
			this._integralMax = 1000000f / this.Ki;
		}

		public Vector3 ComputeOutput(Vector3 error, Vector3 delta, float deltaTime)
		{
			this._integral += new Vector3(error.x * deltaTime, error.y * deltaTime, error.z * deltaTime);
			this._integral = new Vector3(Mathf.Clamp(this._integral.x, -this._integralMax, this._integralMax), Mathf.Clamp(this._integral.y, -this._integralMax, this._integralMax), Mathf.Clamp(this._integral.z, -this._integralMax, this._integralMax));
			Vector3 vector3 = new Vector3(delta.x / deltaTime, delta.y / deltaTime, delta.z / deltaTime);
			Vector3 kp = ((this.Kp * error) + (this.Ki * this._integral)) + (this.Kd * vector3);
			kp = new Vector3(Mathf.Clamp(kp.x, -1000000f, 1000000f), Mathf.Clamp(kp.y, -1000000f, 1000000f), Mathf.Clamp(kp.z, -1000000f, 1000000f));
			return kp;
		}
	}
}