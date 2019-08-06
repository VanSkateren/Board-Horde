using FSMHelper;
using RootMotion.Dynamics;
using System;
using UnityEngine;

public class Bail : MonoBehaviour
{
	[SerializeField]
	private Rigidbody _skaterRigidbody;

	[SerializeField]
	private CapsuleCollider _capsuleCollider;

	[SerializeField]
	private PuppetMaster _puppetMaster;

	public bool bailed;

	public Bail()
	{
	}

	private void CorrectRotation()
	{
		this._puppetMaster.targetRoot.rotation = Quaternion.FromToRotation(this._puppetMaster.targetRoot.up, Vector3.up) * this._puppetMaster.targetRoot.rotation;
	}

	private void EnableCapsuleCollider()
	{
		this._skaterRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
		this._capsuleCollider.enabled = true;
		this._skaterRigidbody.isKinematic = false;
		this._skaterRigidbody.useGravity = true;
	}

	public void OnBailed()
	{
		this.bailed = true;
		this._puppetMaster.angularLimits = true;
		PlayerController.Instance.animationController.skaterAnim.CrossFade("Fall", 0.3f);
		PlayerController.Instance.playerSM.OnBailedSM();
		PlayerController.Instance.SetIKOnOff(0f);
	}

	public void RegainBalance()
	{
		this.CorrectRotation();
	}
}