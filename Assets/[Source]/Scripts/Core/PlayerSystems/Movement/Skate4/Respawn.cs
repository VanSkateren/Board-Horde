using FSMHelper;
using Rewired;
using RootMotion.Dynamics;
using RootMotion.FinalIK;
using System;
using UnityEngine;

public class Respawn : MonoBehaviour
{
	public Transform pin;

	[SerializeField]
	private FullBodyBipedIK _finalIk;

	public PuppetMaster puppetMaster;

	public BehaviourPuppet behaviourPuppet;

	public Bail bail;

	public bool respawning;

	private bool _backwards;

	private string _idleAnimation = "Riding";

	public Transform[] getSpawn = new Transform[8];

	private Vector3[] _setPos = new Vector3[8];

	private Quaternion[] _setRot = new Quaternion[8];

	private bool _canPress;

	private bool _init;

	private Vector3 _playerOffset = Vector3.zero;

	private bool _retryRespawn;

	private bool _dPadYCentered;

	private float _dPadResetTimer;

	public Respawn()
	{
	}

	private void DelayPress()
	{
		this._canPress = true;
	}

	public void DoRespawn()
	{
		if (this._canPress && !this.respawning)
		{
			PlayerController.Instance.IsRespawning = true;
			this.respawning = true;
			this._canPress = false;
			this.GetSpawnPos();
			PlayerController.Instance.CancelInvoke("DoBail");
			base.CancelInvoke("DelayPress");
			base.CancelInvoke("EndRespawning");
			base.Invoke("DelayPress", 0.4f);
			base.Invoke("EndRespawning", 0.25f);
		}
	}

	private void EndRespawning()
	{
		this.respawning = false;
		PlayerController.Instance.IsRespawning = false;
		this._retryRespawn = false;
	}

	private void GetSpawnPos()
	{
		PlayerController.Instance.respawn.behaviourPuppet.BoostImmunity(1000f);
		base.CancelInvoke("DoRespawn");
		PlayerController.Instance.CancelRespawnInvoke();
		this.puppetMaster.FixTargetToSampledState(1f);
		this.puppetMaster.FixMusclePositions();
		this.behaviourPuppet.StopAllCoroutines();
		this._finalIk.enabled = false;
		for (int i = 0; i < (int)this.getSpawn.Length; i++)
		{
			this.getSpawn[i].position = this._setPos[i];
			this.getSpawn[i].rotation = this._setRot[i];
		}
		this.bail.bailed = false;
		PlayerController.Instance.playerSM.OnRespawnSM();
		PlayerController.Instance.ResetIKOffsets();
		PlayerController.Instance.cameraController._leanForward = false;
		PlayerController.Instance.cameraController._pivot.rotation = PlayerController.Instance.cameraController._pivotCentered.rotation;
		PlayerController.Instance.comController.COMRigidbody.velocity = Vector3.zero;
		PlayerController.Instance.boardController.boardRigidbody.velocity = Vector3.zero;
		PlayerController.Instance.boardController.boardRigidbody.angularVelocity = Vector3.zero;
		PlayerController.Instance.boardController.frontTruckRigidbody.velocity = Vector3.zero;
		PlayerController.Instance.boardController.frontTruckRigidbody.angularVelocity = Vector3.zero;
		PlayerController.Instance.boardController.backTruckRigidbody.velocity = Vector3.zero;
		PlayerController.Instance.boardController.backTruckRigidbody.angularVelocity = Vector3.zero;
		PlayerController.Instance.skaterController.skaterRigidbody.velocity = Vector3.zero;
		PlayerController.Instance.skaterController.skaterRigidbody.angularVelocity = Vector3.zero;
		PlayerController.Instance.skaterController.skaterRigidbody.useGravity = false;
		PlayerController.Instance.boardController.IsBoardBackwards = this._backwards;
		PlayerController.Instance.SetBoardToMaster();
		PlayerController.Instance.SetTurningMode(InputController.TurningMode.Grounded);
		PlayerController.Instance.ResetAllAnimations();
		PlayerController.Instance.animationController.ForceAnimation("Riding");
		PlayerController.Instance.boardController.firstVel = 0f;
		PlayerController.Instance.boardController.secondVel = 0f;
		PlayerController.Instance.boardController.thirdVel = 0f;
		PlayerController.Instance.skaterController.ResetRotationLerps();
		PlayerController.Instance.SetLeftIKLerpTarget(0f);
		PlayerController.Instance.SetRightIKLerpTarget(0f);
		PlayerController.Instance.SetMaxSteeze(0f);
		PlayerController.Instance.AnimSetPush(false);
		PlayerController.Instance.AnimSetMongo(false);
		PlayerController.Instance.CrossFadeAnimation("Riding", 0.05f);
		PlayerController.Instance.cameraController.ResetAllCamera();
		this.puppetMaster.targetRoot.position = this._setPos[1] + this._playerOffset;
		this.puppetMaster.targetRoot.rotation = this._setRot[0];
		this.puppetMaster.angularLimits = false;
		this.puppetMaster.Resurrect();
		this.puppetMaster.state = PuppetMaster.State.Alive;
		this.puppetMaster.targetAnimator.Play(this._idleAnimation, 0, 0f);
		this.behaviourPuppet.SetState(BehaviourPuppet.State.Puppet);
		this.puppetMaster.Teleport(this._setPos[1] + this._playerOffset, this._setRot[0], true);
		PlayerController.Instance.SetIKOnOff(1f);
		PlayerController.Instance.skaterController.skaterRigidbody.useGravity = false;
		PlayerController.Instance.skaterController.skaterRigidbody.constraints = RigidbodyConstraints.None;
		this._finalIk.enabled = true;
		this._retryRespawn = false;
		this.puppetMaster.FixMusclePositions();
		PlayerController.Instance.respawn.behaviourPuppet.BoostImmunity(1000f);
	}

