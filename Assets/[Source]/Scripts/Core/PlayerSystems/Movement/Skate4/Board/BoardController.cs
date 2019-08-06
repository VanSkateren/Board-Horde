using FSMHelper;
using System;
using UnityEngine;
using VacuumBreather;

public class BoardController : MonoBehaviour
{
	public Trajectory trajectory;

	public TriggerManager triggerManager;

	[SerializeField]
	private Transform _wheel1;

	[SerializeField]
	private Transform _wheel2;

	[SerializeField]
	private Transform _wheel3;

	[SerializeField]
	private Transform _wheel4;

	[SerializeField]
	private LayerMask _layers;

	[SerializeField]
	private RaycastHit _hit;

	public float groundNormalLerpSpeed = 20f;

	public float grindForce = 500f;

	public float grindDamp = 300f;

	public float manualForce = 50f;

	public float manualDamp = 10f;

	public float minManualAngle = 10f;

	public float maxManualAngle = 20f;

	public string grindTag;

	public bool isSliding;

	public AnimationCurve triggerCurve;

	private bool removingTorque;

	private float _groundY;

	private float _turnTarget;

	private Vector3 _groundNormal = Vector3.up;

	private Vector3 _lerpedGroundNormal = Vector3.up;

	private Vector3 _lastGroundNormal = Vector3.up;

	[SerializeField]
	private bool _isBoardBackwards;

	[SerializeField]
	private bool _released;

	private bool _grounded;

	private bool _allDown;

	private bool _twoDown;

	private bool[] _wheelsDown = new bool[4];

	[Header("Transforms")]
	public Transform boardTransform;

	public Transform leanPressurePointLeft;

	public Transform leanPressurePointRight;

	public Transform boardMesh;

	[SerializeField]
	public Transform boardControlTransform;

	[SerializeField]
	private Transform _boardTargetRotation;

	[SerializeField]
	private Transform _backwardsTargetRotation;

	public Transform boardTargetPosition;

	[SerializeField]
	private Transform _frontPivot;

	[SerializeField]
	private Transform _frontPivotCenter;

	[SerializeField]
	private Transform _backPivot;

	[SerializeField]
	private Transform _backPivotCenter;

	[SerializeField]
	private Transform _rightPivotRotationTarget;

	[SerializeField]
	private Transform _leftPivotRotationTarget;

	[SerializeField]
	private Transform _sidePivotRotation;

	[SerializeField]
	private Transform _forwardPivotRotationTarget;

	[SerializeField]
	private Transform _backwardPivotRotationTarget;

	[SerializeField]
	private Transform _catchForwardRotation;

	public Transform frontTruckCoM;

	public Transform backTruckCoM;

	[Header("Rigidbodies")]
	public Rigidbody boardRigidbody;

	public Rigidbody frontTruckRigidbody;

	public Rigidbody backTruckRigidbody;

	[Header("Truck Joints")]
	public ConfigurableJoint backTruckJoint;

	public ConfigurableJoint frontTruckJoint;

	public float popForce;

	private Vector3 _lastPos = Vector3.zero;

	private float _angle;

	private Vector3 _axis = Vector3.zero;

	private Vector3 _positionDelta = Vector3.zero;

	private Quaternion _rotationDelta = Quaternion.identity;

	public Quaternion currentRotationTarget;

	private float _firstVel;

	public float secondVel;

	public float thirdVel;

	private float _thirdVel;

	private float _firstDelta;

	private float _secondDelta;

	private float _thirdDelta;

	private Quaternion _bufferedRotation;

	private Quaternion _rotDeltaThisFrame;

	private float _bufferedFlip;

	private float _bufferedBodyRot;

	private float _rollSoundSpeed;

	private bool _bearingSoundSet;

	private Vector3 _lastInAirVelocity = Vector3.zero;

	public float wheelBase;

	public float boardLean;

	public float maxBoardLean;

	private float _theta;

	public float _targetPosition;

	private float _frontPivotLerp;

	private float _backPivotLerp;

	private float _sidePivotLerp;

	private float _forwardPivotLerp;

	private readonly PidController _pidControllerx = new PidController(8f, 0f, 0.05f);

	private readonly PidController _pidControllery = new PidController(8f, 0f, 0.05f);

	private readonly PidController _pidControllerz = new PidController(8f, 0f, 0.05f);

	public float Kp;

	public float Ki;

	public float Kd;

	[SerializeField]
	private float _catchUpRotateSpeed = 35f;

	[SerializeField]
	private float _catchRotateSpeed = 5f;

	public float _catchSignedAngle;

	private float _mag;

	private float _tempRotateSpeed;

	private float _tempUpAngle;

	private readonly PidQuaternionController _pidRotController = new PidQuaternionController(8f, 0f, 0.05f);

