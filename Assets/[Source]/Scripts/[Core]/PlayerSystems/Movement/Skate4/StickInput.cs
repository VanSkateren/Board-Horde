using FSMHelper;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class StickInput : MonoBehaviour
{
	public StickInput.InputData rawInput;

	public StickInput.InputData augmentedInput;

	private bool _holdingManual;

	private int _manualFrameCount;

	private bool _holdingNoseManual;

	private int _noseManualFrameCount;

	private float _toeAxisLerp;

	private float _forwardDirLerp;

	private bool _popStickCentered = true;

	private bool _flipStickCentered = true;

	public float AugmentedFlipDir
	{
		get
		{
			float single = 0f;
			switch (SettingsManager.Instance.controlType)
			{
				case SettingsManager.ControlType.Same:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.x : -this.augmentedInput.pos.x);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.x : -this.augmentedInput.pos.x);
						break;
					}
				}
				case SettingsManager.ControlType.Swap:
				{
					if (PlayerController.Instance.IsSwitch)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.x : this.augmentedInput.pos.x);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.x : this.augmentedInput.pos.x);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.x : -this.augmentedInput.pos.x);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.x : -this.augmentedInput.pos.x);
						break;
					}
				}
				case SettingsManager.ControlType.Simple:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.x : -this.augmentedInput.pos.x);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.x : -this.augmentedInput.pos.x);
						break;
					}
				}
			}
			return single;
		}
	}

	public float AugmentedForwardDir
	{
		get
		{
			float single = 0f;
			switch (SettingsManager.Instance.controlType)
			{
				case SettingsManager.ControlType.Same:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.y : this.augmentedInput.pos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.y : this.augmentedInput.pos.y);
						break;
					}
				}
				case SettingsManager.ControlType.Swap:
				{
					if (!PlayerController.Instance.IsSwitch)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.y : this.augmentedInput.pos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.y : this.augmentedInput.pos.y);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.y : -this.augmentedInput.pos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.y : -this.augmentedInput.pos.y);
						break;
					}
				}
				case SettingsManager.ControlType.Simple:
				{
					if (!PlayerController.Instance.IsSwitch)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.y : this.augmentedInput.pos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.y : this.augmentedInput.pos.y);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.y : -this.augmentedInput.pos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.y : -this.augmentedInput.pos.y);
						break;
					}
				}
			}
			return single;
		}
	}

	public float AugmentedLastSetupDir
	{
		get
		{
			float single = 0f;
			switch (SettingsManager.Instance.controlType)
			{
				case SettingsManager.ControlType.Same:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.prevPos.y : -this.augmentedInput.prevPos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.prevPos.y : this.augmentedInput.prevPos.y);
						break;
					}
				}
				case SettingsManager.ControlType.Swap:
				{
					if (PlayerController.Instance.IsSwitch)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.prevPos.y : this.augmentedInput.prevPos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.prevPos.y : -this.augmentedInput.prevPos.y);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.prevPos.y : -this.augmentedInput.prevPos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.prevPos.y : this.augmentedInput.prevPos.y);
						break;
					}
				}
				case SettingsManager.ControlType.Simple:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.prevPos.y : this.augmentedInput.prevPos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.prevPos.y : -this.augmentedInput.prevPos.y);
						break;
					}
				}
			}
			return single;
		}
	}

	public float AugmentedLastToeAxis
	{
		get
		{
			float single = 0f;
			switch (SettingsManager.Instance.controlType)
			{
				case SettingsManager.ControlType.Same:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.prevPos.x : -this.augmentedInput.prevPos.x);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.prevPos.x : -this.augmentedInput.prevPos.x);
						break;
					}
				}
				case SettingsManager.ControlType.Swap:
				{
					if (PlayerController.Instance.IsSwitch)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.prevPos.x : this.augmentedInput.prevPos.x);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.prevPos.x : this.augmentedInput.prevPos.x);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.prevPos.x : -this.augmentedInput.prevPos.x);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.prevPos.x : -this.augmentedInput.prevPos.x);
						break;
					}
				}
				case SettingsManager.ControlType.Simple:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.prevPos.x : -this.augmentedInput.prevPos.x);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.prevPos.x : -this.augmentedInput.prevPos.x);
						break;
					}
				}
			}
			return single;
		}
	}

	public float AugmentedManualAxis
	{
		get
		{
			float single = 0f;
			if (!this.IsRightStick)
			{
				switch (SettingsManager.Instance.controlType)
				{
					case SettingsManager.ControlType.Same:
					{
						if (PlayerController.Instance.IsSwitch)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.y : 0f);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : -this.augmentedInput.pos.y);
							break;
						}
					}
					case SettingsManager.ControlType.Swap:
					{
						if (PlayerController.Instance.IsSwitch)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.y : 0f);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : -this.augmentedInput.pos.y);
							break;
						}
					}
					case SettingsManager.ControlType.Simple:
					{
						if (PlayerController.Instance.IsSwitch)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : 0f);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : 0f);
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
						if (PlayerController.Instance.IsSwitch)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : this.augmentedInput.pos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.y : 0f);
							break;
						}
					}
					case SettingsManager.ControlType.Swap:
					{
						if (PlayerController.Instance.IsSwitch)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : -this.augmentedInput.pos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.y : 0f);
							break;
						}
					}
					case SettingsManager.ControlType.Simple:
					{
						if (PlayerController.Instance.IsSwitch)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.y : -this.augmentedInput.pos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.y : -this.augmentedInput.pos.y);
							break;
						}
					}
				}
			}
			return Mathf.Clamp(single, 0f, 1f);
		}
	}

	public float AugmentedNollieSetupDir
	{
		get
		{
			float single = 0f;
			switch (SettingsManager.Instance.controlType)
			{
				case SettingsManager.ControlType.Same:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.y : 0f);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : this.augmentedInput.pos.y);
						break;
					}
				}
				case SettingsManager.ControlType.Swap:
				{
					if (!PlayerController.Instance.IsSwitch)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.y : 0f);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : this.augmentedInput.pos.y);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : this.augmentedInput.pos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.y : 0f);
						break;
					}
				}
				case SettingsManager.ControlType.Simple:
				{
					if (!PlayerController.Instance.IsSwitch)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.y : this.augmentedInput.pos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : 0f);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.y : this.augmentedInput.pos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : 0f);
						break;
					}
				}
			}
			return single;
		}
	}

	public float AugmentedNoseManualAxis
	{
		get
		{
			float single = 0f;
			if (!this.IsRightStick)
			{
				switch (SettingsManager.Instance.controlType)
				{
					case SettingsManager.ControlType.Same:
					{
						if (PlayerController.Instance.IsSwitch)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : -this.augmentedInput.pos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.y : 0f);
							break;
						}
					}
					case SettingsManager.ControlType.Swap:
					{
						if (PlayerController.Instance.IsSwitch)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : this.augmentedInput.pos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.y : 0f);
							break;
						}
					}
					case SettingsManager.ControlType.Simple:
					{
						if (PlayerController.Instance.IsSwitch)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.y : this.augmentedInput.pos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.y : this.augmentedInput.pos.y);
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
						if (PlayerController.Instance.IsSwitch)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.y : 0f);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : this.augmentedInput.pos.y);
							break;
						}
					}
					case SettingsManager.ControlType.Swap:
					{
						if (PlayerController.Instance.IsSwitch)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.y : 0f);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : this.augmentedInput.pos.y);
							break;
						}
					}
					case SettingsManager.ControlType.Simple:
					{
						if (PlayerController.Instance.IsSwitch)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : 0f);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : 0f);
							break;
						}
					}
				}
			}
			return Mathf.Clamp(single, 0f, 1f);
		}
	}

	public float AugmentedOllieSetupDir
	{
		get
		{
			float single = 0f;
			switch (SettingsManager.Instance.controlType)
			{
				case SettingsManager.ControlType.Same:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : -this.augmentedInput.pos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.y : 0f);
						break;
					}
				}
				case SettingsManager.ControlType.Swap:
				{
					if (!PlayerController.Instance.IsSwitch)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : -this.augmentedInput.pos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.y : 0f);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.y : 0f);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : -this.augmentedInput.pos.y);
						break;
					}
				}
				case SettingsManager.ControlType.Simple:
				{
					if (!PlayerController.Instance.IsSwitch)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : 0f);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.y : -this.augmentedInput.pos.y);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : 0f);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.y : -this.augmentedInput.pos.y);
						break;
					}
				}
			}
			return single;
		}
	}

	public float AugmentedPopDir
	{
		get
		{
			float single = 0f;
			switch (SettingsManager.Instance.controlType)
			{
				case SettingsManager.ControlType.Same:
				{
					if (this.IsPopStick)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.y : this.augmentedInput.pos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.y : -this.augmentedInput.pos.y);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.y : -this.augmentedInput.pos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.y : this.augmentedInput.pos.y);
						break;
					}
				}
				case SettingsManager.ControlType.Swap:
				{
					if (PlayerController.Instance.IsSwitch)
					{
						if (this.IsPopStick)
						{
							if (!this.IsRightStick)
							{
								single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.y : -this.augmentedInput.pos.y);
								break;
							}
							else
							{
								single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.y : this.augmentedInput.pos.y);
								break;
							}
						}
						else if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.y : this.augmentedInput.pos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.y : -this.augmentedInput.pos.y);
							break;
						}
					}
					else if (this.IsPopStick)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.y : this.augmentedInput.pos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.y : -this.augmentedInput.pos.y);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.y : -this.augmentedInput.pos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.y : this.augmentedInput.pos.y);
						break;
					}
				}
				case SettingsManager.ControlType.Simple:
				{
					if (!this.IsPopStick)
					{
						break;
					}
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.y : -this.augmentedInput.pos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.y : this.augmentedInput.pos.y);
						break;
					}
				}
			}
			return single;
		}
	}

	public float AugmentedPopSpeed
	{
		get
		{
			return this.AugmentedPopToeVel.y;
		}
	}

	public float AugmentedPopToeSpeed
	{
		get
		{
			return this.AugmentedPopToeVel.magnitude;
		}
	}

	public Vector2 AugmentedPopToeVector
	{
		get
		{
			return Vector2.ClampMagnitude(new Vector2(this.AugmentedToeAxis, this.AugmentedSetupDir), 1f);
		}
	}

	public Vector2 AugmentedPopToeVel
	{
		get
		{
			return new Vector2(this.AugmentedToeAxisVel, this.AugmentedSetupDirVel);
		}
	}

	public float AugmentedSetupDir
	{
		get
		{
			float single = 0f;
			switch (SettingsManager.Instance.controlType)
			{
				case SettingsManager.ControlType.Same:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.y : -this.augmentedInput.pos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.y : this.augmentedInput.pos.y);
						break;
					}
				}
				case SettingsManager.ControlType.Swap:
				{
					if (PlayerController.Instance.IsSwitch)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.y : this.augmentedInput.pos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.y : -this.augmentedInput.pos.y);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.y : -this.augmentedInput.pos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.y : this.augmentedInput.pos.y);
						break;
					}
				}
				case SettingsManager.ControlType.Simple:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.y : this.augmentedInput.pos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.y : -this.augmentedInput.pos.y);
						break;
					}
				}
			}
			return single;
		}
	}

	public float AugmentedSetupDirVel
	{
		get
		{
			float single = 0f;
			switch (SettingsManager.Instance.controlType)
			{
				case SettingsManager.ControlType.Same:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.maxVelLastUpdate.y : -this.augmentedInput.maxVelLastUpdate.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.maxVelLastUpdate.y : this.augmentedInput.maxVelLastUpdate.y);
						break;
					}
				}
				case SettingsManager.ControlType.Swap:
				{
					if (PlayerController.Instance.IsSwitch)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.maxVelLastUpdate.y : this.augmentedInput.maxVelLastUpdate.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.maxVelLastUpdate.y : -this.augmentedInput.maxVelLastUpdate.y);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.maxVelLastUpdate.y : -this.augmentedInput.maxVelLastUpdate.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.maxVelLastUpdate.y : this.augmentedInput.maxVelLastUpdate.y);
						break;
					}
				}
				case SettingsManager.ControlType.Simple:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.maxVelLastUpdate.y : this.augmentedInput.maxVelLastUpdate.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.maxVelLastUpdate.y : -this.augmentedInput.maxVelLastUpdate.y);
						break;
					}
				}
			}
			return single;
		}
	}

	public float AugmentedToeAxis
	{
		get
		{
			float single = 0f;
			switch (SettingsManager.Instance.controlType)
			{
				case SettingsManager.ControlType.Same:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.x : -this.augmentedInput.pos.x);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.x : -this.augmentedInput.pos.x);
						break;
					}
				}
				case SettingsManager.ControlType.Swap:
				{
					if (PlayerController.Instance.IsSwitch)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.x : this.augmentedInput.pos.x);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.pos.x : this.augmentedInput.pos.x);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.x : -this.augmentedInput.pos.x);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.x : -this.augmentedInput.pos.x);
						break;
					}
				}
				case SettingsManager.ControlType.Simple:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.x : -this.augmentedInput.pos.x);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.pos.x : -this.augmentedInput.pos.x);
						break;
					}
				}
			}
			return single;
		}
	}

	public float AugmentedToeAxisVel
	{
		get
		{
			float single = 0f;
			switch (SettingsManager.Instance.controlType)
			{
				case SettingsManager.ControlType.Same:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.maxVelLastUpdate.x : -this.augmentedInput.maxVelLastUpdate.x);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.maxVelLastUpdate.x : -this.augmentedInput.maxVelLastUpdate.x);
						break;
					}
				}
				case SettingsManager.ControlType.Swap:
				{
					if (PlayerController.Instance.IsSwitch)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.maxVelLastUpdate.x : this.augmentedInput.maxVelLastUpdate.x);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.augmentedInput.maxVelLastUpdate.x : this.augmentedInput.maxVelLastUpdate.x);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.maxVelLastUpdate.x : -this.augmentedInput.maxVelLastUpdate.x);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.maxVelLastUpdate.x : -this.augmentedInput.maxVelLastUpdate.x);
						break;
					}
				}
				case SettingsManager.ControlType.Simple:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.maxVelLastUpdate.x : -this.augmentedInput.maxVelLastUpdate.x);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.augmentedInput.maxVelLastUpdate.x : -this.augmentedInput.maxVelLastUpdate.x);
						break;
					}
				}
			}
			return single;
		}
	}

	public float AugmentedToeSpeed
	{
		get
		{
			return this.AugmentedPopToeVel.x;
		}
	}

	public float FlipDir
	{
		get
		{
			float single = 0f;
			switch (SettingsManager.Instance.controlType)
			{
				case SettingsManager.ControlType.Same:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.x : -this.rawInput.pos.x);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.x : -this.rawInput.pos.x);
						break;
					}
				}
				case SettingsManager.ControlType.Swap:
				{
					if (PlayerController.Instance.IsSwitch)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.x : this.rawInput.pos.x);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.x : this.rawInput.pos.x);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.x : -this.rawInput.pos.x);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.x : -this.rawInput.pos.x);
						break;
					}
				}
				case SettingsManager.ControlType.Simple:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.x : -this.rawInput.pos.x);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.x : -this.rawInput.pos.x);
						break;
					}
				}
			}
			return single;
		}
	}

	public float ForwardDir
	{
		get
		{
			float single = 0f;
			switch (SettingsManager.Instance.controlType)
			{
				case SettingsManager.ControlType.Same:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.y : this.rawInput.pos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.y : this.rawInput.pos.y);
						break;
					}
				}
				case SettingsManager.ControlType.Swap:
				{
					if (!PlayerController.Instance.IsSwitch)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.y : this.rawInput.pos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.y : this.rawInput.pos.y);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.y : -this.rawInput.pos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.y : -this.rawInput.pos.y);
						break;
					}
				}
				case SettingsManager.ControlType.Simple:
				{
					if (!PlayerController.Instance.IsSwitch)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.y : this.rawInput.pos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.y : this.rawInput.pos.y);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.y : -this.rawInput.pos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.y : -this.rawInput.pos.y);
						break;
					}
				}
			}
			return single;
		}
	}

	public bool HoldingManual
	{
		get
		{
			return this._holdingManual;
		}
		set
		{
			this._holdingManual = value;
		}
	}

	public bool HoldingNoseManual
	{
		get
		{
			return this._holdingNoseManual;
		}
		set
		{
			this._holdingNoseManual = value;
		}
	}

	public bool IsFrontFoot
	{
		get;
		set;
	}

	public bool IsPopStick
	{
		get
		{
			if (PlayerController.Instance.playerSM.GetPopStickSM() != this)
			{
				return false;
			}
			return true;
		}
	}

	public bool IsRightStick
	{
		get;
		set;
	}

	public float LastSetupDir
	{
		get
		{
			float single = 0f;
			switch (SettingsManager.Instance.controlType)
			{
				case SettingsManager.ControlType.Same:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.prevPos.y : -this.rawInput.prevPos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.prevPos.y : this.rawInput.prevPos.y);
						break;
					}
				}
				case SettingsManager.ControlType.Swap:
				{
					if (PlayerController.Instance.IsSwitch)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.prevPos.y : this.rawInput.prevPos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.prevPos.y : -this.rawInput.prevPos.y);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.prevPos.y : -this.rawInput.prevPos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.prevPos.y : this.rawInput.prevPos.y);
						break;
					}
				}
				case SettingsManager.ControlType.Simple:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.prevPos.y : this.rawInput.prevPos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.prevPos.y : -this.rawInput.prevPos.y);
						break;
					}
				}
			}
			return single;
		}
	}

	public float LastToeAxis
	{
		get
		{
			float single = 0f;
			switch (SettingsManager.Instance.controlType)
			{
				case SettingsManager.ControlType.Same:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.prevPos.x : -this.rawInput.prevPos.x);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.prevPos.x : -this.rawInput.prevPos.x);
						break;
					}
				}
				case SettingsManager.ControlType.Swap:
				{
					if (PlayerController.Instance.IsSwitch)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.prevPos.x : this.rawInput.prevPos.x);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.prevPos.x : this.rawInput.prevPos.x);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.prevPos.x : -this.rawInput.prevPos.x);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.prevPos.x : -this.rawInput.prevPos.x);
						break;
					}
				}
				case SettingsManager.ControlType.Simple:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.prevPos.x : -this.rawInput.prevPos.x);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.prevPos.x : -this.rawInput.prevPos.x);
						break;
					}
				}
			}
			return single;
		}
	}

	public float ManualAxis
	{
		get
		{
			float single = 0f;
			if (!this.IsRightStick)
			{
				switch (SettingsManager.Instance.controlType)
				{
					case SettingsManager.ControlType.Same:
					{
						if (PlayerController.Instance.IsSwitch)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.y : 0f);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : -this.rawInput.pos.y);
							break;
						}
					}
					case SettingsManager.ControlType.Swap:
					{
						if (PlayerController.Instance.IsSwitch)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.y : 0f);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : -this.rawInput.pos.y);
							break;
						}
					}
					case SettingsManager.ControlType.Simple:
					{
						if (PlayerController.Instance.IsSwitch)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : 0f);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : 0f);
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
						if (PlayerController.Instance.IsSwitch)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : this.rawInput.pos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.y : 0f);
							break;
						}
					}
					case SettingsManager.ControlType.Swap:
					{
						if (PlayerController.Instance.IsSwitch)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : -this.rawInput.pos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.y : 0f);
							break;
						}
					}
					case SettingsManager.ControlType.Simple:
					{
						if (PlayerController.Instance.IsSwitch)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.y : -this.rawInput.pos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.y : -this.rawInput.pos.y);
							break;
						}
					}
				}
			}
			return Mathf.Clamp(single, 0f, 1f);
		}
	}

	public int ManualFrameCount
	{
		get
		{
			return this._manualFrameCount;
		}
		set
		{
			this._manualFrameCount = value;
		}
	}

	public float NollieSetupDir
	{
		get
		{
			float single = 0f;
			switch (SettingsManager.Instance.controlType)
			{
				case SettingsManager.ControlType.Same:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.y : 0f);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : this.rawInput.pos.y);
						break;
					}
				}
				case SettingsManager.ControlType.Swap:
				{
					if (!PlayerController.Instance.IsSwitch)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.y : 0f);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : this.rawInput.pos.y);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : this.rawInput.pos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.y : 0f);
						break;
					}
				}
				case SettingsManager.ControlType.Simple:
				{
					if (!PlayerController.Instance.IsSwitch)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.y : this.rawInput.pos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : 0f);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.y : this.rawInput.pos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : 0f);
						break;
					}
				}
			}
			return single;
		}
	}

	public float NoseManualAxis
	{
		get
		{
			float single = 0f;
			if (!this.IsRightStick)
			{
				switch (SettingsManager.Instance.controlType)
				{
					case SettingsManager.ControlType.Same:
					{
						if (PlayerController.Instance.IsSwitch)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : -this.rawInput.pos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.y : 0f);
							break;
						}
					}
					case SettingsManager.ControlType.Swap:
					{
						if (PlayerController.Instance.IsSwitch)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : this.rawInput.pos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.y : 0f);
							break;
						}
					}
					case SettingsManager.ControlType.Simple:
					{
						if (PlayerController.Instance.IsSwitch)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.y : this.rawInput.pos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.y : this.rawInput.pos.y);
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
						if (PlayerController.Instance.IsSwitch)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.y : 0f);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : this.rawInput.pos.y);
							break;
						}
					}
					case SettingsManager.ControlType.Swap:
					{
						if (PlayerController.Instance.IsSwitch)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.y : 0f);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : this.rawInput.pos.y);
							break;
						}
					}
					case SettingsManager.ControlType.Simple:
					{
						if (PlayerController.Instance.IsSwitch)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : 0f);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : 0f);
							break;
						}
					}
				}
			}
			return Mathf.Clamp(single, 0f, 1f);
		}
	}

	public int NoseManualFrameCount
	{
		get
		{
			return this._noseManualFrameCount;
		}
		set
		{
			this._noseManualFrameCount = value;
		}
	}

	public float OllieSetupDir
	{
		get
		{
			float single = 0f;
			switch (SettingsManager.Instance.controlType)
			{
				case SettingsManager.ControlType.Same:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : -this.rawInput.pos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.y : 0f);
						break;
					}
				}
				case SettingsManager.ControlType.Swap:
				{
					if (!PlayerController.Instance.IsSwitch)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : -this.rawInput.pos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.y : 0f);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.y : 0f);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : -this.rawInput.pos.y);
						break;
					}
				}
				case SettingsManager.ControlType.Simple:
				{
					if (!PlayerController.Instance.IsSwitch)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : 0f);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.y : -this.rawInput.pos.y);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? 0f : 0f);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.y : -this.rawInput.pos.y);
						break;
					}
				}
			}
			return single;
		}
	}

	public float PopDir
	{
		get
		{
			float single = 0f;
			switch (SettingsManager.Instance.controlType)
			{
				case SettingsManager.ControlType.Same:
				{
					if (this.IsPopStick)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.y : this.rawInput.pos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.y : -this.rawInput.pos.y);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.y : -this.rawInput.pos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.y : this.rawInput.pos.y);
						break;
					}
				}
				case SettingsManager.ControlType.Swap:
				{
					if (PlayerController.Instance.IsSwitch)
					{
						if (this.IsPopStick)
						{
							if (!this.IsRightStick)
							{
								single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.y : -this.rawInput.pos.y);
								break;
							}
							else
							{
								single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.y : this.rawInput.pos.y);
								break;
							}
						}
						else if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.y : this.rawInput.pos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.y : -this.rawInput.pos.y);
							break;
						}
					}
					else if (this.IsPopStick)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.y : this.rawInput.pos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.y : -this.rawInput.pos.y);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.y : -this.rawInput.pos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.y : this.rawInput.pos.y);
						break;
					}
				}
				case SettingsManager.ControlType.Simple:
				{
					if (!this.IsPopStick)
					{
						break;
					}
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.y : -this.rawInput.pos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.y : this.rawInput.pos.y);
						break;
					}
				}
			}
			return single;
		}
	}

	public float PopSpeed
	{
		get
		{
			return this.PopToeVel.y;
		}
	}

	public float PopToeSpeed
	{
		get
		{
			return this.PopToeVel.magnitude;
		}
	}

	public Vector2 PopToeVector
	{
		get
		{
			return Vector2.ClampMagnitude(new Vector2(this.ToeAxis, this.SetupDir), 1f);
		}
	}

	public Vector2 PopToeVel
	{
		get
		{
			return new Vector2(this.ToeAxisVel, this.SetupDirVel);
		}
	}

	public float SetupDir
	{
		get
		{
			float single = 0f;
			switch (SettingsManager.Instance.controlType)
			{
				case SettingsManager.ControlType.Same:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.y : -this.rawInput.pos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.y : this.rawInput.pos.y);
						break;
					}
				}
				case SettingsManager.ControlType.Swap:
				{
					if (PlayerController.Instance.IsSwitch)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.y : this.rawInput.pos.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.y : -this.rawInput.pos.y);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.y : -this.rawInput.pos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.y : this.rawInput.pos.y);
						break;
					}
				}
				case SettingsManager.ControlType.Simple:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.y : this.rawInput.pos.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.y : -this.rawInput.pos.y);
						break;
					}
				}
			}
			return single;
		}
	}

	public float SetupDirVel
	{
		get
		{
			float single = 0f;
			switch (SettingsManager.Instance.controlType)
			{
				case SettingsManager.ControlType.Same:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.maxVelLastUpdate.y : -this.rawInput.maxVelLastUpdate.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.maxVelLastUpdate.y : this.rawInput.maxVelLastUpdate.y);
						break;
					}
				}
				case SettingsManager.ControlType.Swap:
				{
					if (PlayerController.Instance.IsSwitch)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.maxVelLastUpdate.y : this.rawInput.maxVelLastUpdate.y);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.maxVelLastUpdate.y : -this.rawInput.maxVelLastUpdate.y);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.maxVelLastUpdate.y : -this.rawInput.maxVelLastUpdate.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.maxVelLastUpdate.y : this.rawInput.maxVelLastUpdate.y);
						break;
					}
				}
				case SettingsManager.ControlType.Simple:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.maxVelLastUpdate.y : this.rawInput.maxVelLastUpdate.y);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.maxVelLastUpdate.y : -this.rawInput.maxVelLastUpdate.y);
						break;
					}
				}
			}
			return single;
		}
	}

	public float ToeAxis
	{
		get
		{
			float single = 0f;
			switch (SettingsManager.Instance.controlType)
			{
				case SettingsManager.ControlType.Same:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.x : -this.rawInput.pos.x);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.x : -this.rawInput.pos.x);
						break;
					}
				}
				case SettingsManager.ControlType.Swap:
				{
					if (PlayerController.Instance.IsSwitch)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.x : this.rawInput.pos.x);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.pos.x : this.rawInput.pos.x);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.x : -this.rawInput.pos.x);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.x : -this.rawInput.pos.x);
						break;
					}
				}
				case SettingsManager.ControlType.Simple:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.x : -this.rawInput.pos.x);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.pos.x : -this.rawInput.pos.x);
						break;
					}
				}
			}
			return single;
		}
	}

	public float ToeAxisVel
	{
		get
		{
			float single = 0f;
			switch (SettingsManager.Instance.controlType)
			{
				case SettingsManager.ControlType.Same:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.maxVelLastUpdate.x : -this.rawInput.maxVelLastUpdate.x);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.maxVelLastUpdate.x : -this.rawInput.maxVelLastUpdate.x);
						break;
					}
				}
				case SettingsManager.ControlType.Swap:
				{
					if (PlayerController.Instance.IsSwitch)
					{
						if (!this.IsRightStick)
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.maxVelLastUpdate.x : this.rawInput.maxVelLastUpdate.x);
							break;
						}
						else
						{
							single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? -this.rawInput.maxVelLastUpdate.x : this.rawInput.maxVelLastUpdate.x);
							break;
						}
					}
					else if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.maxVelLastUpdate.x : -this.rawInput.maxVelLastUpdate.x);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.maxVelLastUpdate.x : -this.rawInput.maxVelLastUpdate.x);
						break;
					}
				}
				case SettingsManager.ControlType.Simple:
				{
					if (!this.IsRightStick)
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.maxVelLastUpdate.x : -this.rawInput.maxVelLastUpdate.x);
						break;
					}
					else
					{
						single = (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.rawInput.maxVelLastUpdate.x : -this.rawInput.maxVelLastUpdate.x);
						break;
					}
				}
			}
			return single;
		}
	}

	public float ToeSpeed
	{
		get
		{
			return this.PopToeVel.x;
		}
	}

	public StickInput()
	{
	}

	private void Awake()
	{
		this.InitializeRawData();
	}

	private void InitializeRawData()
	{
		this.rawInput.pos = Vector2.zero;
		this.rawInput.prevPos = Vector2.zero;
		this.rawInput.lastPos = Vector2.zero;
		this.rawInput.avgSpeedLastUpdate = 0f;
		this.augmentedInput.pos = Vector2.zero;
		this.augmentedInput.prevPos = Vector2.zero;
		this.augmentedInput.lastPos = Vector2.zero;
		this.augmentedInput.avgSpeedLastUpdate = 0f;
	}

	private void LerpFootDir()
	{
		this._toeAxisLerp = Mathf.Lerp(this._toeAxisLerp, this.ToeAxis, Time.deltaTime * 10f);
		this._forwardDirLerp = Mathf.Lerp(this._forwardDirLerp, this.ForwardDir, Time.deltaTime * 10f);
	}

	private void OnFlipStickCentered()
	{
		if (!this._flipStickCentered)
		{
			if (this.rawInput.lastPos.magnitude < 0.05f && this.rawInput.prevPos.magnitude < 0.05f && this.rawInput.pos.magnitude < 0.05f && this.rawInput.avgSpeedLastUpdate < 5f)
			{
				this._flipStickCentered = true;
				PlayerController.Instance.playerSM.OnFlipStickCenteredSM();
				return;
			}
		}
		else if (this.rawInput.pos.magnitude > 0.05f || this.rawInput.avgSpeedLastUpdate < 5f)
		{
			this._flipStickCentered = false;
		}
	}

	private void OnPopStickCentered()
	{
		if (!this._popStickCentered)
		{
			if (this.rawInput.lastPos.magnitude < 0.05f && this.rawInput.prevPos.magnitude < 0.05f && this.rawInput.pos.magnitude < 0.05f && this.rawInput.avgSpeedLastUpdate < 1f)
			{
				this._popStickCentered = true;
				PlayerController.Instance.playerSM.OnPopStickCenteredSM();
				return;
			}
		}
		else if (this.rawInput.pos.magnitude > 0.05f || this.rawInput.avgSpeedLastUpdate < 5f)
		{
			this._popStickCentered = false;
		}
	}

	private void OnStickCenteredUpdate(bool p_right)
	{
		if (p_right)
		{
			PlayerController.Instance.playerSM.OnRightStickCenteredUpdateSM();
			return;
		}
		PlayerController.Instance.playerSM.OnLeftStickCenteredUpdateSM();
	}

	public void OnStickPressed(bool p_right)
	{
		PlayerController.Instance.playerSM.OnStickPressedSM(p_right);
	}

	private void SteezeIKWeights()
	{
		if (SettingsManager.Instance.controlType != SettingsManager.ControlType.Simple)
		{
			if (this.IsRightStick)
			{
				PlayerController.Instance.SetRightSteezeWeight(this.rawInput.pos.magnitude);
				return;
			}
			PlayerController.Instance.SetLeftSteezeWeight(this.rawInput.pos.magnitude);
			return;
		}
		if (SettingsManager.Instance.stance == SettingsManager.Stance.Regular)
		{
			if (!PlayerController.Instance.IsSwitch)
			{
				if (this.IsRightStick)
				{
					PlayerController.Instance.SetRightSteezeWeight(this.rawInput.pos.magnitude);
					return;
				}
				PlayerController.Instance.SetLeftSteezeWeight(this.rawInput.pos.magnitude);
				return;
			}
			if (this.IsRightStick)
			{
				PlayerController.Instance.SetLeftSteezeWeight(this.rawInput.pos.magnitude);
				return;
			}
			PlayerController.Instance.SetRightSteezeWeight(this.rawInput.pos.magnitude);
			return;
		}
		if (!PlayerController.Instance.IsSwitch)
		{
			if (this.IsRightStick)
			{
				PlayerController.Instance.SetLeftSteezeWeight(this.rawInput.pos.magnitude);
				return;
			}
			PlayerController.Instance.SetRightSteezeWeight(this.rawInput.pos.magnitude);
			return;
		}
		if (this.IsRightStick)
		{
			PlayerController.Instance.SetRightSteezeWeight(this.rawInput.pos.magnitude);
			return;
		}
		PlayerController.Instance.SetLeftSteezeWeight(this.rawInput.pos.magnitude);
	}

	public void StickUpdate(bool p_right, InputThread _inputThread)
	{
		this.UpdateRawInput(p_right, _inputThread);
		this.UpdateInterpretedInput(p_right);
		if (this.rawInput.pos.magnitude < 0.1f && this.rawInput.avgVelLastUpdate.magnitude < 5f)
		{
			this.OnStickCenteredUpdate(p_right);
		}
		if (!this.IsPopStick)
		{
			PlayerController.Instance.playerSM.OnFlipStickUpdateSM();
			this.OnFlipStickCentered();
			PlayerController.Instance.DebugPopStick(false, p_right);
		}
		else
		{
			PlayerController.Instance.playerSM.OnPopStickUpdateSM();
			this.OnPopStickCentered();
			PlayerController.Instance.DebugPopStick(true, p_right);
		}
		PlayerController.Instance.OnManualUpdate(this);
		PlayerController.Instance.OnNoseManualUpdate(this);
		this.LerpFootDir();
		PlayerController.Instance.SetInAirFootPlacement(this._toeAxisLerp, this._forwardDirLerp, this.IsFrontFoot);
		this.SteezeIKWeights();
	}

	private void UpdateInterpretedInput(bool p_right)
	{
		this.IsRightStick = p_right;
	}

	private void UpdateRawInput(bool _right, InputThread _inputThread)
	{
		Vector2 vector2;
		Vector2 vector21;
		Vector2 vector22;
		Vector2 vector23;
		this.rawInput.stick = (_right ? StickInput.InputData.Stick.Right : StickInput.InputData.Stick.Left);
		this.rawInput.lastPos = this.rawInput.prevPos;
		this.rawInput.prevPos = this.rawInput.pos;
		vector2 = (_right ? _inputThread.lastPosRight : _inputThread.lastPosLeft);
		this.rawInput.pos = vector2;
		vector21 = (_right ? _inputThread.maxVelLastUpdateRight : _inputThread.maxVelLastUpdateLeft);
		this.rawInput.maxVelLastUpdate = vector21;
		this.rawInput.radialVel = (this.rawInput.pos.magnitude - this.rawInput.prevPos.magnitude) / Time.deltaTime;
		this.augmentedInput.stick = (_right ? StickInput.InputData.Stick.Right : StickInput.InputData.Stick.Left);
		this.augmentedInput.lastPos = this.augmentedInput.prevPos;
		this.augmentedInput.prevPos = this.augmentedInput.pos;
		vector22 = (_right ? Mathd.RotateVector2(_inputThread.lastPosRight, PlayerController.Instance.playerSM.GetAugmentedAngleSM(this)) : Mathd.RotateVector2(_inputThread.lastPosLeft, PlayerController.Instance.playerSM.GetAugmentedAngleSM(this)));
		this.augmentedInput.pos = vector22;
		vector23 = (_right ? Mathd.RotateVector2(_inputThread.maxVelLastUpdateRight, PlayerController.Instance.playerSM.GetAugmentedAngleSM(this)) : Mathd.RotateVector2(_inputThread.maxVelLastUpdateLeft, PlayerController.Instance.playerSM.GetAugmentedAngleSM(this)));
		this.augmentedInput.maxVelLastUpdate = vector23;
		if (SettingsManager.Instance.controlType != SettingsManager.ControlType.Same && SettingsManager.Instance.controlType != SettingsManager.ControlType.Swap)
		{
			if (this.IsRightStick)
			{
				this.IsFrontFoot = false;
				return;
			}
			this.IsFrontFoot = true;
			return;
		}
		if (SettingsManager.Instance.stance == SettingsManager.Stance.Regular)
		{
			if (this.IsRightStick)
			{
				this.IsFrontFoot = false;
				return;
			}
			this.IsFrontFoot = true;
			return;
		}
		if (this.IsRightStick)
		{
			this.IsFrontFoot = true;
			return;
		}
		this.IsFrontFoot = false;
	}

	public struct InputData
	{
		public StickInput.InputData.Stick stick;

		public Vector2 pos;

		public Vector2 prevPos;

		public Vector2 lastPos;

		public float avgSpeedLastUpdate;

		public Vector2 maxVelLastUpdate;

		public Vector2 avgVelLastUpdate;

		public float radialVel;

		public enum Stick
		{
			Left,
			Right
		}
	}
}