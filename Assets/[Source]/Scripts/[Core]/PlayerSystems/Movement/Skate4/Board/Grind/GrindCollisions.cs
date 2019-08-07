using System;
using UnityEngine;

public class GrindCollisions : MonoBehaviour
{
	public Vector3 lastCollision = Vector3.zero;

	public bool isColliding;

	public GrindCollisions()
	{
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Grindable"))
		{
			this.lastCollision = collision.contacts[0].point;
			this.isColliding = true;
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Grindable"))
		{
			this.isColliding = false;
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Grindable"))
		{
			this.lastCollision = collision.contacts[0].point;
			this.isColliding = true;
		}
	}
}