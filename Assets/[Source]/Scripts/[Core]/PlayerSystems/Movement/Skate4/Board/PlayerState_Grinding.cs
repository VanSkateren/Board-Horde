using Dreamteck.Splines;
using FSMHelper;
using System;
using UnityEngine;

public class PlayerState_Grinding : PlayerState_OnBoard
{
	private float _tweak;

	private StickInput _popStick;

	private StickInput _flipStick;

	private bool _forwardLoad;

	private bool _flipDetected;

	private bool _potentialFlip;

	private Vector2 _initialFlipDir = Vector2.zero;

	private float _toeAxis;

	private float _flipVel;

	private float _popVel;

	private float _popDir;

	private float _flip;

	private float _popForce = 2f;

	private bool _popped;

	private float _invertVel;

	private PlayerController.SetupDir _setupDir;

	private Vector3 _lastVelocity = Vector3.zero;

	private float _grindTimer;

	private bool _waitDone;

	private float _augmentedLeftAngle;

	private float _augmentedRightAngle;

	private float _initialVelocityMagnitude;

	private int _flipFrameCount;

	private int _flipFrameMax = 25;

	private float _leftTrigger;

	private float _rightTrigger;

	private float _triggerDif;

	private float _leftTriggerTimer;

	private float _rightTriggerTimer;

	private float _bothTriggerTimer;

	private bool _canGrind = true;

	private SplineComputer _spline;

	private bool _wasSliding;

	private Vector3 _grindDirection = Vector3.zero;

	private float _flipWindowTimer;

	private float _grindTime;

	private Vector3 _vel;

	private Vector3 _angVel;

	private float _horizontalTarget;

	private float _horizontalLerp;

	private float _verticalTarget;

	private float _verticalLerp;

	private float _playerTurn;

	public PlayerState_Grinding()
	{
	}

	public override bool CanGrind()
	{
		return this._canGrind;
	}

	public override void Enter()
	{
		int num;
		this._grindDirection = PlayerController.Instance.boardController.triggerManager.grindDirection;
		this._lastVelocity = PlayerController.Instance.VelocityOnPop;
		if (PlayerController.Instance.boardController.boardRigidbody.velocity.magnitude > this._lastVelocity.magnitude)
		{
			PlayerController.Instance.boardController.boardRigidbody.velocity = this._grindDirection.normalized * this._lastVelocity.magnitude;
		}
		PlayerController.Instance.SetIKRigidbodyKinematic(false);
		PlayerController.Instance.boardController.ResetAll();
		SoundManager instance = SoundManager.Instance;
		num = (PlayerController.Instance.boardController.isSliding ? 1 : PlayerController.Instance.boardController.GetGrindSoundInt());
		Vector3 vector3 = Vector3.ProjectOnPlane(PlayerController.Instance.boardController.boardRigidbody.velocity, Vector3.up);
		instance.PlayGrindSound(num, vector3.magnitude);
		this._initialVelocityMagnitude = this._lastVelocity.magnitude;
		this._wasSliding = PlayerController.Instance.boardController.isSliding;
		PlayerController.Instance.playerRotationReference.rotation = PlayerController.Instance.skaterController.skaterTransform.rotation;
		PlayerController.Instance.boardController.triggerManager.sideEnteredGrind = (PlayerController.Instance.IsRightSideOfGrind() ? TriggerManager.SideEnteredGrind.Right : TriggerManager.SideEnteredGrind.Left);
		PlayerController.Instance.boardController.triggerManager.grindDetection.grindSide = (PlayerController.Instance.IsBacksideGrind() ? GrindDetection.GrindSide.Backside : GrindDetection.GrindSide.Frontside);
		this._tweak = PlayerController.Instance.GetLastTweakAxis();
		PlayerController.Instance.SetGrindTweakAxis(this._tweak);
		PlayerController.Instance.SetTurnMultiplier(1f);
		PlayerController.Instance.SetTurningMode(InputController.TurningMode.None);
		PlayerController.Instance.AnimGrindTransition(true);
		PlayerController.Instance.CrossFadeAnimation("GrindImpact", 0.2f);
		PlayerController.Instance.AnimOllieTransition(false);
		PlayerController.Instance.AnimSetupTransition(false);
		PlayerController.Instance.AnimRelease(false);
		PlayerController.Instance.SetBoardToMaster();
		PlayerController.Instance.SetTurningMode(InputController.TurningMode.Grounded);
		PlayerController.Instance.SetLeftIKLerpTarget(0f);
		PlayerController.Instance.SetRightIKLerpTarget(0f);
		PlayerController.Instance.animationController.ScaleAnimSpeed(1f);
		PlayerController.Instance.SetGrindPIDRotationValues();
		PlayerController.Instance.BoardGrindRotation = PlayerController.Instance.boardController.boardRigidbody.rotation;
		this._vel = PlayerController.Instance.boardController.boardRigidbody.velocity;
		this._angVel = PlayerController.Instance.boardController.boardRigidbody.angularVelocity;
		if (!Mathd.Vector3IsInfinityOrNan(PlayerController.Instance.GetGrindContactPosition()))
		{
			PlayerController.Instance.SetBoardCenterOfMass(PlayerController.Instance.boardController.boardTransform.InverseTransformPoint(PlayerController.Instance.GetGrindContactPosition()));
		}
		PlayerController.Instance.boardController.boardRigidbody.velocity = this._vel;
		PlayerController.Instance.boardController.boardRigidbody.angularVelocity = this._angVel;
	}

