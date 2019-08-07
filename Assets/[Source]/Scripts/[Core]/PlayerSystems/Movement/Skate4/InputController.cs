using FSMHelper;
using Rewired;
using System;
using UnityEngine;

public class InputController : MonoBehaviour
{
	public bool controlsActive;

	public InputThread inputThread;

	public DebugUI debugUI;

	public TutorialControllerUI tutControllerUI;

	public Player player;

	private float _timeSinceActivity;

	public float inactiveTime;

	[SerializeField]
	private StickInput _leftStick;

	[SerializeField]
	private StickInput _rightStick;

	public InputController.TurningMode turningMode = InputController.TurningMode.InAir;

	private static InputController _instance;

	private float _triggerMultiplier = 1f;

	private float _leftTrigger;

	private float _rightTrigger;

	private float _lastLeftTrigger;

	private float _lastRightTrigger;

	private float _triggerDeadZone = 0.05f;

	private bool _leftHeld;

	private bool _rightHeld;

	private float _turn;

	public static InputController Instance
	{
		get
		{
			return InputController._instance;
		}
	}

	public StickInput LeftStick
	{
		get
		{
			return this._leftStick;
		}
	}

	public StickInput RightStick
	{
		get
		{
			return this._rightStick;
		}
	}

	public float TriggerMultiplier
	{
		get
		{
			return this._triggerMultiplier;
		}
		set
		{
			this._triggerMultiplier = value;
		}
	}

	public InputController()
	{
	}

