using System;
using UnityEngine;

public class Trajectory : MonoBehaviour
{
	public LayerMask layers;

	private RaycastHit _hit;

	private Vector3 _predictedGroundNormal = Vector3.up;

	private Vector3[] _hitNormals = new Vector3[49];

	private Vector3[] _hitPoints = new Vector3[49];

	private Vector2[] _offsets = new Vector2[49];

	private int[] _occurances = new int[49];

	private float[] _tempTimes = new float[49];

	private float increment = 0.143f;

	private Vector3 _lastHitPoint = Vector3.zero;

	public Vector3 PredictedGroundNormal
	{
		get
		{
			return this._predictedGroundNormal;
		}
		set
		{
			this._predictedGroundNormal = value;
		}
	}

	public Trajectory()
	{
	}

	private void Awake()
	{
		float single = -0.5f;
		float single1 = -0.5f;
		int num = 0;
		for (int i = 0; i < (int)this._offsets.Length; i++)
		{
			this._offsets[i].x = single;
			single += this.increment;
			this._offsets[i].y = single1;
			num++;
			if (num == 7)
			{
				single1 += this.increment;
				single = -0.5f;
				num = 0;
			}
			Vector2 vector2 = new Vector2(this._offsets[i].x, this._offsets[i].y);
			vector2 = Vector2.ClampMagnitude(vector2, 0.5f);
			this._offsets[i].x = vector2.x;
			this._offsets[i].y = vector2.y;
		}
	}

	public float CalculateTrajectory(Vector3 p_pos, Vector3 p_vel, float p_maxTime, out Vector3 p_correctiveForce)
	{
		Vector3 vector3 = Vector3.zero;
		this.CalculateTrajectorySpread(p_pos, p_vel, p_maxTime, out vector3);
		p_correctiveForce = vector3;
		return this.PlotTrajectory(p_pos, p_vel, Time.deltaTime, p_maxTime);
	}

	public float CalculateTrajectory(Vector3 p_pos, Rigidbody p_rb, float p_maxTime, out RaycastHit p_hit)
	{
		return this.PlotTrajectory(p_pos, p_rb.velocity, Time.deltaTime, p_maxTime, out p_hit);
	}

	public float CalculateTrajectorySpread(Vector3 p_pos, Vector3 p_vel, float p_maxTime, out Vector3 p_correctiveForce)
	{
		Vector3 vector3 = Vector3.ProjectOnPlane(PlayerController.Instance.skaterController.skaterTransform.forward, Vector3.up);
		Vector3 vector31 = vector3.normalized;
		Vector3 vector32 = Vector3.Cross(vector31, -Vector3.up);
		for (int i = 0; i < (int)this._hitNormals.Length; i++)
		{
			this._hitNormals[i] = this.PlotSingleTrajectory((p_pos + (vector32 * this._offsets[i].x)) + (vector31 * this._offsets[i].y), p_vel, Time.deltaTime * 10f, p_maxTime, out this._tempTimes[i], out this._hitPoints[i]);
		}
		Vector3 vector33 = Vector3.up;
		for (int j = 0; j < (int)this._hitNormals.Length; j++)
		{
			if (Vector3.Angle(PlayerController.Instance.boardController.LastGroundNormal, this._hitNormals[j]) < 60f || Vector3.Angle(Vector3.up, this._hitNormals[j]) < 45f)
			{
				vector33 += this._hitNormals[j];
			}
		}
		Vector3 length = this._hitPoints[0];
		for (int k = 1; k < (int)this._hitPoints.Length; k++)
		{
			length += this._hitPoints[k];
		}
		float single = 0f;
		for (int l = 0; l < (int)this._tempTimes.Length; l++)
		{
			single += this._tempTimes[l];
		}
		single /= (float)((int)this._tempTimes.Length);
		length /= (float)((int)this._hitPoints.Length);
		vector33 = vector33.normalized;
		this.PredictedGroundNormal = vector33;
		PlayerController.Instance.boardController.GroundNormal = vector33;
		vector3 = Vector3.ProjectOnPlane(p_vel, Vector3.up);
		Vector3 vector34 = Vector3.ProjectOnPlane(vector33, vector3.normalized);
		float single1 = single;
		float single2 = Vector3.Angle(Vector3.up, vector34);
		Debug.Log(Vector3.Cross(p_vel, vector33).y);
		float single3 = 1.04196f * Mathf.Tan(0.0174532924f * single2) * Mathf.Sign(-Vector3.Cross(p_vel, vector34).y) / single1;
		Vector3 vector35 = Vector3.Cross(p_vel.normalized, Vector3.up) * single3;
		if (Mathd.Vector3IsInfinityOrNan(vector35) || vector35.magnitude > 5f)
		{
			vector35 = Vector3.zero;
		}
		p_correctiveForce = vector35;
		return 0f;
	}