	public float KRp;

	public float KRi;

	public float KRd;

	public float grindKRp;

	public float grindKRi;

	public float grindKRd;

	private float _originalKRp;

	private float _originalKRi;

	private float _originalKRd;

	public float rotateSpeed = 90f;

	private Vector3 _angularTarget;

	private Vector3 _angVelTarget;

	public float maxAngularVel;

	public float onBoardMaxRollAngle;

	public Vector3 boardUpAtPopBegin;

	public float angVelMult;

	public float popBoardVelAdd;

	private float _lastuplift;

	public float popSpeed;

	private Vector3 _lastVel;

	private Vector3 _acceleration;

	public float absLocalXAccel
	{
		get
		{
			return Mathf.Abs(this.boardTransform.InverseTransformDirection(this.acceleration).x);
		}
	}

	public Vector3 acceleration
	{
		get
		{
			Vector3 vector3 = this.boardTransform.InverseTransformDirection(this._acceleration);
			vector3 = new Vector3(vector3.x * 0.25f, vector3.y, vector3.z);
			vector3 = this.boardTransform.TransformDirection(vector3);
			return vector3;
		}
	}

	public bool AllDown
	{
		get
		{
			return this._allDown;
		}
	}

	public bool AnyAxleOffGround
	{
		get
		{
			if (!this._wheelsDown[0] && !this._wheelsDown[1])
			{
				return true;
			}
			if (this._wheelsDown[2])
			{
				return false;
			}
			return !this._wheelsDown[3];
		}
	}

	public float firstVel
	{
		get
		{
			return this._firstVel;
		}
		set
		{
			this._firstVel = value;
		}
	}

	public bool Grounded
	{
		get
		{
			return this._grounded;
		}
	}

	public Vector3 GroundNormal
	{
		get
		{
			return this._groundNormal;
		}
		set
		{
			this._groundNormal = value;
		}
	}

	public float GroundY
	{
		get
		{
			return this._groundY;
		}
		set
		{
			this._groundY = value;
		}
	}

	public bool IsBoardBackwards
	{
		get
		{
			return this._isBoardBackwards;
		}
		set
		{
			this._isBoardBackwards = value;
		}
	}

	public Vector3 LastGroundNormal
	{
		get
		{
			return this._lastGroundNormal;
		}
	}

	public Vector3 LerpedGroundNormal
	{
		get
		{
			return this._lerpedGroundNormal;
		}
		set
		{
			this._lerpedGroundNormal = value;
		}
	}

	public float localXAccel
	{
		get
		{
			return this.boardTransform.InverseTransformDirection(this.acceleration).x;
		}
	}

	public float localXVel
	{
		get
		{
			return this.boardTransform.InverseTransformDirection(this.boardRigidbody.velocity).x;
		}
	}

	public float TurnTarget
	{
		get
		{
			return this._turnTarget;
		}
		set
		{
			this._turnTarget = value;
		}
	}

	public bool TwoDown
	{
		get
		{
			return this._twoDown;
		}
	}

	public float xacceleration
	{
		get
		{
			return this.boardTransform.InverseTransformDirection(this._acceleration).x;
		}
	}

	public float xzRot
	{
		get
		{
			float single = Mathf.Abs(Mathd.AngleBetween(0f, this.boardTransform.localEulerAngles.x));
			float single1 = Mathf.Abs(Mathd.AngleBetween(0f, this.boardTransform.localEulerAngles.z));
			return Mathf.Sqrt(single * single + single1 * single1);
		}
	}

	public float yAngVel
	{
		get
		{
			return this.boardRigidbody.angularVelocity.y;
		}
	}

	public BoardController()
	{
	}

	public void AddPushForce(float p_value)
	{
		if (this.boardRigidbody.velocity.magnitude < PlayerController.Instance.topSpeed)
		{
			if (this.boardRigidbody.velocity.magnitude >= 0.15f)
			{
				Rigidbody rigidbody = this.boardRigidbody;
				Vector3 vector3 = this.boardRigidbody.velocity;
				rigidbody.AddForce(vector3.normalized * p_value, ForceMode.Impulse);
			}
			else if (Vector3.Angle(PlayerController.Instance.PlayerForward(), Camera.main.transform.forward) >= 90f)
			{
				this.boardRigidbody.AddForce((-PlayerController.Instance.PlayerForward() * p_value) * 1.4f, ForceMode.Impulse);
			}
			else
			{
				this.boardRigidbody.AddForce((PlayerController.Instance.PlayerForward() * p_value) * 1.4f, ForceMode.Impulse);
			}
		}
		SoundManager.Instance.PlayPushOff(0.01f);
	}

