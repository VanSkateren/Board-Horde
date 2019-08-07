using FSMHelper;
using RootMotion.Dynamics;
using RootMotion.FinalIK;
using System;
using UnityEngine;
using VacuumBreather;

public class SkaterController : MonoBehaviour
{
	[Header("Components")]
	public Respawn respawn;

	public PuppetMaster puppetMaster;

	public BehaviourPuppet behaviourPuppet;

	public FullBodyBipedIK finalIk;

	[Header("Transforms")]
	public Transform skaterTransform;

	public Transform animBoardTargetTransform;

	public Transform animBoardFrontWheelTransform;

	public Transform animBoardBackWheelTransform;

	public Transform animBoardBackwardsTargetTransform;

	public Transform physicsBoardTransform;

	public Transform physicsBoardBackwardsTransform;

	public Transform upVectorTransform;

	public Transform regsLeftKneeGuide;

	public Transform regsRightKneeGuide;

	public Transform goofyLeftKneeGuide;

	public Transform goofyRightKneeGuide;

	[Header("Rigidbodies")]
	public Rigidbody skaterRigidbody;

	[Header("Colliders")]
	public CapsuleCollider leftFootCollider;

	public CapsuleCollider rightFootCollider;

	[Header("Variables")]
	[Range(1f, 10f)]
	public float pushForce = 8f;

	[Range(1f, 10f)]
	public float breakForce = 3f;

	private Quaternion _currentRotationTarget;

	private Vector3 _currentForwardTarget;

	private float _duration;

	private float _startTime;

	private Vector3 _startUpVector = Vector3.up;

	private Quaternion _startRotation = Quaternion.identity;

	private Quaternion _newUp = Quaternion.identity;

	private Vector3 _boardTargetVel;

	private Vector3 _boardTargetLastPos;

	private bool _landingPrediction;

	private float _animSwitch;

	private readonly PidQuaternionController _pidController = new PidQuaternionController(8f, 0f, 0.05f);

	public float Kp;

	public float Ki;

	public float Kd;

	public float rotSmooth;

	public float maxCrouchAtAngle;

	public float crouchSmooth;

	private float _crouchAmount;

	public Vector3 pushBrakeForce;

	public float totalSystemMass;

	private Vector3 _angularVelocity;

	public Rigidbody leanProxy;

	public Vector3 comboAccelLerp;

	public float maxComboAccelLerp;

	public Vector3 BoardTargetVel
	{
		get
		{
			return this._boardTargetVel;
		}
	}

	public float crouchAmount
	{
		get
		{
			return this._crouchAmount;
		}
	}

	public Transform LowestWheelTransform
	{
		get
		{
			if ((this.skaterRigidbody.position - this.animBoardFrontWheelTransform.position).magnitude > (this.skaterRigidbody.position - this.animBoardBackWheelTransform.position).magnitude)
			{
				return this.animBoardFrontWheelTransform;
			}
			return this.animBoardBackWheelTransform;
		}
	}

	public SkaterController()
	{
	}

	public void AddCollisionOffset()
	{
		if (!Mathd.Vector3IsInfinityOrNan(PlayerController.Instance.BoardToTargetVector()))
		{
			Transform targetVector = this.skaterTransform;
			targetVector.position = targetVector.position + PlayerController.Instance.BoardToTargetVector();
		}
	}

	public void AddTurnTorque(float p_value)
	{
		p_value /= 10f;
		this.skaterRigidbody.AddTorque(this.skaterTransform.up * p_value, ForceMode.VelocityChange);
		this.leanProxy.AddTorque(this.skaterTransform.up * p_value, ForceMode.VelocityChange);
	}

	public void AddTurnTorque(float p_value, bool p_fast)
	{
		p_value = Mathf.Lerp(p_value / 10f, p_value, Mathf.Abs(p_value));
		this.skaterRigidbody.AddTorque(this.skaterTransform.up * p_value, ForceMode.VelocityChange);
	}

	public void AddUpwardDisplacement(float p_timeStep)
	{
		if (!Mathd.Vector3IsInfinityOrNan(this.GetBoardDisplacement()))
		{
			Vector3 coMDisplacement = this.skaterTransform.up * PlayerController.Instance.GetCoMDisplacement(p_timeStep);
			Transform transforms = this.skaterTransform;
			transforms.position = transforms.position + coMDisplacement;
		}
	}

	private void Awake()
	{
		float single = 0f;
		Rigidbody[] componentsInChildren = PlayerController.Instance.gameObject.GetComponentsInChildren<Rigidbody>();
		for (int i = 0; i < (int)componentsInChildren.Length; i++)
		{
			single += componentsInChildren[i].mass;
		}
		this.totalSystemMass = single;
	}