	private void SetSpawnPos()
	{
		this.pin.position = this.getSpawn[1].position + this._playerOffset;
		Quaternion quaternion = Quaternion.LookRotation(this.getSpawn[0].rotation * Vector3.forward, Vector3.up);
		this._backwards = PlayerController.Instance.GetBoardBackwards();
		for (int i = 0; i < (int)this._setPos.Length; i++)
		{
			if ((float)i == 0f)
			{
				this._setPos[i] = this.getSpawn[1].position + this._playerOffset;
				this._setRot[i] = quaternion;
			}
			else if (i == 5)
			{
				this._setPos[i] = this.getSpawn[i].position;
				this._setRot[i] = quaternion;
			}
			else if (i != 7)
			{
				this._setPos[i] = this.getSpawn[i].position;
				this._setRot[i] = this.getSpawn[i].rotation;
			}
			else
			{
				this._setPos[i] = this.getSpawn[1].position + this._playerOffset;
				this._setRot[i] = quaternion;
			}
		}
	}

	private void Start()
	{
		this._canPress = true;
		this._playerOffset = this.getSpawn[0].position - this.getSpawn[1].position;
	}

	public void Update()
	{
		if (!this._init && PlayerController.Instance.boardController.AllDown)
		{
			this.SetSpawnPos();
			this._init = true;
		}
		if (this._init && !this._dPadYCentered && this._canPress && !this.respawning && !this.puppetMaster.isBlending)
		{
			if (PlayerController.Instance.inputController.player.GetAxis("DPadY") < 0f && PlayerController.Instance.IsGrounded() && !this.bail.bailed && Time.timeScale != 0f)
			{
				this._dPadYCentered = true;
				this.SetSpawnPos();
				this._canPress = false;
				base.CancelInvoke("DelayPress");
				base.Invoke("DelayPress", 0.4f);
				this.pin.gameObject.SetActive(true);
			}
			if (PlayerController.Instance.inputController.player.GetAxis("DPadY") > 0f && Time.timeScale != 0f)
			{
				this._dPadYCentered = true;
				this.DoRespawn();
			}
		}
		if (PlayerController.Instance.inputController.player.GetAxis("DPadY") == 0f && this._dPadYCentered)
		{
			if (this._dPadResetTimer >= 0.2f)
			{
				this._dPadResetTimer = 0f;
				this._dPadYCentered = false;
			}
			else
			{
				this._dPadResetTimer += Time.deltaTime;
			}
		}
		if (this.respawning && !this._retryRespawn && this.behaviourPuppet.state == BehaviourPuppet.State.Unpinned)
		{
			this._canPress = true;
			this.respawning = false;
			this._retryRespawn = true;
			this.DoRespawn();
		}
	}
}