using FSMHelper;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Setup : PlayerState_OnBoard
{
	private StickInput _popStick;

	private StickInput _flipStick;

	private bool _forwardLoad;

	private bool _flipDetected;

	private bool _potentialFlip;

	private Vector2 _initialFlipDir = Vector2.zero;

	private int _flipFrameCount;

	private int _flipFrameMax = 3;

	private float _toeAxis;

	private float _flipVel;

	private float _popVel;

	private float _popDir;

	private float _flip;

	private float _setupBlend;

	private float _popStrength;

	private float _popStrengthTarget;

	private float _invertVel;

	private float _augmentedLeftAngle;

	private float _augmentedRightAngle;

	private float _windUpLerp;

	private float _doubleSetupTimer;

	private float _flipWindowTimer;

	private bool _mongo;

	private float _noComplyTimer;

	private bool _canNoComply;

	private bool _noComply;

	private PlayerController.SetupDir _setupDir;

	private float setupHeight = 0.75636f;

	private float setupHeightHigh = 0.75636f;

	private float setupHeightLow = 0.64514f;

	public PlayerState_Setup(StickInput p_popStick, StickInput p_flipStick, bool p_forwardLoad, PlayerController.SetupDir p_setupDir)
	{
		this._popStick = p_popStick;
		this._flipStick = p_flipStick;
		this._forwardLoad = p_forwardLoad;
		this._setupDir = p_setupDir;
	}

	public PlayerState_Setup(StickInput p_popStick, StickInput p_flipStick, bool p_forwardLoad, PlayerController.SetupDir p_setupDir, bool p_mongo)
	{
		this._popStick = p_popStick;
		this._flipStick = p_flipStick;
		this._forwardLoad = p_forwardLoad;
		this._setupDir = p_setupDir;
		this._mongo = p_mongo;
	}

	public override void Enter()
	{
		PlayerController.Instance.AnimSetPush(false);
		PlayerController.Instance.AnimSetMongo(false);
		PlayerController.Instance.AnimSetupTransition(true);
		PlayerController.Instance.boardController.ResetAll();
		PlayerController.Instance.SetTurnMultiplier(0.5f);
		PlayerController.Instance.SetTurningMode(InputController.TurningMode.Grounded);
		PlayerController.Instance.AnimSetupTransition(true);
		PlayerController.Instance.AnimPopInterruptedTransitions(false);
		PlayerController.Instance.OnEndImpact();
		PlayerController.Instance.AnimLandedEarly(false);
	}

	public override void Exit()
	{
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		PlayerController.Instance.LimitAngularVelocity(5f);
		PlayerController.Instance.SkaterRotation(true, false);
		PlayerController.Instance.boardController.DoBoardLean();
		PlayerController.Instance.comController.UpdateCOM(this.setupHeight, 2);
		PlayerController.Instance.boardController.ApplyOnBoardMaxRoll();
	}

	public override float GetAugmentedAngle(StickInput p_stick)
	{
		if (p_stick.IsRightStick)
		{
			return this._augmentedRightAngle;
		}
		return this._augmentedLeftAngle;
	}

	public override StickInput GetPopStick()
	{
		return this._popStick;
	}

	public override bool IsOnGroundState()
	{
		return false;
	}

	public override void OnFlipStickUpdate()
	{
		float single;
		if (this._flipStick.SetupDir >= -0.1f)
		{
			if (PlayerController.Instance.inputController.turningMode == InputController.TurningMode.PreWind)
			{
				PlayerController.Instance.SetTurningMode(InputController.TurningMode.Grounded);
			}
			this._popStrengthTarget = 0f;
			this._doubleSetupTimer = 0f;
		}
		else
		{
			if (PlayerController.Instance.inputController.turningMode == InputController.TurningMode.Grounded)
			{
				PlayerController.Instance.SetTurningMode(InputController.TurningMode.PreWind);
			}
			this._popStrengthTarget = Mathf.Clamp(1.1f * Mathf.Abs(this._flipStick.SetupDir), -1f, 1f);
			if (Mathf.Abs(PlayerController.Instance.GetWindUp()) > 0.2f)
			{
				this._doubleSetupTimer += Time.deltaTime;
			}
		}
		if (this._popStrength >= this._popStrengthTarget)
		{
			this._popStrength = Mathf.Lerp(this._popStrength, this._popStrengthTarget, Time.deltaTime * 5f);
		}
		else
		{
			this._popStrength = this._popStrengthTarget;
		}
		this._setupBlend = Mathf.Lerp(this._setupBlend, this._popStrength, Time.deltaTime * 10f);
		float single1 = this.setupHeightHigh - this.setupHeightLow;
		this.setupHeight = this.setupHeightHigh - this._setupBlend * single1;
		if (this._doubleSetupTimer > 0.2f)
		{
			float windUp = PlayerController.Instance.GetWindUp();
			windUp = Mathf.Clamp(windUp, -this._setupBlend, this._setupBlend);
			if (Mathf.Abs(this._windUpLerp) >= Mathf.Abs(windUp))
			{
				this._windUpLerp = Mathf.Lerp(this._windUpLerp, windUp, Time.deltaTime * 2f);
			}
			else
			{
				this._windUpLerp = Mathf.Lerp(this._windUpLerp, windUp, Time.deltaTime * 10f);
			}
		}
		PlayerController.Instance.AnimSetWindUp(this._windUpLerp);
		PlayerController.Instance.AnimSetPopStrength(this._popStrength);
		PlayerController.Instance.AnimSetSetupBlend(this._setupBlend);
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

	public override void OnNextState()
	{
		PlayerController.Instance.boardController.ReferenceBoardRotation();
		PlayerController.Instance.FixTargetNormal();
		PlayerController.Instance.SetTargetToMaster();
		PlayerController.Instance.AnimOllieTransition(true);
		if (this._windUpLerp > 0.1f && this._popStrength > 0.1f)
		{
			PlayerController.Instance.SetTurnMultiplier(this._windUpLerp);
			PlayerController.Instance.SetTurningMode(InputController.TurningMode.FastRight);
		}
		else if (this._windUpLerp >= -0.1f || this._popStrength <= 0.1f)
		{
			PlayerController.Instance.SetTurnMultiplier(1f);
			PlayerController.Instance.SetTurningMode(InputController.TurningMode.InAir);
		}
		else
		{
			PlayerController.Instance.SetTurnMultiplier(Mathf.Abs(this._windUpLerp));
			PlayerController.Instance.SetTurningMode(InputController.TurningMode.FastLeft);
		}
		if (!this._flipDetected)
		{
			object[] instance = new object[] { this._popStick, this._flipStick, PlayerController.Instance.popForce + this._popStrength * 0.75f, this._forwardLoad, this._invertVel, this._setupDir, this._augmentedLeftAngle, this._augmentedRightAngle, this._popVel, this._toeAxis, this._popDir };
			base.DoTransition(typeof(PlayerState_BeginPop), instance);
			return;
		}
		object[] objArray = new object[] { this._popStick, this._flipStick, this._initialFlipDir, this._flipVel, this._popVel, this._toeAxis, this._popDir, this._flipDetected, this._flip, PlayerController.Instance.popForce + this._popStrength * 0.75f, this._forwardLoad, this._invertVel, this._setupDir, this._augmentedLeftAngle, this._augmentedRightAngle };
		base.DoTransition(typeof(PlayerState_BeginPop), objArray);
	}

	public override void OnPopStickCentered()
	{
		PlayerController.Instance.AnimSetupTransition(false);
		base.DoTransition(typeof(PlayerState_Riding), null);
	}

	public override void OnPopStickUpdate()
	{
		float single;
		PlayerController instance = PlayerController.Instance;
		bool flag = PlayerController.Instance.IsGrounded();
		StickInput stickInput = this._popStick;
		bool flag1 = this._forwardLoad;
		float instance1 = PlayerController.Instance.popThreshold;
		ref float singlePointer = ref this._invertVel;
		single = (this._popStick.IsRightStick ? this._augmentedRightAngle : this._augmentedLeftAngle);
		instance.OnPopStartCheck(flag, stickInput, ref this._setupDir, flag1, instance1, ref singlePointer, single, ref this._popVel);
	}

	public override void OnStickUpdate(StickInput p_leftStick, StickInput p_rightStick)
	{
		PlayerController.Instance.SetLeftIKOffset(p_leftStick.ToeAxis, p_leftStick.ForwardDir, p_leftStick.PopDir, p_leftStick.IsPopStick, false, false);
		PlayerController.Instance.SetRightIKOffset(p_rightStick.ToeAxis, p_rightStick.ForwardDir, p_rightStick.PopDir, p_rightStick.IsPopStick, false, false);
	}

	public override void SetupDefinition(ref FSMStateType stateType, ref List<Type> children)
	{
		stateType = FSMStateType.Type_OR;
	}

	public override void Update()
	{
		base.Update();
		if (!PlayerController.Instance.IsGrounded())
		{
			PlayerController.Instance.SetTurningMode(InputController.TurningMode.InAir);
			PlayerController.Instance.SetSkaterToMaster();
			PlayerController.Instance.AnimSetRollOff(true);
			PlayerController.Instance.CrossFadeAnimation("Extend", 0.3f);
			PlayerController.Instance.skaterController.skaterRigidbody.angularVelocity = Vector3.zero;
			Vector3 vector3 = PlayerController.Instance.skaterController.PredictLanding(PlayerController.Instance.skaterController.skaterRigidbody.velocity);
			PlayerController.Instance.skaterController.skaterRigidbody.AddForce(vector3, ForceMode.Impulse);
			object[] objArray = new object[] { false, false };
			base.DoTransition(typeof(PlayerState_InAir), objArray);
		}
		PlayerController.Instance.CacheRidingTransforms();
	}
}