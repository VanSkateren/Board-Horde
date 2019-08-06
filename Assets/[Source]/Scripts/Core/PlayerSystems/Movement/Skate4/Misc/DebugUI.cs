using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour
{
	[Header("Button Images")]
	[SerializeField]
	private Image A;

	[SerializeField]
	private Image B;

	[SerializeField]
	private Image X;

	[SerializeField]
	private Image Y;

	[SerializeField]
	private Image Start;

	[SerializeField]
	private Image Select;

	[SerializeField]
	private Image DPadUp;

	[SerializeField]
	private Image DPadDown;

	[SerializeField]
	private Image DPadLeft;

	[SerializeField]
	private Image DPadRight;

	[SerializeField]
	private Image RT;

	[SerializeField]
	private Image LT;

	[SerializeField]
	private Image LeftStickClick;

	[SerializeField]
	private Image RightStickClick;

	[Header("Transforms")]
	[SerializeField]
	private RectTransform LeftTriggerFill;

	[SerializeField]
	private RectTransform LeftTriggerZero;

	[SerializeField]
	private RectTransform LeftTriggerOne;

	[SerializeField]
	private RectTransform RightTriggerFill;

	[SerializeField]
	private RectTransform RightTriggerZero;

	[SerializeField]
	private RectTransform RightTriggerOne;

	[SerializeField]
	private RectTransform[] LeftAnalogPos = new RectTransform[4];

	[SerializeField]
	private RectTransform[] RightAnalogPos = new RectTransform[4];

	[SerializeField]
	private RectTransform[] LeftAugmentedAnalogPos = new RectTransform[4];

	[SerializeField]
	private RectTransform[] RightAugmentedAnalogPos = new RectTransform[4];

	[Header("Debug Text Info")]
	[SerializeField]
	private TextMeshProUGUI _inputThread;

	[SerializeField]
	private TextMeshProUGUI _skaterVelocity;

	[SerializeField]
	private TextMeshProUGUI _skaterAngularVelocity;

	[SerializeField]
	private TextMeshProUGUI _boardVelocity;

	[SerializeField]
	private TextMeshProUGUI _boardAngularVelocity;

	[Header("Helper Data")]
	[SerializeField]
	private float _stickInputScaler = 300f;

	public DebugUI()
	{
	}

	private void FixedUpdate()
	{
		TextMeshProUGUI str = this._skaterVelocity;
		Vector3 instance = PlayerController.Instance.skaterController.skaterRigidbody.velocity;
		double num = Math.Round((double)instance.magnitude, 3);
		str.text = num.ToString("n3");
		TextMeshProUGUI textMeshProUGUI = this._skaterAngularVelocity;
		instance = PlayerController.Instance.skaterController.skaterRigidbody.angularVelocity;
		num = Math.Round((double)instance.magnitude, 3);
		textMeshProUGUI.text = num.ToString("n3");
		TextMeshProUGUI str1 = this._boardVelocity;
		instance = PlayerController.Instance.boardController.boardRigidbody.velocity;
		num = Math.Round((double)instance.magnitude, 5);
		str1.text = num.ToString("n3");
		TextMeshProUGUI textMeshProUGUI1 = this._boardAngularVelocity;
		instance = PlayerController.Instance.boardController.boardRigidbody.angularVelocity;
		num = Math.Round((double)instance.magnitude, 3);
		textMeshProUGUI1.text = num.ToString("n3");
	}

	private Image GetButton(DebugUI.Buttons p_button)
	{
		Image a = this.A;
		switch (p_button)
		{
			case DebugUI.Buttons.A:
			{
				a = this.A;
				break;
			}
			case DebugUI.Buttons.B:
			{
				a = this.B;
				break;
			}
			case DebugUI.Buttons.X:
			{
				a = this.X;
				break;
			}
			case DebugUI.Buttons.Y:
			{
				a = this.Y;
				break;
			}
			case DebugUI.Buttons.Start:
			{
				a = this.Start;
				break;
			}
			case DebugUI.Buttons.Select:
			{
				a = this.Select;
				break;
			}
			case DebugUI.Buttons.DPadUp:
			{
				a = this.DPadUp;
				break;
			}
			case DebugUI.Buttons.DPadDown:
			{
				a = this.DPadDown;
				break;
			}
			case DebugUI.Buttons.DPadLeft:
			{
				a = this.DPadLeft;
				break;
			}
			case DebugUI.Buttons.DPadRight:
			{
				a = this.DPadRight;
				break;
			}
			case DebugUI.Buttons.RT:
			{
				a = this.RT;
				break;
			}
			case DebugUI.Buttons.LT:
			{
				a = this.LT;
				break;
			}
			case DebugUI.Buttons.LS:
			{
				a = this.LeftStickClick;
				break;
			}
			case DebugUI.Buttons.RS:
			{
				a = this.RightStickClick;
				break;
			}
		}
		return a;
	}

	public void LerpLeftTrigger(float p_value)
	{
		this.LeftTriggerFill.localPosition = Vector3.Lerp(this.LeftTriggerZero.localPosition, this.LeftTriggerOne.localPosition, p_value);
	}

	public void LerpRightTrigger(float p_value)
	{
		this.RightTriggerFill.localPosition = Vector3.Lerp(this.RightTriggerZero.localPosition, this.RightTriggerOne.localPosition, p_value);
	}

	public void ResetColor(DebugUI.Buttons p_button)
	{
		if (this.GetButton(p_button).color != Color.white)
		{
			this.GetButton(p_button).color = Color.white;
		}
	}

	public void ResetColor(DebugUI.Buttons p_button, float p_alpha)
	{
		if (this.GetButton(p_button).color != new Color(Color.white.r, Color.white.g, Color.white.b, p_alpha))
		{
			this.GetButton(p_button).color = new Color(Color.white.r, Color.white.g, Color.white.b, p_alpha);
		}
	}

	public IEnumerator ResetColor(Image p_image)
	{
		yield return null;
		p_image.color = Color.white;
	}

	public void SetColor(DebugUI.Buttons p_button)
	{
		if (this.GetButton(p_button).color != Color.red)
		{
			this.GetButton(p_button).color = Color.red;
		}
	}

	public void SetColorOnce(DebugUI.Buttons p_button)
	{
		this.GetButton(p_button).color = Color.red;
		base.StartCoroutine(this.ResetColor(this.GetButton(p_button)));
	}

	public void SetThreadActive(bool p_active)
	{
	}

	public void UpdateLeftAugmentedStickInput(Vector2 p_pos)
	{
		this.UpdateStickInput(ref this.LeftAugmentedAnalogPos, p_pos);
	}

	public void UpdateLeftStickInput(Vector2 p_pos)
	{
		this.UpdateStickInput(ref this.LeftAnalogPos, p_pos);
	}

	public void UpdateRightAugmentedStickInput(Vector2 p_pos)
	{
		this.UpdateStickInput(ref this.RightAugmentedAnalogPos, p_pos);
	}

	public void UpdateRightStickInput(Vector2 p_pos)
	{
		this.UpdateStickInput(ref this.RightAnalogPos, p_pos);
	}

	public void UpdateStickInput(ref RectTransform[] p_rectTransform, Vector2 p_pos)
	{
		for (int i = 0; i < 3; i++)
		{
			p_rectTransform[i + 1].localPosition = p_rectTransform[i].localPosition;
		}
		p_rectTransform[0].localPosition = p_pos * this._stickInputScaler;
	}

	public enum Buttons
	{
		A,
		B,
		X,
		Y,
		Start,
		Select,
		DPadUp,
		DPadDown,
		DPadLeft,
		DPadRight,
		RT,
		LT,
		LS,
		RS
	}
}