	private void Awake()
	{
		this.player = ReInput.players.GetPlayer(0);
		this._leftStick = base.gameObject.AddComponent<StickInput>();
		this._rightStick = base.gameObject.AddComponent<StickInput>();
		if (!(InputController._instance != null) || !(InputController._instance != this))
		{
			InputController._instance = this;
			return;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void BothTriggersReleased()
	{
		PlayerController.Instance.playerSM.BothTriggersReleasedSM(this.turningMode);
	}

	private void CheckForInactivity()
	{
		if (this.player.GetAnyButtonUp() || this.JoystickActive())
		{
			this._timeSinceActivity = 0f;
		}
		else
		{
			this._timeSinceActivity += Time.deltaTime;
		}
		if (this._timeSinceActivity > 300f)
		{
			this.inactiveTime += Time.deltaTime;
		}
	}

	private void FixedUpdate()
	{
		this.FixedUpdateTriggers();
		PlayerController.Instance.playerSM.OnStickFixedUpdateSM(this._leftStick, this._rightStick);
	}

	private void FixedUpdateTriggers()
	{
		if (PlayerController.Instance.playerSM.IsOnGroundStateSM())
		{
			if (!this._leftStick.IsPopStick)
			{
				if (this._leftStick.rawInput.pos.x < -0.3f)
				{
					this.LeftTriggerHeld(Mathf.Abs(this._leftStick.rawInput.pos.x) * this.TriggerMultiplier);
				}
				else if (this._leftStick.rawInput.pos.x > 0.3f)
				{
					this.RightTriggerHeld(Mathf.Abs(this._leftStick.rawInput.pos.x) * this.TriggerMultiplier);
				}
			}
			if (!this._rightStick.IsPopStick)
			{
				if (this._rightStick.rawInput.pos.x < -0.3f)
				{
					this.LeftTriggerHeld(Mathf.Abs(this._rightStick.rawInput.pos.x) * this.TriggerMultiplier);
				}
				else if (this._rightStick.rawInput.pos.x > 0.3f)
				{
					this.RightTriggerHeld(Mathf.Abs(this._rightStick.rawInput.pos.x) * this.TriggerMultiplier);
				}
			}
		}
		if (this._leftHeld)
		{
			this.LeftTriggerHeld(this._leftTrigger * this.TriggerMultiplier);
		}
		if (this._rightHeld)
		{
			this.RightTriggerHeld(this._rightTrigger * this.TriggerMultiplier);
		}
	}

	public float GetWindUp()
	{
		if (SettingsManager.Instance.stance != SettingsManager.Stance.Regular)
		{
			return this._leftTrigger - this._rightTrigger;
		}
		return -this._leftTrigger + this._rightTrigger;
	}

	private bool IsTurningWithSticks()
	{
		if (!PlayerController.Instance.playerSM.IsOnGroundStateSM())
		{
			return false;
		}
		if (Mathf.Abs(this._leftStick.rawInput.pos.x) > 0.3f && Mathf.Abs(this._leftStick.rawInput.pos.y) < 0.5f)
		{
			return true;
		}
		if (Mathf.Abs(this._rightStick.rawInput.pos.x) <= 0.3f)
		{
			return false;
		}
		return Mathf.Abs(this._rightStick.rawInput.pos.y) < 0.5f;
	}

	private bool JoystickActive()
	{
		if (Mathf.Abs(this.player.GetAxis("LeftStickX")) <= 0.1f && Mathf.Abs(this.player.GetAxis("RightStickX")) <= 0.1f && Mathf.Abs(this.player.GetAxis("LeftStickY")) <= 0.1f && Mathf.Abs(this.player.GetAxis("RightStickY")) <= 0.1f)
		{
			return false;
		}
		return true;
	}

	private void LeftTriggerHeld(float p_value)
	{
		PlayerController.Instance.playerSM.LeftTriggerHeldSM(p_value, this.turningMode);
	}

	private void LeftTriggerHeld(float p_value, bool p_skateControls)
	{
		PlayerController.Instance.playerSM.LeftTriggerHeldSM(p_value, this.turningMode);
	}

	private void LeftTriggerPressed()
	{
		PlayerController.Instance.playerSM.LeftTriggerPressedSM();
	}

	private void LeftTriggerReleased()
	{
		PlayerController.Instance.playerSM.LeftTriggerReleasedSM();
	}

	private void RightTriggerHeld(float p_value)
	{
		PlayerController.Instance.playerSM.RightTriggerHeldSM(p_value, this.turningMode);
	}

	private void RightTriggerPressed()
	{
		PlayerController.Instance.playerSM.RightTriggerPressedSM();
	}

	private void RightTriggerReleased()
	{
		PlayerController.Instance.playerSM.RightTriggerReleasedSM();
	}

	private void Update()
	{
		this.CheckForInactivity();
		if (this.controlsActive)
		{
			this.UpdateTriggers();
			this.UpdateSticks();
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				Application.Quit();
			}
			if (this.player.GetButtonDown("Start"))
			{
				this.debugUI.SetColor(DebugUI.Buttons.Start);
			}
			if (this.player.GetButtonUp("Start"))
			{
				this.debugUI.ResetColor(DebugUI.Buttons.Start);
			}
			if (this.player.GetButtonDown("Select"))
			{
				if (PlayerController.Instance.playerSM.IsOnGroundStateSM())
				{
					SettingsManager.Instance.SetStance((SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? SettingsManager.Stance.Goofy : SettingsManager.Stance.Regular));
					PlayerController.Instance.respawn.DoRespawn();
				}
				this.debugUI.SetColor(DebugUI.Buttons.Select);
			}
			if (this.player.GetButtonUp("Select"))
			{
				this.debugUI.ResetColor(DebugUI.Buttons.Select);
			}
			if (this.player.GetButtonDown("Y"))
			{
				this.debugUI.SetColor(DebugUI.Buttons.Y);
				this.tutControllerUI.SetColor(TutorialControllerUI.Buttons.Y);
				if (Application.isEditor)
				{
					Time.timeScale = 0.05f;
				}
			}
			if (this.player.GetButtonUp("Y"))
			{
				this.debugUI.ResetColor(DebugUI.Buttons.Y);
				this.tutControllerUI.ResetColor(TutorialControllerUI.Buttons.Y);
			}
			if (this.player.GetButtonDown("B"))
			{
				PlayerController.Instance.playerSM.OnBrakePressedSM();
			}
			if (this.player.GetButton("B"))
			{
				this.debugUI.SetColor(DebugUI.Buttons.B);
				this.tutControllerUI.SetColor(TutorialControllerUI.Buttons.B);
				PlayerController.Instance.playerSM.OnBrakeHeldSM();
			}
			if (this.player.GetButtonUp("B"))
			{
				this.debugUI.ResetColor(DebugUI.Buttons.B);
				this.tutControllerUI.ResetColor(TutorialControllerUI.Buttons.B);
				PlayerController.Instance.playerSM.OnBrakeReleasedSM();
			}
			if (this.player.GetButtonDown("A"))
			{
				if (SettingsManager.Instance.stance == SettingsManager.Stance.Regular)
				{
					if (PlayerController.Instance.IsSwitch)
					{
						PlayerController.Instance.playerSM.OnPushButtonPressedSM(true);
					}
					else
					{
						PlayerController.Instance.playerSM.OnPushButtonPressedSM(false);
					}
				}
				else if (PlayerController.Instance.IsSwitch)
				{
					PlayerController.Instance.playerSM.OnPushButtonPressedSM(false);
				}
				else
				{
					PlayerController.Instance.playerSM.OnPushButtonPressedSM(true);
				}
			}
			if (!this.player.GetButton("A"))
			{
				PlayerController.Instance.skaterController.pushBrakeForce = Vector3.zero;
			}
			else
			{
				this.debugUI.SetColor(DebugUI.Buttons.A);
				this.tutControllerUI.SetColor(TutorialControllerUI.Buttons.A);
				if (SettingsManager.Instance.stance == SettingsManager.Stance.Regular)
				{
					if (PlayerController.Instance.IsSwitch)
					{
						PlayerController.Instance.playerSM.OnPushButtonHeldSM(true);
					}
					else
					{
						PlayerController.Instance.playerSM.OnPushButtonHeldSM(false);
					}
				}
				else if (PlayerController.Instance.IsSwitch)
				{
					PlayerController.Instance.playerSM.OnPushButtonHeldSM(false);
				}
				else
				{
					PlayerController.Instance.playerSM.OnPushButtonHeldSM(true);
				}
			}
			if (this.player.GetButtonUp("A"))
			{
				this.debugUI.ResetColor(DebugUI.Buttons.A);
				this.tutControllerUI.ResetColor(TutorialControllerUI.Buttons.A);
				PlayerController.Instance.playerSM.OnPushButtonReleasedSM();
			}
			if (this.player.GetButtonDown("X"))
			{
				if (Application.isEditor)
				{
					Time.timeScale = 1f;
				}
				this.debugUI.SetColor(DebugUI.Buttons.X);
				this.tutControllerUI.SetColor(TutorialControllerUI.Buttons.X);
				if (SettingsManager.Instance.stance == SettingsManager.Stance.Regular)
				{
					if (PlayerController.Instance.IsSwitch)
					{
						PlayerController.Instance.playerSM.OnPushButtonPressedSM(false);
					}
					else
					{
						PlayerController.Instance.playerSM.OnPushButtonPressedSM(true);
					}
				}
				else if (PlayerController.Instance.IsSwitch)
				{
					PlayerController.Instance.playerSM.OnPushButtonPressedSM(true);
				}
				else
				{
					PlayerController.Instance.playerSM.OnPushButtonPressedSM(false);
				}
			}
			if (!this.player.GetButton("X"))
			{
				PlayerController.Instance.skaterController.pushBrakeForce = Vector3.zero;
			}
			else
			{
				this.debugUI.SetColor(DebugUI.Buttons.X);
				this.tutControllerUI.SetColor(TutorialControllerUI.Buttons.X);
				if (SettingsManager.Instance.stance == SettingsManager.Stance.Regular)
				{
					if (PlayerController.Instance.IsSwitch)
					{
						PlayerController.Instance.playerSM.OnPushButtonHeldSM(false);
					}
					else
					{
						PlayerController.Instance.playerSM.OnPushButtonHeldSM(true);
					}
				}
				else if (PlayerController.Instance.IsSwitch)
				{
					PlayerController.Instance.playerSM.OnPushButtonHeldSM(true);
				}
				else
				{
					PlayerController.Instance.playerSM.OnPushButtonHeldSM(false);
				}
			}
			if (this.player.GetButtonUp("X"))
			{
				this.debugUI.ResetColor(DebugUI.Buttons.X);
				this.tutControllerUI.ResetColor(TutorialControllerUI.Buttons.X);
				PlayerController.Instance.playerSM.OnPushButtonReleasedSM();
			}
		}
		if (this.player.GetAxis("DPadX") > 0f)
		{
			this.debugUI.SetColor(DebugUI.Buttons.DPadRight);
			this.debugUI.ResetColor(DebugUI.Buttons.DPadLeft, 0.2f);
			this.tutControllerUI.SetColor(TutorialControllerUI.Buttons.DPadRight);
			this.tutControllerUI.ResetColor(TutorialControllerUI.Buttons.DPadLeft, 0.2f);
		}
		else if (this.player.GetAxis("DPadX") >= 0f)
		{
			this.debugUI.ResetColor(DebugUI.Buttons.DPadLeft, 0.2f);
			this.debugUI.ResetColor(DebugUI.Buttons.DPadRight, 0.2f);
			this.tutControllerUI.ResetColor(TutorialControllerUI.Buttons.DPadLeft, 0.2f);
			this.tutControllerUI.ResetColor(TutorialControllerUI.Buttons.DPadRight, 0.2f);
		}
		else
		{
			this.debugUI.SetColor(DebugUI.Buttons.DPadLeft);
			this.debugUI.ResetColor(DebugUI.Buttons.DPadRight, 0.2f);
			this.tutControllerUI.SetColor(TutorialControllerUI.Buttons.DPadLeft);
			this.tutControllerUI.ResetColor(TutorialControllerUI.Buttons.DPadRight, 0.2f);
		}
		if (this.player.GetAxis("DPadY") > 0f)
		{
			this.debugUI.SetColor(DebugUI.Buttons.DPadUp);
			this.debugUI.ResetColor(DebugUI.Buttons.DPadDown, 0.2f);
			this.tutControllerUI.SetColor(TutorialControllerUI.Buttons.DPadUp);
			this.tutControllerUI.ResetColor(TutorialControllerUI.Buttons.DPadDown, 0.2f);
		}
		else if (this.player.GetAxis("DPadY") >= 0f)
		{
			this.debugUI.ResetColor(DebugUI.Buttons.DPadDown, 0.2f);
			this.debugUI.ResetColor(DebugUI.Buttons.DPadUp, 0.2f);
			this.tutControllerUI.ResetColor(TutorialControllerUI.Buttons.DPadDown, 0.2f);
			this.tutControllerUI.ResetColor(TutorialControllerUI.Buttons.DPadUp, 0.2f);
		}
		else
		{
			this.debugUI.SetColor(DebugUI.Buttons.DPadDown);
			this.debugUI.ResetColor(DebugUI.Buttons.DPadUp, 0.2f);
			this.tutControllerUI.SetColor(TutorialControllerUI.Buttons.DPadDown);
			this.tutControllerUI.ResetColor(TutorialControllerUI.Buttons.DPadUp, 0.2f);
		}
		PlayerController.Instance.DebugRawAngles(this._leftStick.rawInput.pos, false);
		PlayerController.Instance.DebugRawAngles(this._rightStick.rawInput.pos, true);
	}

	private void UpdateSticks()
	{
		this._leftStick.StickUpdate(false, this.inputThread);
		this._rightStick.StickUpdate(true, this.inputThread);
		PlayerController.Instance.playerSM.OnStickUpdateSM(this._leftStick, this._rightStick);
		this.debugUI.UpdateLeftStickInput(this._leftStick.rawInput.pos);
		this.debugUI.UpdateRightStickInput(this._rightStick.rawInput.pos);
		this.tutControllerUI.UpdateLeftStickInput(this._leftStick.rawInput.pos);
		this.tutControllerUI.UpdateRightStickInput(this._rightStick.rawInput.pos);
		this.debugUI.UpdateLeftAugmentedStickInput(this._leftStick.augmentedInput.pos);
		this.debugUI.UpdateRightAugmentedStickInput(this._rightStick.augmentedInput.pos);
		if (this.player.GetButtonDown("Left Stick Button"))
		{
			this._leftStick.OnStickPressed(false);
		}
		if (this.player.GetButtonDown("Right Stick Button"))
		{
			this._rightStick.OnStickPressed(true);
		}
		if (this.player.GetButton("Left Stick Button"))
		{
			this.debugUI.SetColor(DebugUI.Buttons.LS);
			this.tutControllerUI.ShowClickLeft(true);
		}
		if (this.player.GetButtonUp("Left Stick Button"))
		{
			this.debugUI.ResetColor(DebugUI.Buttons.LS);
			this.tutControllerUI.ShowClickLeft(false);
		}
		if (this.player.GetButton("Right Stick Button"))
		{
			this.debugUI.SetColor(DebugUI.Buttons.RS);
			this.tutControllerUI.ShowClickRight(true);
		}
		if (this.player.GetButtonUp("Right Stick Button"))
		{
			this.debugUI.ResetColor(DebugUI.Buttons.RS);
			this.tutControllerUI.ShowClickRight(false);
		}
	}

	private void UpdateTriggers()
	{
		this._leftTrigger = this.player.GetAxis("LT");
		this._rightTrigger = this.player.GetAxis("RT");
		this.debugUI.LerpLeftTrigger(this._leftTrigger);
		this.debugUI.LerpRightTrigger(this._rightTrigger);
		this.tutControllerUI.LerpLeftTrigger(this._leftTrigger);
		this.tutControllerUI.LerpRightTrigger(this._rightTrigger);
		if (this._lastLeftTrigger < this._triggerDeadZone && this._leftTrigger > this._triggerDeadZone)
		{
			this.LeftTriggerPressed();
		}
		this._leftHeld = (this._leftTrigger > this._triggerDeadZone ? true : false);
		if (this._lastLeftTrigger > this._triggerDeadZone && this._leftTrigger < this._triggerDeadZone && !this.IsTurningWithSticks())
		{
			this.LeftTriggerReleased();
		}
		if (this._lastRightTrigger < this._triggerDeadZone && this._rightTrigger > this._triggerDeadZone)
		{
			this.RightTriggerPressed();
		}
		this._rightHeld = (this._rightTrigger > this._triggerDeadZone ? true : false);
		if (this._lastRightTrigger > this._triggerDeadZone && this._rightTrigger < this._triggerDeadZone && !this.IsTurningWithSticks())
		{
			this.RightTriggerReleased();
		}
		if (this._leftTrigger < this._triggerDeadZone && this._rightTrigger < this._triggerDeadZone && !this.IsTurningWithSticks())
		{
			this.BothTriggersReleased();
		}
		if (SettingsManager.Instance.stance != SettingsManager.Stance.Regular)
		{
			float single = this._leftTrigger - this._rightTrigger;
			PlayerController.Instance.AnimSetTurn(single);
			this._turn = Mathf.MoveTowards(this._turn, single, Time.deltaTime * 1.5f);
			PlayerController.Instance.AnimSetInAirTurn(this._turn);
		}
		else
		{
			float single1 = -this._leftTrigger + this._rightTrigger;
			PlayerController.Instance.AnimSetTurn(single1);
			this._turn = Mathf.MoveTowards(this._turn, single1, Time.deltaTime * 1.5f);
			PlayerController.Instance.AnimSetInAirTurn(this._turn);
		}
		this._lastLeftTrigger = this._leftTrigger;
		this._lastRightTrigger = this._rightTrigger;
	}

	public enum TurningMode
	{
		Grounded,
		PreWind,
		InAir,
		FastLeft,
		FastRight,
		Manual,
		None
	}
}