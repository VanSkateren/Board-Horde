using FSMHelper;
using System;
using UnityEngine;

public class PlayerState_Released : PlayerState_OnBoard
{
	private bool _predictedCollision;

	private float _timer;

	private bool _timerEnded;

	private bool _caught;

	private bool _caughtRight;

	private bool _caughtLeft;

	private StickInput _popStick;

	private StickInput _flipStick;

	private bool _manualling;

	private bool _noseManualling;

	private bool _canManual;

	private bool _wasGrinding;

	private bool _leftCentered;

	private bool _rightCentered;

	private bool _catchRegistered;

	private bool _leftCaughtFirst;

	private bool _rightCaughtFirst;

	private bool _forwardLoad;

	private bool _flipDetected;

	private bool _potentialFlip;

	private Vector2 _initialFlipDir = Vector2.zero;

	private float _toeAxis;

	private float _flipVel;

	private float _popVel;

	private float _popDir;

	private float _flip;

	private float _invertVel;

	private float _lMagnitude;

	private float _rMagnitude;

	private float _leftCenteredTimer;

	private float _rightCenteredTimer;

	private bool _bothCaught;

	private PlayerController.SetupDir _setupDir;

	private bool isExitingState;

	private float _leftToeAxis;

	private float _rightToeAxis;

	private float _leftForwardDir;

	private float _rightForwardDir;

	public PlayerState_Released(StickInput p_popStick, StickInput p_flipStick, bool p_forwardLoad, float p_invertVel, PlayerController.SetupDir p_setupDir)
	{
		this._popStick = p_popStick;
		this._flipStick = p_flipStick;
		this._forwardLoad = p_forwardLoad;
		this._invertVel = p_invertVel;
		this._setupDir = p_setupDir;
	}