	public void CorrectVelocity()
	{
		float single = Vector3.Angle(Vector3.ProjectOnPlane(this.skaterRigidbody.velocity, PlayerController.Instance.GetGroundNormal()), Vector3.up);
		float single1 = Vector3.Angle(PlayerController.Instance.GetGroundNormal(), Vector3.up);
		if (PlayerController.Instance.IsGrounded() && single > 45f && single < 80f && single1 > 10f && single1 < 85f)
		{
			this.skaterRigidbody.velocity = Vector3.ProjectOnPlane(this.skaterRigidbody.velocity, PlayerController.Instance.GetGroundNormal());
		}
	}

	private void FixedUpdate()
	{
		this._currentRotationTarget = this.UpdateTargetRotation();
		this._currentForwardTarget = this.UpdateTargetForward();
		this.UpdateBoardTargetVel();
	}

	public Vector3 GetBoardDisplacement()
	{
		return PlayerController.Instance.boardController.boardTransform.position - PlayerController.Instance.boardController.boardTargetPosition.position;
	}

	private void InAirRotation(float p_slerp)
	{
		Quaternion quaternion = Quaternion.Slerp(this._startRotation, this._newUp, p_slerp);
		this.upVectorTransform.rotation = quaternion;
		Quaternion rotation = Quaternion.FromToRotation(this.skaterTransform.up, this.upVectorTransform.up);
		rotation *= this.skaterRigidbody.rotation;
		this.skaterRigidbody.rotation = rotation;
		this.leanProxy.rotation = rotation;
		this.comboAccelLerp = this.skaterTransform.up;
	}

	private void InAirRotationLogic()
	{
		this.InAirRotation(Mathf.Clamp((Time.time - this._startTime) / this._duration, 0f, 1f));
	}

	private void OnAnimatorIK(int p_layerIndex)
	{
		PlayerController.Instance.playerSM.OnAnimatorUpdateSM();
	}

	public Vector3 PredictLanding(Vector3 p_popForce)
	{
		p_popForce = this.skaterRigidbody.velocity + p_popForce;
		this._startTime = Time.time;
		Vector3 vector3 = Vector3.zero;
		this._duration = PlayerController.Instance.boardController.trajectory.CalculateTrajectory(this.skaterTransform.position - (Vector3.up * 0.9765f), p_popForce, 50f, out vector3);
		this._startRotation = this.skaterRigidbody.rotation;
		this._startUpVector = this.skaterTransform.up;
		this._landingPrediction = true;
		this._newUp = Quaternion.FromToRotation(this._startUpVector, PlayerController.Instance.boardController.trajectory.PredictedGroundNormal);
		this._newUp *= this.skaterRigidbody.rotation;
		base.CancelInvoke("PreLandingEvent");
		base.Invoke("PreLandingEvent", this._duration - 0.3f);
		return vector3;
	}

	private void PreLandingEvent()
	{
		PlayerController.Instance.playerSM.OnPreLandingEventSM();
	}

	public void RemoveTurnTorque(float p_value)
	{
		Vector3 pValue = this.skaterTransform.InverseTransformDirection(this.skaterRigidbody.angularVelocity);
		pValue.y *= p_value;
		this.skaterRigidbody.angularVelocity = this.skaterTransform.TransformDirection(pValue);
	}

	public void ResetRotationLerps()
	{
		this.leanProxy.rotation = this.skaterTransform.rotation;
		this.comboAccelLerp = this.skaterTransform.up;
	}

	public void SetPuppetMode(BehaviourPuppet.NormalMode p_mode)
	{
		this.behaviourPuppet.masterProps.normalMode = p_mode;
	}

	private void UdpateSkater()
	{
		if (!Mathd.Vector3IsInfinityOrNan(this.GetBoardDisplacement()))
		{
			Transform boardDisplacement = this.skaterTransform;
			boardDisplacement.position = boardDisplacement.position + this.GetBoardDisplacement();
			this.skaterRigidbody.velocity = PlayerController.Instance.boardController.boardRigidbody.velocity;
		}
	}

	private void Update()
	{
		this._animSwitch = Mathf.Lerp(this._animSwitch, (PlayerController.Instance.IsSwitch ? 1f : 0f), Time.deltaTime * 10f);
		PlayerController.Instance.AnimSetSwitch(this._animSwitch);
	}

	private void UpdateBoardTargetVel()
	{
		this._boardTargetVel = (this.animBoardTargetTransform.position - this._boardTargetLastPos) / Time.deltaTime;
		this._boardTargetLastPos = this.animBoardTargetTransform.position;
	}

	public void UpdatePositionDuringPop()
	{
		this._landingPrediction = false;
		this._duration = 0f;
		this._startTime = 0f;
	}