	private void AddTorqueRotation(Quaternion p_targetRot)
	{
		Mathd.DampTorqueTowards(this.boardRigidbody, this.boardRigidbody.rotation, p_targetRot, this.grindForce, this.grindDamp);
	}

	public void AddTurnTorque(float p_value)
	{
		this.TurnTarget = p_value;
		this.removingTorque = false;
		Vector3 vector3 = this.boardTransform.InverseTransformDirection(this.boardRigidbody.angularVelocity);
		vector3.y = Mathf.MoveTowards(vector3.y, 3f * p_value, Time.deltaTime * 16f);
		this.boardRigidbody.angularVelocity = this.boardTransform.TransformDirection(vector3);
	}

	public void AddTurnTorqueManuals(float p_value)
	{
		this.boardRigidbody.AddTorque(((this.boardTransform.up * p_value) * 5f) * Time.deltaTime, ForceMode.VelocityChange);
	}

	public void ApplyFriction()
	{
		if (this._allDown)
		{
			Vector3 vector3 = this.boardTransform.InverseTransformDirection(this.boardRigidbody.angularVelocity);
			ref float singlePointer = ref vector3.z;
			singlePointer = singlePointer * (0.9f * Time.deltaTime * 120f);
			ref float singlePointer1 = ref vector3.x;
			singlePointer1 = singlePointer1 * (0.9f * Time.deltaTime * 120f);
			this.boardRigidbody.angularVelocity = this.boardTransform.TransformDirection(vector3);
		}
		Vector3 vector31 = this.boardTransform.InverseTransformDirection(this.boardRigidbody.velocity);
		vector31.x *= 0.3f;
		vector31.z *= 0.999f;
		this.boardRigidbody.velocity = this.boardTransform.TransformDirection(vector31);
	}

	public void ApplyOnBoardMaxRoll()
	{
		if (!this.Grounded)
		{
			if (Vector3.Angle(this.boardRigidbody.transform.up, PlayerController.Instance.skaterController.transform.up) > this.onBoardMaxRollAngle)
			{
				this.PIDRotation(this.currentRotationTarget);
			}
			this.boardRigidbody.angularVelocity = Vector3.Lerp(this.boardRigidbody.angularVelocity, Vector3.zero, Time.deltaTime * 50f);
		}
	}

	public void AutoCatchRotation()
	{
		this.CatchRotation();
	}

	private void Awake()
	{
		this._lastPos = this.boardRigidbody.position;
		this.boardRigidbody.maxAngularVelocity = 150f;
		this.backTruckRigidbody.maxAngularVelocity = 150f;
		this.frontTruckRigidbody.maxAngularVelocity = 150f;
		this.boardRigidbody.maxDepenetrationVelocity = 1f;
		this.backTruckRigidbody.maxDepenetrationVelocity = 1f;
		this.frontTruckRigidbody.maxDepenetrationVelocity = 1f;
		this.boardRigidbody.solverIterations = 10;
		this.backTruckRigidbody.solverIterations = 10;
		this.frontTruckRigidbody.solverIterations = 10;
		if (this.trajectory == null)
		{
			this.trajectory = base.GetComponent<Trajectory>();
		}
		this._originalKRp = this.KRp;
		this._originalKRi = this.KRi;
		this._originalKRd = this.KRd;
	}

	public void CacheBoardUp()
	{
		this.boardUpAtPopBegin = this.boardTransform.up;
	}

	public void CatchRotation()
	{
		Vector3 vector3 = Vector3.ProjectOnPlane(Vector3.up, this.boardRigidbody.transform.forward);
		this._catchSignedAngle = Vector3.SignedAngle(vector3, PlayerController.Instance.skaterController.skaterTransform.up, PlayerController.Instance.skaterController.skaterTransform.right);
		PlayerController.Instance.AnimSetCatchAngle(this._catchSignedAngle);
		Quaternion quaternion = Quaternion.LookRotation(this._catchForwardRotation.forward, vector3);
		Quaternion.Slerp(this.boardRigidbody.rotation, quaternion, Time.fixedDeltaTime * this._catchUpRotateSpeed);
		this.PIDRotation(Quaternion.Slerp(quaternion, this.currentRotationTarget, Time.fixedDeltaTime * this._catchRotateSpeed));
	}

