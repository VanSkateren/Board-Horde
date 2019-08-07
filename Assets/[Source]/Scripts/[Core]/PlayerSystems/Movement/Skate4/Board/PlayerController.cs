using Dreamteck.Splines;
using FSMHelper;
using RootMotion.Dynamics;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
	private static PlayerController _instance;

	public string currentState = "";

	public Respawn respawn;

	public Transform CenterOfMass;

	public float popForce;

	public float horizontalSpeed = 200f;

	public float verticalSpeed = 100f;

	public float flipStickDeadZone;

	public float scoopFlipWindowNoFlipDetected;

	public float olliexVelThreshold;

	public float flipThreshold;

	public float popThreshold;

	public float shuvMult;

	public float doubleShuvMult;

	public float shuvFlip;

	public float shuvMax;

	public float flipMult;

	public float invertMult;

	public float boneMult;

	public float kickForce;

	public float topSpeed;

	public float spinDeccelerate;

	public COMController comController;

	public CameraController cameraController;

	public SkaterController skaterController;

	public BoardController boardController;

	public AnimationController animationController;

	public InputController inputController;

	public IKController ikController;

	public Transform playerRotationReference;

	public Transform boardOffsetRoot;

	public Transform playerOffsetRoot;

	[SerializeField]
	private CoMDisplacement _comDisplacement;

	public PlayerController.MovementMaster movementMaster;

	public PlayerStateMachine playerSM;

	private Vector3 _ridingCameraForward = Vector3.zero;

	private Vector3 _ridingPlayerForward = Vector3.zero;

	private Vector3 _lastRidingPosition = Vector3.zero;

	private bool _isRespawning;

	private Quaternion _playerGrindRotation = Quaternion.identity;

	private Quaternion _boardGrindRotation = Quaternion.identity;

	private bool _manualling;

	private Vector3 _velocityOnPop = Vector3.zero;

	private bool _isInSetupState;

	private Vector2 _flick = Vector2.zero;

	private float _flickSpeed;

	private float _stickAngle;

	public bool popped;

	private float _turnAnimAmount;

	private float _turnVel;

	public float turnAnimSpring;

	public float turnAnimDamp;

	public float torsoTorqueMult;

	private float _frontFootForwardAxis;

	private float _backFootForwardAxis;

	private float _frontFootToeAxis;

	private float _backFootToeAxis;

	private Vector2 _frontMagnitude = Vector2.zero;

	private Vector2 _backMagnitude = Vector2.zero;

	private float _forwardTweakAxis;

	private float _toeTweakAxis;

	private float _boardAngleToGround;

	private float _flipAxis;

	private float _flipAxisTarget;

	public float impactBoardDownForce;

	private Quaternion newRot = Quaternion.identity;

	public Image leftRawVector;

	public Image rightRawVector;

	public Image leftAugmentedVector;

	public Image rightAugmentedVector;

	private Vector3 _leftRawRot;

	private Vector3 _rightRawRot;

	private Vector3 _leftAugRot;

	private Vector3 _rightAugRot;

	private Vector3 _leftRawScale;

	private Vector3 _rightRawScale;

	private Vector3 _leftAugScale;

	private Vector3 _rightAugScale;

	private Vector2 _tempAugmentedUp;

	public Quaternion BoardGrindRotation
	{
		get => this._boardGrindRotation;
		set => this._boardGrindRotation = value;
	}

	public static PlayerController Instance => PlayerController._instance;

	public bool IsAnimSwitch
	{
		get
		{
			if (Vector3.Angle(this.cameraController._actualCam.forward, this.skaterController.skaterTransform.forward) > 90f)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsInSetupState => this._isInSetupState;

	public bool IsRespawning
	{
		get => this._isRespawning;
		set => this._isRespawning = value;
	}

	public bool IsSwitch
	{
		get
		{
			if (Vector3.Angle(this._ridingCameraForward, this._ridingPlayerForward) > 90f)
			{
				return true;
			}
			return false;
		}
	}

	public bool Manualling
	{
		get => this._manualling;
		set => this._manualling = value;
	}

	public Quaternion PlayerGrindRotation
	{
		get => this._playerGrindRotation;
		set => this._playerGrindRotation = value;
	}

	public Vector3 VelocityOnPop
	{
		get => this._velocityOnPop;
		set => this._velocityOnPop = value;
	}

	public PlayerController()
	{
	}

	public void AddForceAtPosition(Rigidbody p_rb, Vector3 p_position, Vector3 p_direction, float p_force, ForceMode p_forceMode)
	{
		Debug.DrawLine(p_position, p_position + ((p_direction * p_force) * Time.fixedDeltaTime), Color.red, 0.5f);
		p_rb.AddForceAtPosition((p_direction * p_force) * Time.fixedDeltaTime, p_position, p_forceMode);
	}

	public void AddForwardSpeed(float p_value)
	{
		BoardController boardController = this.boardController;
		boardController.firstVel = boardController.firstVel + (this.boardController.IsBoardBackwards ? -p_value : p_value);
	}

	public void AddPushForce(float p_value)
	{
		this.boardController.AddPushForce(p_value);
	}

	public void AddSidwaysGrindVelocity(Rigidbody p_rb, Vector3 p_up, Vector3 p_direction, float p_force)
	{
		if (p_direction != Vector3.zero && p_direction.x != Single.NaN && p_direction.y != Single.NaN && p_direction.z != Single.NaN)
		{
			Rigidbody pRb = p_rb;
			pRb.velocity = pRb.velocity + ((Vector3.ProjectOnPlane(p_direction.normalized, p_up) * Time.fixedDeltaTime) * p_force);
		}
	}

	public void AddToScoopSpeed(float p_value)
	{
		Debug.LogError(string.Concat("add tp ", p_value));
		this.boardController.secondVel += p_value;
	}

	public void AddUpwardDisplacement(float p_timeStep)
	{
		this.skaterController.AddUpwardDisplacement(p_timeStep);
	}

	public bool AngleMagnitudeGrindCheck(Vector3 p_velocity, SplineResult p_splineResult)
	{
		float single = Vector3.Angle(Vector3.ProjectOnPlane(PlayerController.Instance.boardController.boardRigidbody.velocity, p_splineResult.normal), p_splineResult.direction);
		if (single < 60f)
		{
			return true;
		}
		if (single > 110f)
		{
			return true;
		}
		return false;
	}

	public float AngleToBoardTargetRotation()
	{
		return Quaternion.Angle(this.boardController.boardRigidbody.rotation, this.boardController.currentRotationTarget);
	}

	public void AnimCaught(bool p_value)
	{
		this.animationController.SetValue("Caught", p_value);
	}

	public void AnimEndImpactEarly(bool p_value)
	{
		this.animationController.SetValue("EndImpact", p_value);
	}

	public void AnimForceFlipValue(float p_value)
	{
		this._flipAxisTarget = p_value;
		this._flipAxis = p_value;
		this.animationController.SetValue("FlipAxis", p_value);
	}

	public void AnimForceScoopValue(float p_value)
	{
		this.animationController.SetValue("ScoopAxis", p_value);
	}

	public float AnimGetManualAxis()
	{
		return this.animationController.skaterAnim.GetFloat("ManualAxis");
	}

	public void AnimGrindTransition(bool p_value)
	{
		this.animationController.SetValue("Grind", p_value);
	}

	public void AnimLandedEarly(bool p_value)
	{
		this.animationController.SetValue("LandedEarly", p_value);
	}

	public void AnimOllieTransition(bool p_value)
	{
		this.animationController.SetValue("Ollie", p_value);
	}

	public void AnimPopInterruptedTransitions(bool p_value)
	{
		this.animationController.SetValue("PopInterrupted", p_value);
	}

	public void AnimRelease(bool p_value)
	{
		this.animationController.SetValue("Released", p_value);
	}

	public void AnimSetAllDown(bool p_value)
	{
		this.animationController.SetValue("AllDown", p_value);
	}

	public void AnimSetBraking(bool p_value)
	{
		this.animationController.SetValue("Braking", p_value);
	}

	public void AnimSetCatchAngle(float p_value)
	{
		this.animationController.SetValue("CatchAngle", p_value);
	}

	public void AnimSetFlip(float p_value)
	{
		this._flipAxisTarget = p_value;
	}

	public void AnimSetGrindBlend(float p_x, float p_y)
	{
		this.animationController.SetValue("GrindX", p_x);
		this.animationController.SetValue("GrindY", p_y);
	}

	public void AnimSetGrinding(bool p_value)
	{
		this.animationController.SetValue("Grinding", p_value);
	}

	public void AnimSetInAirTurn(float p_value)
	{
		this.animationController.SetValue("InAirTurn", p_value);
	}

	public void AnimSetManual(bool p_value, float p_manualAxis)
	{
		float single;
		if (!p_value)
		{
			this.AnimSetManualAxis(p_manualAxis);
			this.animationController.SetValue("Manual", p_value);
			this.animationController.SetValue("NoseManual", p_value);
		}
		else
		{
			this._boardAngleToGround = Vector3.Angle(PlayerController.Instance.boardController.boardTransform.up, PlayerController.Instance.GetGroundNormal());
			if (p_manualAxis > 0.1f)
			{
				single = this._boardAngleToGround;
			}
			else
			{
				single = (p_manualAxis < -0.1f ? -this._boardAngleToGround : this._boardAngleToGround);
			}
			this._boardAngleToGround = single;
			this._boardAngleToGround = Mathf.Clamp(this._boardAngleToGround, -30f, 30f);
			this._boardAngleToGround /= 30f;
			this.AnimSetManualAxis(this._boardAngleToGround);
			this.animationController.SetValue("Manual", p_value);
		}
		this.Manualling = p_value;
	}

	public void AnimSetManualAxis(float p_value)
	{
		this.animationController.SetValue("ManualAxis", p_value);
	}

	public void AnimSetMongo(bool p_value)
	{
		this.animationController.SetValue("PushMongo", p_value);
	}

	public void AnimSetNoComply(bool p_value)
	{
		this.animationController.SetValue("NoComply", p_value);
	}

	public void AnimSetNollie(float p_value)
	{
		this.animationController.SetValue("Nollie", p_value);
		this.animationController.SetNollieSteezeIK(p_value);
	}

	public void AnimSetNoseManual(bool p_value, float p_manualAxis)
	{
		float single;
		if (!p_value)
		{
			this.AnimSetManualAxis(p_manualAxis);
			this.animationController.SetValue("Manual", p_value);
			this.animationController.SetValue("NoseManual", p_value);
		}
		else
		{
			this._boardAngleToGround = Vector3.Angle(PlayerController.Instance.boardController.boardTransform.up, PlayerController.Instance.GetGroundNormal());
			if (p_manualAxis > 0.1f)
			{
				single = this._boardAngleToGround;
			}
			else
			{
				single = (p_manualAxis < -0.1f ? -this._boardAngleToGround : this._boardAngleToGround);
			}
			this._boardAngleToGround = single;
			this._boardAngleToGround = Mathf.Clamp(this._boardAngleToGround, -30f, 30f);
			this._boardAngleToGround /= 30f;
			this.AnimSetManualAxis(this._boardAngleToGround);
			this.animationController.SetValue("Manual", p_value);
		}
		this.Manualling = p_value;
	}

	public void AnimSetPopStrength(float p_value)
	{
		this.animationController.SetValue("PopStrength", p_value);
	}

	public void AnimSetPush(bool p_value)
	{
		this.animationController.SetValue("Push", p_value);
	}

	public void AnimSetRollOff(bool p_value)
	{
		this.animationController.SetValue("RollOff", p_value);
	}

	public void AnimSetScoop(float p_value)
	{
		this.animationController.SetValue("ScoopAxis", p_value);
	}

	public void AnimSetSetupBlend(float p_value)
	{
		this.animationController.SetValue("SetupBlend", p_value);
	}

	public void AnimSetSwitch(float p_animSwitch)
	{
		this.animationController.SetValue("Switch", p_animSwitch);
	}

	public void AnimSetTurn(float p_value)
	{
		float single = Mathd.DampSpring((float)((this.GetBoardBackwards() ? 1 : -1)) * PlayerController.Instance.boardController.localXAccel / 5.5f - this._turnAnimAmount, this._turnVel, this.turnAnimSpring, this.turnAnimDamp);
		this._turnVel += single;
		this._turnAnimAmount += this._turnVel;
		this.animationController.SetValue("Turn", (this.IsAnimSwitch ? -this._turnAnimAmount : this._turnAnimAmount));
	}

	public void AnimSetupTransition(bool p_value)
	{
		this.animationController.SetValue("Setup", p_value);
	}

	public void AnimSetWindUp(float p_value)
	{
		this.animationController.SetValue("WindUp", p_value);
	}

	public void ApplyWeightOnBoard()
	{
		if (this.boardController.TurnTarget == 0f)
		{
			Vector3 vector3 = this.boardController.boardRigidbody.velocity;
			this.boardController.boardRigidbody.AddForce((-this.skaterController.skaterTransform.up * 80f) * this.impactBoardDownForce, ForceMode.Force);
			if (this.boardController.boardRigidbody.velocity.magnitude < vector3.magnitude)
			{
				Rigidbody rigidbody = this.boardController.boardRigidbody;
				Vector3 vector31 = this.boardController.boardRigidbody.velocity;
				rigidbody.velocity = vector31.normalized * vector3.magnitude;
			}
		}
	}

	private void Awake()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		if (!(PlayerController._instance != null) || !(PlayerController._instance != this))
		{
			PlayerController._instance = this;
			return;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public Vector3 BoardToTargetVector()
	{
		return this.boardController.boardTransform.position - this.boardController.boardTargetPosition.position;
	}

	public void Brake(float p_force)
	{
		Vector3 vector3 = this.boardController.boardRigidbody.velocity;
		this.boardController.boardRigidbody.velocity = vector3.normalized * (vector3.magnitude - p_force * Time.deltaTime);
	}

	public void CacheRidingTransforms()
	{
		this._lastRidingPosition = this.skaterController.skaterTransform.position;
		this._ridingCameraForward = this.cameraController._actualCam.forward;
		this._ridingPlayerForward = this.skaterController.skaterTransform.forward;
	}

	public void CameraLookAtPlayer()
	{
		this.cameraController.LookAtPlayer();
	}

	public void CancelRespawnInvoke()
	{
		base.CancelInvoke("DoBail");
	}

	private void CanManual()
	{
		this.playerSM.OnCanManualSM();
	}

	public bool CanNollieOutOfGrind()
	{
		return this.boardController.triggerManager.canNollie;
	}

	public bool CanOllieOutOfGrind()
	{
		return this.boardController.triggerManager.canOllie;
	}

	public void CorrectVelocity()
	{
		this.skaterController.CorrectVelocity();
	}

	public void CrossFadeAnimation(string p_animName, float p_transitionDuration)
	{
		this.animationController.CrossFadeAnimation(p_animName, p_transitionDuration);
	}

	public void DebugAugmentedAngles(Vector2 p_pos, bool p_isRight)
	{
		Quaternion quaternion;
		if (p_isRight)
		{
			this._rightAugScale = this.rightAugmentedVector.rectTransform.localScale;
			this._rightAugScale.y = Mathf.Clamp(p_pos.magnitude, 0f, 1f);
			this.rightAugmentedVector.rectTransform.localScale = this._rightAugScale;
			quaternion = this.rightAugmentedVector.rectTransform.rotation;
			this._rightAugRot = quaternion.eulerAngles;
			this._rightAugRot.z = Vector2.SignedAngle(-Vector2.up, p_pos);
			this.rightAugmentedVector.rectTransform.rotation = Quaternion.Euler(this._rightAugRot);
			return;
		}
		this._leftAugScale = this.leftAugmentedVector.rectTransform.localScale;
		this._leftAugScale.y = Mathf.Clamp(p_pos.magnitude, 0f, 1f);
		this.leftAugmentedVector.rectTransform.localScale = this._leftAugScale;
		quaternion = this.leftAugmentedVector.rectTransform.rotation;
		this._leftAugRot = quaternion.eulerAngles;
		this._leftAugRot.z = Vector2.SignedAngle(-Vector2.up, p_pos);
		this.leftAugmentedVector.rectTransform.rotation = Quaternion.Euler(this._leftAugRot);
	}

	public void DebugPopStick(bool p_canPop, bool p_isRight)
	{
		if (p_isRight)
		{
			this.rightRawVector.color = (p_canPop ? Color.red : Color.green);
			return;
		}
		this.leftRawVector.color = (p_canPop ? Color.red : Color.green);
	}

	public void DebugRawAngles(Vector2 p_pos, bool p_isRight)
	{
		Quaternion quaternion;
		if (p_isRight)
		{
			this._rightRawScale = this.rightRawVector.rectTransform.localScale;
			this._rightRawScale.y = Mathf.Clamp(p_pos.magnitude, 0f, 1f);
			this.rightRawVector.rectTransform.localScale = this._rightRawScale;
			quaternion = this.rightRawVector.rectTransform.rotation;
			this._rightRawRot = quaternion.eulerAngles;
			this._rightRawRot.z = Vector2.SignedAngle(-Vector2.up, p_pos);
			this.rightRawVector.rectTransform.rotation = Quaternion.Euler(this._rightRawRot);
			return;
		}
		this._leftRawScale = this.leftRawVector.rectTransform.localScale;
		this._leftRawScale.y = Mathf.Clamp(p_pos.magnitude, 0f, 1f);
		this.leftRawVector.rectTransform.localScale = this._leftRawScale;
		quaternion = this.leftRawVector.rectTransform.rotation;
		this._leftRawRot = quaternion.eulerAngles;
		this._leftRawRot.z = Vector2.SignedAngle(-Vector2.up, p_pos);
		this.leftRawVector.rectTransform.rotation = Quaternion.Euler(this._leftRawRot);
	}

	public float DistanceToBoardTarget()
	{
		return Vector3.Distance(this.boardController.boardTransform.position, this.boardController.boardTargetPosition.position);
	}

	public void DoBail()
	{
		this.respawn.DoRespawn();
	}

	public void DoBailDelay()
	{
		base.Invoke("DoBail", 2.5f);
	}

	private void DoInvert(StickInput p_popStick, ref float r_invertVel, bool p_forwardLoad)
	{
		float pPopStick = this.invertMult * p_popStick.rawInput.radialVel;
		if (pPopStick > r_invertVel && pPopStick > 0.1f)
		{
			this.GetBoardBackwards();
			Debug.LogWarning("changed");
			Debug.Log(string.Concat(new object[] { "newraw:", p_popStick.rawInput.radialVel, " new?:", pPopStick, "   firstvel:", this.boardController.firstVel }));
		}
	}

	public void DoKick(bool p_forwardLoad, float strength)
	{
		float single = (float)((this.GetBoardBackwards() ? 1 : -1));
		float single1 = (float)((p_forwardLoad ? -1 : 1));
		this.boardController.RotateWithPop(single * single1 * strength, false);
	}

	private void FixedUpdate()
	{
		this.playerSM.FixedUpdateSM();
	}

	public void FixTargetNormal()
	{
		this.boardController.trajectory.PredictedGroundNormal = Vector3.up;
	}

	public void FlipTrickRotation()
	{
		this.boardController.Rotate(false, true);
	}

	public void ForceBail()
	{
		this.skaterController.respawn.bail.OnBailed();
		this.playerSM.OnBailedSM();
	}

	public void ForcePivotForwardRotation(float p_value)
	{
		this.boardController.ForcePivotForwardRotation(p_value);
	}

	public float GetAngleToAugment(Vector2 p_pos, bool p_isRight)
	{
		if (p_pos.magnitude < 0.1f)
		{
			return 0f;
		}
		return -Vector2.SignedAngle(this.GetUpVectorToAugment(p_isRight), p_pos);
	}

	public bool GetAnimReleased()
	{
		return this.animationController.skaterAnim.GetBool("Released");
	}

	public bool GetBoardBackwards()
	{
		return this.boardController.IsBoardBackwards;
	}

	public float GetBrakeForce()
	{
		return this.skaterController.breakForce;
	}

	public float GetCoMDisplacement(float p_timeStep)
	{
		return this._comDisplacement.GetDisplacement(p_timeStep);
	}

	public float GetDisplacementSum()
	{
		return this._comDisplacement.sum;
	}

	public float GetFlipSpeed()
	{
		return this.boardController.thirdVel;
	}

	public float GetForwardSpeed()
	{
		return this.boardController.firstVel;
	}

	public Vector3 GetGrindContactPosition()
	{
		return this.boardController.triggerManager.grindContactSplinePosition.position;
	}

	public Vector3 GetGrindDirection()
	{
		return this.boardController.triggerManager.grindDirection;
	}

	public Vector3 GetGrindRight()
	{
		return this.boardController.triggerManager.grindRight;
	}

	public Vector3 GetGrindUp()
	{
		return this.boardController.triggerManager.grindUp;
	}

	public Vector3 GetGroundNormal()
	{
		return this.boardController.GroundNormal;
	}

	public float GetKneeBendWeight()
	{
		return this.ikController.GetKneeBendWeight();
	}

	public float GetLastTweakAxis()
	{
		return this._toeTweakAxis / 2f;
	}

	public Vector3 GetLerpedGroundNormal()
	{
		return this.boardController.LerpedGroundNormal;
	}

	public float GetNollie(bool p_isRight)
	{
		switch (SettingsManager.Instance.controlType)
		{
			case SettingsManager.ControlType.Same:
			{
				if (SettingsManager.Instance.stance == SettingsManager.Stance.Regular)
				{
					if (p_isRight)
					{
						return 0f;
					}
					return 1f;
				}
				if (p_isRight)
				{
					return 1f;
				}
				return 0f;
			}
			case SettingsManager.ControlType.Swap:
			{
				if (SettingsManager.Instance.stance == SettingsManager.Stance.Regular)
				{
					if (!PlayerController.Instance.IsSwitch)
					{
						if (p_isRight)
						{
							return 0f;
						}
						return 1f;
					}
					if (p_isRight)
					{
						return 0f;
					}
					return 1f;
				}
				if (!PlayerController.Instance.IsSwitch)
				{
					if (p_isRight)
					{
						return 1f;
					}
					return 0f;
				}
				if (p_isRight)
				{
					return 1f;
				}
				return 0f;
			}
			case SettingsManager.ControlType.Simple:
			{
				if (!PlayerController.Instance.IsSwitch)
				{
					if (p_isRight)
					{
						return 0f;
					}
					return 1f;
				}
				if (p_isRight)
				{
					return 0f;
				}
				return 1f;
			}
		}
		return 0f;
	}

	public bool GetPopped()
	{
		return this.playerSM.PoppedSM();
	}

	public float GetPopStrength()
	{
		return this.animationController.skaterAnim.GetFloat("PopStrength");
	}

	public float GetPushForce()
	{
		return this.skaterController.pushForce;
	}

	public float GetScoopSpeed()
	{
		return this.boardController.secondVel;
	}

	public Vector2 GetUpVectorToAugment(bool p_isRight)
	{
		switch (SettingsManager.Instance.controlType)
		{
			case SettingsManager.ControlType.Same:
			{
				if (!p_isRight)
				{
					this._tempAugmentedUp = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? Vector2.up : -Vector2.up);
					break;
				}
				else
				{
					this._tempAugmentedUp = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -Vector2.up : Vector2.up);
					break;
				}
			}
			case SettingsManager.ControlType.Swap:
			{
				if (PlayerController.Instance.IsSwitch)
				{
					if (!p_isRight)
					{
						this._tempAugmentedUp = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? Vector2.up : -Vector2.up);
						break;
					}
					else
					{
						this._tempAugmentedUp = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -Vector2.up : Vector2.up);
						break;
					}
				}
				else if (!p_isRight)
				{
					this._tempAugmentedUp = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -Vector2.up : Vector2.up);
					break;
				}
				else
				{
					this._tempAugmentedUp = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? Vector2.up : -Vector2.up);
					break;
				}
			}
			case SettingsManager.ControlType.Simple:
			{
				if (PlayerController.Instance.IsSwitch)
				{
					if (!p_isRight)
					{
						this._tempAugmentedUp = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? Vector2.up : Vector2.up);
						break;
					}
					else
					{
						this._tempAugmentedUp = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -Vector2.up : -Vector2.up);
						break;
					}
				}
				else if (!p_isRight)
				{
					this._tempAugmentedUp = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? Vector2.up : Vector2.up);
					break;
				}
				else
				{
					this._tempAugmentedUp = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -Vector2.up : -Vector2.up);
					break;
				}
			}
		}
		return this._tempAugmentedUp;
	}

	public float GetWindUp()
	{
		return this.inputController.GetWindUp();
	}

	public void GrindRotateBoard(float p_horizontal, float p_vertical)
	{
		this.newRot = (Quaternion.AngleAxis(p_vertical, (!this.GetBoardBackwards() ? this.boardController.boardTransform.right : -this.boardController.boardTransform.right)) * Quaternion.AngleAxis(p_horizontal, this.skaterController.skaterTransform.up)) * this.skaterController.skaterTransform.rotation;
		this.boardOffsetRoot.rotation = this.newRot;
	}

	public void GrindRotatePlayerHorizontal(float p_value)
	{
		if (!Mathd.Vector3IsInfinityOrNan(this.GetGrindContactPosition()))
		{
			this.playerRotationReference.RotateAround(this.GetGrindContactPosition(), this.boardController.triggerManager.playerOffset.up, p_value);
		}
	}

	public void Impact()
	{
		this.SetBoardToMaster();
		PlayerController.Instance.SetTurningMode(InputController.TurningMode.Grounded);
		this.AnimOllieTransition(false);
		this.AnimSetupTransition(false);
	}

	public bool IsBacksideGrind()
	{
		bool flag = false;
		float single = Vector3.SignedAngle(Vector3.ProjectOnPlane(this.skaterController.skaterTransform.position - this._lastRidingPosition, this.GetGrindUp()), this.GetGrindDirection(), this.GetGrindUp());
		if (SettingsManager.Instance.stance == SettingsManager.Stance.Regular)
		{
			if (single >= 0f)
			{
				flag = (this.IsSwitch ? false : true);
			}
			else
			{
				flag = (!this.IsSwitch ? false : true);
			}
		}
		else if (single >= 0f)
		{
			flag = (!this.IsSwitch ? false : true);
		}
		else
		{
			flag = (this.IsSwitch ? false : true);
		}
		return flag;
	}

	public bool IsCurrentAnimationPlaying(string p_name)
	{
		AnimatorStateInfo currentAnimatorStateInfo = this.animationController.skaterAnim.GetCurrentAnimatorStateInfo(0);
		return currentAnimatorStateInfo.IsName(p_name);
	}

	public bool IsCurrentGrindMetal()
	{
		if (SoundManager.Instance.deckSounds.grindState != DeckSounds.GrindState.metal)
		{
			return false;
		}
		return true;
	}

	public bool IsGrounded()
	{
		return this.boardController.Grounded;
	}

	public bool IsRightSideOfGrind()
	{
		if (Vector3.SignedAngle(Vector3.ProjectOnPlane(this.skaterController.skaterTransform.position - this._lastRidingPosition, this.GetGrindUp()), this.GetGrindDirection(), this.GetGrindUp()) > 0f)
		{
			return true;
		}
		return false;
	}

	private void LerpFlipAxis()
	{
		this._flipAxis = Mathf.Lerp(this._flipAxis, this._flipAxisTarget, Time.deltaTime * 15f);
		this.animationController.SetValue("FlipAxis", this._flipAxis);
	}

	public void LimitAngularVelocity(float _maxY)
	{
		this.boardController.LimitAngularVelocity(_maxY);
	}

	public void LockAngularVelocity(Quaternion p_rot)
	{
		this.boardController.LockAngularVelocity(p_rot);
	}

	public void LogVelY()
	{
		Debug.LogError(string.Concat("xxxxxxxxxx p_pop aft1: ", this.skaterController.skaterRigidbody.velocity.y));
	}

	public void ManualRotation(bool p_manual, float p_manualAxis, float p_secondaryAxis, float p_swivel)
	{
		this.boardController.SetManualAngularVelocity(p_manual, p_manualAxis, p_secondaryAxis, p_swivel);
	}

	public void MoveCameraToPlayer()
	{
		this.cameraController.MoveCameraToPlayer();
	}

	private void OnDestroy()
	{
		if (this.playerSM != null)
		{
			this.playerSM.StopSM();
			this.playerSM = null;
		}
	}

	public void OnEndImpact()
	{
		this.playerSM.OnEndImpactSM();
	}

	public void OnEnterSetupState()
	{
		this._isInSetupState = true;
	}

	public void OnExitSetupState()
	{
		this._isInSetupState = false;
	}

	private void OnFlipStickReset()
	{
	}

	public void OnFlipStickUpdate(ref bool p_p_flipDetected, ref bool p_potentialFlip, ref Vector2 p_initialFlipDir, ref int p_p_flipFrameCount, ref int p_p_flipFrameMax, ref float p_toeAxis, ref float p_p_flipVel, ref float p_popVel, ref float p_popDir, ref float p_flip, StickInput p_flipStick, bool p_releaseBoard, bool p_isSettingUp, ref float p_invertVel, float p_augmentedAngle, bool popRotationDone, bool p_forwardLoad, ref float p_flipWindowTimer)
	{
		float single;
		float single1;
		if (p_p_flipDetected)
		{
			if (p_flip == 0f)
			{
				single = 0f;
			}
			else
			{
				single = (p_flip > 0f ? 1f : -1f);
			}
			this.AnimSetFlip(single);
			this.AnimRelease(true);
			if (this.playerSM.PoppedSM())
			{
				this.SetLeftIKLerpTarget(1f);
				this.SetRightIKLerpTarget(1f);
			}
			if (p_toeAxis == 0f)
			{
				single1 = 0f;
			}
			else
			{
				single1 = (p_toeAxis > 0f ? 1f : -1f);
			}
			float single2 = single1;
			float popToeVel = this.boneMult * p_flipStick.PopToeVel.y;
			float single3 = (float)((p_forwardLoad ? -1 : 1));
			float popToeSpeed = this.flipMult * p_flipStick.PopToeSpeed;
			if ((Mathf.Sign(p_flipStick.ToeAxisVel) == Mathf.Sign(p_flip) || !this.playerSM.PoppedSM()) && Mathf.Abs(popToeSpeed) > Mathf.Abs(p_p_flipVel))
			{
				p_p_flipVel = popToeSpeed;
				p_flipWindowTimer = 0f;
			}
			if (Mathf.Abs(p_invertVel) == 0f || Mathf.Sign(popToeVel) == Mathf.Sign(1f) && Mathf.Abs(popToeVel) > Mathf.Abs(p_invertVel))
			{
				p_invertVel = single3 * popToeVel;
			}
			if (0 == 0 && !this.playerSM.PoppedSM())
			{
				p_flipWindowTimer += Time.deltaTime;
				if (p_flipWindowTimer >= 0.3f)
				{
					p_p_flipVel = 0f;
					p_invertVel = 0f;
					p_p_flipDetected = false;
					this.AnimRelease(false);
					this.AnimSetFlip(0f);
					this.AnimForceFlipValue(0f);
					p_flipWindowTimer = 0f;
				}
			}
			this.SetFlipSpeed(Mathf.Clamp(p_p_flipVel, -4000f, 4000f) * single2);
		}
		else
		{
			float popToeSpeed1 = this.flipMult * p_flipStick.PopToeSpeed;
			if (popToeSpeed1 > this.flipThreshold)
			{
				float single4 = Vector3.Angle(p_flipStick.PopToeVector, Vector2.up);
				if (single4 < 150f && single4 > 15f && p_flipStick.PopToeVector.magnitude > this.flipStickDeadZone && Vector2.Angle(p_flipStick.PopToeVel, p_flipStick.PopToeVector - Vector2.zero) < 90f)
				{
					p_initialFlipDir = p_flipStick.PopToeVector;
					p_toeAxis = p_flipStick.FlipDir;
					p_flip = p_flipStick.ToeAxis;
					float forwardDir = p_flipStick.ForwardDir;
					if (forwardDir <= 0.2f)
					{
						forwardDir += 0.2f;
					}
					p_popDir = Mathf.Clamp(forwardDir, 0f, 1f);
					p_p_flipVel = popToeSpeed1;
					p_flipWindowTimer = 0f;
					p_p_flipDetected = true;
					this.playerSM.PoppedSM();
					return;
				}
			}
		}
	}

	public void OnImpact()
	{
		this.boardController.boardRigidbody.angularVelocity = Vector3.zero;
		this.boardController.backTruckRigidbody.angularVelocity = Vector3.zero;
		this.boardController.frontTruckRigidbody.angularVelocity = Vector3.zero;
	}

	public void OnInAir(float p_animTimeToImpact)
	{
		Vector3 vector3 = Vector3.zero;
		float single = this.boardController.trajectory.CalculateTrajectory(this.skaterController.skaterTransform.position - (Vector3.up * 0.9765f), this.skaterController.skaterRigidbody.velocity, 50f, out vector3);
		float single1 = Mathf.Clamp(p_animTimeToImpact / single, 0.01f, 1f);
		this.animationController.ScaleAnimSpeed(single1);
	}

	public void OnManualEnter(bool rightFirst)
	{
		if (rightFirst)
		{
			this.playerSM.OnManualEnterSM(this.inputController.RightStick, this.inputController.LeftStick);
			return;
		}
		this.playerSM.OnManualEnterSM(this.inputController.LeftStick, this.inputController.RightStick);
	}

	public void OnManualUpdate(StickInput stick)
	{
		bool flag;
		if (stick.HoldingManual)
		{
			this.UpdateManual(stick.IsRightStick);
			if ((stick.rawInput.pos.magnitude <= 0.9f || stick.rawInput.avgSpeedLastUpdate >= 2f) && (stick.ManualAxis >= 0.1f && stick.ManualAxis <= 0.95f || stick.rawInput.avgSpeedLastUpdate >= 10f))
			{
				stick.ManualFrameCount = 0;
			}
			else
			{
				StickInput manualFrameCount = stick;
				manualFrameCount.ManualFrameCount = manualFrameCount.ManualFrameCount + 1;
				if (stick.ManualFrameCount >= 7)
				{
					this.playerSM.OnManualExitSM();
					this.AnimSetManual(false, Mathf.Lerp(this.AnimGetManualAxis(), 0f, Time.deltaTime * 10f));
					stick.HoldingManual = false;
					stick.ManualFrameCount = 0;
					return;
				}
			}
		}
		else
		{
			if (this.boardController.Grounded)
			{
				flag = (Mathf.Abs(stick.rawInput.pos.x) >= 0.6f || stick.ManualAxis <= 0.2f || stick.ManualAxis >= 0.85f ? false : stick.rawInput.avgSpeedLastUpdate < 7f);
			}
			else
			{
				flag = (Mathf.Abs(stick.rawInput.pos.x) >= 0.1f || stick.rawInput.pos.magnitude >= 0.85f || stick.ManualAxis <= 0.4f || stick.ManualAxis >= 0.95f ? false : stick.rawInput.avgSpeedLastUpdate < 2f);
			}
			if (!flag)
			{
				stick.ManualFrameCount = 0;
				return;
			}
			StickInput stickInput = stick;
			stickInput.ManualFrameCount = stickInput.ManualFrameCount + 1;
			if (stick.ManualFrameCount >= (this.boardController.Grounded ? 16 : 2))
			{
				this.OnManualEnter(stick.IsRightStick);
				stick.HoldingManual = true;
				stick.ManualFrameCount = 0;
				return;
			}
		}
	}

	public void OnNextState()
	{
		this.playerSM.OnNextStateSM();
	}

	public void OnNoseManualEnter(bool rightFirst)
	{
		if (rightFirst)
		{
			this.playerSM.OnNoseManualEnterSM(this.inputController.RightStick, this.inputController.LeftStick);
			return;
		}
		this.playerSM.OnNoseManualEnterSM(this.inputController.LeftStick, this.inputController.RightStick);
	}

	public void OnNoseManualUpdate(StickInput stick)
	{
		bool flag;
		if (stick.HoldingNoseManual)
		{
			this.UpdateNoseManual(stick.IsRightStick);
			if ((stick.rawInput.pos.magnitude <= 0.9f || stick.rawInput.avgSpeedLastUpdate >= 2f) && (stick.NoseManualAxis >= 0.1f && stick.NoseManualAxis <= 0.95f || stick.rawInput.avgSpeedLastUpdate >= 10f))
			{
				stick.NoseManualFrameCount = 0;
			}
			else
			{
				StickInput noseManualFrameCount = stick;
				noseManualFrameCount.NoseManualFrameCount = noseManualFrameCount.NoseManualFrameCount + 1;
				if (stick.NoseManualFrameCount >= 7)
				{
					this.AnimSetNoseManual(false, PlayerController.Instance.AnimGetManualAxis());
					this.playerSM.OnNoseManualExitSM();
					stick.HoldingNoseManual = false;
					stick.NoseManualFrameCount = 0;
					return;
				}
			}
		}
		else
		{
			if (this.boardController.Grounded)
			{
				flag = (Mathf.Abs(stick.rawInput.pos.x) >= 0.6f || stick.NoseManualAxis <= 0.2f || stick.NoseManualAxis >= 0.85f ? false : stick.rawInput.avgSpeedLastUpdate < 7f);
			}
			else
			{
				flag = (Mathf.Abs(stick.rawInput.pos.x) >= 0.1f || stick.rawInput.pos.magnitude >= 0.85f || stick.NoseManualAxis <= 0.4f || stick.NoseManualAxis >= 0.95f ? false : stick.rawInput.avgSpeedLastUpdate < 2f);
			}
			if (!flag)
			{
				stick.NoseManualFrameCount = 0;
				return;
			}
			StickInput stickInput = stick;
			stickInput.NoseManualFrameCount = stickInput.NoseManualFrameCount + 1;
			if (stick.NoseManualFrameCount >= (this.boardController.Grounded ? 16 : 2))
			{
				this.OnNoseManualEnter(stick.IsRightStick);
				stick.HoldingNoseManual = true;
				stick.NoseManualFrameCount = 0;
				return;
			}
		}
	}

	public void OnPop(float p_pop, float p_scoop)
	{
		this.VelocityOnPop = this.boardController.boardRigidbody.velocity;
		PlayerController.Instance.SetTurningMode(InputController.TurningMode.InAir);
		this.SetSkaterToMaster();
		Vector3 pPop = this.skaterController.skaterTransform.up * p_pop;
		Vector3 vector3 = this.skaterController.skaterRigidbody.velocity + pPop;
		Vector3.Angle(this.cameraController._actualCam.forward, vector3);
		Vector3 vector31 = this.skaterController.PredictLanding(pPop);
		this.skaterController.skaterRigidbody.AddForce(pPop, ForceMode.Impulse);
		this.skaterController.skaterRigidbody.AddForce(vector31, ForceMode.VelocityChange);
		SoundManager.Instance.PlayPopSound(p_scoop);
		this.comController.popForce = pPop;
	}

	public void OnPop(float p_pop, float p_scoop, Vector3 p_popOutDir)
	{
		this.VelocityOnPop = this.boardController.boardRigidbody.velocity;
		PlayerController.Instance.SetTurningMode(InputController.TurningMode.InAir);
		this.SetSkaterToMaster();
		Vector3 pPopOutDir = (this.skaterController.skaterTransform.up + p_popOutDir) * p_pop;
		this.skaterController.PredictLanding(pPopOutDir);
		this.skaterController.skaterRigidbody.AddForce(pPopOutDir, ForceMode.Impulse);
		SoundManager.Instance.PlayPopSound(p_scoop);
		this.comController.popForce = pPopOutDir;
	}

	public void OnPopStartCheck(bool p_canPop, StickInput p_popStick, ref PlayerController.SetupDir _setupDir, bool p_forwardLoad, float p_popThreshold, ref float r_invertVel, float p_augmentedAngle, ref float r_popVel)
	{
		if (p_canPop)
		{
			if (p_popStick.AugmentedPopToeSpeed <= p_popThreshold || p_popStick.AugmentedPopToeVel.y >= 0f)
			{
				this.SetupDirection(p_popStick, ref _setupDir, p_augmentedAngle);
			}
			else
			{
				this.SetupDirection(p_popStick, ref _setupDir, p_augmentedAngle);
				float single = (float)((p_forwardLoad ? -1 : 1));
				if (Mathf.Abs(p_popStick.AugmentedToeAxisVel) < this.olliexVelThreshold && p_popStick.AugmentedPopToeVel.y < -5f)
				{
					this.SetPopValues(false, 0f, 0f, 0f, ref r_popVel);
				}
				if (p_popStick.AugmentedToeAxisVel >= p_popThreshold)
				{
					if ((int)_setupDir != 1)
					{
						Mathf.Clamp(single * this.shuvMult * p_popStick.AugmentedToeAxisVel, -this.shuvMax, this.shuvMax);
						this.SetPopValues(true, single * 1f, single * this.shuvMult * p_popStick.AugmentedToeAxisVel, 1f, ref r_popVel);
					}
					else
					{
						this.SetPopValues(true, this.doubleShuvMult, single * this.doubleShuvMult * p_popStick.AugmentedToeAxisVel, 1f, ref r_popVel);
					}
				}
				if (p_popStick.AugmentedToeAxisVel <= -p_popThreshold)
				{
					if ((int)_setupDir == 2)
					{
						this.SetPopValues(true, -this.doubleShuvMult, single * this.doubleShuvMult * p_popStick.AugmentedToeAxisVel, 1f, ref r_popVel);
						return;
					}
					Mathf.Clamp(single * this.shuvMult * p_popStick.AugmentedToeAxisVel, -this.shuvMax, this.shuvMax);
					this.SetPopValues(true, single * -1f, single * this.shuvMult * p_popStick.AugmentedToeAxisVel, 1f, ref r_popVel);
					return;
				}
			}
		}
	}

	public void OnPopStickUpdate(float p_threshold, bool p_canPop, StickInput p_popStick, ref float r_popVel, float p_popThreshold, bool p_forwardLoad, ref PlayerController.SetupDir _setupDir, ref float r_invertVel, float p_augmentedAngle)
	{
		if (p_popStick.AugmentedPopToeSpeed > p_popThreshold && p_popStick.AugmentedPopToeVel.y < 0f)
		{
			float single = (float)((p_forwardLoad ? -1 : 1));
			float pPopStick = this.invertMult * p_popStick.rawInput.radialVel;
			if (pPopStick > r_invertVel && pPopStick > 0.1f)
			{
				this.GetBoardBackwards();
			}
			float augmentedToeAxisVel = single * this.shuvMult * p_popStick.AugmentedToeAxisVel;
			if (Mathf.Abs(r_popVel) < 0.1f || Mathf.Sign(augmentedToeAxisVel) == Mathf.Sign(r_popVel) && Mathf.Abs(augmentedToeAxisVel) > Mathf.Abs(r_popVel))
			{
				if (p_popStick.AugmentedToeAxisVel > 0f)
				{
					int num = (int)_setupDir;
				}
				if (p_popStick.AugmentedToeAxisVel < 0f)
				{
					int num1 = (int)_setupDir;
				}
				augmentedToeAxisVel = Mathf.Clamp(augmentedToeAxisVel, -this.shuvMax, this.shuvMax);
				this.SetScoopSpeed(augmentedToeAxisVel);
				r_popVel = augmentedToeAxisVel;
			}
		}
	}

	public void PhysicsRotation(float p_force, float p_damper)
	{
		this.boardController.PhysicsRotation(p_force, p_damper);
	}

	public Vector3 PlayerForward()
	{
		return this.skaterController.skaterTransform.forward;
	}

	public void ReduceImpactBounce()
	{
		this.boardController.ReduceImpactBounce();
	}

	public void RemoveBoardAngularVelocity()
	{
		this.boardController.boardRigidbody.angularVelocity = Vector3.zero;
	}

	public void RemoveTurnTorque(float p_value, InputController.TurningMode p_turningMode)
	{
		switch (p_turningMode)
		{
			case InputController.TurningMode.Grounded:
			{
				this.boardController.RemoveTurnTorqueLinear();
				return;
			}
			case InputController.TurningMode.PreWind:
			{
				this.boardController.RemoveTurnTorque(0.5f);
				return;
			}
			case InputController.TurningMode.InAir:
			{
				this.skaterController.RemoveTurnTorque(p_value);
				return;
			}
			case InputController.TurningMode.FastLeft:
			{
				this.skaterController.RemoveTurnTorque(0.95f);
				return;
			}
			case InputController.TurningMode.FastRight:
			{
				this.skaterController.RemoveTurnTorque(0.95f);
				return;
			}
			case InputController.TurningMode.Manual:
			{
				this.boardController.RemoveTurnTorqueLinear();
				return;
			}
			default:
			{
				return;
			}
		}
	}

	public void ResetAfterGrinds()
	{
		this.AnimForceFlipValue(0f);
		this.AnimForceScoopValue(0f);
		this.AnimSetFlip(0f);
		this.AnimSetScoop(0f);
		this.AnimCaught(false);
	}

	public void ResetAllAnimations()
	{
		this.animationController.ScaleAnimSpeed(1f);
		this.AnimCaught(false);
		this.AnimForceFlipValue(0f);
		this.AnimForceScoopValue(0f);
		this.AnimSetFlip(0f);
		this.AnimSetScoop(0f);
		this.AnimRelease(false);
	}

	public void ResetAllAnimationsExceptSpeed()
	{
		this.AnimCaught(false);
		this.AnimForceFlipValue(0f);
		this.AnimForceScoopValue(0f);
		this.AnimSetFlip(0f);
		this.AnimSetScoop(0f);
		this.AnimRelease(false);
		this.AnimSetupTransition(false);
		this.AnimOllieTransition(false);
	}

	public void ResetAllExceptSetup()
	{
		this.animationController.ScaleAnimSpeed(1f);
		this.AnimCaught(false);
		this.AnimForceFlipValue(0f);
		this.AnimForceScoopValue(0f);
		this.AnimSetFlip(0f);
		this.AnimSetScoop(0f);
		this.AnimRelease(false);
		this.AnimOllieTransition(false);
	}

	public void ResetAnimationsAfterImpact()
	{
		this.animationController.ScaleAnimSpeed(1f);
		this.AnimCaught(false);
		this.AnimRelease(false);
		this.AnimSetupTransition(false);
		this.AnimOllieTransition(false);
	}

	public void ResetBackTruckCenterOfMass()
	{
		this.boardController.backTruckRigidbody.ResetCenterOfMass();
	}

	public void ResetBoardCenterOfMass()
	{
		this.boardController.boardRigidbody.ResetCenterOfMass();
	}

	public void ResetFrontTruckCenterOfMass()
	{
		this.boardController.frontTruckRigidbody.ResetCenterOfMass();
	}

	public void ResetIKOffsets()
	{
		this.ikController.ResetIKOffsets();
	}

	public void ResetPIDRotationValues()
	{
		this.boardController.ResetPIDRotationValues();
	}

	public void RotateToCatchRotation()
	{
		this.boardController.CatchRotation();
	}

	public void ScaleDisplacementCurve(float p_skaterHeight)
	{
		this._comDisplacement.ScaleDisplacementCurve(p_skaterHeight);
	}

	private void SetBackFootAxis(float p_backForwardAxis, float p_backToeAxis)
	{
		this._backFootForwardAxis = p_backForwardAxis;
		this._backFootToeAxis = p_backToeAxis;
	}

	public void SetBackPivotRotation(float p_frontToeAxis)
	{
		this.boardController.SetBackPivotRotation(p_frontToeAxis);
	}

	public void SetBackTruckCenterOfMass(Vector3 p_position)
	{
		this.boardController.backTruckRigidbody.centerOfMass = p_position;
	}

	public void SetBoardBackwards()
	{
		this.boardController.SetBoardBackwards();
	}

	public void SetBoardCenterOfMass(Vector3 p_position)
	{
		this.boardController.boardRigidbody.centerOfMass = p_position;
		this.CenterOfMass.position = this.boardController.boardTransform.TransformPoint(this.boardController.boardRigidbody.centerOfMass);
	}

	public void SetBoardTargetPosition(float p_frontMagnitudeMinusBackMagnitude)
	{
		this.boardController.SetBoardTargetPosition(p_frontMagnitudeMinusBackMagnitude);
	}

	public void SetBoardToMaster()
	{
		this.movementMaster = PlayerController.MovementMaster.Board;
		this.skaterController.skaterRigidbody.useGravity = false;
		this.skaterController.skaterRigidbody.velocity = Vector3.zero;
	}

	public void SetCatchForwardRotation()
	{
		this.boardController.SetCatchForwardRotation();
	}

	public void SetFlipSpeed(float p_value)
	{
		this.boardController.thirdVel = (this.boardController.IsBoardBackwards ? -p_value : p_value) * 1.2f;
	}

	public void SetForwardSpeed(float p_value)
	{
		this.boardController.firstVel = (this.boardController.IsBoardBackwards ? -p_value : p_value);
	}

	private void SetFrontFootAxis(float p_frontForwardAxis, float p_frontToeAxis)
	{
		this._frontFootForwardAxis = p_frontForwardAxis;
		this._frontFootToeAxis = p_frontToeAxis;
	}

	public void SetFrontPivotRotation(float p_backToeAxis)
	{
		this.boardController.SetFrontPivotRotation(p_backToeAxis);
	}

	public void SetFrontTruckCenterOfMass(Vector3 p_position)
	{
		this.boardController.frontTruckRigidbody.centerOfMass = p_position;
	}

	public void SetGrindPIDRotationValues()
	{
		this.boardController.SetGrindPIDRotationValues();
	}

	public void SetGrindTweakAxis(float p_value)
	{
		this.animationController.SetGrindTweakValue(p_value);
	}

	public void SetIKOnOff(float p_value)
	{
		this.ikController.OnOffIK(p_value);
	}

	public void SetIKRigidboardKinematicFalse()
	{
		this.ikController.SetIKRigidbodyKinematic(false);
	}

	public void SetIKRigidbodyKinematic(bool p_value)
	{
		this.ikController.SetIKRigidbodyKinematic(p_value);
	}

	public void SetIKRigidbodyKinematicNextFrame()
	{
		DHTools.Instance.InvokeNextFrame(new DHTools.Function(this.SetIKRigidboardKinematicFalse));
	}

	public void SetInAirFootPlacement(float p_toeAxis, float p_forwardAxis, bool p_front)
	{
		if (p_front)
		{
			this._frontMagnitude.x = p_toeAxis;
			this._frontMagnitude.y = p_forwardAxis;
			this.animationController.SetValue("FrontToeAxis", p_toeAxis);
			this.animationController.SetValue("FrontForwardAxis", p_forwardAxis);
			this.animationController.SetSteezeValue("FrontToeAxis", p_toeAxis);
			this.animationController.SetSteezeValue("FrontForwardAxis", p_forwardAxis);
			this.SetFrontFootAxis(p_forwardAxis, p_toeAxis);
			return;
		}
		this._backMagnitude.x = p_toeAxis;
		this._backMagnitude.y = p_forwardAxis;
		this.animationController.SetValue("BackToeAxis", p_toeAxis);
		this.animationController.SetValue("BackForwardAxis", p_forwardAxis);
		this.animationController.SetSteezeValue("BackToeAxis", p_toeAxis);
		this.animationController.SetSteezeValue("BackForwardAxis", p_forwardAxis);
		this.SetBackFootAxis(p_forwardAxis, p_toeAxis);
	}

	public void SetKneeBendWeight(float p_value)
	{
		this.ikController.SetKneeBendWeight(p_value);
	}

	public void SetLeftIKLerpTarget(float p_value)
	{
		this.ikController.SetLeftLerpTarget(p_value, p_value);
	}

	public void SetLeftIKLerpTarget(float p_pos, float p_rot)
	{
		this.ikController.SetLeftLerpTarget(p_pos, p_rot);
	}

	public void SetLeftIKOffset(float p_toeAxis, float p_forwardDir, float p_popDir, bool p_isPopStick, bool p_lockHorizontal, bool p_popping)
	{
		this.ikController.SetLeftIKOffset(p_toeAxis, p_forwardDir, p_popDir, p_isPopStick, p_lockHorizontal, p_popping);
	}

	public void SetLeftIKRotationWeight(float p_value)
	{
		this.ikController.SetLeftIKRotationWeight(p_value);
	}

	public void SetLeftIKWeight(float p_value)
	{
		this.ikController.LeftIKWeight(p_value);
	}

	public void SetLeftSteezeWeight(float p_value)
	{
		this.ikController.SetLeftSteezeWeight(p_value);
	}

	public void SetManual(bool p_value)
	{
		this.Manualling = p_value;
	}

	public void SetManualStrength(float p_value)
	{
		this.animationController.SetValue("ManualStrength", p_value);
	}

	public void SetMaxSteeze(float p_value)
	{
		this.ikController.SetMaxSteeze(p_value);
	}

	public void SetMaxSteezeLeft(float p_value)
	{
		this.ikController.SetMaxSteezeLeft(p_value);
	}

	public void SetMaxSteezeRight(float p_value)
	{
		this.ikController.SetMaxSteezeRight(p_value);
	}

	public void SetPivotForwardRotation(float p_leftForwardAxisPlusRightForwardAxis, float p_speed)
	{
		this.boardController.SetPivotForwardRotation(p_leftForwardAxisPlusRightForwardAxis, p_speed);
	}

	public void SetPivotSideRotation(float p_leftToeAxisMinusRightToeAxis)
	{
		this.boardController.SetPivotSideRotation(p_leftToeAxisMinusRightToeAxis);
	}

	public void SetPopValues(bool p_animReleased, float p_animScoopSpeed, float p_scoopSpeed, float p_popValue, ref float r_popVel)
	{
		if (Mathf.Sign(p_animScoopSpeed) != Mathf.Sign(p_scoopSpeed))
		{
			p_animScoopSpeed *= -1f;
		}
		r_popVel = p_scoopSpeed;
		this.ResetAfterGrinds();
		this.AnimRelease(p_animReleased);
		this.AnimSetScoop(p_animScoopSpeed);
		this.SetTargetToMaster();
		this.SetScoopSpeed(p_scoopSpeed);
		this.SetLeftIKLerpTarget(p_popValue);
		this.SetRightIKLerpTarget(p_popValue);
		this.playerSM.OnNextStateSM();
	}

	public void SetPuppetMasterMode(BehaviourPuppet.NormalMode p_mode)
	{
		this.skaterController.SetPuppetMode(p_mode);
	}

	public void SetPushForce(float p_value)
	{
		this.skaterController.pushForce = p_value;
	}

	public void SetRightIKLerpTarget(float p_value)
	{
		this.ikController.SetRightLerpTarget(p_value, p_value);
	}

	public void SetRightIKLerpTarget(float p_pos, float p_rot)
	{
		this.ikController.SetRightLerpTarget(p_pos, p_rot);
	}

	public void SetRightIKOffset(float p_toeAxis, float p_forwardDir, float p_popDir, bool p_isPopStick, bool p_lockHorizontal, bool p_popping)
	{
		this.ikController.SetRightIKOffset(p_toeAxis, p_forwardDir, p_popDir, p_isPopStick, p_lockHorizontal, p_popping);
	}

	public void SetRightIKRotationWeight(float p_value)
	{
		this.ikController.SetRightIKRotationWeight(p_value);
	}

	public void SetRightIKWeight(float p_value)
	{
		this.ikController.RightIKWeight(p_value);
	}

	public void SetRightSteezeWeight(float p_value)
	{
		this.ikController.SetRightSteezeWeight(p_value);
	}

	public void SetScoopSpeed(float p_value)
	{
		this.boardController.secondVel = -p_value;
	}

	public void SetSkaterToMaster()
	{
		this.movementMaster = PlayerController.MovementMaster.Skater;
		this.skaterController.skaterRigidbody.useGravity = true;
	}

	public void SetTargetToMaster()
	{
		this.skaterController.skaterRigidbody.angularVelocity = this.boardController.boardRigidbody.angularVelocity;
		this.skaterController.skaterRigidbody.velocity = this.boardController.boardRigidbody.velocity;
		this.boardController.boardRigidbody.angularVelocity = Vector3.zero;
		this.movementMaster = PlayerController.MovementMaster.Target;
	}

	public void SetTurningMode(InputController.TurningMode p_turningMode)
	{
		this.inputController.turningMode = p_turningMode;
	}

	public void SetTurnMultiplier(float p_value)
	{
		this.inputController.TriggerMultiplier = p_value;
	}

	private void SetTweakAxis()
	{
		float animatorSpeed = 1f / this.animationController.GetAnimatorSpeed();
		this._forwardTweakAxis = Mathf.MoveTowards(this._forwardTweakAxis, this._frontFootForwardAxis + this._backFootForwardAxis, Time.deltaTime * animatorSpeed * 10f);
		this._toeTweakAxis = Mathf.MoveTowards(this._toeTweakAxis, this._frontFootToeAxis + -this._backFootToeAxis, Time.deltaTime * animatorSpeed * 10f);
		this.animationController.SetTweakValues(Mathf.Clamp(this._forwardTweakAxis / 2f, -1f, 1f), this._toeTweakAxis / 2f);
		this.animationController.SetTweakMagnitude(this._frontMagnitude.magnitude, this._backMagnitude.magnitude);
	}

	public void SetupDirection(StickInput p_popStick, ref PlayerController.SetupDir _setupDir)
	{
		this._stickAngle = Vector2.SignedAngle(p_popStick.AugmentedPopToeVector, Vector2.up);
		if ((int)_setupDir == 0)
		{
			if (this._stickAngle < -4f)
			{
				_setupDir = PlayerController.SetupDir.Left;
				return;
			}
			if (this._stickAngle > 4f)
			{
				_setupDir = PlayerController.SetupDir.Right;
			}
		}
	}

	public void SetupDirection(StickInput p_popStick, ref PlayerController.SetupDir _setupDir, float p_augmentedAngle)
	{
		if (p_augmentedAngle == 0f)
		{
			this._stickAngle = Vector2.SignedAngle(p_popStick.AugmentedPopToeVector, Vector2.up);
			if ((int)_setupDir == 0)
			{
				if (this._stickAngle < -4f)
				{
					_setupDir = PlayerController.SetupDir.Left;
					return;
				}
				if (this._stickAngle > 4f)
				{
					_setupDir = PlayerController.SetupDir.Right;
				}
			}
		}
	}

	public void SkaterRotation(bool p_canRotate, bool p_manualling)
	{
		this.skaterController.UpdateSkaterRotation(p_canRotate, p_manualling);
	}

	public void SkaterRotation(Quaternion p_rot)
	{
		this.skaterController.UpdateSkaterRotation(true, p_rot);
		this.skaterController.skaterRigidbody.angularVelocity = Vector3.zero;
	}

	public void SnapRotation()
	{
		this.boardController.SnapRotation();
	}

	public void SnapRotation(bool p_value)
	{
		this.boardController.SnapRotation(p_value);
	}

	public void SnapRotation(float p_value)
	{
		this.boardController.SnapRotation(p_value);
	}

	private void Start()
	{
		this.playerSM = new PlayerStateMachine(base.gameObject);
		this.playerSM.StartSM();
	}

	public void TurnLeft(float p_value, InputController.TurningMode p_turningMode)
	{
		switch (p_turningMode)
		{
			case InputController.TurningMode.Grounded:
			{
				this.boardController.AddTurnTorque(-p_value);
				this.skaterController.AddTurnTorque(-p_value * this.torsoTorqueMult);
				return;
			}
			case InputController.TurningMode.PreWind:
			{
				this.boardController.AddTurnTorque(-(p_value / 5f));
				return;
			}
			case InputController.TurningMode.InAir:
			{
				this.skaterController.AddTurnTorque(-p_value);
				return;
			}
			case InputController.TurningMode.FastLeft:
			{
				if (SettingsManager.Instance.stance != SettingsManager.Stance.Regular)
				{
					this.skaterController.AddTurnTorque(-p_value);
					return;
				}
				this.skaterController.AddTurnTorque(-p_value, true);
				return;
			}
			case InputController.TurningMode.FastRight:
			{
				if (SettingsManager.Instance.stance == SettingsManager.Stance.Regular)
				{
					this.skaterController.AddTurnTorque(-p_value);
					return;
				}
				this.skaterController.AddTurnTorque(-p_value, true);
				return;
			}
			case InputController.TurningMode.Manual:
			{
				this.boardController.AddTurnTorqueManuals(-p_value);
				return;
			}
			default:
			{
				return;
			}
		}
	}

	public void TurnRight(float p_value, InputController.TurningMode p_turningMode)
	{
		switch (p_turningMode)
		{
			case InputController.TurningMode.Grounded:
			{
				this.boardController.AddTurnTorque(p_value);
				this.skaterController.AddTurnTorque(p_value * this.torsoTorqueMult);
				return;
			}
			case InputController.TurningMode.PreWind:
			{
				this.boardController.AddTurnTorque(p_value / 5f);
				return;
			}
			case InputController.TurningMode.InAir:
			{
				this.skaterController.AddTurnTorque(p_value);
				return;
			}
			case InputController.TurningMode.FastLeft:
			{
				if (SettingsManager.Instance.stance == SettingsManager.Stance.Regular)
				{
					this.skaterController.AddTurnTorque(p_value);
					return;
				}
				this.skaterController.AddTurnTorque(p_value, true);
				return;
			}
			case InputController.TurningMode.FastRight:
			{
				if (SettingsManager.Instance.stance != SettingsManager.Stance.Regular)
				{
					this.skaterController.AddTurnTorque(p_value);
					return;
				}
				this.skaterController.AddTurnTorque(p_value, true);
				return;
			}
			case InputController.TurningMode.Manual:
			{
				this.boardController.AddTurnTorqueManuals(p_value);
				return;
			}
			default:
			{
				return;
			}
		}
	}

	public bool TwoWheelsDown()
	{
		return this.boardController.TwoDown;
	}

	private void Update()
	{
		this.playerSM.UpdateSM();
		if (this._flipAxisTarget != 0f)
		{
			this.LerpFlipAxis();
		}
		this.SetTweakAxis();
	}

	public void UpdateBoardPosition()
	{
		this.boardController.UpdateBoardPosition();
	}

	public void UpdateManual(bool rightFirst)
	{
		if (rightFirst)
		{
			this.playerSM.OnManualUpdateSM(this.inputController.RightStick, this.inputController.LeftStick);
			return;
		}
		this.playerSM.OnManualUpdateSM(this.inputController.LeftStick, this.inputController.RightStick);
	}

	public void UpdateNoseManual(bool rightFirst)
	{
		if (rightFirst)
		{
			this.playerSM.OnNoseManualUpdateSM(this.inputController.RightStick, this.inputController.LeftStick);
			return;
		}
		this.playerSM.OnNoseManualUpdateSM(this.inputController.LeftStick, this.inputController.RightStick);
	}

	public void UpdateSkaterDuringPop()
	{
		this.skaterController.UpdatePositionDuringPop();
	}

	public void UpdateSkaterPosition()
	{
		this.skaterController.UpdatePositions();
	}

	public enum MovementMaster
	{
		Board,
		Target,
		Skater
	}

	public enum SetupDir
	{
		Middle,
		Left,
		Right
	}
}