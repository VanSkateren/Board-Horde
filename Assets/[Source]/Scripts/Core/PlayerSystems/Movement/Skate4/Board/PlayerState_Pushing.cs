using FSMHelper;
using RootMotion.Dynamics;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Pushing : PlayerState_OnBoard
{
	private PlayerController.SetupDir _setupDir;

	private StickInput _noComplyStick;

	private StickInput _pushStick;

	private int _pushCount;

	private bool _mongo;

	private bool _pushButtonPressed;

	private bool _canPushWithButton;

	private bool _pushButtonHeld;

	private bool _pushing;

	private float _holdTimer;

	private float _pushPower;

	private float _pushTimer;

	public PlayerState_Pushing(bool p_mongo)
	{
		this._mongo = p_mongo;
	}

	public PlayerState_Pushing(StickInput p_pushStick, StickInput p_noComplyStick)
	{
		this._pushStick = p_pushStick;
		this._noComplyStick = p_noComplyStick;
	}

	public override void Enter()
	{
		PlayerController.Instance.CacheRidingTransforms();
		PlayerController.Instance.respawn.puppetMaster.internalCollisions = false;
		if (SettingsManager.Instance.stance == SettingsManager.Stance.Regular)
		{
			if (!this._mongo)
			{
				if (PlayerController.Instance.IsSwitch)
				{
					PlayerController.Instance.skaterController.leftFootCollider.isTrigger = true;
					PlayerController.Instance.SetLeftIKWeight(0f);
					PlayerController.Instance.SetLeftIKLerpTarget(1f);
					PlayerController.Instance.SetRightIKWeight(1f);
					PlayerController.Instance.SetRightIKLerpTarget(0f);
				}
				else
				{
					PlayerController.Instance.skaterController.rightFootCollider.isTrigger = true;
					PlayerController.Instance.SetRightIKWeight(0f);
					PlayerController.Instance.SetRightIKLerpTarget(1f);
					PlayerController.Instance.SetLeftIKWeight(1f);
					PlayerController.Instance.SetLeftIKLerpTarget(0f);
				}
			}
			else if (PlayerController.Instance.IsSwitch)
			{
				PlayerController.Instance.skaterController.rightFootCollider.isTrigger = true;
				PlayerController.Instance.SetRightIKWeight(0f);
				PlayerController.Instance.SetRightIKLerpTarget(1f);
				PlayerController.Instance.SetLeftIKWeight(1f);
				PlayerController.Instance.SetLeftIKLerpTarget(0f);
			}
			else
			{
				PlayerController.Instance.skaterController.leftFootCollider.isTrigger = true;
				PlayerController.Instance.SetLeftIKWeight(0f);
				PlayerController.Instance.SetLeftIKLerpTarget(1f);
				PlayerController.Instance.SetRightIKWeight(1f);
				PlayerController.Instance.SetRightIKLerpTarget(0f);
			}
		}
		else if (!this._mongo)
		{
			if (PlayerController.Instance.IsSwitch)
			{
				PlayerController.Instance.skaterController.rightFootCollider.isTrigger = true;
				PlayerController.Instance.SetRightIKWeight(0f);
				PlayerController.Instance.SetRightIKLerpTarget(1f);
				PlayerController.Instance.SetLeftIKWeight(1f);
				PlayerController.Instance.SetLeftIKLerpTarget(0f);
			}
			else
			{
				PlayerController.Instance.skaterController.leftFootCollider.isTrigger = true;
				PlayerController.Instance.SetLeftIKWeight(0f);
				PlayerController.Instance.SetLeftIKLerpTarget(1f);
				PlayerController.Instance.SetRightIKWeight(1f);
				PlayerController.Instance.SetRightIKLerpTarget(0f);
			}
		}
		else if (PlayerController.Instance.IsSwitch)
		{
			PlayerController.Instance.skaterController.leftFootCollider.isTrigger = true;
			PlayerController.Instance.SetLeftIKWeight(0f);
			PlayerController.Instance.SetLeftIKLerpTarget(1f);
			PlayerController.Instance.SetRightIKWeight(1f);
			PlayerController.Instance.SetRightIKLerpTarget(0f);
		}
		else
		{
			PlayerController.Instance.skaterController.rightFootCollider.isTrigger = true;
			PlayerController.Instance.SetRightIKWeight(0f);
			PlayerController.Instance.SetRightIKLerpTarget(1f);
			PlayerController.Instance.SetLeftIKWeight(1f);
			PlayerController.Instance.SetLeftIKLerpTarget(0f);
		}
		if (!this._mongo)
		{
			PlayerController.Instance.AnimSetPush(true);
		}
		else
		{
			PlayerController.Instance.AnimSetMongo(true);
		}
		PlayerController.Instance.SetTurnMultiplier(1f);
		PlayerController.Instance.SetTurningMode(InputController.TurningMode.Grounded);
	}

	public override void Exit()
	{
		PlayerController.Instance.skaterController.rightFootCollider.isTrigger = false;
		PlayerController.Instance.skaterController.leftFootCollider.isTrigger = false;
		PlayerController.Instance.respawn.puppetMaster.internalCollisions = true;
		PlayerController.Instance.animationController.ScaleAnimSpeed(1f);
		PlayerController.Instance.SetRightIKLerpTarget(0f);
		PlayerController.Instance.SetRightIKWeight(1f);
		PlayerController.Instance.SetLeftIKLerpTarget(0f);
		PlayerController.Instance.SetLeftIKWeight(1f);
		PlayerController.Instance.AnimSetPush(false);
		PlayerController.Instance.AnimSetMongo(false);
	}

	public override void FixedUpdate()
	{
		PlayerController.Instance.comController.UpdateCOM(1.06499f, 1);
		base.FixedUpdate();
		PlayerController.Instance.SkaterRotation(true, false);
		PlayerController.Instance.boardController.ApplyOnBoardMaxRoll();
		PlayerController.Instance.boardController.DoBoardLean();
		PlayerController.Instance.SetBoardTargetPosition(0f);
		PlayerController.Instance.SetFrontPivotRotation(0f);
		PlayerController.Instance.SetBackPivotRotation(0f);
		PlayerController.Instance.SetPivotForwardRotation(0f, 40f);
		PlayerController.Instance.SetPivotSideRotation(0f);
	}

	public override bool IsOnGroundState()
	{
		return true;
	}

	public override bool IsPushing()
	{
		return true;
	}

	public override void OnPush()
	{
		if (!this._pushing)
		{
			this._pushPower = Mathf.Clamp(this._pushPower, (this._pushCount > 2 ? 0.9f : 0.5f), 1f);
			PlayerController.Instance.AddPushForce(PlayerController.Instance.GetPushForce() * 1.8f * this._pushPower);
			this._pushPower = 0f;
			this._pushing = true;
		}
	}

	public override void OnPushButtonHeld(bool p_mongo)
	{
		if (p_mongo != this._mongo)
		{
			PlayerController.Instance.animationController.ScaleAnimSpeed(1f);
			base.DoTransition(typeof(PlayerState_Riding), null);
		}
		if (this._canPushWithButton)
		{
			this._pushButtonPressed = true;
		}
		if (!this._pushButtonHeld)
		{
			this._holdTimer += Time.deltaTime;
			if (this._holdTimer > 0.2f)
			{
				PlayerController.Instance.animationController.ScaleAnimSpeed(1.25f);
				this._pushButtonHeld = true;
				this._pushPower = Mathf.Clamp(this._pushPower, 0.8f, 1f);
			}
		}
		this._pushPower += Time.deltaTime;
	}

	public override void OnPushButtonPressed(bool p_mongo)
	{
		if (p_mongo != this._mongo)
		{
			PlayerController.Instance.animationController.ScaleAnimSpeed(1f);
			base.DoTransition(typeof(PlayerState_Riding), null);
		}
		this._pushCount++;
		this._pushButtonPressed = true;
		if ((float)this._pushCount > 2f)
		{
			PlayerController.Instance.animationController.ScaleAnimSpeed(1.25f);
		}
	}

	public override void OnPushButtonReleased()
	{
		this._holdTimer = 0f;
		if (this._pushButtonHeld)
		{
			this._pushButtonPressed = false;
		}
	}

	public override void OnPushEnd()
	{
		if (this._pushButtonPressed)
		{
			this._pushCount = 0;
			this._pushButtonPressed = false;
			return;
		}
		PlayerController.Instance.animationController.ScaleAnimSpeed(1f);
		base.DoTransition(typeof(PlayerState_Riding), null);
	}

	public override void OnPushLastCheck()
	{
		this._canPushWithButton = true;
	}

	public override void OnStickUpdate(StickInput p_leftStick, StickInput p_rightStick)
	{
		if (p_leftStick.SetupDir > 0.8f || (new Vector2(p_leftStick.ToeAxis, p_leftStick.SetupDir)).magnitude > 0.8f && p_leftStick.SetupDir > 0.325f)
		{
			PlayerController.Instance.SetRightIKWeight(1f);
			PlayerController.Instance.SetRightIKLerpTarget(0f);
			PlayerController.Instance.SetLeftIKWeight(1f);
			PlayerController.Instance.SetLeftIKLerpTarget(0f);
			PlayerController.Instance.AnimSetNollie(PlayerController.Instance.GetNollie(false));
			PlayerController.Instance.SetupDirection(p_leftStick, ref this._setupDir);
			PlayerController.Instance.AnimSetPush(false);
			PlayerController.Instance.AnimSetMongo(false);
			PlayerController.Instance.AnimSetupTransition(true);
			object[] pLeftStick = new object[] { p_leftStick, p_rightStick, p_leftStick.ForwardDir > 0.2f, this._setupDir };
			base.DoTransition(typeof(PlayerState_Setup), pLeftStick);
			return;
		}
		if (p_rightStick.AugmentedSetupDir > 0.8f || (new Vector2(p_rightStick.ToeAxis, p_rightStick.SetupDir)).magnitude > 0.8f && p_rightStick.SetupDir > 0.325f)
		{
			PlayerController.Instance.SetRightIKWeight(1f);
			PlayerController.Instance.SetRightIKLerpTarget(0f);
			PlayerController.Instance.SetLeftIKWeight(1f);
			PlayerController.Instance.SetLeftIKLerpTarget(0f);
			PlayerController.Instance.AnimSetNollie(PlayerController.Instance.GetNollie(true));
			PlayerController.Instance.SetupDirection(p_rightStick, ref this._setupDir);
			PlayerController.Instance.AnimSetupTransition(true);
			PlayerController.Instance.AnimSetPush(false);
			PlayerController.Instance.AnimSetMongo(false);
			object[] pRightStick = new object[] { p_rightStick, p_leftStick, p_rightStick.ForwardDir > 0.2f, this._setupDir };
			base.DoTransition(typeof(PlayerState_Setup), pRightStick);
		}
	}

	public override void SetupDefinition(ref FSMStateType p_stateType, ref List<Type> children)
	{
		p_stateType = FSMStateType.Type_OR;
	}

	public override void Update()
	{
		base.Update();
		PlayerController.Instance.CacheRidingTransforms();
		if (this._pushing)
		{
			this._pushTimer += Time.deltaTime;
			if (this._pushTimer > 0.25f)
			{
				this._pushing = false;
				this._pushTimer = 0f;
			}
		}
		if (!PlayerController.Instance.IsGrounded() && !PlayerController.Instance.IsRespawning)
		{
			PlayerController.Instance.SetTurningMode(InputController.TurningMode.InAir);
			PlayerController.Instance.SetSkaterToMaster();
			PlayerController.Instance.AnimSetRollOff(true);
			PlayerController.Instance.CrossFadeAnimation("Extend", 0.3f);
			PlayerController.Instance.skaterController.skaterRigidbody.angularVelocity = Vector3.zero;
			Vector3 vector3 = PlayerController.Instance.skaterController.PredictLanding(PlayerController.Instance.skaterController.skaterRigidbody.velocity);
			PlayerController.Instance.skaterController.skaterRigidbody.AddForce(vector3, ForceMode.Impulse);
			base.DoTransition(typeof(PlayerState_InAir), null);
		}
	}
}