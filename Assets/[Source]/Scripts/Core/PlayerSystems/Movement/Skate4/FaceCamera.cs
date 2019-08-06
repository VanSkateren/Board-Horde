using System;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
	public Transform camera;

	public FaceCamera()
	{
	}

	private void Update()
	{
		base.transform.rotation = Quaternion.LookRotation(-this.camera.forward);
	}
}