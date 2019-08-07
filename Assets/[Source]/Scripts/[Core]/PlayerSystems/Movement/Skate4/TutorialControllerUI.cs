using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class TutorialControllerUI : MonoBehaviour
{
	public Color neutralColor;

	public Color hightlightColor;

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
	private Image LeftTriggerFadeSprite;

	[SerializeField]
	private Text LeftTriggerFadeText;

	[SerializeField]
	private RectTransform RightTriggerFill;

	[SerializeField]
	private RectTransform RightTriggerZero;

	[SerializeField]
	private RectTransform RightTriggerOne;

	[SerializeField]
	private Image RightTriggerFadeSprite;

	[SerializeField]
	private Text RightTriggerFadeText;

	[SerializeField]
	private RectTransform[] LeftAnalogPos = new RectTransform[4];

	[SerializeField]
	private RectTransform[] RightAnalogPos = new RectTransform[4];

	[SerializeField]
	private RectTransform leftArrow;

	[SerializeField]
	private Image leftArrowImage;

	[SerializeField]
	private RectTransform rightArrow;

	[SerializeField]
	private Image rightArrowImage;

	[Header("Helper Data")]
	[SerializeField]
	private float _stickInputScaler = 300f;

	public float stickBoundsMult;

	public float _leftRotMult;

	private Vector2 _leftLastPos;

	private Vector2 _leftLerpedVel;

	private Vector2 _rightLastPos;

	private Vector2 _rightLerpedVel;

	public TutorialControllerUI()
	{
	}

	private Image GetButton(TutorialControllerUI.Buttons p_button)
	{
		Image a = this.A;
		switch (p_button)
		{
			case TutorialControllerUI.Buttons.A:
			{
				a = this.A;
				break;
			}
			case TutorialControllerUI.Buttons.B:
			{
				a = this.B;
				break;
			}
			case TutorialControllerUI.Buttons.X:
			{
				a = this.X;
				break;
			}
			case TutorialControllerUI.Buttons.Y:
			{
				a = this.Y;
				break;
			}
			case TutorialControllerUI.Buttons.Start:
			{
				a = this.Start;
				break;
			}
			case TutorialControllerUI.Buttons.Select:
			{
				a = this.Select;
				break;
			}
			case TutorialControllerUI.Buttons.DPadUp:
			{
				a = this.DPadUp;
				break;
			}
			case TutorialControllerUI.Buttons.DPadDown:
			{
				a = this.DPadDown;
				break;
			}
			case TutorialControllerUI.Buttons.DPadLeft:
			{
				a = this.DPadLeft;
				break;
			}
			case TutorialControllerUI.Buttons.DPadRight:
			{
				a = this.DPadRight;
				break;
			}
			case TutorialControllerUI.Buttons.RT:
			{
				a = this.RT;
				break;
			}
			case TutorialControllerUI.Buttons.LT:
			{
				a = this.LT;
				break;
			}
			case TutorialControllerUI.Buttons.LS:
			{
				a = this.LeftStickClick;
				break;
			}
			case TutorialControllerUI.Buttons.RS:
			{
				a = this.RightStickClick;
				break;
			}
		}
		return a;
	}

	public void LerpLeftTrigger(float p_value)
	{
		this.LeftTriggerFadeSprite.color = new Color(1f, 1f, 1f, p_value);
		this.LeftTriggerFadeText.color = new Color(0.007843138f, 0.4509804f, 1f, p_value);
		this.LeftTriggerFill.localPosition = Vector3.Lerp(this.LeftTriggerZero.localPosition, this.LeftTriggerOne.localPosition, p_value);
	}

	public void LerpRightTrigger(float p_value)
	{
		this.RightTriggerFadeSprite.color = new Color(1f, 1f, 1f, p_value);
		this.RightTriggerFadeText.color = new Color(0.007843138f, 0.4509804f, 1f, p_value);
		this.RightTriggerFill.localPosition = Vector3.Lerp(this.RightTriggerZero.localPosition, this.RightTriggerOne.localPosition, p_value);
	}

	public void ResetColor(TutorialControllerUI.Buttons p_button)
	{
		if (this.GetButton(p_button).color != this.neutralColor)
		{
			this.GetButton(p_button).color = this.neutralColor;
		}
	}

	public void ResetColor(TutorialControllerUI.Buttons p_button, float p_alpha)
	{
		if (this.GetButton(p_button).color != new Color(this.neutralColor.r, this.neutralColor.g, this.neutralColor.b, p_alpha))
		{
			this.GetButton(p_button).color = new Color(this.neutralColor.r, this.neutralColor.g, this.neutralColor.b, p_alpha);
		}
	}

	public IEnumerator ResetColor(Image p_image)
	{
		TutorialControllerUI tutorialControllerUI = null;
		yield return null;
		p_image.color = tutorialControllerUI.neutralColor;
	}

	public void SetColor(TutorialControllerUI.Buttons p_button)
	{
		if (this.GetButton(p_button).color != this.hightlightColor)
		{
			this.GetButton(p_button).color = this.hightlightColor;
		}
	}

	public void SetColorOnce(TutorialControllerUI.Buttons p_button)
	{
		UnityEngine.Debug.Log("Asdfasdf");
		this.GetButton(p_button).color = this.hightlightColor;
		base.StartCoroutine(this.ResetColor(this.GetButton(p_button)));
	}

	public void ShowClickLeft(bool val)
	{
		this.LeftStickClick.enabled = val;
	}

	public void ShowClickRight(bool val)
	{
		this.RightStickClick.enabled = val;
	}

	public void UpdateLeftStickInput(Vector2 p_pos)
	{
		if (p_pos.magnitude != 0f)
		{
			Vector2 pPos = p_pos - this._leftLastPos;
			this.leftArrow.localPosition = p_pos.normalized * this.stickBoundsMult;
			this.leftArrow.localRotation = Quaternion.FromToRotation(Vector3.up, p_pos);
			this._leftLastPos = p_pos;
		}
		if (this.leftArrowImage.color.a != p_pos.magnitude)
		{
			this.leftArrowImage.color = new Color(0.01176471f, 0.8705882f, 0.7450981f, p_pos.magnitude);
		}
		this.UpdateStickInput(ref this.LeftAnalogPos, p_pos);
	}

	public void UpdateRightStickInput(Vector2 p_pos)
	{
		if (p_pos.magnitude != 0f)
		{
			Vector2 pPos = p_pos - this._rightLastPos;
			this.rightArrow.localPosition = p_pos.normalized * this.stickBoundsMult;
			this.rightArrow.localRotation = Quaternion.FromToRotation(Vector3.up, p_pos);
			this._rightLastPos = p_pos;
		}
		this.rightArrowImage.color = new Color(1f, 0.003921569f, 0.4588235f, p_pos.magnitude * 2f);
		this.UpdateStickInput(ref this.RightAnalogPos, p_pos);
	}

	public void UpdateStickInput(ref RectTransform[] p_rectTransform, Vector2 p_pos)
	{
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