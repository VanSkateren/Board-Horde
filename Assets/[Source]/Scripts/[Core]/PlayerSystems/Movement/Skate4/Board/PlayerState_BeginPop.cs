using FSMHelper;
using System;
using UnityEngine;

public class PlayerState_BeginPop : PlayerState_OnBoard
{
	private StickInput _popStick;

	private StickInput _flipStick;

	private bool _forwardLoad;

	private bool _flipDetected;

	private bool _potentialFlip;

	private Vector2 _initialFlipDir = Vector2.zero;

	private int _flipFrameCount;

	private int _flipFrameMax = 25;

	private float _toeAxis;

	private float _flip;

	private float _flipVel;

	private float _popVel;

	private float _popDir;

	private float _timer;

	private float _popForce;

	private bool _wasGrinding;

	private float _invertVel;

	private float _augmentedLeftAngle;

	private float _augmentedRightAngle;

	private float _kickAddSoFar;

	private PlayerController.SetupDir _setupDir;

	public PlayerState_BeginPop(StickInput p_popStick, StickInput p_flipStick, float p_popForce, bool p_forwardLoad, float p_invertVel, PlayerController.SetupDir p_setupDir, float p_augmentedLeftAngle, float p_augmentedRightAngle, float p_popVel, float p_toeAxis, float p_popDir)
	{
		this._popStick = p_popStick;
		this._flipStick = p_flipStick;
		this._popForce = p_popForce;
		this._forwardLoad = p_forwardLoad;
		this._invertVel = p_invertVel;
		this._setupDir = p_setupDir;
		this._augmentedLeftAngle = p_augmentedLeftAngle;
		this._augmentedRightAngle = p_augmentedRightAngle;
		this._popVel = p_popVel;
		this._toeAxis = p_toeAxis;
		this._popDir = p_popDir;
		PlayerController.Instance.animationController.skaterAnim.CrossFadeInFixedTime("Pop", 0.1f);
		PlayerController.Instance.animationController.ikAnim.CrossFadeInFixedTime("Pop", 0.1f);
	}

	public PlayerState_BeginPop(StickInput p_popStick, StickInput p_flipStick, float p_popForce, bool p_wasGrinding, bool p_forwardLoad, float p_invertVel, PlayerController.SetupDir p_setupDir, float p_augmentedLeftAngle, float p_augmentedRightAngle, float p_popVel, float p_toeAxis, float p_popDir)
	{
		this._popStick = p_popStick;
		this._flipStick = p_flipStick;
		this._popForce = p_popForce;
		this._wasGrinding = p_wasGrinding;
		this._forwardLoad = p_forwardLoad;
		this._invertVel = p_invertVel;
		this._setupDir = p_setupDir;
		this._augmentedLeftAngle = p_augmentedLeftAngle;
		this._augmentedRightAngle = p_augmentedRightAngle;
		this._popVel = p_popVel;
		this._toeAxis = p_toeAxis;
		this._popDir = p_popDir;
	}

	public PlayerState_BeginPop(StickInput p_popStick, StickInput p_flipStick, Vector2 p_initialFlipDir, float p_flipVel, float p_popVel, float p_toeAxis, float p_popDir, bool p_flipDetected, float p_flip, float p_popForce, bool p_forwardLoad, float p_invertVel, PlayerController.SetupDir p_setupDir, float p_augmentedLeftAngle, float p_augmentedRightAngle)
	{
		this._popStick = p_popStick;
		this._flipStick = p_flipStick;
		this._potentialFlip = false;
		this._flipDetected = p_flipDetected;
		this._initialFlipDir = p_initialFlipDir;
		this._toeAxis = p_toeAxis;
		this._popDir = p_popDir;
		this._flipVel = p_flipVel;
		this._popVel = p_popVel;
		this._flip = p_flip;
		this._popForce = p_popForce;
		this._forwardLoad = p_forwardLoad;
		this._invertVel = p_invertVel;
		this._setupDir = p_setupDir;
		this._augmentedLeftAngle = p_augmentedLeftAngle;
		this._augmentedRightAngle = p_augmentedRightAngle;
		PlayerController.Instance.animationController.skaterAnim.CrossFadeInFixedTime("Pop", 0.1f);
		PlayerController.Instance.animationController.ikAnim.CrossFadeInFixedTime("Pop", 0.1f);
	}

	public PlayerState_BeginPop(StickInput p_popStick, StickInput p_flipStick, Vector2 p_initialFlipDir, float p_flipVel, float p_popVel, float p_toeAxis, float p_popDir, bool p_flipDetected, float p_flip, float p_popForce, bool p_wasGrinding, bool p_forwardLoad, float p_invertVel, PlayerController.SetupDir p_setupDir, float p_augmentedLeftAngle, float p_augmentedRightAngle)
	{
		this._popStick = p_popStick;
		this._flipStick = p_flipStick;
		this._potentialFlip = false;
		this._flipDetected = p_flipDetected;
		this._initialFlipDir = p_initialFlipDir;
		this._toeAxis = p_toeAxis;
		this._popDir = p_popDir;
		this._flipVel = p_flipVel;
		this._popVel = p_popVel;
		this._flip = p_flip;
		this._popForce = p_popForce;
		this._wasGrinding = p_wasGrinding;
		this._forwardLoad = p_forwardLoad;
		this._invertVel = p_invertVel;
		this._setupDir = p_setupDir;
		this._augmentedLeftAngle = p_augmentedLeftAngle;
		this._augmentedRightAngle = p_augmentedRightAngle;
	}

	public override bool CanGrind()
	{
		return false;
	}

