using FSMHelper;
using System;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
	public Animator skaterAnim;

	[SerializeField]
	public Animator ikAnim;

	[SerializeField]
	private Animator _steezeAnim;

	[SerializeField]
	private string _activeAnimation;

	public AnimationController()
	{
	}

	public void CrossFadeAnimation(string p_value, float p_transitionDuration)
	{
		this.skaterAnim.CrossFadeInFixedTime(p_value, p_transitionDuration);
		this.ikAnim.CrossFadeInFixedTime(p_value, p_transitionDuration);
	}

	public void ForceAnimation(string p_anim)
	{
		this.skaterAnim.Play(p_anim, 0, 0f);
		this.ikAnim.Play(p_anim, 0, 0f);
	}

	public void ForceBeginPop()
	{
		PlayerController.Instance.playerSM.SendEventBeginPopSM();
	}

	public float GetAnimatorSpeed()
	{
		return this.skaterAnim.speed;
	}

	public void GetCurrentAnim()
	{
		float single = 0f;
		string str = "";
		AnimatorClipInfo[] currentAnimatorClipInfo = this.skaterAnim.GetCurrentAnimatorClipInfo(0);
		for (int i = 0; i < (int)currentAnimatorClipInfo.Length; i++)
		{
			AnimatorClipInfo animatorClipInfo = currentAnimatorClipInfo[i];
			if (animatorClipInfo.weight > single)
			{
				str = animatorClipInfo.clip.name;
				single = animatorClipInfo.weight;
			}
		}
		this._activeAnimation = str;
	}

	public void ScaleAnimSpeed(float p_speed)
	{
		this.skaterAnim.speed = p_speed;
		this.ikAnim.speed = p_speed;
	}

	public void SendEventBeginPop(string p_animName)
	{
		this.GetCurrentAnim();
		if (p_animName == this._activeAnimation)
		{
			PlayerController.Instance.playerSM.SendEventBeginPopSM();
		}
	}

	public void SendEventEndFlipPeriod(string p_animName)
	{
		PlayerController.Instance.playerSM.SendEventEndFlipPeriodSM();
	}

	public void SendEventExtend(AnimationEvent p_animationEvent)
	{
	}

	public void SendEventImpactEnd(string p_animName)
	{
	}

	public void SendEventLastPushCheck(string p_animName)
	{
		if (p_animName == this._activeAnimation)
		{
			PlayerController.Instance.playerSM.OnPushLastCheckSM();
		}
	}

	public void SendEventPop(AnimationEvent p_animationEvent)
	{
		float pAnimationEvent = p_animationEvent.floatParameter;
		string str = p_animationEvent.stringParameter;
		this.GetCurrentAnim();
	}

	public void SendEventPush(string p_animName)
	{
		this.GetCurrentAnim();
		PlayerController.Instance.playerSM.OnPushSM();
	}

	public void SendEventPushEnd(string p_animName)
	{
		if (p_animName == this._activeAnimation)
		{
			PlayerController.Instance.playerSM.OnPushEndSM();
		}
	}

	public void SendEventReleased(string p_animName)
	{
	}

	public void SetGrindTweakValue(float p_tweak)
	{
		this.SetValue("GrindTweak", p_tweak);
	}

	public void SetNollieSteezeIK(float p_value)
	{
		this._steezeAnim.SetFloat("Nollie", p_value);
	}

	public void SetSteezeValue(string p_animName, float p_value)
	{
		this._steezeAnim.SetFloat(p_animName, p_value);
	}

	public void SetTweakMagnitude(float p_frontMagnitude, float p_backMagnitude)
	{
		this.SetValue("TweakMagnitude", p_frontMagnitude - p_backMagnitude);
	}

	public void SetTweakValues(float p_forwardAxis, float p_toeAxis)
	{
		this.SetValue("ForwardTweak", p_forwardAxis);
		this.SetValue("ToeSideTweak", p_toeAxis);
	}

	public void SetValue(string p_animName, bool p_value)
	{
		this.skaterAnim.SetBool(p_animName, p_value);
		this.ikAnim.SetBool(p_animName, p_value);
	}

	public void SetValue(string p_animName, float p_value)
	{
		this.skaterAnim.SetFloat(p_animName, p_value);
		this.ikAnim.SetFloat(p_animName, p_value);
	}

	public void SetValue(string p_animName, int p_value)
	{
		this.skaterAnim.SetInteger(p_animName, p_value);
		this.ikAnim.SetInteger(p_animName, p_value);
	}

	public void SetValue(string p_animName)
	{
		this.skaterAnim.SetTrigger(p_animName);
		this.ikAnim.SetTrigger(p_animName);
	}
}