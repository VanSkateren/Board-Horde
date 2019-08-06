using System;
using UnityEngine;
using VacuumBreather;

public class COMController : MonoBehaviour
{
	public Vector3 popForce;

	public Vector3 comOffset;

	public Rigidbody COMRigidbody;

	private readonly PidVector3Controller _pidVector3Controller = new PidVector3Controller(8f, 0f, 0.05f);

	public float Kp;

	public float Ki;

	public float Kd;

	public float KpImpact;

	public float KdImpact;

	public float KpImpactUp;

	public float KdImpactUp;

	public float KpSetup;

	public float KdSetup;

	public float KpGrind;

	public float KdGrind;

	private Vector3 _lastCOMTargetLocalPos;

	private Vector3 _lastCOMTargetLocalVel;

	public float maxLegForce;

	private Vector3 lastBoardVel;

	public float comHeightRiding;

	public float comHeightLoading;

	private Vector3 lastVel;

	private bool isCompressing
	{
		get
		{
			return Vector3.Dot(-PlayerController.Instance.skaterController.skaterTransform.up, this.COMRigidbody.velocity) > 0f;
		}
	}

	public COMController()
	{
	}

	private void Start()
	{
		this._lastCOMTargetLocalPos = base.transform.position;
	}

	public void UpdateCOM()
	{
		this.COMRigidbody.position = Vector3.Lerp(this.COMRigidbody.position, PlayerController.Instance.skaterController.skaterRigidbody.position, Time.deltaTime * 10f);
		this.COMRigidbody.velocity = PlayerController.Instance.skaterController.skaterRigidbody.velocity;
	}

	public void UpdateCOM(float targetHeight, int mode = 0)
	{
		if (mode == 1)
		{
			if (!this.isCompressing)
			{
				this._pidVector3Controller.Kp = this.KpImpactUp;
				this._pidVector3Controller.Kd = this.KdImpactUp;
			}
			else
			{
				this._pidVector3Controller.Kp = this.KpImpact;
				this._pidVector3Controller.Kd = this.KdImpact;
			}
		}
		else if (mode == 2)
		{
			this._pidVector3Controller.Kp = this.KpSetup;
			this._pidVector3Controller.Kd = this.KdSetup;
		}
		else if (mode != 3)
		{
			this._pidVector3Controller.Kp = this.Kp;
			this._pidVector3Controller.Ki = this.Ki;
			this._pidVector3Controller.Kd = this.Kd;
		}
		else
		{
			this._pidVector3Controller.Kp = this.KpGrind;
			this._pidVector3Controller.Kd = this.KdGrind;
		}
		targetHeight = targetHeight + (-1f + PlayerController.Instance.skaterController.crouchAmount) / 4f;
		this.UpdateComObject(targetHeight);
		PlayerController.Instance.skaterController.UpdateSkaterPosFromComPos();
	}

	private void UpdateComObject(float targetHeight)
	{
		Vector3 vector3 = targetHeight * PlayerController.Instance.skaterController.skaterTransform.TransformDirection(Vector3.up);
		Vector3 instance = PlayerController.Instance.boardController.boardTransform.position + vector3;
		Vector3 cOMRigidbody = -(this.COMRigidbody.velocity - PlayerController.Instance.boardController.boardRigidbody.velocity) * Time.deltaTime;
		Vector3 vector31 = -(base.transform.position - instance);
		Vector3 vector32 = this._pidVector3Controller.ComputeOutput(vector31, cOMRigidbody, Time.deltaTime);
		vector32 = new Vector3(Mathf.Clamp(vector32.x, -this.maxLegForce, this.maxLegForce), Mathf.Clamp(vector32.y, -this.maxLegForce, this.maxLegForce), Mathf.Clamp(vector32.z, -this.maxLegForce, this.maxLegForce));
		Quaternion rotation = Quaternion.FromToRotation(Vector3.ProjectOnPlane(this.lastBoardVel, Vector3.up), Vector3.ProjectOnPlane(PlayerController.Instance.boardController.boardRigidbody.velocity, Vector3.up));
		this.lastBoardVel = PlayerController.Instance.boardController.boardRigidbody.velocity;
		this.COMRigidbody.velocity = rotation * this.COMRigidbody.velocity;
		Vector3 vector33 = Mathf.Abs(PlayerController.Instance.boardController.xacceleration) * Mathf.Abs(Vector3.Dot(PlayerController.Instance.skaterController.skaterTransform.up, PlayerController.Instance.boardController.boardTransform.right)) * -PlayerController.Instance.skaterController.skaterTransform.up;
		this.COMRigidbody.AddForce(vector32, ForceMode.Force);
		float single = this.COMRigidbody.position.y - PlayerController.Instance.boardController.boardRigidbody.position.y;
		Vector3 vector34 = PlayerController.Instance.skaterController.skaterTransform.InverseTransformPoint(PlayerController.Instance.boardController.boardTransform.position);
		Vector3 vector35 = PlayerController.Instance.skaterController.skaterTransform.InverseTransformPoint(this.COMRigidbody.position);
		if (vector35.y - vector34.y < 0.53379f)
		{
			vector35 = new Vector3(vector35.x, vector34.y + 0.53379f, vector35.z);
			this.COMRigidbody.position = PlayerController.Instance.skaterController.skaterTransform.TransformPoint(vector35);
		}
		this.UpdateCrouch(single);
	}

	private void UpdateCrouch(float targetHeight)
	{
		float single = PlayerController.Instance.skaterController.skaterTransform.InverseTransformPoint(this.COMRigidbody.position).y - PlayerController.Instance.skaterController.skaterTransform.InverseTransformPoint(PlayerController.Instance.boardController.boardRigidbody.position).y;
		targetHeight = this.comHeightRiding;
		float single1 = Mathf.Clamp(targetHeight - single, 0f, Single.PositiveInfinity);
		single1 /= 0.53054f;
		PlayerController.Instance.animationController.SetValue("Impact", Mathf.Clamp(single1, 0f, 1f));
	}
}