	public override void Enter()
	{
		PlayerController.Instance.SetKneeBendWeight(0.8f);
		PlayerController.Instance.CrossFadeAnimation("Pop", 0.06f);
		PlayerController instance = PlayerController.Instance;
		Vector3 vector3 = Vector3.ProjectOnPlane(PlayerController.Instance.skaterController.skaterTransform.position - PlayerController.Instance.boardController.boardTransform.position, PlayerController.Instance.skaterController.skaterTransform.forward);
		instance.ScaleDisplacementCurve(vector3.magnitude + 0.0935936f);
		PlayerController.Instance.boardController.ResetTweakValues();
		PlayerController.Instance.boardController.CacheBoardUp();
		PlayerController.Instance.boardController.UpdateReferenceBoardTargetRotation();
		this.KickAdd();
	}

	public override void Exit()
	{
	}

	public override void FixedUpdate()
	{
		if (this._timer > 1f)
		{
			PlayerController.Instance.AnimPopInterruptedTransitions(true);
			PlayerController.Instance.SetTurningMode(InputController.TurningMode.Grounded);
			PlayerController.Instance.SetBoardToMaster();
			base.DoTransition(typeof(PlayerState_Riding), null);
		}
		PlayerController.Instance.comController.UpdateCOM();
		this._timer += Time.deltaTime;
		PlayerController.Instance.AddUpwardDisplacement(this._timer);
		if (this._timer > 0.06f)
		{
			this.SendEventPop(0f);
			return;
		}
		this.KickAdd();
		PlayerController.Instance.UpdateSkaterDuringPop();
		PlayerController.Instance.MoveCameraToPlayer();
		PlayerController.Instance.boardController.Rotate(true, false);
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

	private void KickAdd()
	{
		float single = 5f;
		float single1 = Mathf.Clamp(Mathf.Abs(this._popVel) / single, -0.7f, 0.7f);
		float single2 = 1.1f;
		if (this._wasGrinding)
		{
			single2 *= 0.5f;
		}
		float single3 = single2 - single2 * single1 - this._kickAddSoFar;
		this._kickAddSoFar += single3;
		PlayerController.Instance.DoKick(this._forwardLoad, single3);
	}

	public override void OnAnimatorUpdate()
	{
	}

	public override void OnFlipStickUpdate()
	{
		float single;
		float single1 = 0f;
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
		instance.OnFlipStickUpdate(ref this._flipDetected, ref flagPointer, ref vector2Pointer, ref numPointer, ref numPointer1, ref singlePointer, ref singlePointer1, ref singlePointer2, ref singlePointer3, ref singlePointer4, stickInput, false, false, ref singlePointer5, single, false, this._forwardLoad, ref single1);
	}

	public override void OnPopStickUpdate()
	{
		float single;
		PlayerController instance = PlayerController.Instance;
		bool flag = PlayerController.Instance.IsGrounded();
		StickInput stickInput = this._popStick;
		bool flag1 = this._forwardLoad;
		ref PlayerController.SetupDir setupDirPointer = ref this._setupDir;
		ref float singlePointer = ref this._invertVel;
		single = (this._popStick.IsRightStick ? this._augmentedRightAngle : this._augmentedLeftAngle);
		instance.OnPopStickUpdate(0.1f, flag, stickInput, ref this._popVel, 10f, flag1, ref setupDirPointer, ref singlePointer, single);
	}

	public override void OnStickFixedUpdate(StickInput p_leftStick, StickInput p_rightStick)
	{
	}

	public override void OnStickUpdate(StickInput p_leftStick, StickInput p_rightStick)
	{
		PlayerController.Instance.SetLeftIKOffset(p_leftStick.ToeAxis, p_leftStick.ForwardDir, p_leftStick.PopDir, p_leftStick.IsPopStick, false, PlayerController.Instance.GetAnimReleased());
		PlayerController.Instance.SetRightIKOffset(p_rightStick.ToeAxis, p_rightStick.ForwardDir, p_rightStick.PopDir, p_rightStick.IsPopStick, false, PlayerController.Instance.GetAnimReleased());
	}

	public override void SendEventPop(float p_value)
	{
		if (!this._wasGrinding)
		{
			PlayerController.Instance.AnimSetupTransition(false);
			PlayerController.Instance.OnPop(this._popForce, this._popVel);
			object[] objArray = new object[] { this._popStick, this._flipStick, this._initialFlipDir, this._flipVel, this._popVel, this._toeAxis, this._popDir, this._flipDetected, this._flip, this._forwardLoad, this._invertVel, this._setupDir, this._augmentedLeftAngle, this._augmentedRightAngle, this._kickAddSoFar };
			base.DoTransition(typeof(PlayerState_Pop), objArray);
			return;
		}
		Vector3 vector3 = (PlayerController.Instance.boardController.triggerManager.sideEnteredGrind == TriggerManager.SideEnteredGrind.Right ? PlayerController.Instance.boardController.triggerManager.grindRight : -PlayerController.Instance.boardController.triggerManager.grindRight);
		PlayerController.Instance.AnimSetupTransition(false);
		if (PlayerController.Instance.boardController.triggerManager.sideEnteredGrind == TriggerManager.SideEnteredGrind.Center)
		{
			PlayerController.Instance.OnPop(this._popForce, this._popVel);
		}
		else
		{
			PlayerController.Instance.OnPop(this._popForce, this._popStick.AugmentedToeAxisVel, vector3 * 0.5f);
		}
		object[] objArray1 = new object[] { this._popStick, this._flipStick, this._initialFlipDir, this._flipVel, this._popVel, this._toeAxis, this._popDir, this._flipDetected, this._flip, true, this._forwardLoad, this._invertVel, this._setupDir, this._augmentedLeftAngle, this._augmentedRightAngle, this._kickAddSoFar };
		base.DoTransition(typeof(PlayerState_Pop), objArray1);
	}

	public override void Update()
	{
		base.Update();
	}
}