	public void CatchRotation(float p_mag)
	{
		this._mag = Mathf.MoveTowards(this._mag, p_mag, Time.deltaTime * 10f);
		Vector3 vector3 = Vector3.ProjectOnPlane(Vector3.up, this.boardRigidbody.transform.forward);
		this._catchSignedAngle = Vector3.SignedAngle(vector3, PlayerController.Instance.skaterController.skaterTransform.up, PlayerController.Instance.skaterController.skaterTransform.right);
		PlayerController.Instance.AnimSetCatchAngle(this._catchSignedAngle);
		this._tempUpAngle = Vector3.Angle(this.boardTransform.up, PlayerController.Instance.skaterController.skaterTransform.up);
		Quaternion quaternion = Quaternion.LookRotation(this._catchForwardRotation.forward, vector3);
		Quaternion quaternion1 = Quaternion.Slerp(Quaternion.Slerp(this.boardRigidbody.rotation, quaternion, Time.fixedDeltaTime * this._catchUpRotateSpeed), this.currentRotationTarget, Time.fixedDeltaTime * (this._tempUpAngle <= 35f || this._tempUpAngle >= 90f ? this._catchRotateSpeed : this._catchUpRotateSpeed));
		this.PIDRotation(Quaternion.Slerp(quaternion1, this.currentRotationTarget, this._mag));
	}

	public void DoBoardLean()
	{
		float single = -Mathf.Sign(Vector3.Dot(this.boardMesh.forward, this.boardRigidbody.velocity));
		float single1 = this.wheelBase * this.boardTransform.InverseTransformDirection(this.acceleration).x / this.boardTransform.InverseTransformDirection(this.boardRigidbody.velocity).z;
		single1 = Mathf.Clamp(single1, -1f, 1f);
		if (Mathf.Abs(this.boardTransform.InverseTransformDirection(this.boardRigidbody.velocity).z) <= 0.1f)
		{
			this._theta = 0f;
		}
		else
		{
			this._theta = single * 57.29578f * Mathf.Asin(single1);
		}
		this._theta = Mathf.Clamp(this._theta, -this.maxBoardLean, this.maxBoardLean);
	}

	private void FixedUpdate()
	{
		this.SetBoardControlPosition();
		if (Time.deltaTime != 0f)
		{
			this._acceleration = (this.boardRigidbody.velocity - this._lastVel) / Time.deltaTime;
		}
		this._lastVel = this.boardRigidbody.velocity;
		this.SetRotationTarget();
		this.triggerManager.GrindTriggerCheck();
		this._lastGroundNormal = this.GroundNormal;
		this.LerpedGroundNormal = Vector3.Lerp(this.LerpedGroundNormal, this.GroundNormal, Time.fixedDeltaTime * this.groundNormalLerpSpeed);
		this._grounded = this.GroundCheck();
		if (PlayerController.Instance.movementMaster == PlayerController.MovementMaster.Board && this._grounded && !this.triggerManager.IsColliding)
		{
			this.ApplyFriction();
		}
		this._lastPos = this.boardRigidbody.position;
	}

	public void ForcePivotForwardRotation(float p_value)
	{
		p_value *= 0.25f;
		p_value += 0.5f;
		this._forwardPivotLerp = p_value;
	}

	public int GetGrindSoundInt()
	{
		int num = 0;
		string str = this.grindTag;
		if (str == "Concrete")
		{
			num = 0;
		}
		else if (str == "Wood")
		{
			num = 1;
		}
		else if (str == "Metal")
		{
			num = 2;
		}
		return num;
	}

	private Vector3 GetTargetForward()
	{
		if (this.IsBoardBackwards)
		{
			return this._backwardsTargetRotation.forward;
		}
		return this._boardTargetRotation.forward;
	}

	public bool GroundCheck()
	{
		if (!Physics.Raycast(this._wheel1.position, -this._wheel1.up, out this._hit, 0.05f, this._layers))
		{
			this._wheelsDown[0] = false;
		}
		else
		{
			PlayerController.Instance.boardController.GroundY = this._hit.point.y;
			this._groundNormal = this._hit.normal;
			this._wheelsDown[0] = true;
		}
		if (!Physics.Raycast(this._wheel2.position, -this._wheel2.up, out this._hit, 0.05f, this._layers))
		{
			this._wheelsDown[1] = false;
		}
		else
		{
			PlayerController.Instance.boardController.GroundY = this._hit.point.y;
			this._groundNormal = this._hit.normal;
			this._wheelsDown[1] = true;
		}
		if (!Physics.Raycast(this._wheel3.position, -this._wheel3.up, out this._hit, 0.05f, this._layers))
		{
			this._wheelsDown[2] = false;
		}
		else
		{
			PlayerController.Instance.boardController.GroundY = this._hit.point.y;
			this._groundNormal = this._hit.normal;
			this._wheelsDown[2] = true;
		}
		if (!Physics.Raycast(this._wheel4.position, -this._wheel4.up, out this._hit, 0.05f, this._layers))
		{
			this._wheelsDown[3] = false;
		}
		else
		{
			PlayerController.Instance.boardController.GroundY = this._hit.point.y;
			this._groundNormal = this._hit.normal;
			this._wheelsDown[3] = true;
		}
		if (!this._wheelsDown[0] || !this._wheelsDown[1] || !this._wheelsDown[2] || !this._wheelsDown[3])
		{
			this._allDown = false;
		}
		else
		{
			if (!this._allDown)
			{
				PlayerController.Instance.playerSM.OnAllWheelsDownSM();
			}
			this._allDown = true;
		}
		if ((!this._wheelsDown[0] || !this._wheelsDown[1]) && (!this._wheelsDown[2] || !this._wheelsDown[3]))
		{
			this._twoDown = false;
		}
		else
		{
			this._twoDown = true;
		}
		PlayerController.Instance.AnimSetAllDown(this._allDown);
		if (!this._wheelsDown[0] && !this._wheelsDown[1] && !this._wheelsDown[2] && !this._wheelsDown[3])
		{
			if (this._grounded)
			{
				PlayerController.Instance.animationController.SetValue("Grounded", false);
				PlayerController.Instance.playerSM.OnWheelsLeftGroundSM();
			}
			return false;
		}
		if (!this._grounded)
		{
			PlayerController.Instance.animationController.SetValue("Grounded", true);
			PlayerController.Instance.playerSM.OnFirstWheelDownSM();
			if (this.boardRigidbody.velocity.y < 0f)
			{
				SoundManager instance = SoundManager.Instance;
				Vector3 vector3 = Vector3.ProjectOnPlane(this._lastInAirVelocity, Vector3.ProjectOnPlane(this._lastInAirVelocity, Vector3.up));
				instance.PlayLandingSound(vector3.magnitude);
			}
		}
		return true;
	}

