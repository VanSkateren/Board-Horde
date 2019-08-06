using FSMHelper;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Manualling : PlayerState_OnBoard
{
	private StickInput _popStick;

	private StickInput _flipStick;

	private bool _manual;

	private bool _delayExit;

	private float _delayTimer;

	private bool _flipDetected;

	private bool _potentialFlip;

	private Vector2 _initialFlipDir = Vector2.zero;

	private int _flipFrameCount;

	private int _flipFrameMax = 20;

	private float _toeAxis;

	private float _flipVel;

	private float _popVel;

	private float _popDir;

	private float _popWait;

	private bool _canPop;

	private float _manualStrength;

	private float _flip;

	private float _popForce = 2.5f;

	private float _invertVel;

	private bool _forwardLoad;

	private float _augmentedLeftAngle;

	private float _augmentedRightAngle;

	private float _flipWindowTimer;

	private float _boardAngleToGround;

	private float _manualAxis;

	private int _manualSign = 1;

	private PlayerController.SetupDir _setupDir;

	private float frontTruckSpringCache;

	private float frontTruckDampCache;

	private float backTruckSpringCache;

	private float backTruckDampCache;

	public PlayerState_Manualling(StickInput p_popStick, StickInput p_flipStick, bool p_manual)
	{
		this._popStick = p_popStick;
		this._flipStick = p_flipStick;
		this._manual = p_manual;
		if (!PlayerController.Instance.IsSwitch)
		{
			if (this._manual)
			{
				this._manualSign = -1;
				return;
			}
			this._manualSign = 1;
			return;
		}
		if (this._manual)
		{
			this._manualSign = 1;
			return;
		}
		this._manualSign = -1;
	}

	public override void BothTriggersReleased(InputController.TurningMode p_turningMode)
	{
		PlayerController.Instance.RemoveTurnTorque(0.3f, p_turningMode);
	}

	public override bool CanGrind()
	{
		if (!this._canPop)
		{
			return true;
		}
		return false;
	}

	public override void Enter()
	{
		PlayerController.Instance.boardController.ResetAll();
		PlayerController.Instance.CrossFadeAnimation("Manual", 0.3f);
		this.SetManualTruckSprings();
		this.SetCenterOfMass();
		PlayerController.Instance.SetTurnMultiplier(1f);
		PlayerController.Instance.SetTurningMode(InputController.TurningMode.Grounded);
		if (!PlayerController.Instance.IsGrounded())
		{
			this._manualStrength = 0f;
			PlayerController.Instance.SetManualStrength(0f);
		}
		else
		{
			this._manualStrength = 1f;
			PlayerController.Instance.SetManualStrength(1f);
		}
		PlayerController.Instance.OnImpact();
	}

	public override void Exit()
	{
		PlayerController.Instance.VelocityOnPop = PlayerController.Instance.boardController.boardRigidbody.velocity;
		this.UnsetManualTruckSprings();
		PlayerController.Instance.AnimSetManual(false, PlayerController.Instance.AnimGetManualAxis());
		PlayerController.Instance.AnimSetNoseManual(false, PlayerController.Instance.AnimGetManualAxis());
		PlayerController.Instance.ResetBoardCenterOfMass();
		PlayerController.Instance.ResetBackTruckCenterOfMass();
		PlayerController.Instance.ResetFrontTruckCenterOfMass();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		PlayerController.Instance.comController.UpdateCOM(0.95f, 0);
		PlayerController.Instance.ManualRotation((this._popStick.ForwardDir > 0.1f ? false : true), this._popStick.ForwardDir, -this._flipStick.PopDir, this._flipStick.ToeAxis);
		PlayerController.Instance.SkaterRotation(true, true);
		PlayerController.Instance.ReduceImpactBounce();
	}

	public override StickInput GetPopStick()
	{
		return this._popStick;
	}

	public override bool IsOnGroundState()
	{
		return true;
	}

	public override void OnCollisionEnterEvent()
	{
		if (PlayerController.Instance.movementMaster != PlayerController.MovementMaster.Board)
		{
			PlayerController.Instance.ResetAllAnimations();
			PlayerController.Instance.SetBoardToMaster();
			PlayerController.Instance.SetTurningMode(InputController.TurningMode.Grounded);
		}
	}

	public override void OnFirstWheelDown()
	{
	}

	public override void OnFlipStickUpdate()
	{
		float single;
		if (this._canPop && PlayerController.Instance.IsGrounded())
		{
			PlayerController instance = PlayerController.Instance;
			ref bool flagPointer = ref this._potentialFlip;
			ref Vector2 vector2Pointer = ref this._initialFlipDir;
			ref int numPointer = ref this._flipFrameCount;
			ref int numPointer1 = ref this._flipFrameMax;
			ref float singlePointer = ref this._toeAxis;
			ref float singlePointer1 = ref this._flipVel;
			ref float singlePointer2 = ref this._popVel;
			ref float singlePointer3 = ref this._popDir;
			ref float singlePointer4 = ref this._flip;
			StickInput stickInput = this._flipStick;
			ref float singlePointer5 = ref this._invertVel;
			single = (this._popStick.IsRightStick ? this._augmentedLeftAngle : this._augmentedRightAngle);
			instance.OnFlipStickUpdate(ref this._flipDetected, ref flagPointer, ref vector2Pointer, ref numPointer, ref numPointer1, ref singlePointer, ref singlePointer1, ref singlePointer2, ref singlePointer3, ref singlePointer4, stickInput, false, true, ref singlePointer5, single, false, this._forwardLoad, ref this._flipWindowTimer);
		}
	}

	public override void OnGrindDetected()
	{
		if (!this._canPop)
		{
			base.DoTransition(typeof(PlayerState_Grinding), null);
		}
	}

	public override void OnImpactUpdate()
	{
	}

	public override void OnManualExit()
	{
		if (!this._canPop)
		{
			this._delayExit = true;
			return;
		}
		PlayerController.Instance.AnimSetManual(false, PlayerController.Instance.AnimGetManualAxis());
		base.DoTransition(typeof(PlayerState_Riding), null);
	}

	public override void OnNextState()
	{
		PlayerController.Instance.AnimSetPopStrength(0f);
		PlayerController.Instance.boardController.ReferenceBoardRotation();
		PlayerController.Instance.SetTurnMultiplier(1.2f);
		PlayerController.Instance.SetTurningMode(InputController.TurningMode.InAir);
		PlayerController.Instance.FixTargetNormal();
		PlayerController.Instance.SetTargetToMaster();
		PlayerController.Instance.AnimOllieTransition(true);
		if (!this._potentialFlip)
		{
			object[] objArray = new object[] { this._popStick, this._flipStick, this._popForce, false, this._forwardLoad, this._invertVel, this._setupDir, this._augmentedLeftAngle, this._augmentedRightAngle, this._popVel, this._toeAxis, this._popDir };
			base.DoTransition(typeof(PlayerState_BeginPop), objArray);
			return;
		}
		object[] objArray1 = new object[] { this._popStick, this._flipStick, this._initialFlipDir, this._flipVel, this._popVel, this._toeAxis, this._popDir, this._flipDetected, this._flip, this._popForce, false, this._forwardLoad, this._invertVel, this._setupDir, this._augmentedLeftAngle, this._augmentedRightAngle };
		base.DoTransition(typeof(PlayerState_BeginPop), objArray1);
	}

	public override void OnNoseManualExit()
	{
		if (!this._canPop)
		{
			this._delayExit = true;
			return;
		}
		PlayerController.Instance.AnimSetNoseManual(false, PlayerController.Instance.AnimGetManualAxis());
		base.DoTransition(typeof(PlayerState_Riding), null);
	}

	public override void OnPopStickUpdate()
	{
		if (this._canPop && PlayerController.Instance.IsGrounded())
		{
			this._forwardLoad = PlayerController.Instance.GetNollie(this._popStick.IsRightStick) > 0.1f;
			PlayerController.Instance.AnimSetNollie(PlayerController.Instance.GetNollie(this._popStick.IsRightStick));
			PlayerController.Instance.OnPopStartCheck(true, this._popStick, ref this._setupDir, this._forwardLoad, 10f, ref this._invertVel, 0f, ref this._popVel);
		}
	}

	public override void OnPredictedCollisionEvent()
	{
		if (PlayerController.Instance.movementMaster != PlayerController.MovementMaster.Board)
		{
			PlayerController.Instance.ResetAllAnimations();
			PlayerController.Instance.SetBoardToMaster();
			PlayerController.Instance.SetTurningMode(InputController.TurningMode.Grounded);
		}
	}

	public override void OnWheelsLeftGround()
	{
		PlayerController.Instance.skaterController.skaterRigidbody.angularVelocity = Vector3.zero;
		PlayerController.Instance.AnimSetManual(false, PlayerController.Instance.AnimGetManualAxis());
		PlayerController.Instance.SetTurningMode(InputController.TurningMode.InAir);
		PlayerController.Instance.SetSkaterToMaster();
		PlayerController.Instance.AnimSetRollOff(true);
		PlayerController.Instance.CrossFadeAnimation("Extend", 0.3f);
		Vector3 vector3 = PlayerController.Instance.skaterController.PredictLanding(PlayerController.Instance.skaterController.skaterRigidbody.velocity);
		PlayerController.Instance.skaterController.skaterRigidbody.AddForce(vector3, ForceMode.Impulse);
		object[] objArray = new object[] { false, false };
		base.DoTransition(typeof(PlayerState_InAir), objArray);
	}

	private void SetCenterOfMass()
	{
		bool flag;
		if (!PlayerController.Instance.GetBoardBackwards())
		{
			flag = (this._popStick.ForwardDir > 0f ? false : true);
		}
		else
		{
			flag = (this._popStick.ForwardDir > 0f ? true : false);
		}
		this._manual = flag;
		Vector3 vector3 = (this._manual ? PlayerController.Instance.boardController.backTruckCoM.position : PlayerController.Instance.boardController.frontTruckCoM.position);
		PlayerController.Instance.SetBoardCenterOfMass(PlayerController.Instance.boardController.boardTransform.InverseTransformPoint(vector3));
		PlayerController.Instance.SetBackTruckCenterOfMass(PlayerController.Instance.boardController.backTruckRigidbody.transform.InverseTransformPoint(vector3));
		PlayerController.Instance.SetFrontTruckCenterOfMass(PlayerController.Instance.boardController.frontTruckRigidbody.transform.InverseTransformPoint(vector3));
	}

	private void SetManualTruckSprings()
	{
		JointDrive instance = PlayerController.Instance.boardController.frontTruckJoint.angularXDrive;
		this.frontTruckSpringCache = instance.positionSpring;
		instance = PlayerController.Instance.boardController.frontTruckJoint.angularXDrive;
		this.frontTruckDampCache = instance.positionDamper;
		instance = PlayerController.Instance.boardController.backTruckJoint.angularXDrive;
		this.backTruckSpringCache = instance.positionSpring;
		instance = PlayerController.Instance.boardController.backTruckJoint.angularXDrive;
		this.backTruckDampCache = instance.positionDamper;
		JointDrive jointDrive = new JointDrive()
		{
			positionDamper = 1f,
			positionSpring = 20f
		};
		instance = PlayerController.Instance.boardController.frontTruckJoint.angularXDrive;
		jointDrive.maximumForce = instance.maximumForce;
		PlayerController.Instance.boardController.frontTruckJoint.angularXDrive = jointDrive;
		PlayerController.Instance.boardController.backTruckJoint.angularXDrive = jointDrive;
	}

	public override void SetupDefinition(ref FSMStateType stateType, ref List<Type> children)
	{
		stateType = FSMStateType.Type_OR;
	}

	private void UnsetManualTruckSprings()
	{
		JointDrive jointDrive = new JointDrive()
		{
			positionDamper = this.frontTruckDampCache,
			positionSpring = this.frontTruckSpringCache
		};
		JointDrive instance = PlayerController.Instance.boardController.frontTruckJoint.angularXDrive;
		jointDrive.maximumForce = instance.maximumForce;
		PlayerController.Instance.boardController.frontTruckJoint.angularXDrive = jointDrive;
		jointDrive.positionDamper = this.backTruckDampCache;
		jointDrive.positionSpring = this.backTruckSpringCache;
		instance = PlayerController.Instance.boardController.backTruckJoint.angularXDrive;
		jointDrive.maximumForce = instance.maximumForce;
		PlayerController.Instance.boardController.backTruckJoint.angularXDrive = jointDrive;
	}

	public override void Update()
	{
		base.Update();
		this._boardAngleToGround = Vector3.Angle(PlayerController.Instance.boardController.boardTransform.up, PlayerController.Instance.GetGroundNormal());
		this._boardAngleToGround *= (float)this._manualSign;
		this._boardAngleToGround = Mathf.Clamp(this._boardAngleToGround, -30f, 30f);
		this._boardAngleToGround /= 30f;
		this._manualAxis = Mathf.Lerp(this._manualAxis, this._boardAngleToGround, Time.deltaTime * 10f);
		PlayerController.Instance.AnimSetManualAxis(this._manualAxis);
		this._manualStrength = Mathf.Clamp(this._manualStrength + Time.deltaTime * 2f, 0f, 1f);
		PlayerController.Instance.SetManualStrength(this._manualStrength);
		if (this._popWait >= 0.2f)
		{
			PlayerController.Instance.ResetAfterGrinds();
			this._canPop = true;
		}
		else if (PlayerController.Instance.IsGrounded())
		{
			this._popWait += Time.deltaTime;
		}
		if (this._delayExit)
		{
			this._delayTimer += Time.deltaTime;
			if (this._delayTimer > 0.2f)
			{
				PlayerController.Instance.AnimSetManual(false, PlayerController.Instance.AnimGetManualAxis());
				PlayerController.Instance.AnimSetNoseManual(false, PlayerController.Instance.AnimGetManualAxis());
				base.DoTransition(typeof(PlayerState_Riding), null);
			}
		}
		PlayerController.Instance.SetBoardTargetPosition(0f);
		PlayerController.Instance.SetFrontPivotRotation(0f);
		PlayerController.Instance.SetBackPivotRotation(0f);
		PlayerController.Instance.SetPivotForwardRotation(0f, 40f);
		PlayerController.Instance.SetPivotSideRotation(0f);
	}
}