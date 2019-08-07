using System;
using UnityEngine;

public class CameraAnimationController : MonoBehaviour
{
	public Animator cameraAnimator;

	public AnimationClip[] clips;

	public int index;

	public CameraAnimationController()
	{
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			Debug.LogError("Spacebar Pressed");
			if (this.index < (int)this.clips.Length)
			{
				Debug.LogError("Play");
				this.cameraAnimator.Play(this.clips[this.index].name);
				this.index++;
			}
		}
	}
}