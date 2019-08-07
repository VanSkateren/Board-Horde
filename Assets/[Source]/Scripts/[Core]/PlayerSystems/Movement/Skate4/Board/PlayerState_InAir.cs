using Dreamteck.Splines;
using FSMHelper;
using System;
using UnityEngine;

public class PlayerState_InAir : PlayerState_OnBoard
{
	private bool _predictedCollision;

	private bool _manualling;

	private bool _noseManualling;

	private float _grindExitTimer;

	private bool _wasGrinding;

	private float _boardCenteredTimer;

	private StickInput _popStick;

	private StickInput _flipStick;

	private bool _canGrind = true;

	private SplineComputer _spline;

	private bool _caughtLeft = true;

	private bool _caughtRight = true;

	private bool _caughtBoth = true;

	private bool _popped;

	private float _leftToeAxis;

	private float _rightToeAxis;

	private float _leftForwardDir;

	private float _rightForwardDir;

	public PlayerState_InAir()
	{
	}

	public PlayerState_InAir(int p_popped)
	{
		this._popped = true;
	}

	public PlayerState_InAir(bool p_wasGrinding)
	{
		this._wasGrinding = p_wasGrinding;
	}

	public PlayerState_InAir(bool p_wasGrinding, int p_popped)
	{
		this._wasGrinding = p_wasGrinding;
		this._popped = true;
	}

	public PlayerState_InAir(bool p_wasGrinding, bool p_caughtLeft, bool p_caughtRight, bool p_popped)
	{
		this._popped = p_popped;
		this._wasGrinding = p_wasGrinding;
		this._caughtLeft = p_caughtLeft;
		this._caughtRight = p_caughtRight;
		if (!this._caughtRight && this._caughtLeft)
		{
			PlayerController.Instance.SetRightIKLerpTarget(1f, 1f);
			PlayerController.Instance.SetRightSteezeWeight(1f);
			PlayerController.Instance.SetMaxSteezeRight(1f);
			this._caughtBoth = false;
		}
		if (!this._caughtLeft && this._caughtRight)
		{
			PlayerController.Instance.SetLeftIKLerpTarget(1f, 1f);
			PlayerController.Instance.SetLeftSteezeWeight(1f);
			PlayerController.Instance.SetMaxSteezeLeft(1f);
			this._caughtBoth = false;
		}
	}

	public PlayerState_InAir(bool p_wasGrinding, bool p_canGrind)
	{
		this._wasGrinding = p_wasGrinding;
		this._canGrind = p_canGrind;
	}

	public PlayerState_InAir(bool p_wasGrinding, bool p_canGrind, SplineComputer p_spline)
	{
		this._wasGrinding = p_wasGrinding;
		this._canGrind = p_canGrind;
		this._spline = p_spline;
	}

	public override bool CanGrind()
	{
		return this._canGrind;
	}

	private void CatchBoth()
	{
		PlayerController.Instance.SetRightIKLerpTarget(0f, 0f);
		PlayerController.Instance.SetRightSteezeWeight(0f);
		PlayerController.Instance.SetMaxSteezeRight(0f);
		PlayerController.Instance.boardController.ResetAll();
		PlayerController.Instance.SetCatchForwardRotation();
		PlayerController.Instance.SetLeftIKLerpTarget(0f, 0f);
		PlayerController.Instance.SetLeftSteezeWeight(0f);
		PlayerController.Instance.SetMaxSteezeLeft(0f);
		this._caughtLeft = true;
		this._caughtRight = true;
		this._caughtBoth = true;
		PlayerController.Instance.SetRightIKRotationWeight(1f);
		PlayerController.Instance.SetLeftIKRotationWeight(1f);
		PlayerController.Instance.SetMaxSteeze(0f);
	}

	public override void Enter()
	{
		if (PlayerController.Instance.inputController.turningMode != InputController.TurningMode.FastLeft && PlayerController.Instance.inputController.turningMode != InputController.TurningMode.FastRight)
		{
			if (PlayerController.Instance.movementMaster == PlayerController.MovementMaster.Skater)
			{
				PlayerController.Instance.SetTurningMode(InputController.TurningMode.InAir);
				return;
			}
			if (PlayerController.Instance.movementMaster == PlayerController.MovementMaster.Board)
			{
				PlayerController.Instance.SetTurningMode(InputController.TurningMode.Grounded);
			}
		}
	}

	public override void Exit()
	{
		PlayerController.Instance.AnimSetNoComply(false);
	}

