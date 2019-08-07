using System;
using UnityEngine;

public class ShowcaseMovement : MonoBehaviour
{
	public ShowcaseMovement()
	{
	}

	private void Update()
	{
		Transform axis = base.transform;
		axis.position = axis.position + ((Vector3.forward * Input.GetAxis("Horizontal")) * Time.deltaTime);
	}
}