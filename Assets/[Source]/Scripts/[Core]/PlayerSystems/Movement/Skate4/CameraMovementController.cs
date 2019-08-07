using Rewired;
using System;
using UnityEngine;

public class CameraMovementController : MonoBehaviour
{
	public Camera inGameCam;

	public Camera photoCam;

	public float movementSpeed = 1.5f;

	public float rotateSpeed = 3f;

	private float lift;

	private float drop;

	private float horizontal;

	private float vertical;

	private float stickX;

	private float stickY;

	private float delayTimer;

	private bool _cameraMode;

	private bool slowmo;

	private bool movementDelay;

	private bool freezeCamera;

	public CameraMovementController()
	{
	}

	private void ActivateCameraMode()
	{
		if (PlayerController.Instance.inputController.player.GetButtonDown("Y") && !this._cameraMode)
		{
			if (this.slowmo)
			{
				Time.timeScale = 1f;
				this.slowmo = false;
			}
			else
			{
				Time.timeScale = 0.05f;
				this.slowmo = true;
			}
		}
		if (PlayerController.Instance.inputController.player.GetButtonDown("X"))
		{
			if (this._cameraMode)
			{
				this.freezeCamera = false;
				this.photoCam.enabled = false;
				this._cameraMode = false;
				this.slowmo = false;
				Time.timeScale = 1f;
			}
			else
			{
				this.movementDelay = true;
				this.photoCam.transform.position = this.inGameCam.transform.position;
				this.photoCam.transform.rotation = this.inGameCam.transform.rotation;
				this.photoCam.enabled = true;
				this._cameraMode = true;
				this.slowmo = false;
				Time.timeScale = 1E-06f;
			}
		}
		if (this._cameraMode && PlayerController.Instance.inputController.player.GetButtonDown("RB"))
		{
			if (!this.freezeCamera)
			{
				this.slowmo = false;
				Time.timeScale = 1f;
				this.freezeCamera = true;
				return;
			}
			this.movementDelay = true;
			this._cameraMode = true;
			this.slowmo = false;
			this.freezeCamera = false;
			Time.timeScale = 1E-06f;
		}
	}

	private void EndMovementDelay()
	{
		this.movementDelay = false;
		this.delayTimer = 0f;
	}

	private void MoveCamera()
	{
		this.drop = PlayerController.Instance.inputController.player.GetAxis("LT") * Time.deltaTime * (1f / Time.timeScale);
		this.lift = PlayerController.Instance.inputController.player.GetAxis("RT") * Time.deltaTime * (1f / Time.timeScale);
		this.horizontal = PlayerController.Instance.inputController.player.GetAxis("LeftStickX") * Time.deltaTime * (1f / Time.timeScale);
		this.vertical = PlayerController.Instance.inputController.player.GetAxis("LeftStickY") * Time.deltaTime * (1f / Time.timeScale);
		this.drop *= this.movementSpeed;
		this.lift *= this.movementSpeed;
		this.horizontal *= this.movementSpeed;
		this.vertical *= this.movementSpeed;
		Vector3 vector3 = ((this.photoCam.transform.right * this.horizontal) + (this.photoCam.transform.forward * this.vertical)) + (Vector3.up * (this.lift - this.drop));
		Transform transforms = this.photoCam.transform;
		transforms.position = transforms.position + vector3;
		this.stickX = PlayerController.Instance.inputController.player.GetAxis("RightStickX") * Time.deltaTime * (1f / Time.timeScale);
		this.stickY = PlayerController.Instance.inputController.player.GetAxis("RightStickY") * Time.deltaTime * (1f / Time.timeScale);
		this.photoCam.transform.rotation = Quaternion.AngleAxis(this.stickY * this.rotateSpeed, Vector3.ProjectOnPlane(-this.photoCam.transform.right, Vector3.up)) * this.photoCam.transform.rotation;
		this.photoCam.transform.rotation = Quaternion.AngleAxis(this.stickX * this.rotateSpeed, Vector3.up) * this.photoCam.transform.rotation;
	}

	private void MovePhotoCamToInGameCam()
	{
		this.photoCam.transform.position = this.inGameCam.transform.position;
		this.photoCam.transform.rotation = this.inGameCam.transform.rotation;
	}

	private void Update()
	{
		this.ActivateCameraMode();
		if (!this._cameraMode)
		{
			this.MovePhotoCamToInGameCam();
			return;
		}
		if (this.movementDelay)
		{
			this.delayTimer = this.delayTimer + Time.deltaTime * (1f / Time.timeScale);
			if (this.delayTimer > 0.3f)
			{
				this.EndMovementDelay();
			}
		}
		else if (!this.freezeCamera)
		{
			this.MoveCamera();
			return;
		}
	}
}