	public PlayerState_Released(StickInput p_popStick, StickInput p_flipStick, Vector2 p_initialFlipDir, float p_flipVel, float p_popVel, float p_toeAxis, float p_popDir, bool p_flipDetected, float p_flip, bool p_forwardLoad, float p_invertVel, PlayerController.SetupDir p_setupDir, bool p_catchRegistered, bool p_leftCaughtFirst, bool p_rightCaughtFirst)
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
		this._forwardLoad = p_forwardLoad;
		this._invertVel = p_invertVel;
		this._setupDir = p_setupDir;
		this._catchRegistered = p_catchRegistered;
		this._leftCaughtFirst = p_leftCaughtFirst;
		this._rightCaughtFirst = p_rightCaughtFirst;
	}

	public PlayerState_Released(StickInput p_popStick, StickInput p_flipStick, Vector2 p_initialFlipDir, float p_flipVel, float p_popVel, float p_toeAxis, float p_popDir, bool p_flipDetected, float p_flip, bool p_forwardLoad, float p_invertVel, PlayerController.SetupDir p_setupDir, bool p_wasGrinding, bool p_catchRegistered, bool p_leftCaughtFirst, bool p_rightCaughtFirst)
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
		this._forwardLoad = p_forwardLoad;
		this._invertVel = p_invertVel;
		this._setupDir = p_setupDir;
		this._wasGrinding = p_wasGrinding;
		this._catchRegistered = p_catchRegistered;
		this._leftCaughtFirst = p_leftCaughtFirst;
		this._rightCaughtFirst = p_rightCaughtFirst;
	}

	public override bool CanGrind()
	{
		if (this._caught)
		{
			return true;
		}
		return false;
	}

	private void CatchBoth()
	{
		PlayerController.Instance.boardController.LeaveFlipMode();
		PlayerController.Instance.SetCatchForwardRotation();
		PlayerController.Instance.SetRightIKLerpTarget(0f, 0f);
		PlayerController.Instance.SetRightSteezeWeight(0f);
		PlayerController.Instance.SetMaxSteezeRight(0f);
		PlayerController.Instance.boardController.ResetAll();
		PlayerController.Instance.SetCatchForwardRotation();
		PlayerController.Instance.SetLeftIKLerpTarget(0f, 0f);
		PlayerController.Instance.SetLeftSteezeWeight(0f);
		PlayerController.Instance.SetMaxSteezeLeft(0f);
		PlayerController.Instance.SetBoardBackwards();
		this._flipDetected = false;
		this._caught = true;
		this._caughtLeft = true;
		this._caughtRight = true;
		if (!this._bothCaught)
		{
			SoundManager.Instance.PlayCatchSound();
			this._bothCaught = true;
		}
		PlayerController.Instance.SetRightIKRotationWeight(1f);
		PlayerController.Instance.SetLeftIKRotationWeight(1f);
		PlayerController.Instance.SetMaxSteeze(0f);
		PlayerController.Instance.AnimCaught(true);
		PlayerController.Instance.AnimRelease(false);
	}

	private bool CatchForwardAngleCheck()
	{
		float single = Vector3.Angle(Vector3.ProjectOnPlane(PlayerController.Instance.boardController.boardTransform.forward, PlayerController.Instance.skaterController.skaterTransform.up), PlayerController.Instance.skaterController.skaterTransform.forward);
		if (single >= 35f && single <= 145f)
		{
			return false;
		}
		return true;
	}

	private void CatchLeft()
	{
		PlayerController.Instance.boardController.LeaveFlipMode();
		this._caughtLeft = true;
		PlayerController.Instance.SetCatchForwardRotation();
		SoundManager.Instance.PlayCatchSound();
		PlayerController.Instance.SetLeftIKLerpTarget(0f, 0f);
		PlayerController.Instance.SetLeftSteezeWeight(0f);
		PlayerController.Instance.SetMaxSteezeLeft(0f);
		PlayerController.Instance.SetBoardBackwards();
		PlayerController.Instance.boardController.ResetAll();
		this._flipDetected = false;
		this._caught = true;
	}

	private void CatchRight()
	{
		PlayerController.Instance.boardController.LeaveFlipMode();
		this._caughtRight = true;
		PlayerController.Instance.SetCatchForwardRotation();
		SoundManager.Instance.PlayCatchSound();
		PlayerController.Instance.SetRightIKLerpTarget(0f, 0f);
		PlayerController.Instance.SetRightSteezeWeight(0f);
		PlayerController.Instance.SetMaxSteezeRight(0f);
		PlayerController.Instance.SetBoardBackwards();
		PlayerController.Instance.boardController.ResetAll();
		this._flipDetected = false;
		this._caught = true;
	}

	private bool CatchUpAngleCheck()
	{
		if (Vector3.Angle(Vector3.ProjectOnPlane(PlayerController.Instance.boardController.boardTransform.up, PlayerController.Instance.skaterController.skaterTransform.forward), PlayerController.Instance.skaterController.skaterTransform.up) < 130f)
		{
			return true;
		}
		return false;
	}

	public override void Enter()
	{
		PromptController.Instance.StateChangePopToRelease();
		if (PlayerController.Instance.inputController.turningMode != InputController.TurningMode.FastLeft && PlayerController.Instance.inputController.turningMode != InputController.TurningMode.FastRight)
		{
			PlayerController.Instance.SetTurningMode(InputController.TurningMode.InAir);
		}
	}

	public override void Exit()
	{
		PlayerController.Instance.AnimSetNoComply(false);
		PlayerController.Instance.SetMaxSteeze(0f);
		PlayerController.Instance.SetLeftIKLerpTarget(0f);
		PlayerController.Instance.SetRightIKLerpTarget(0f);
	}

	public override void FixedUpdate()
	{
		Vector3 velocityOnPop;
		base.FixedUpdate();
		if (!this.isExitingState)
		{
			PlayerController.Instance.comController.UpdateCOM();
			if (!this._caught)
			{
				PlayerController.Instance.SetMaxSteeze(1f);
				PlayerController.Instance.SetBoardTargetPosition(0f);
				PlayerController.Instance.SetFrontPivotRotation(0f);
				PlayerController.Instance.SetBackPivotRotation(0f);
				PlayerController.Instance.SetPivotForwardRotation(0f, 10f);
				PlayerController.Instance.SetPivotSideRotation(0f);
				PlayerController.Instance.FlipTrickRotation();
			}
			else if (!this._caughtLeft || !this._caughtRight)
			{
				PlayerController.Instance.RotateToCatchRotation();
			}
			else
			{
				PlayerController.Instance.SnapRotation((this._lMagnitude + this._rMagnitude) / 2f);
			}
			if (PlayerController.Instance.boardController.triggerManager.IsColliding && PlayerController.Instance.boardController.boardRigidbody.velocity.magnitude > PlayerController.Instance.VelocityOnPop.magnitude)
			{
				Vector3 instance = PlayerController.Instance.boardController.boardRigidbody.velocity;
				Vector3 vector3 = instance.normalized;
				velocityOnPop = PlayerController.Instance.VelocityOnPop;
				instance = vector3 * velocityOnPop.magnitude;
				PlayerController.Instance.boardController.boardRigidbody.velocity = instance;
			}
			if (PlayerController.Instance.boardController.triggerManager.IsColliding && PlayerController.Instance.skaterController.skaterRigidbody.velocity.magnitude > PlayerController.Instance.VelocityOnPop.magnitude)
			{
				Vector3 instance1 = PlayerController.Instance.skaterController.skaterRigidbody.velocity;
				Vector3 vector31 = instance1.normalized;
				velocityOnPop = PlayerController.Instance.VelocityOnPop;
				instance1 = vector31 * velocityOnPop.magnitude;
				PlayerController.Instance.skaterController.skaterRigidbody.velocity = instance1;
			}
		}
	}

	private void ForceCatchBoth()
	{
		this._caughtRight = true;
		this._caughtLeft = true;
		this._flipDetected = false;
		PlayerController.Instance.SetRightIKLerpTarget(0f);
		PlayerController.Instance.SetLeftIKLerpTarget(0f);
		PlayerController.Instance.SetRightIKRotationWeight(1f);
		PlayerController.Instance.SetLeftIKRotationWeight(1f);
		PlayerController.Instance.SetBoardBackwards();
		PlayerController.Instance.boardController.ResetAll();
		PlayerController.Instance.SetMaxSteeze(0f);
		PlayerController.Instance.AnimCaught(true);
		PlayerController.Instance.AnimRelease(false);
	}

	public override StickInput GetPopStick()
	{
		return this._popStick;
	}

	public override bool IsInImpactState()
	{
		return this._predictedCollision;
	}

	public override void OnAllWheelsDown()
	{
		this.TransitionToNextState();
	}

	public override void OnCollisionStayEvent()
	{
	}

	public override void OnGrindDetected()
	{
		if (this._caught)
		{
			PlayerController.Instance.AnimSetManual(false, PlayerController.Instance.AnimGetManualAxis());
			PlayerController.Instance.AnimSetNoseManual(false, PlayerController.Instance.AnimGetManualAxis());
			base.DoTransition(typeof(PlayerState_Grinding), null);
		}
	}

	public override void OnLeftStickCenteredUpdate()
	{
		if (!this._leftCentered)
		{
			this._leftCentered = true;
			if (this._rightCentered)
			{
				this._rightCaughtFirst = true;
				this._catchRegistered = true;
			}
		}
	}

	public override void OnManualExit()
	{
		PlayerController.Instance.AnimSetManual(false, PlayerController.Instance.AnimGetManualAxis());
		this._manualling = false;
	}

	public override void OnManualUpdate(StickInput p_popStick, StickInput p_flipStick)
	{
		if ((this._caughtLeft || this._caughtRight) && this._canManual)
		{
			this.ForceCatchBoth();
			PlayerController.Instance.AnimSetManual(true, Mathf.Lerp(PlayerController.Instance.AnimGetManualAxis(), p_popStick.ForwardDir, Time.deltaTime * 10f));
			this._manualling = true;
			this._noseManualling = false;
			this._popStick = p_popStick;
			this._flipStick = p_flipStick;
		}
	}

	public override void OnNextState()
	{
		base.DoTransition(typeof(PlayerState_InAir), null);
	}

	public override void OnNoseManualExit()
	{
		PlayerController.Instance.AnimSetNoseManual(false, PlayerController.Instance.AnimGetManualAxis());
		this._noseManualling = false;
	}

	public override void OnNoseManualUpdate(StickInput p_popStick, StickInput p_flipStick)
	{
		if ((this._caughtLeft || this._caughtRight) && this._canManual)
		{
			this.ForceCatchBoth();
			PlayerController.Instance.AnimSetNoseManual(true, Mathf.Lerp(PlayerController.Instance.AnimGetManualAxis(), p_popStick.ForwardDir, Time.deltaTime * 10f));
			this._popStick = p_popStick;
			this._flipStick = p_flipStick;
			this._noseManualling = true;
			this._manualling = false;
		}
	}

	public override void OnPredictedCollisionEvent()
	{
		this.PredictedNextState();
	}

	public override void OnPreLandingEvent()
	{
		this._canManual = true;
	}

	public override void OnRightStickCenteredUpdate()
	{
		if (!this._rightCentered)
		{
			this._rightCentered = true;
			if (this._leftCentered)
			{
				this._leftCaughtFirst = true;
				this._catchRegistered = true;
			}
		}
	}

	public override void OnStickFixedUpdate(StickInput p_leftStick, StickInput p_rightStick)
	{
		if (!this._caughtLeft || !this._caughtRight)
		{
			this._leftToeAxis = (this._caughtLeft ? p_leftStick.ToeAxis : 0f);
			this._rightToeAxis = (this._caughtRight ? p_rightStick.ToeAxis : 0f);
			this._leftForwardDir = (this._caughtLeft ? p_leftStick.ForwardDir : 0f);
			this._rightForwardDir = (this._caughtRight ? p_rightStick.ForwardDir : 0f);
			switch (SettingsManager.Instance.controlType)
			{
				case SettingsManager.ControlType.Same:
				{
					if (SettingsManager.Instance.stance == SettingsManager.Stance.Regular)
					{
						PlayerController.Instance.SetFrontPivotRotation(this._rightToeAxis);
						PlayerController.Instance.SetBackPivotRotation(this._leftToeAxis);
						PlayerController.Instance.SetPivotForwardRotation((this._leftForwardDir + this._rightForwardDir) * 0.7f, 15f);
						PlayerController.Instance.SetPivotSideRotation(this._leftToeAxis - this._rightToeAxis);
						return;
					}
					PlayerController.Instance.SetFrontPivotRotation(this._leftToeAxis);
					PlayerController.Instance.SetBackPivotRotation(-this._rightToeAxis);
					PlayerController.Instance.SetPivotForwardRotation((p_leftStick.ForwardDir + p_rightStick.ForwardDir) * 0.7f, 15f);
					PlayerController.Instance.SetPivotSideRotation(this._leftToeAxis - this._rightToeAxis);
					return;
				}
				case SettingsManager.ControlType.Swap:
				{
					if (SettingsManager.Instance.stance == SettingsManager.Stance.Regular)
					{
						PlayerController.Instance.SetFrontPivotRotation(this._rightToeAxis);
						PlayerController.Instance.SetBackPivotRotation(this._leftToeAxis);
						PlayerController.Instance.SetPivotForwardRotation((this._leftForwardDir + this._rightForwardDir) * 0.7f, 15f);
						PlayerController.Instance.SetPivotSideRotation(this._leftToeAxis - this._rightToeAxis);
						return;
					}
					PlayerController.Instance.SetFrontPivotRotation(this._leftToeAxis);
					PlayerController.Instance.SetBackPivotRotation(this._rightToeAxis);
					PlayerController.Instance.SetPivotForwardRotation((this._leftForwardDir + this._rightForwardDir) * 0.7f, 15f);
					PlayerController.Instance.SetPivotSideRotation(this._leftToeAxis - this._rightToeAxis);
					return;
				}
				case SettingsManager.ControlType.Simple:
				{
					if (!PlayerController.Instance.IsSwitch)
					{
						if (SettingsManager.Instance.stance == SettingsManager.Stance.Regular)
						{
							PlayerController.Instance.SetFrontPivotRotation(this._rightToeAxis);
							PlayerController.Instance.SetBackPivotRotation(this._leftToeAxis);
							PlayerController.Instance.SetPivotForwardRotation((this._leftForwardDir + this._rightForwardDir) * 0.7f, 15f);
							PlayerController.Instance.SetPivotSideRotation(this._leftToeAxis - this._rightToeAxis);
							return;
						}
						PlayerController.Instance.SetFrontPivotRotation(this._rightToeAxis);
						PlayerController.Instance.SetBackPivotRotation(this._leftToeAxis);
						PlayerController.Instance.SetPivotForwardRotation((this._leftForwardDir + this._rightForwardDir) * 0.7f, 15f);
						PlayerController.Instance.SetPivotSideRotation(this._leftToeAxis - this._rightToeAxis);
						return;
					}
					if (SettingsManager.Instance.stance == SettingsManager.Stance.Regular)
					{
						PlayerController.Instance.SetFrontPivotRotation(this._leftToeAxis);
						PlayerController.Instance.SetBackPivotRotation(this._rightToeAxis);
						PlayerController.Instance.SetPivotForwardRotation((this._leftForwardDir + this._rightForwardDir) * 0.7f, 15f);
						PlayerController.Instance.SetPivotSideRotation(this._leftToeAxis - this._rightToeAxis);
						return;
					}
					PlayerController.Instance.SetFrontPivotRotation(this._leftToeAxis);
					PlayerController.Instance.SetBackPivotRotation(this._rightToeAxis);
					PlayerController.Instance.SetPivotForwardRotation((this._leftForwardDir + this._rightForwardDir) * 0.7f, 15f);
					PlayerController.Instance.SetPivotSideRotation(this._leftToeAxis - this._rightToeAxis);
					return;
				}
				default:
				{
					return;
				}
			}
		}
		switch (SettingsManager.Instance.controlType)
		{
			case SettingsManager.ControlType.Same:
			{
				if (SettingsManager.Instance.stance == SettingsManager.Stance.Regular)
				{
					PlayerController.Instance.SetBoardTargetPosition(Mathf.Abs(p_leftStick.rawInput.pos.x) - Mathf.Abs(p_rightStick.rawInput.pos.x));
					PlayerController.Instance.SetFrontPivotRotation(p_rightStick.ToeAxis);
					PlayerController.Instance.SetBackPivotRotation(p_leftStick.ToeAxis);
					PlayerController.Instance.SetPivotForwardRotation((p_leftStick.ForwardDir + p_rightStick.ForwardDir) * 0.7f, 15f);
					PlayerController.Instance.SetPivotSideRotation(p_leftStick.ToeAxis - p_rightStick.ToeAxis);
					return;
				}
				PlayerController.Instance.SetBoardTargetPosition(Mathf.Abs(p_rightStick.rawInput.pos.x) - Mathf.Abs(p_leftStick.rawInput.pos.x));
				PlayerController.Instance.SetFrontPivotRotation(-p_leftStick.ToeAxis);
				PlayerController.Instance.SetBackPivotRotation(-p_rightStick.ToeAxis);
				PlayerController.Instance.SetPivotForwardRotation((p_leftStick.ForwardDir + p_rightStick.ForwardDir) * 0.7f, 15f);
				PlayerController.Instance.SetPivotSideRotation(p_leftStick.ToeAxis - p_rightStick.ToeAxis);
				return;
			}
			case SettingsManager.ControlType.Swap:
			{
				if (SettingsManager.Instance.stance == SettingsManager.Stance.Regular)
				{
					PlayerController.Instance.SetBoardTargetPosition(Mathf.Abs(p_leftStick.rawInput.pos.x) - Mathf.Abs(p_rightStick.rawInput.pos.x));
					PlayerController.Instance.SetFrontPivotRotation(p_rightStick.ToeAxis);
					PlayerController.Instance.SetBackPivotRotation(p_leftStick.ToeAxis);
					PlayerController.Instance.SetPivotForwardRotation((p_leftStick.ForwardDir + p_rightStick.ForwardDir) * 0.7f, 15f);
					PlayerController.Instance.SetPivotSideRotation(p_leftStick.ToeAxis - p_rightStick.ToeAxis);
					return;
				}
				PlayerController.Instance.SetBoardTargetPosition(Mathf.Abs(p_rightStick.rawInput.pos.x) - Mathf.Abs(p_leftStick.rawInput.pos.x));
				PlayerController.Instance.SetFrontPivotRotation(p_leftStick.ToeAxis);
				PlayerController.Instance.SetBackPivotRotation(p_rightStick.ToeAxis);
				PlayerController.Instance.SetPivotForwardRotation((p_leftStick.ForwardDir + p_rightStick.ForwardDir) * 0.7f, 15f);
				PlayerController.Instance.SetPivotSideRotation(p_leftStick.ToeAxis - p_rightStick.ToeAxis);
				return;
			}
			case SettingsManager.ControlType.Simple:
			{
				if (!PlayerController.Instance.IsSwitch)
				{
					if (SettingsManager.Instance.stance == SettingsManager.Stance.Regular)
					{
						PlayerController.Instance.SetBoardTargetPosition(Mathf.Abs(p_leftStick.rawInput.pos.x) - Mathf.Abs(p_rightStick.rawInput.pos.x));
						PlayerController.Instance.SetFrontPivotRotation(p_rightStick.ToeAxis);
						PlayerController.Instance.SetBackPivotRotation(p_leftStick.ToeAxis);
						PlayerController.Instance.SetPivotForwardRotation((p_leftStick.ForwardDir + p_rightStick.ForwardDir) * 0.7f, 15f);
						PlayerController.Instance.SetPivotSideRotation(p_leftStick.ToeAxis - p_rightStick.ToeAxis);
						return;
					}
					PlayerController.Instance.SetBoardTargetPosition(Mathf.Abs(p_leftStick.rawInput.pos.x) - Mathf.Abs(p_rightStick.rawInput.pos.x));
					PlayerController.Instance.SetFrontPivotRotation(p_rightStick.ToeAxis);
					PlayerController.Instance.SetBackPivotRotation(p_leftStick.ToeAxis);
					PlayerController.Instance.SetPivotForwardRotation((p_leftStick.ForwardDir + p_rightStick.ForwardDir) * 0.7f, 15f);
					PlayerController.Instance.SetPivotSideRotation(p_leftStick.ToeAxis - p_rightStick.ToeAxis);
					return;
				}
				if (SettingsManager.Instance.stance == SettingsManager.Stance.Regular)
				{
					PlayerController.Instance.SetBoardTargetPosition(Mathf.Abs(p_leftStick.rawInput.pos.x) - Mathf.Abs(p_rightStick.rawInput.pos.x));
					PlayerController.Instance.SetFrontPivotRotation(p_leftStick.ToeAxis);
					PlayerController.Instance.SetBackPivotRotation(p_rightStick.ToeAxis);
					PlayerController.Instance.SetPivotForwardRotation((p_leftStick.ForwardDir + p_rightStick.ForwardDir) * 0.7f, 15f);
					PlayerController.Instance.SetPivotSideRotation(p_leftStick.ToeAxis - p_rightStick.ToeAxis);
					return;
				}
				PlayerController.Instance.SetBoardTargetPosition(Mathf.Abs(p_leftStick.rawInput.pos.x) - Mathf.Abs(p_rightStick.rawInput.pos.x));
				PlayerController.Instance.SetFrontPivotRotation(p_leftStick.ToeAxis);
				PlayerController.Instance.SetBackPivotRotation(p_rightStick.ToeAxis);
				PlayerController.Instance.SetPivotForwardRotation((p_leftStick.ForwardDir + p_rightStick.ForwardDir) * 0.7f, 15f);
				PlayerController.Instance.SetPivotSideRotation(p_leftStick.ToeAxis - p_rightStick.ToeAxis);
				return;
			}
			default:
			{
				return;
			}
		}
	}

	public override void OnStickPressed(bool p_right)
	{
		PromptController.Instance.catchCount++;
		if (!p_right)
		{
			this._caughtLeft = true;
			if (!this._caught)
			{
				PlayerController.Instance.boardController.LeaveFlipMode();
				PlayerController.Instance.SetCatchForwardRotation();
				SoundManager.Instance.PlayCatchSound();
				PlayerController.Instance.SetLeftIKLerpTarget(0f, 0f);
				PlayerController.Instance.SetLeftSteezeWeight(0f);
				PlayerController.Instance.SetMaxSteezeLeft(0f);
				PlayerController.Instance.SetBoardBackwards();
				PlayerController.Instance.boardController.ResetAll();
				this._flipDetected = false;
				this._caught = true;
			}
		}
		else
		{
			this._caughtRight = true;
			if (!this._caught)
			{
				PlayerController.Instance.boardController.LeaveFlipMode();
				PlayerController.Instance.SetCatchForwardRotation();
				SoundManager.Instance.PlayCatchSound();
				PlayerController.Instance.SetRightIKLerpTarget(0f, 0f);
				PlayerController.Instance.SetRightSteezeWeight(0f);
				PlayerController.Instance.SetMaxSteezeRight(0f);
				PlayerController.Instance.SetBoardBackwards();
				PlayerController.Instance.boardController.ResetAll();
				this._flipDetected = false;
				this._caught = true;
			}
		}
		if (this._caughtLeft && this._caughtRight)
		{
			if (!this._bothCaught)
			{
				SoundManager.Instance.PlayCatchSound();
				this._bothCaught = true;
			}
			PlayerController.Instance.SetRightIKRotationWeight(1f);
			PlayerController.Instance.SetLeftIKRotationWeight(1f);
			PlayerController.Instance.SetMaxSteeze(0f);
			PlayerController.Instance.AnimCaught(true);
			PlayerController.Instance.AnimRelease(false);
		}
	}

	public override void OnStickUpdate(StickInput p_leftStick, StickInput p_rightStick)
	{
		this._lMagnitude = p_leftStick.rawInput.pos.magnitude;
		this._rMagnitude = p_rightStick.rawInput.pos.magnitude;
		PlayerController.Instance.SetLeftIKOffset(p_leftStick.ToeAxis, p_leftStick.ForwardDir, p_leftStick.PopDir, p_leftStick.IsPopStick, true, PlayerController.Instance.GetAnimReleased());
		PlayerController.Instance.SetRightIKOffset(p_rightStick.ToeAxis, p_rightStick.ForwardDir, p_rightStick.PopDir, p_rightStick.IsPopStick, true, PlayerController.Instance.GetAnimReleased());
	}

	public override bool Popped()
	{
		return true;
	}

	private void PredictedNextState()
	{
		this._predictedCollision = true;
		this.isExitingState = true;
		if (!this._caught)
		{
			PromptController.Instance.StateChangeReleaseToBail();
			PlayerController.Instance.ForceBail();
			return;
		}
		this.CatchBoth();
		if (this._manualling || this._noseManualling)
		{
			PlayerController.Instance.SetRightIKRotationWeight(1f);
			PlayerController.Instance.SetLeftIKRotationWeight(1f);
			PlayerController.Instance.SetMaxSteeze(0f);
			PlayerController.Instance.SetLeftIKLerpTarget(0f);
			PlayerController.Instance.SetRightIKLerpTarget(0f);
			PlayerController.Instance.animationController.ScaleAnimSpeed(1f);
			PlayerController.Instance.SetBoardBackwards();
			PlayerController.Instance.boardController.ResetAll();
			PlayerController.Instance.AnimRelease(false);
			PlayerController.Instance.SetBoardToMaster();
			PlayerController.Instance.SetTurningMode(InputController.TurningMode.Grounded);
			PlayerController.Instance.AnimOllieTransition(false);
			PlayerController.Instance.AnimSetupTransition(false);
			object[] objArray = new object[] { this._popStick, this._flipStick, !this._noseManualling };
			base.DoTransition(typeof(PlayerState_Manualling), objArray);
		}
	}

	private void TransitionToNextState()
	{
		this.isExitingState = true;
		if (!this._caught)
		{
			PromptController.Instance.StateChangeReleaseToBail();
			PlayerController.Instance.ForceBail();
			return;
		}
		PlayerController.Instance.SetRightIKRotationWeight(1f);
		PlayerController.Instance.SetLeftIKRotationWeight(1f);
		if (!this._manualling && !this._noseManualling)
		{
			PlayerController.Instance.SetMaxSteeze(0f);
			PlayerController.Instance.SetLeftIKLerpTarget(0f);
			PlayerController.Instance.SetRightIKLerpTarget(0f);
			PlayerController.Instance.SetBoardBackwards();
			PlayerController.Instance.boardController.ResetAll();
			PlayerController.Instance.AnimRelease(false);
			PlayerController.Instance.SetBoardToMaster();
			PlayerController.Instance.SetTurningMode(InputController.TurningMode.Grounded);
			PlayerController.Instance.AnimOllieTransition(false);
			PlayerController.Instance.AnimSetupTransition(false);
			PlayerController.Instance.ResetAnimationsAfterImpact();
			PlayerController.Instance.AnimLandedEarly(true);
			PlayerController.Instance.AnimSetManual(false, PlayerController.Instance.AnimGetManualAxis());
			this._manualling = false;
			PlayerController.Instance.AnimSetNoseManual(false, PlayerController.Instance.AnimGetManualAxis());
			this._noseManualling = false;
			base.DoTransition(typeof(PlayerState_Impact), null);
			return;
		}
		PlayerController.Instance.SetMaxSteeze(0f);
		PlayerController.Instance.SetLeftIKLerpTarget(0f);
		PlayerController.Instance.SetRightIKLerpTarget(0f);
		PlayerController.Instance.animationController.ScaleAnimSpeed(1f);
		PlayerController.Instance.SetBoardBackwards();
		PlayerController.Instance.boardController.ResetAll();
		PlayerController.Instance.AnimRelease(false);
		PlayerController.Instance.SetBoardToMaster();
		PlayerController.Instance.SetTurningMode(InputController.TurningMode.Grounded);
		PlayerController.Instance.AnimOllieTransition(false);
		PlayerController.Instance.AnimSetupTransition(false);
		object[] objArray = new object[] { this._popStick, this._flipStick, !this._noseManualling };
		base.DoTransition(typeof(PlayerState_Manualling), objArray);
	}

	public override void Update()
	{
		base.Update();
		if (!this._timerEnded)
		{
			if (this._timer > 0.15f && this.CatchForwardAngleCheck() && this.CatchUpAngleCheck())
			{
				this._timerEnded = true;
			}
			if (this._timer >= 0.3f)
			{
				this._timerEnded = true;
			}
			else
			{
				this._timer += Time.deltaTime;
			}
		}
		else if (!this._caught && this._catchRegistered)
		{
			this.CatchBoth();
		}
		if (!this._caught && PlayerController.Instance.DistanceToBoardTarget() > 0.4f)
		{
			PlayerController.Instance.ForceBail();
		}
	}
}