	public void LeaveFlipMode()
	{
		this.boardRigidbody.angularVelocity = Mathd.GlobalAngularVelocityFromLocal(this.boardRigidbody, new Vector3(this.firstVel * this.angVelMult, this.secondVel * this.angVelMult, 0f));
	}

	public void LimitAngularVelocity(float _maxY)
	{
		Vector3 vector3 = this.boardTransform.InverseTransformDirection(this.boardRigidbody.angularVelocity);
		vector3.y = Mathf.Clamp(vector3.y, -_maxY, _maxY);
		this.boardRigidbody.angularVelocity = this.boardTransform.TransformDirection(vector3);
		Vector3 vector31 = this.frontTruckRigidbody.transform.InverseTransformDirection(this.frontTruckRigidbody.angularVelocity);
		vector31.y = Mathf.Clamp(vector31.y, -_maxY, _maxY);
		this.frontTruckRigidbody.angularVelocity = this.frontTruckRigidbody.transform.TransformDirection(vector31);
		Vector3 vector32 = this.backTruckRigidbody.transform.InverseTransformDirection(this.backTruckRigidbody.angularVelocity);
		vector32.y = Mathf.Clamp(vector32.y, -_maxY, _maxY);
		this.backTruckRigidbody.angularVelocity = this.backTruckRigidbody.transform.TransformDirection(vector32);
	}

	public void LockAngularVelocity(Quaternion p_rot)
	{
		this.AddTorqueRotation(p_rot);
	}

	private void ManageCapsuleCollider()
	{
	}

	private void OnCollisionEnter(Collision collision)
	{
		PlayerController.Instance.playerSM.OnCollisionEnterEventSM();
	}

	private void OnCollisionExit(Collision collision)
	{
		PlayerController.Instance.playerSM.OnCollisionExitEventSM();
	}

	private void OnCollisionStay(Collision collision)
	{
		PlayerController.Instance.playerSM.OnCollisionStayEventSM();
	}

	private void PhysicsPosition()
	{
		this._positionDelta = this.boardTargetPosition.position - this.boardRigidbody.position;
		Vector3 vector3 = (this._positionDelta * 4000f) * Time.fixedDeltaTime;
		if (!float.IsNaN(vector3.x) && !float.IsNaN(vector3.y) && !float.IsNaN(vector3.z))
		{
			this.boardRigidbody.velocity = vector3;
		}
	}

	public void PhysicsRotation(float p_force, float p_damper)
	{
		this.PIDRotation(this.currentRotationTarget);
	}

	private void PIDPosition()
	{
		this._pidControllerx.Kp = this.Kp;
		this._pidControllerx.Ki = this.Ki;
		this._pidControllerx.Kd = this.Kd;
		this._pidControllery.Kp = this.Kp;
		this._pidControllery.Ki = this.Ki;
		this._pidControllery.Kd = this.Kd;
		this._pidControllerz.Kp = this.Kp;
		this._pidControllerz.Ki = this.Ki;
		this._pidControllerz.Kd = this.Kd;
		Vector3 boardTargetVel = -(this.boardRigidbody.velocity - PlayerController.Instance.skaterController.BoardTargetVel) * Time.deltaTime;
		Vector3 vector3 = -(this.boardRigidbody.position - this.boardTargetPosition.position);
		Vector3 vector31 = new Vector3(this._pidControllerx.ComputeOutput(vector3.x, boardTargetVel.x, Time.deltaTime), this._pidControllery.ComputeOutput(vector3.y, boardTargetVel.y, Time.deltaTime), this._pidControllerz.ComputeOutput(vector3.z, boardTargetVel.z, Time.deltaTime));
		if (Mathd.Vector3IsInfinityOrNan(vector31))
		{
			Debug.LogError("nan found in PID");
			return;
		}
		this.boardRigidbody.AddForce(vector31, ForceMode.Acceleration);
	}

