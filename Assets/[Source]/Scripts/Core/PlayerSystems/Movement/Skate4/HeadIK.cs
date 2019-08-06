using FSMHelper;
using System;
using UnityEngine;

public class HeadIK : MonoBehaviour
{
	public Transform head;

	public Transform currentRot;

	public Transform targetRot;

	public Transform goofyTargetRot;

	public float speed = 15f;

	public HeadIK()
	{
	}

	private bool IsAllowedAnimation()
	{
		if (PlayerController.Instance.IsCurrentAnimationPlaying("Riding") || PlayerController.Instance.IsCurrentAnimationPlaying("RidingToPush") || PlayerController.Instance.IsCurrentAnimationPlaying("PushToRiding") || PlayerController.Instance.IsCurrentAnimationPlaying("Braking"))
		{
			return false;
		}
		return !PlayerController.Instance.IsCurrentAnimationPlaying("Push Button");
	}

	private void LateUpdate()
	{
		if (!this.IsAllowedAnimation() || !PlayerController.Instance.IsGrounded() || !PlayerController.Instance.IsAnimSwitch || PlayerController.Instance.playerSM.IsPushingSM())
		{
			this.currentRot.rotation = Quaternion.Slerp(this.currentRot.rotation, this.head.rotation, Time.deltaTime * this.speed);
		}
		else
		{
			this.currentRot.rotation = Quaternion.Slerp(this.currentRot.rotation, (SettingsManager.Instance.stance == SettingsManager.Stance.Regular ? this.targetRot.rotation : this.goofyTargetRot.rotation), Time.deltaTime * this.speed);
		}
		this.head.rotation = this.currentRot.rotation;
	}
}