	public void UpdatePositions()
	{
		if (PlayerController.Instance.movementMaster == PlayerController.MovementMaster.Skater)
		{
			this.InAirRotationLogic();
			return;
		}
		if (PlayerController.Instance.movementMaster != PlayerController.MovementMaster.Target)
		{
			this.UdpateSkater();
		}
		this._landingPrediction = false;
		this._duration = 0f;
		this._startTime = 0f;
	}

	public void UpdateRidingPositionsCOMTempWallie()
	{
	}

	public void UpdateSkaterPosFromComPos()
	{
		Vector3 vector3 = PlayerController.Instance.skaterController.skaterTransform.InverseTransformPoint(PlayerController.Instance.boardController.boardTransform.position);
		Vector3 vector31 = this.skaterTransform.InverseTransformPoint(PlayerController.Instance.comController.transform.position);
		if (vector31.y - vector3.y < 0.4f)
		{
			vector31 = new Vector3(vector31.x, vector3.y + 0.4f, vector31.z);
		}
		Vector3 vector32 = new Vector3(0f, vector31.y, 0f);
		this.skaterTransform.position = this.skaterTransform.TransformPoint(vector32);
		this.skaterRigidbody.velocity = PlayerController.Instance.comController.COMRigidbody.velocity;
	}

	public void UpdateSkaterRotation(bool canRotate, bool manualling)
	{
		if (canRotate && PlayerController.Instance.IsGrounded())
		{
			Vector3 vector3 = this.pushBrakeForce / this.totalSystemMass;
			float instance = 1f - PlayerController.Instance.boardController.xzRot / this.maxCrouchAtAngle;
			this._crouchAmount = Mathf.SmoothStep(this._crouchAmount, instance, Time.fixedDeltaTime * this.crouchSmooth);
			Vector3 vector31 = -Physics.gravity;
			Vector3 instance1 = PlayerController.Instance.boardController.acceleration - vector3;
			Vector3 vector32 = vector31 + instance1;
			this.comboAccelLerp = Vector3.RotateTowards(this.comboAccelLerp, vector32, 0.0174532924f * this.maxComboAccelLerp * Time.deltaTime, 5f * Time.deltaTime);
			Debug.DrawRay(this.skaterTransform.position, vector32 * 0.1f, Color.red);
			Debug.DrawRay(this.skaterTransform.position, this.skaterTransform.up, Color.green);
			Debug.DrawRay(this.skaterTransform.position, this.comboAccelLerp * 0.1f, Color.blue);
			this._pidController.Kp = this.Kp;
			this._pidController.Ki = this.Ki;
			this._pidController.Kd = this.Kd;
			Vector3 vector33 = (!PlayerController.Instance.GetBoardBackwards() ? this.physicsBoardTransform.forward : -this.physicsBoardTransform.forward);
			if (manualling)
			{
				vector33 = Vector3.ProjectOnPlane(vector33, PlayerController.Instance.GetGroundNormal());
			}
			Quaternion quaternion = Quaternion.LookRotation(vector33, this.comboAccelLerp);
			Vector3 vector34 = this._pidController.ComputeRequiredAngularAcceleration(this.skaterRigidbody.rotation, quaternion, this.leanProxy.angularVelocity, Time.fixedDeltaTime);
			this.leanProxy.AddTorque(vector34, ForceMode.Acceleration);
			this.skaterRigidbody.rotation = this.leanProxy.rotation;
		}
	}

	public void UpdateSkaterRotation(bool p_canRotate, Quaternion p_rot)
	{
		if (p_canRotate)
		{
			this.skaterTransform.rotation = Quaternion.Slerp(this.skaterTransform.rotation, p_rot, Time.fixedDeltaTime * 50f);
		}
	}

	public void UpdateSkaterRotationOLD(bool canRotate)
	{
		if (canRotate)
		{
			this.skaterTransform.rotation = Quaternion.Slerp(this.skaterTransform.rotation, this._currentRotationTarget, Time.fixedDeltaTime * 10f);
			Quaternion rotation = Quaternion.FromToRotation(this.skaterTransform.forward, Vector3.ProjectOnPlane(this._currentForwardTarget, this.skaterTransform.up)) * this.skaterTransform.rotation;
			this.skaterTransform.rotation = rotation;
		}
	}

	private Vector3 UpdateTargetForward()
	{
		if (!PlayerController.Instance.boardController.IsBoardBackwards)
		{
			return this.physicsBoardTransform.forward;
		}
		return this.physicsBoardBackwardsTransform.forward;
	}

	private Quaternion UpdateTargetRotation()
	{
		if (!PlayerController.Instance.boardController.IsBoardBackwards)
		{
			return this.physicsBoardTransform.rotation;
		}
		return this.physicsBoardBackwardsTransform.rotation;
	}
}