	private void PIDRotation(Quaternion p_targetRot)
	{
		this._pidRotController.Kp = this.KRp;
		this._pidRotController.Ki = this.KRi;
		this._pidRotController.Kd = this.KRd;
		Vector3 vector3 = this._pidRotController.ComputeRequiredAngularAcceleration(this.boardRigidbody.transform.rotation, p_targetRot, this.boardRigidbody.angularVelocity, Time.deltaTime);
		Debug.DrawRay(base.transform.position, this.currentRotationTarget * Vector3.forward, Color.yellow);
		Debug.DrawRay(base.transform.position, this.boardRigidbody.transform.rotation * Vector3.forward, Color.green);
		this.boardRigidbody.AddTorque(vector3, ForceMode.Acceleration);
	}

	private void PredictCollision()
	{
		RaycastHit raycastHit;
		if (this.boardRigidbody.SweepTest(this.boardRigidbody.velocity.normalized, out raycastHit, (this.boardRigidbody.position - this._lastPos).magnitude * 2.5f))
		{
			PlayerController.Instance.playerSM.OnPredictedCollisionEventSM();
		}
	}

	private void ProcessSounds()
	{
		if (this._grounded)
		{
			this._rollSoundSpeed = this.boardRigidbody.velocity.magnitude;
			SoundManager.Instance.SetRollingVolumeFromRPS(this.boardTransform.GetComponent<PhysicMaterial>(), this._rollSoundSpeed);
			if (!this._bearingSoundSet && this._allDown)
			{
				SoundManager.Instance.StopBearingSound();
				this._bearingSoundSet = true;
			}
		}
		if (!this._allDown)
		{
			this._rollSoundSpeed = Mathf.Lerp(this._rollSoundSpeed, 0f, Time.deltaTime * 10f);
			SoundManager.Instance.SetRollingVolumeFromRPS(this.boardTransform.GetComponent<PhysicMaterial>(), this._rollSoundSpeed);
			if (this._bearingSoundSet)
			{
				if (this._grounded)
				{
					this._rollSoundSpeed *= 0.5f;
				}
				SoundManager.Instance.StartBearingSound(this._rollSoundSpeed);
				this._bearingSoundSet = false;
			}
		}
		if (!this.Grounded)
		{
			this._lastInAirVelocity = this.boardRigidbody.velocity;
		}
	}

	public void ReduceImpactBounce()
	{
		if (this.removingTorque)
		{
			this.LimitAngularVelocity(0f);
		}
		else
		{
			this.LimitAngularVelocity(5f);
		}
		bool grounded = this.Grounded;
	}

	public void ReferenceBoardRotation()
	{
		this._bufferedFlip = 0f;
	}

	public void RemoveTurnTorque(float p_value)
	{
		this.TurnTarget = 0f;
		Vector3 vector3 = this.boardTransform.InverseTransformDirection(this.boardRigidbody.angularVelocity);
		ref float pValue = ref vector3.y;
		pValue = pValue * (p_value * Time.deltaTime * 60f);
		this.boardRigidbody.angularVelocity = this.boardTransform.TransformDirection(vector3);
	}

	public void RemoveTurnTorqueLinear()
	{
		this.TurnTarget = 0f;
		this.removingTorque = true;
		Vector3 vector3 = this.boardTransform.InverseTransformDirection(this.boardRigidbody.angularVelocity);
		vector3.y = Mathf.MoveTowards(vector3.y, 0f, Time.deltaTime * 80f);
		this.boardRigidbody.angularVelocity = this.boardTransform.TransformDirection(vector3);
	}

	public void ResetAll()
	{
		this.firstVel = 0f;
		this.secondVel = 0f;
		this.thirdVel = 0f;
		this._firstDelta = 0f;
		this._secondDelta = 0f;
		this._thirdDelta = 0f;
		this._rotDeltaThisFrame = Quaternion.identity;
		this._bufferedFlip = 0f;
		this._bufferedBodyRot = 0f;
		this._lastuplift = 0f;
	}