	public Vector3 PlotSingleTrajectory(Vector3 p_start, Vector3 p_startVelocity, float p_timestep, float p_maxTime, out float p_totalTime, out Vector3 p_hitPoint)
	{
		float single = 0f;
		Vector3 pStart = p_start;
		int num = 1;
		while (true)
		{
			float pTimestep = p_timestep * (float)num;
			if (pTimestep > p_maxTime)
			{
				p_hitPoint = Vector3.up;
				p_totalTime = p_maxTime;
				return Vector3.up;
			}
			Vector3 vector3 = this.PlotTrajectoryAtTime(p_start, p_startVelocity, pTimestep);
			p_totalTime = single;
			if (Physics.Raycast(pStart, vector3 - pStart, out this._hit, Vector3.Magnitude(vector3 - pStart), this.layers) && this._hit.collider.gameObject.layer == LayerMask.NameToLayer("Default"))
			{
				break;
			}
			pStart = vector3;
			single = pTimestep;
			num++;
		}
		this._lastHitPoint = this._hit.point;
		p_hitPoint = this._hit.point;
		return this._hit.normal;
	}

	public float PlotTrajectory(Vector3 p_start, Vector3 p_startVelocity, float p_timestep, float p_maxTime)
	{
		float single = 0f;
		Vector3 pStart = p_start;
		int num = 1;
		while (true)
		{
			float pTimestep = p_timestep * (float)num;
			if (pTimestep > p_maxTime)
			{
				return p_maxTime;
			}
			Vector3 vector3 = this.PlotTrajectoryAtTime(p_start, p_startVelocity, pTimestep);
			if (Physics.Raycast(pStart, vector3 - pStart, out this._hit, Vector3.Magnitude(vector3 - pStart), this.layers) && this._hit.collider.gameObject.layer == LayerMask.NameToLayer("Default"))
			{
				break;
			}
			pStart = vector3;
			single = pTimestep;
			num++;
		}
		if (this._hit.point.y >= PlayerController.Instance.boardController.GroundY)
		{
			PlayerController.Instance.boardController.GroundY = this._hit.point.y;
		}
		return single;
	}

	public float PlotTrajectory(Vector3 p_start, Vector3 p_startVelocity, float p_timestep, float p_maxTime, out RaycastHit p_hit)
	{
		float single = 0f;
		Vector3 pStart = p_start;
		int num = 1;
		while (true)
		{
			float pTimestep = p_timestep * (float)num;
			Vector3 vector3 = this.PlotTrajectoryAtTime(p_start, p_startVelocity, pTimestep);
			if (pTimestep > p_maxTime)
			{
				this.PredictedGroundNormal = Vector3.up;
				p_hit = this._hit;
				return p_maxTime;
			}
			if (Physics.Raycast(pStart, vector3 - pStart, out this._hit, Vector3.Magnitude(vector3 - pStart), this.layers) && this._hit.collider.gameObject.layer == LayerMask.NameToLayer("Default"))
			{
				break;
			}
			pStart = vector3;
			single = pTimestep;
			num++;
		}
		if (this._hit.point.y >= PlayerController.Instance.boardController.GroundY)
		{
			PlayerController.Instance.boardController.GroundY = this._hit.point.y;
		}
		p_hit = this._hit;
		return single;
	}

	public Vector3 PlotTrajectoryAtTime(Vector3 p_start, Vector3 p_startVelocity, float p_time)
	{
		return (p_start + (p_startVelocity * p_time)) + (((Physics.gravity * p_time) * p_time) * 0.5f);
	}
}