	public override void Exit()
	{
		if (this._grindTime < 0.2f && (PlayerController.Instance.boardController.boardRigidbody.velocity.magnitude > this._initialVelocityMagnitude + 0.5f || PlayerController.Instance.skaterController.skaterRigidbody.velocity.magnitude > this._initialVelocityMagnitude))
		{
			PlayerController.Instance.boardController.boardRigidbody.velocity = this._grindDirection.normalized * this._initialVelocityMagnitude;
			PlayerController.Instance.skaterController.skaterRigidbody.velocity = this._grindDirection.normalized * this._initialVelocityMagnitude;
		}
		PlayerController.Instance.boardController.triggerManager.spline = null;
		SoundManager.Instance.StopGrindSound((!Mathd.IsInfinityOrNaN(this._lastVelocity.magnitude) ? this._lastVelocity.magnitude : 0f));
		PlayerController.Instance.ResetPIDRotationValues();
		PlayerController.Instance.ResetBoardCenterOfMass();
		if (!this._popped)
		{
			PlayerController.Instance.AnimSetPopStrength(0f);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		PlayerController.Instance.comController.UpdateCOM(0.89f, 0);
		PlayerController.Instance.SkaterRotation(PlayerController.Instance.PlayerGrindRotation);
		this._vel = PlayerController.Instance.boardController.boardRigidbody.velocity;
		this._angVel = PlayerController.Instance.boardController.boardRigidbody.angularVelocity;
		if ((PlayerController.Instance.GetGrindContactPosition() - PlayerController.Instance.boardController.boardRigidbody.worldCenterOfMass).magnitude > 0.1f)
		{
			PlayerController.Instance.SetBoardCenterOfMass(PlayerController.Instance.boardController.boardTransform.InverseTransformPoint(PlayerController.Instance.GetGrindContactPosition()));
		}
		PlayerController.Instance.boardController.boardRigidbody.velocity = this._vel;
		PlayerController.Instance.boardController.boardRigidbody.angularVelocity = this._angVel;
		PlayerController.Instance.BoardGrindRotation = Quaternion.Slerp(PlayerController.Instance.boardController.boardRigidbody.rotation, (PlayerController.Instance.GetBoardBackwards() ? PlayerController.Instance.boardController.triggerManager.grindOffset.rotation : PlayerController.Instance.boardOffsetRoot.rotation), Time.fixedDeltaTime * 80f);
		PlayerController.Instance.LockAngularVelocity(PlayerController.Instance.BoardGrindRotation);
		if (PlayerController.Instance.boardController.boardRigidbody.velocity.magnitude > this._lastVelocity.magnitude + 0.5f)
		{
			PlayerController.Instance.boardController.boardRigidbody.velocity = this._grindDirection.normalized * this._lastVelocity.magnitude;
		}
		this._lastVelocity = PlayerController.Instance.boardController.boardRigidbody.velocity;
	}

	public override float GetAugmentedAngle(StickInput p_stick)
	{
		if (p_stick.PopToeSpeed >= 10f)
		{
			if (p_stick.IsRightStick)
			{
				return this._augmentedRightAngle;
			}
			return this._augmentedLeftAngle;
		}
		if (p_stick.IsRightStick)
		{
			if (p_stick.IsPopStick)
			{
				PlayerController.Instance.DebugAugmentedAngles(p_stick.augmentedInput.pos, true);
				this._augmentedRightAngle = PlayerController.Instance.GetAngleToAugment(p_stick.rawInput.pos, true);
				return PlayerController.Instance.GetAngleToAugment(p_stick.rawInput.pos, true);
			}
			if (PlayerController.Instance.playerSM.GetPopStickSM() == null)
			{
				PlayerController.Instance.DebugAugmentedAngles(p_stick.augmentedInput.pos, true);
				this._augmentedRightAngle = PlayerController.Instance.GetAngleToAugment(p_stick.rawInput.pos, true);
				return PlayerController.Instance.GetAngleToAugment(p_stick.rawInput.pos, true);
			}
			PlayerController.Instance.DebugAugmentedAngles(p_stick.augmentedInput.pos, false);
			this._augmentedRightAngle = PlayerController.Instance.GetAngleToAugment(p_stick.rawInput.pos, false);
			return PlayerController.Instance.GetAngleToAugment(p_stick.rawInput.pos, false);
		}
		if (p_stick.IsPopStick)
		{
			PlayerController.Instance.DebugAugmentedAngles(p_stick.augmentedInput.pos, false);
			this._augmentedLeftAngle = PlayerController.Instance.GetAngleToAugment(p_stick.rawInput.pos, false);
			return PlayerController.Instance.GetAngleToAugment(p_stick.rawInput.pos, false);
		}
		if (PlayerController.Instance.playerSM.GetPopStickSM() == null)
		{
			PlayerController.Instance.DebugAugmentedAngles(p_stick.augmentedInput.pos, false);
			this._augmentedLeftAngle = PlayerController.Instance.GetAngleToAugment(p_stick.rawInput.pos, false);
			return PlayerController.Instance.GetAngleToAugment(p_stick.rawInput.pos, false);
		}
		PlayerController.Instance.DebugAugmentedAngles(p_stick.augmentedInput.pos, true);
		this._augmentedLeftAngle = PlayerController.Instance.GetAngleToAugment(p_stick.rawInput.pos, true);
		return PlayerController.Instance.GetAngleToAugment(p_stick.rawInput.pos, true);
	}

	public override StickInput GetPopStick()
	{
		return this._popStick;
	}

	public override bool IsCurrentSpline(SplineComputer p_spline)
	{
		return this._spline == p_spline;
	}

	public override bool IsGrinding()
	{
		return true;
	}

	private void Lean()
	{
		if (this._rightTrigger < 0.1f)
		{
			if (this._leftTrigger <= 0.5f)
			{
				if (this._leftTriggerTimer != 0f && this._leftTriggerTimer < 0.2f)
				{
					PlayerController.Instance.boardController.triggerManager.sideEnteredGrind = TriggerManager.SideEnteredGrind.Left;
				}
				this._leftTriggerTimer = 0f;
			}
			else
			{
				this._leftTriggerTimer += Time.deltaTime;
			}
		}
		if (this._leftTrigger < 0.1f)
		{
			if (this._rightTrigger <= 0.5f)
			{
				if (this._rightTriggerTimer != 0f && this._rightTriggerTimer < 0.2f)
				{
					PlayerController.Instance.boardController.triggerManager.sideEnteredGrind = TriggerManager.SideEnteredGrind.Right;
				}
				this._rightTriggerTimer = 0f;
			}
			else
			{
				this._rightTriggerTimer += Time.deltaTime;
			}
		}
		if (this._leftTrigger > 0.3f && this._rightTrigger > 0.3f)
		{
			this._bothTriggerTimer += Time.deltaTime;
			return;
		}
		if (this._leftTrigger < 0.3f && this._rightTrigger < 0.3f)
		{
			if (this._bothTriggerTimer != 0f && this._bothTriggerTimer < 0.2f)
			{
				PlayerController.Instance.boardController.triggerManager.sideEnteredGrind = TriggerManager.SideEnteredGrind.Center;
			}
			this._bothTriggerTimer = 0f;
		}
	}

	public override void LeftTriggerHeld(float p_value, InputController.TurningMode p_turningMode)
	{
		this.RotatePlayer(-p_value);
		this._leftTrigger = p_value;
	}

	public override void LeftTriggerReleased()
	{
		this._leftTrigger = 0f;
	}

	public override void OnGrindDetected()
	{
		if (!this._popped && this._canGrind)
		{
			base.DoTransition(typeof(PlayerState_Grinding), null);
		}
	}

	public override void OnGrindEnded()
	{
		PlayerController.Instance.boardController.boardRigidbody.velocity = this._lastVelocity;
		PlayerController.Instance.boardController.backTruckRigidbody.velocity = this._lastVelocity;
		PlayerController.Instance.boardController.frontTruckRigidbody.velocity = this._lastVelocity;
		PlayerController.Instance.boardController.boardRigidbody.angularVelocity = Vector3.zero;
		PlayerController.Instance.boardController.backTruckRigidbody.angularVelocity = Vector3.zero;
		PlayerController.Instance.boardController.frontTruckRigidbody.angularVelocity = Vector3.zero;
		PlayerController.Instance.skaterController.skaterRigidbody.angularVelocity = Vector3.zero;
		if (PlayerController.Instance.TwoWheelsDown())
		{
			PlayerController.Instance.AnimGrindTransition(false);
			PlayerController.Instance.SetTurningMode(InputController.TurningMode.Grounded);
			PlayerController.Instance.SetBoardToMaster();
			if (!PlayerController.Instance.IsRespawning)
			{
				PlayerController.Instance.CrossFadeAnimation("Riding", 0.3f);
			}
			base.DoTransition(typeof(PlayerState_Riding), null);
			return;
		}
		PlayerController.Instance.AnimGrindTransition(false);
		PlayerController.Instance.SetTurningMode(InputController.TurningMode.InAir);
		PlayerController.Instance.SetSkaterToMaster();
		if (!PlayerController.Instance.IsRespawning)
		{
			PlayerController.Instance.CrossFadeAnimation("Extend", 0.3f);
		}
		Vector3 vector3 = PlayerController.Instance.skaterController.PredictLanding(PlayerController.Instance.skaterController.skaterRigidbody.velocity);
		PlayerController.Instance.skaterController.skaterRigidbody.AddForce(vector3, ForceMode.Impulse);
		object[] objArray = new object[] { true, false };
		base.DoTransition(typeof(PlayerState_InAir), objArray);
	}

	public override void OnGrindStay()
	{
	}

	public override void OnNextState()
	{
		if (this._popStick != null && this._flipStick != null)
		{
			this._popped = true;
			PlayerController.Instance.AnimSetPopStrength(0f);
			PlayerController.Instance.AnimGrindTransition(false);
			PlayerController.Instance.boardController.ReferenceBoardRotation();
			PlayerController.Instance.SetTurnMultiplier(1.2f);
			PlayerController.Instance.SetTurningMode(InputController.TurningMode.InAir);
			PlayerController.Instance.FixTargetNormal();
			PlayerController.Instance.SetTargetToMaster();
			PlayerController.Instance.AnimOllieTransition(true);
			if (this._potentialFlip)
			{
				object[] objArray = new object[] { this._popStick, this._flipStick, this._initialFlipDir, this._flipVel, this._popVel, this._toeAxis, this._popDir, this._flipDetected, this._flip, this._popForce, true, this._forwardLoad, this._invertVel, this._setupDir, this._augmentedLeftAngle, this._augmentedRightAngle };
				base.DoTransition(typeof(PlayerState_BeginPop), objArray);
				return;
			}
			object[] objArray1 = new object[] { this._popStick, this._flipStick, this._popForce, true, this._forwardLoad, this._invertVel, this._setupDir, this._augmentedLeftAngle, this._augmentedRightAngle, this._popVel, this._toeAxis, this._popDir };
			base.DoTransition(typeof(PlayerState_BeginPop), objArray1);
		}
	}

	public override void OnStickFixedUpdate(StickInput p_leftStick, StickInput p_rightStick)
	{
		this.RotateBoard(p_leftStick, p_rightStick);
	}

	public override void OnStickUpdate(StickInput p_leftStick, StickInput p_rightStick)
	{
		float single;
		float single1;
		if (SettingsManager.Instance.stance != SettingsManager.Stance.Regular)
		{
			switch (SettingsManager.Instance.controlType)
			{
				case SettingsManager.ControlType.Same:
				{
					if (PlayerController.Instance.CanOllieOutOfGrind() && PlayerController.Instance.CanNollieOutOfGrind())
					{
						if (p_leftStick.PopToeSpeed > 8f)
						{
							if (this._popStick != p_leftStick)
							{
								this._popStick = p_leftStick;
							}
							this._flipStick = p_rightStick;
						}
						if (p_rightStick.PopToeSpeed <= 8f)
						{
							break;
						}
						if (this._popStick != p_rightStick)
						{
							this._popStick = p_rightStick;
						}
						this._flipStick = p_leftStick;
						break;
					}
					else if (!PlayerController.Instance.CanOllieOutOfGrind())
					{
						if (!PlayerController.Instance.CanNollieOutOfGrind())
						{
							break;
						}
						if (this._popStick != p_rightStick)
						{
							this._popStick = p_rightStick;
						}
						this._flipStick = p_leftStick;
						break;
					}
					else
					{
						if (this._popStick != p_leftStick)
						{
							this._popStick = p_leftStick;
						}
						this._flipStick = p_rightStick;
						break;
					}
				}
				case SettingsManager.ControlType.Swap:
				{
					if (!PlayerController.Instance.IsSwitch)
					{
						if (PlayerController.Instance.CanOllieOutOfGrind() && PlayerController.Instance.CanNollieOutOfGrind())
						{
							if (p_leftStick.PopToeSpeed > 8f)
							{
								if (this._popStick != p_leftStick)
								{
									this._popStick = p_leftStick;
								}
								this._flipStick = p_rightStick;
							}
							if (p_rightStick.PopToeSpeed <= 8f)
							{
								break;
							}
							if (this._popStick != p_rightStick)
							{
								this._popStick = p_rightStick;
							}
							this._flipStick = p_leftStick;
							break;
						}
						else if (!PlayerController.Instance.CanOllieOutOfGrind())
						{
							if (!PlayerController.Instance.CanNollieOutOfGrind())
							{
								break;
							}
							if (this._popStick != p_rightStick)
							{
								this._popStick = p_rightStick;
							}
							this._flipStick = p_leftStick;
							break;
						}
						else
						{
							if (this._popStick != p_leftStick)
							{
								this._popStick = p_leftStick;
							}
							this._flipStick = p_rightStick;
							break;
						}
					}
					else if (PlayerController.Instance.CanOllieOutOfGrind() && PlayerController.Instance.CanNollieOutOfGrind())
					{
						if (p_leftStick.PopToeSpeed > 8f)
						{
							if (this._popStick != p_leftStick)
							{
								this._popStick = p_leftStick;
							}
							this._flipStick = p_rightStick;
						}
						if (p_rightStick.PopToeSpeed <= 8f)
						{
							break;
						}
						if (this._popStick != p_rightStick)
						{
							this._popStick = p_rightStick;
						}
						this._flipStick = p_leftStick;
						break;
					}
					else if (!PlayerController.Instance.CanOllieOutOfGrind())
					{
						if (!PlayerController.Instance.CanNollieOutOfGrind())
						{
							break;
						}
						if (this._popStick != p_leftStick)
						{
							this._popStick = p_leftStick;
						}
						this._flipStick = p_rightStick;
						break;
					}
					else
					{
						if (this._popStick != p_rightStick)
						{
							this._popStick = p_rightStick;
						}
						this._flipStick = p_leftStick;
						break;
					}
				}
				case SettingsManager.ControlType.Simple:
				{
					if (PlayerController.Instance.CanOllieOutOfGrind() && PlayerController.Instance.CanNollieOutOfGrind())
					{
						if (p_leftStick.PopToeSpeed > 8f)
						{
							if (this._popStick != p_leftStick)
							{
								this._popStick = p_leftStick;
							}
							this._flipStick = p_rightStick;
						}
						if (p_rightStick.PopToeSpeed <= 8f)
						{
							break;
						}
						if (this._popStick != p_rightStick)
						{
							this._popStick = p_rightStick;
						}
						this._flipStick = p_leftStick;
						break;
					}
					else if (!PlayerController.Instance.CanOllieOutOfGrind())
					{
						if (!PlayerController.Instance.CanNollieOutOfGrind())
						{
							break;
						}
						if (this._popStick != p_leftStick)
						{
							this._popStick = p_leftStick;
						}
						this._flipStick = p_rightStick;
						break;
					}
					else
					{
						if (this._popStick != p_rightStick)
						{
							this._popStick = p_rightStick;
						}
						this._flipStick = p_leftStick;
						break;
					}
				}
			}
		}
		else
		{
			switch (SettingsManager.Instance.controlType)
			{
				case SettingsManager.ControlType.Same:
				{
					if (PlayerController.Instance.CanOllieOutOfGrind() && PlayerController.Instance.CanNollieOutOfGrind())
					{
						if (p_leftStick.PopToeSpeed > 8f)
						{
							if (this._popStick != p_leftStick)
							{
								this._popStick = p_leftStick;
							}
							this._flipStick = p_rightStick;
						}
						if (p_rightStick.PopToeSpeed <= 8f)
						{
							break;
						}
						if (this._popStick != p_rightStick)
						{
							this._popStick = p_rightStick;
						}
						this._flipStick = p_leftStick;
						break;
					}
					else if (!PlayerController.Instance.CanOllieOutOfGrind())
					{
						if (!PlayerController.Instance.CanNollieOutOfGrind())
						{
							break;
						}
						if (this._popStick != p_leftStick)
						{
							this._popStick = p_leftStick;
						}
						this._flipStick = p_rightStick;
						break;
					}
					else
					{
						if (this._popStick != p_rightStick)
						{
							this._popStick = p_rightStick;
						}
						this._flipStick = p_leftStick;
						break;
					}
				}
				case SettingsManager.ControlType.Swap:
				{
					if (!PlayerController.Instance.IsSwitch)
					{
						if (PlayerController.Instance.CanOllieOutOfGrind() && PlayerController.Instance.CanNollieOutOfGrind())
						{
							if (p_leftStick.PopToeSpeed > 8f)
							{
								if (this._popStick != p_leftStick)
								{
									this._popStick = p_leftStick;
								}
								this._flipStick = p_rightStick;
							}
							if (p_rightStick.PopToeSpeed <= 8f)
							{
								break;
							}
							if (this._popStick != p_rightStick)
							{
								this._popStick = p_rightStick;
							}
							this._flipStick = p_leftStick;
							break;
						}
						else if (!PlayerController.Instance.CanOllieOutOfGrind())
						{
							if (!PlayerController.Instance.CanNollieOutOfGrind())
							{
								break;
							}
							if (this._popStick != p_leftStick)
							{
								this._popStick = p_leftStick;
							}
							this._flipStick = p_rightStick;
							break;
						}
						else
						{
							if (this._popStick != p_rightStick)
							{
								this._popStick = p_rightStick;
							}
							this._flipStick = p_leftStick;
							break;
						}
					}
					else if (PlayerController.Instance.CanOllieOutOfGrind() && PlayerController.Instance.CanNollieOutOfGrind())
					{
						if (p_leftStick.PopToeSpeed > 8f)
						{
							if (this._popStick != p_leftStick)
							{
								this._popStick = p_leftStick;
							}
							this._flipStick = p_rightStick;
						}
						if (p_rightStick.PopToeSpeed <= 8f)
						{
							break;
						}
						if (this._popStick != p_rightStick)
						{
							this._popStick = p_rightStick;
						}
						this._flipStick = p_leftStick;
						break;
					}
					else if (!PlayerController.Instance.CanOllieOutOfGrind())
					{
						if (!PlayerController.Instance.CanNollieOutOfGrind())
						{
							break;
						}
						if (this._popStick != p_rightStick)
						{
							this._popStick = p_rightStick;
						}
						this._flipStick = p_leftStick;
						break;
					}
					else
					{
						if (this._popStick != p_leftStick)
						{
							this._popStick = p_leftStick;
						}
						this._flipStick = p_rightStick;
						break;
					}
				}
				case SettingsManager.ControlType.Simple:
				{
					if (PlayerController.Instance.CanOllieOutOfGrind() && PlayerController.Instance.CanNollieOutOfGrind())
					{
						if (p_leftStick.PopToeSpeed > 8f)
						{
							if (this._popStick != p_leftStick)
							{
								this._popStick = p_leftStick;
							}
							this._flipStick = p_rightStick;
						}
						if (p_rightStick.PopToeSpeed <= 8f)
						{
							break;
						}
						if (this._popStick != p_rightStick)
						{
							this._popStick = p_rightStick;
						}
						this._flipStick = p_leftStick;
						break;
					}
					else if (!PlayerController.Instance.CanOllieOutOfGrind())
					{
						if (!PlayerController.Instance.CanNollieOutOfGrind())
						{
							break;
						}
						if (this._popStick != p_leftStick)
						{
							this._popStick = p_leftStick;
						}
						this._flipStick = p_rightStick;
						break;
					}
					else
					{
						if (this._popStick != p_rightStick)
						{
							this._popStick = p_rightStick;
						}
						this._flipStick = p_leftStick;
						break;
					}
				}
			}
		}
		if (this._popStick != null && this._flipStick != null)
		{
			this._forwardLoad = PlayerController.Instance.GetNollie(this._popStick.IsRightStick) > 0.1f;
			PlayerController.Instance.AnimSetNollie(PlayerController.Instance.GetNollie(this._popStick.IsRightStick));
			PlayerController instance = PlayerController.Instance;
			StickInput stickInput = this._popStick;
			bool flag = this._forwardLoad;
			ref float singlePointer = ref this._invertVel;
			single = (this._popStick.IsRightStick ? this._augmentedRightAngle : this._augmentedLeftAngle);
			instance.OnPopStartCheck(true, stickInput, ref this._setupDir, flag, 15f, ref singlePointer, single, ref this._popVel);
			PlayerController playerController = PlayerController.Instance;
			ref bool flagPointer = ref this._potentialFlip;
			ref Vector2 vector2Pointer = ref this._initialFlipDir;
			ref int numPointer = ref this._flipFrameCount;
			ref int numPointer1 = ref this._flipFrameMax;
			ref float singlePointer1 = ref this._toeAxis;
			ref float singlePointer2 = ref this._flipVel;
			ref float singlePointer3 = ref this._popVel;
			ref float singlePointer4 = ref this._popDir;
			ref float singlePointer5 = ref this._flip;
			StickInput stickInput1 = this._flipStick;
			ref float singlePointer6 = ref this._invertVel;
			single1 = (this._popStick.IsRightStick ? this._augmentedLeftAngle : this._augmentedRightAngle);
			playerController.OnFlipStickUpdate(ref this._flipDetected, ref flagPointer, ref vector2Pointer, ref numPointer, ref numPointer1, ref singlePointer1, ref singlePointer2, ref singlePointer3, ref singlePointer4, ref singlePointer5, stickInput1, false, false, ref singlePointer6, single1, false, this._forwardLoad, ref this._flipWindowTimer);
		}
		PlayerController.Instance.SetLeftIKOffset(p_leftStick.ToeAxis, p_leftStick.ForwardDir, p_leftStick.PopDir, p_leftStick.IsPopStick, true, false);
		PlayerController.Instance.SetRightIKOffset(p_rightStick.ToeAxis, p_rightStick.ForwardDir, p_rightStick.PopDir, p_rightStick.IsPopStick, true, false);
	}

	public override void RightTriggerHeld(float p_value, InputController.TurningMode p_turningMode)
	{
		this.RotatePlayer(p_value);
		this._rightTrigger = p_value;
	}

	public override void RightTriggerReleased()
	{
		this._rightTrigger = 0f;
	}

	private void RotateBoard(StickInput p_leftStick, StickInput p_rightStick)
	{
		this._horizontalTarget = -p_rightStick.ToeAxis * 44f + p_leftStick.ToeAxis * 44f;
		this._horizontalLerp = Mathf.Lerp(this._horizontalLerp, this._horizontalTarget, Time.fixedDeltaTime * PlayerController.Instance.horizontalSpeed);
		this._verticalTarget = p_rightStick.ForwardDir * 17f + p_leftStick.ForwardDir * 17f;
		this._verticalLerp = Mathf.Lerp(this._verticalLerp, this._verticalTarget, Time.fixedDeltaTime * PlayerController.Instance.verticalSpeed);
		PlayerController.Instance.GrindRotateBoard(this._horizontalLerp, this._verticalLerp);
	}

	private void RotatePlayer(float p_value)
	{
		this._playerTurn = p_value * Time.deltaTime * 100f;
		PlayerController.Instance.GrindRotatePlayerHorizontal(this._playerTurn);
	}

	public override void SetSpline(SplineComputer p_spline)
	{
		this._spline = p_spline;
	}

	public override void Update()
	{
		int num;
		int num1;
		base.Update();
		if (this._spline == PlayerController.Instance.boardController.triggerManager.spline)
		{
			this._canGrind = true;
		}
		else
		{
			this._canGrind = false;
		}
		if (!this._waitDone)
		{
			this._grindTimer += Time.deltaTime;
			if (this._grindTimer > 0.4f)
			{
				PlayerController.Instance.AnimSetGrinding(true);
				this._waitDone = true;
			}
		}
		this._grindTime += Time.deltaTime;
		SoundManager instance = SoundManager.Instance;
		Vector3 vector3 = PlayerController.Instance.boardController.boardRigidbody.velocity;
		instance.SetGrindVolume(vector3.magnitude);
		if (!PlayerController.Instance.IsCurrentGrindMetal())
		{
			if (this._wasSliding && !PlayerController.Instance.boardController.isSliding)
			{
				SoundManager.Instance.StopGrindSound((!Mathd.IsInfinityOrNaN(this._lastVelocity.magnitude) ? this._lastVelocity.magnitude : 0f));
				SoundManager soundManager = SoundManager.Instance;
				num1 = (PlayerController.Instance.boardController.isSliding ? 1 : PlayerController.Instance.boardController.GetGrindSoundInt());
				vector3 = Vector3.ProjectOnPlane(PlayerController.Instance.boardController.boardRigidbody.velocity, Vector3.up);
				soundManager.PlayGrindSound(num1, vector3.magnitude);
				vector3 = PlayerController.Instance.boardController.boardRigidbody.velocity;
				this._initialVelocityMagnitude = vector3.magnitude;
				this._wasSliding = false;
			}
			else if (!this._wasSliding && PlayerController.Instance.boardController.isSliding)
			{
				SoundManager.Instance.StopGrindSound((!Mathd.IsInfinityOrNaN(this._lastVelocity.magnitude) ? this._lastVelocity.magnitude : 0f));
				SoundManager instance1 = SoundManager.Instance;
				num = (PlayerController.Instance.boardController.isSliding ? 1 : PlayerController.Instance.boardController.GetGrindSoundInt());
				vector3 = Vector3.ProjectOnPlane(PlayerController.Instance.boardController.boardRigidbody.velocity, Vector3.up);
				instance1.PlayGrindSound(num, vector3.magnitude);
				vector3 = PlayerController.Instance.boardController.boardRigidbody.velocity;
				this._initialVelocityMagnitude = vector3.magnitude;
				this._wasSliding = true;
			}
		}
		this.Lean();
	}
}