	public void ResetPIDRotationValues()
	{
		this.KRp = this._originalKRp;
		this.KRd = this._originalKRd;
		this.KRi = this._originalKRi;
	}

	public void ResetTweakValues()
	{
		this._targetPosition = 0.5f;
		this._frontPivotLerp = 0.5f;
		this._backPivotLerp = 0.5f;
		this._sidePivotLerp = 0.5f;
		this._forwardPivotLerp = 0.5f;
		this._frontPivot.rotation = Quaternion.Slerp(this._leftPivotRotationTarget.rotation, this._rightPivotRotationTarget.rotation, this._frontPivotLerp);
		this._backPivot.rotation = Quaternion.Slerp(this._leftPivotRotationTarget.rotation, this._rightPivotRotationTarget.rotation, this._backPivotLerp);
		this._sidePivotRotation.rotation = Quaternion.Slerp(this._leftPivotRotationTarget.rotation, this._rightPivotRotationTarget.rotation, this._sidePivotLerp);
		this._boardTargetRotation.rotation = Quaternion.Slerp(this._backwardPivotRotationTarget.rotation, this._forwardPivotRotationTarget.rotation, this._forwardPivotLerp);
	}

	public void Rotate(bool doPop, bool doFlip)
	{
		this._firstDelta = this.firstVel * 500f * Time.deltaTime;
		this._secondDelta = this.secondVel * 20f * Time.deltaTime;
		this._thirdDelta = this.thirdVel * 20f * Time.deltaTime;
		if (SettingsManager.Instance.stance == SettingsManager.Stance.Regular)
		{
			this._secondDelta = -this._secondDelta;
		}
		this._firstDelta = Mathf.Clamp(this._firstDelta, -5f, 5f);
		this._secondDelta = Mathf.Clamp(this._secondDelta, -6f, 6f);
		this._thirdDelta = Mathf.Clamp((doFlip ? this._thirdDelta : this._thirdDelta * 0f), -9f, 9f);
		this._rotDeltaThisFrame = Quaternion.Euler(this._firstDelta, -this._secondDelta, 0f);
		this._bufferedRotation *= this._rotDeltaThisFrame;
		Vector3 vector3 = Mathd.LocalAngularVelocity(PlayerController.Instance.skaterController.skaterRigidbody);
		Quaternion quaternion = Quaternion.AngleAxis(57.29578f * vector3.y * Time.deltaTime, PlayerController.Instance.skaterController.skaterTransform.up);
		this._bufferedRotation *= quaternion;
		this.boardTransform.rotation = this._bufferedRotation;
		this._bufferedFlip += this._thirdDelta;
		this.boardTransform.Rotate(new Vector3(0f, 0f, this._bufferedFlip));
		if (doPop)
		{
			float single = this.popBoardVelAdd * Mathf.Abs(Mathf.Atan(0.0174532924f * this._firstDelta)) - this._lastuplift;
			this.boardRigidbody.AddForce(Vector3.up * single, ForceMode.VelocityChange);
		}
	}

	public void RotateWithPop(float _popDir, bool doFlip)
	{
		float single = _popDir * this.popSpeed;
		this.firstVel = this.firstVel + single;
	}

	public void SetBackPivotRotation(float p_frontToeAxis)
	{
		p_frontToeAxis *= 0.25f;
		p_frontToeAxis += 0.5f;
		this._backPivotLerp = Mathf.Lerp(this._backPivotLerp, p_frontToeAxis, Time.fixedDeltaTime * 20f);
		this._backPivot.rotation = Quaternion.Slerp(this._leftPivotRotationTarget.rotation, this._rightPivotRotationTarget.rotation, this._backPivotLerp);
	}

	public void SetBoardBackwards()
	{
		if (Vector3.Angle(this.boardTransform.forward, PlayerController.Instance.PlayerForward()) < 90f)
		{
			this.IsBoardBackwards = false;
			return;
		}
		this.IsBoardBackwards = true;
	}

	private void SetBoardControlPosition()
	{
		this.boardControlTransform.position = PlayerController.Instance.skaterController.animBoardTargetTransform.position;
	}

	public void SetBoardTargetPosition(float p_frontMagnitudeMinusBackMagnitude)
	{
		p_frontMagnitudeMinusBackMagnitude = Mathf.Clamp(p_frontMagnitudeMinusBackMagnitude, -1f, 1f);
		this._targetPosition = p_frontMagnitudeMinusBackMagnitude;
		this._targetPosition *= 0.5f;
		this._targetPosition += 0.5f;
		this._targetPosition = Mathf.Clamp(this._targetPosition, 0f, 1f);
		this.boardTargetPosition.position = Vector3.Lerp(this._frontPivotCenter.position, this._backPivotCenter.position, this._targetPosition);
	}