	public override void FixedUpdate()
	{
		Vector3 velocityOnPop;
		PlayerController.Instance.comController.UpdateCOM();
		base.FixedUpdate();
		PlayerController.Instance.boardController.ApplyOnBoardMaxRoll();
		PlayerController.Instance.SnapRotation();
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

	public override bool IsInImpactState()
	{
		return this._predictedCollision;
	}

	public override bool LeftFootOff()
	{
		return !this._caughtLeft;
	}

	public override void OnAllWheelsDown()
	{
		this.TransitionToNextState();
	}

	public override void OnCollisionEnterEvent()
	{
	}

	public override void OnCollisionStayEvent()
	{
	}

	public override void OnFirstWheelDown()
	{
		if (this._wasGrinding)
		{
			this.SwitchToBoardMaster();
			return;
		}
		this.TransitionToNextState();
	}

	public override void OnGrindDetected()
	{
		if (this._canGrind)
		{
			base.DoTransition(typeof(PlayerState_Grinding), null);
		}
	}

	public override void OnManualExit()
	{
		this._manualling = false;
		PlayerController.Instance.AnimSetManual(false, PlayerController.Instance.AnimGetManualAxis());
	}

	public override void OnManualUpdate(StickInput p_popStick, StickInput p_flipStick)
	{
		if (!this._wasGrinding)
		{
			PlayerController.Instance.AnimSetManual(true, Mathf.Lerp(PlayerController.Instance.AnimGetManualAxis(), p_popStick.ForwardDir, Time.deltaTime * 10f));
			this._manualling = true;
			this._noseManualling = false;
			this._popStick = p_popStick;
			this._flipStick = p_flipStick;
		}
	}

	public override void OnNoseManualExit()
	{
		this._noseManualling = false;
		PlayerController.Instance.AnimSetNoseManual(false, PlayerController.Instance.AnimGetManualAxis());
	}

	public override void OnNoseManualUpdate(StickInput p_popStick, StickInput p_flipStick)
	{
		if (!this._wasGrinding)
		{
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

	public override void OnStickFixedUpdate(StickInput p_leftStick, StickInput p_rightStick)
	{
		if (!this._caughtLeft || !this._caughtRight)
		{
			this._leftToeAxis = (this._caughtLeft ? p_leftStick.ToeAxis * 1.5f : 0f);
			this._rightToeAxis = (this._caughtRight ? p_rightStick.ToeAxis * 1.5f : 0f);
			this._leftForwardDir = (this._caughtLeft ? p_leftStick.ForwardDir * 2f : 0f);
			this._rightForwardDir = (this._caughtRight ? p_rightStick.ForwardDir * 2f : 0f);
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
					PlayerController.Instance.SetFrontPivotRotation(-this._leftToeAxis);
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
		if (!p_right)
		{
			if (this._caughtLeft && this._caughtBoth)
			{
				PlayerController.Instance.SetLeftIKLerpTarget(1f, 1f);
				PlayerController.Instance.SetLeftSteezeWeight(1f);
				PlayerController.Instance.SetMaxSteezeLeft(1f);
				this._caughtLeft = false;
				this._caughtBoth = false;
				return;
			}
			if (this._caughtRight && !this._caughtLeft)
			{
				SoundManager.Instance.PlayCatchSound();
				PlayerController.Instance.SetLeftIKLerpTarget(0f, 0f);
				PlayerController.Instance.SetLeftSteezeWeight(0f);
				PlayerController.Instance.SetMaxSteezeLeft(0f);
				this._caughtLeft = true;
				this._caughtBoth = true;
			}
		}
		else
		{
			if (this._caughtRight && this._caughtBoth)
			{
				PlayerController.Instance.SetRightIKLerpTarget(1f, 1f);
				PlayerController.Instance.SetRightSteezeWeight(1f);
				PlayerController.Instance.SetMaxSteezeRight(1f);
				this._caughtRight = false;
				this._caughtBoth = false;
				return;
			}
			if (!this._caughtRight && this._caughtLeft)
			{
				SoundManager.Instance.PlayCatchSound();
				PlayerController.Instance.SetRightIKLerpTarget(0f, 0f);
				PlayerController.Instance.SetRightSteezeWeight(0f);
				PlayerController.Instance.SetMaxSteezeRight(0f);
				this._caughtRight = true;
				this._caughtBoth = true;
				return;
			}
		}
	}

	public override void OnStickUpdate(StickInput p_leftStick, StickInput p_rightStick)
	{
		PlayerController.Instance.SetLeftIKOffset(p_leftStick.ToeAxis, p_leftStick.ForwardDir, p_leftStick.PopDir, p_leftStick.IsPopStick, true, false);
		PlayerController.Instance.SetRightIKOffset(p_rightStick.ToeAxis, p_rightStick.ForwardDir, p_rightStick.PopDir, p_rightStick.IsPopStick, true, false);
	}

	public override bool Popped()
	{
		return this._popped;
	}

	private void PredictedNextState()
	{
		this._predictedCollision = true;
		this.CatchBoth();
		if (this._manualling || this._noseManualling)
		{
			object[] objArray = new object[] { this._popStick, this._flipStick, !this._noseManualling };
			base.DoTransition(typeof(PlayerState_Manualling), objArray);
		}
	}

	public override bool RightFootOff()
	{
		return !this._caughtRight;
	}

	private void SwitchToBoardMaster()
	{
		PlayerController.Instance.SetBoardToMaster();
		PlayerController.Instance.SetTurningMode(InputController.TurningMode.Grounded);
	}

	private void TransitionToNextState()
	{
		if (this._manualling || this._noseManualling)
		{
			object[] objArray = new object[] { this._popStick, this._flipStick, !this._noseManualling };
			base.DoTransition(typeof(PlayerState_Manualling), objArray);
			return;
		}
		this._manualling = false;
		this._noseManualling = false;
		PlayerController.Instance.AnimSetManual(false, PlayerController.Instance.AnimGetManualAxis());
		PlayerController.Instance.AnimSetNoseManual(false, PlayerController.Instance.AnimGetManualAxis());
		object[] objArray1 = new object[] { this._canGrind };
		PlayerController.Instance.Impact();
		base.DoTransition(typeof(PlayerState_Impact), objArray1);
	}

	public override void Update()
	{
		base.Update();
		if (this._wasGrinding)
		{
			this._grindExitTimer += Time.deltaTime;
			if (this._grindExitTimer > 0.2f)
			{
				this._wasGrinding = false;
				this._grindExitTimer = 0f;
			}
		}
		if (PlayerController.Instance.movementMaster == PlayerController.MovementMaster.Board)
		{
			PlayerController.Instance.SetTurningMode(InputController.TurningMode.Grounded);
			this._boardCenteredTimer += Time.deltaTime;
			if (this._boardCenteredTimer > 0.2f)
			{
				this._boardCenteredTimer = 0f;
				PlayerController.Instance.SetTurningMode(InputController.TurningMode.InAir);
				PlayerController.Instance.SetSkaterToMaster();
			}
		}
	}
}