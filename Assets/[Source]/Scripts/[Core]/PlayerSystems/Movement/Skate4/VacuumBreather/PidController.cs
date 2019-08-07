using System;
using UnityEngine;

namespace VacuumBreather
{
	public class PidController
	{
		private const float MaxOutput = 1000000f;

		private float _integralMax;

		private float _integral;

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
				this._integral = Mathf.Clamp(this._integral, -this._integralMax, this._integralMax);
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

		public PidController(float kp, float ki, float kd)
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

		public float ComputeOutput(float error, float delta, float deltaTime)
		{
			this._integral = this._integral + error * deltaTime;
			this._integral = Mathf.Clamp(this._integral, -this._integralMax, this._integralMax);
			float single = delta / deltaTime;
			return Mathf.Clamp(this.Kp * error + this.Ki * this._integral + this.Kd * single, -1000000f, 1000000f);
		}
	}
}