	public void SetCatchForwardRotation()
	{
		this._catchForwardRotation.rotation = this.boardRigidbody.rotation;
	}

	public void SetFrontPivotRotation(float p_backToeAxis)
	{
		p_backToeAxis *= -0.25f;
		p_backToeAxis += 0.5f;
		this._frontPivotLerp = Mathf.Lerp(this._frontPivotLerp, p_backToeAxis, Time.fixedDeltaTime * 20f);
		this._frontPivot.rotation = Quaternion.Slerp(this._leftPivotRotationTarget.rotation, this._rightPivotRotationTarget.rotation, this._frontPivotLerp);
	}

	public void SetGrindPIDRotationValues()
	{
		this.KRp = this.grindKRp;
		this.KRd = this.grindKRd;
		this.KRi = this.grindKRi;
	}

	public void SetManualAngularVelocity(bool p_manual, float p_manualAxis, float p_secondaryAxis, float p_swivel)
	{
		float single = (Mathf.Abs(p_manualAxis) - 0.5f) * 15f + p_secondaryAxis * 10f;
		Vector3 vector3 = (!this.IsBoardBackwards ? this.boardTransform.forward : -this.boardTransform.forward);
		vector3 = Vector3.ProjectOnPlane(vector3, this.LerpedGroundNormal);
		Vector3 vector31 = Vector3.Cross(vector3, this.LerpedGroundNormal);
		Vector3 vector32 = Quaternion.AngleAxis(15f + single, (p_manual ? vector31 : -vector31)) * this.LerpedGroundNormal;
		Vector3 vector33 = Quaternion.AngleAxis(15f + single, (p_manual ? vector31 : -vector31)) * vector3;
		Vector3 vector34 = Quaternion.AngleAxis(15f + single, (p_manual ? vector31 : -vector31)) * -vector3;
		Quaternion quaternion = (!this.IsBoardBackwards ? Quaternion.LookRotation(vector33, vector32) : Quaternion.LookRotation(vector34, vector32));
		Mathd.DampXTorqueTowards(this.boardRigidbody, this.boardRigidbody.rotation, quaternion, this.manualForce, this.manualDamp);
	}

	public void SetPivotForwardRotation(float p_leftForwardAxisPlusRightForwardAxis, float p_speed)
	{
		p_leftForwardAxisPlusRightForwardAxis *= 0.25f;
		p_leftForwardAxisPlusRightForwardAxis += 0.5f;
		this._forwardPivotLerp = Mathf.Lerp(this._forwardPivotLerp, p_leftForwardAxisPlusRightForwardAxis, Time.fixedDeltaTime * p_speed);
		this._boardTargetRotation.rotation = Quaternion.Slerp(this._backwardPivotRotationTarget.rotation, this._forwardPivotRotationTarget.rotation, this._forwardPivotLerp);
	}

	public void SetPivotSideRotation(float p_leftToeAxisMinusRightToeAxis)
	{
		p_leftToeAxisMinusRightToeAxis *= 0.25f;
		p_leftToeAxisMinusRightToeAxis += 0.5f;
		this._sidePivotLerp = Mathf.Lerp(this._sidePivotLerp, p_leftToeAxisMinusRightToeAxis, Time.fixedDeltaTime * 20f);
		this._sidePivotRotation.rotation = Quaternion.Slerp(this._leftPivotRotationTarget.rotation, this._rightPivotRotationTarget.rotation, this._sidePivotLerp);
	}

	private void SetRotationTarget()
	{
		this.currentRotationTarget = (!this.IsBoardBackwards ? this._boardTargetRotation.rotation : this._backwardsTargetRotation.rotation);
	}

	private void SnapPosition()
	{
		this.PIDPosition();
	}

	public void SnapRotation()
	{
		this.PIDRotation(this.currentRotationTarget);
	}

	public void SnapRotation(bool p_notMovingSticks)
	{
		if (p_notMovingSticks)
		{
			this.AutoCatchRotation();
			return;
		}
		this.PIDRotation(this.currentRotationTarget);
	}

	public void SnapRotation(float p_mag)
	{
		this.CatchRotation(p_mag);
	}

	private void Update()
	{
		this.ProcessSounds();
	}

	public void UpdateBoardPosition()
	{
		if (PlayerController.Instance.movementMaster != PlayerController.MovementMaster.Board)
		{
			this.PredictCollision();
			this.SnapPosition();
			return;
		}
		if (!this.AllDown)
		{
			this.PredictCollision();
		}
	}

	public void UpdateReferenceBoardTargetRotation()
	{
		this._bufferedRotation = this.boardTransform.rotation;
		Vector3 vector3 = this._bufferedRotation * Vector3.forward;
		this._bufferedRotation.SetLookRotation(vector3, this.boardUpAtPopBegin);
	}
}