using Rewired;
using System;
using UnityEngine;

public class CameraFollowAndMove : MonoBehaviour
{
	public Transform skater;

	public Camera cam;

	public Animator khAnim;

	public AnimationClip reset;

	public AnimationClip line;

	private bool follow;

	public CameraFollowAndMove()
	{
	}

	private void FixedUpdate()
	{
		this.cam.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(this.skater.position - this.cam.transform.position, Vector3.up), Vector3.up);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!this.follow)
		{
			this.follow = true;
			this.khAnim.Play(this.line.name);
		}
	}

	private void Start()
	{
		this.cam.fieldOfView = 40f;
	}

	private void Update()
	{
		if (PlayerController.Instance.inputController.player.GetButtonDown("X"))
		{
			this.khAnim.Play(this.reset.name);
			this.follow = false;
		}
	}
}