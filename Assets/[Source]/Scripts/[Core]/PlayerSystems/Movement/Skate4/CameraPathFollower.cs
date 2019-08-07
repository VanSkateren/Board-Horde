using System;
using UnityEngine;

public class CameraPathFollower : MonoBehaviour
{
	public Transform cam;

	public Transform pointA;

	public Transform pointB;

	public float speed = 2f;

	public float lerp;

	public bool began;

	public CameraPathFollower()
	{
	}

	private void Update()
	{
		if (this.began)
		{
			this.lerp = this.lerp + Time.deltaTime * this.speed;
			this.lerp = Mathf.Clamp(this.lerp, 0f, 1f);
			this.cam.position = Vector3.Lerp(this.pointA.position, this.pointB.position, this.lerp);
			this.cam.rotation = Quaternion.Slerp(this.pointA.rotation, this.pointB.rotation, this.lerp);
		}
		else if (Input.GetKeyDown(KeyCode.Space))
		{
			this.began = true;
			return;
		}
	}
}