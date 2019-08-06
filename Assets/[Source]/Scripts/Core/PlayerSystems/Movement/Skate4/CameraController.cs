using FSMHelper;
using Rewired;
using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraController : MonoBehaviour
{
	[SerializeField]
	private Transform _targetPos;

	[SerializeField]
	private Transform _camTransform;

	public Transform _pivot;

	public Transform _pivotCentered;

	public Transform _pivotForward;

	[SerializeField]
	private Rigidbody _camRigidbody;

	[SerializeField]
	private Rigidbody _bailTarget;

	public Transform _actualCam;

	[SerializeField]
	private Transform _leftTopPos;

	[SerializeField]
	private Transform _rightTopPos;

	private Vector3 _forwardTarget;

	private Vector3 _projectedVelocity;

	public LayerMask layerMask;

	private RaycastHit _hit;

	public PostProcessLayer postProcessLayer;

	private int _qualitySettings = 5;

	private float _pushLerp;

	private bool _right;

	private float _initialYPos;

	private float _targetY;

	private float _currentY;

	private float _skaterClampedY;

	private float _groundUnderCam;

	private float _lowestY;

	public bool _leanForward;

	private Vector3 _camVel;

	public CameraController()
	{
	}

	private void Awake()
	{
		this._qualitySettings = QualitySettings.GetQualityLevel();
		int num = this._qualitySettings;
		if (num <= 2)
		{
			this.postProcessLayer.enabled = false;
		}
		else if (num - 3 > 2)
		{
		}
		if (SettingsManager.Instance.stance != SettingsManager.Stance.Regular)
		{
			this._actualCam.position = this._leftTopPos.position;
			this._actualCam.rotation = this._leftTopPos.rotation;
			this._right = false;
		}
		else
		{
			this._actualCam.position = this._rightTopPos.position;
			this._actualCam.rotation = this._rightTopPos.rotation;
			this._right = true;
		}
		this._initialYPos = this._camTransform.position.y;
		this._targetY = this._initialYPos;
		this._currentY = this._targetY;
		this._lowestY = this._currentY;
	}

	public void LookAtPlayer()
	{
		this._camVel = this._camRigidbody.velocity;
		this._camVel.x *= 0.95f;
		this._camVel.y *= 0.5f;
		this._camVel.z *= 0.95f;
		this._camRigidbody.velocity = this._camVel;
		Rigidbody rigidbody = this._camRigidbody;
		rigidbody.angularVelocity = rigidbody.angularVelocity * 0.95f;
	}

	public void MoveCameraToPlayer()
	{
		if (PlayerController.Instance.boardController.boardRigidbody.velocity.magnitude <= 1f)
		{
			this._leanForward = false;
		}
		else if (Physics.Raycast((this._actualCam.position + (Vector3.up * 3f)) + (Vector3.ProjectOnPlane(PlayerController.Instance.skaterController.skaterRigidbody.velocity.normalized, Vector3.up) * 10f), -Vector3.up, out this._hit, 60f, this.layerMask))
		{
			if (this._hit.point.y >= PlayerController.Instance.boardController.GroundY - 0.8f)
			{
				this._leanForward = false;
			}
			else
			{
				this._leanForward = true;
			}
		}
		if (!this._leanForward)
		{
			this._pivot.rotation = Quaternion.Slerp(this._pivot.rotation, this._pivotCentered.rotation, Time.deltaTime * 2f);
		}
		else
		{
			this._pivot.rotation = Quaternion.Slerp(this._pivot.rotation, this._pivotForward.rotation, Time.deltaTime * 2f);
		}
		if (Physics.Raycast(this._actualCam.position, -Vector3.up, out this._hit, 10f, this.layerMask))
		{
			this._groundUnderCam = this._hit.point.y;
		}
		if (this._groundUnderCam > PlayerController.Instance.boardController.GroundY)
		{
			this._lowestY = PlayerController.Instance.boardController.GroundY + this._initialYPos;
		}
		else
		{
			this._lowestY = this._groundUnderCam + this._initialYPos;
		}
		if (!PlayerController.Instance.IsCurrentAnimationPlaying("Push Button"))
		{
			this._pushLerp = Mathf.Lerp(this._pushLerp, 0f, Time.deltaTime * 5f);
		}
		else
		{
			this._pushLerp = Mathf.Lerp(this._pushLerp, 1f, Time.deltaTime * 5f);
		}
		this._camRigidbody.velocity = Vector3.ProjectOnPlane(PlayerController.Instance.skaterController.skaterRigidbody.velocity, Vector3.up);
		Vector3 vector3 = Vector3.Lerp(PlayerController.Instance.skaterController.skaterTransform.position, this._targetPos.position, this._pushLerp);
		Vector3 vector31 = new Vector3(PlayerController.Instance.skaterController.skaterTransform.InverseTransformDirection(PlayerController.Instance.skaterController.skaterTransform.position).x, PlayerController.Instance.skaterController.skaterTransform.InverseTransformDirection(PlayerController.Instance.skaterController.skaterTransform.position).y, PlayerController.Instance.skaterController.skaterTransform.InverseTransformDirection(vector3).z);
		vector31 = PlayerController.Instance.skaterController.skaterTransform.TransformDirection(vector31);
		this._targetY = Mathf.Lerp(PlayerController.Instance.boardController.GroundY + this._initialYPos, PlayerController.Instance.skaterController.skaterTransform.position.y, 0.42f);
		if (!PlayerController.Instance.playerSM.IsGrindingSM() && PlayerController.Instance.skaterController.skaterRigidbody.velocity.y < 0f && !PlayerController.Instance.IsGrounded() && this._targetY > PlayerController.Instance.skaterController.skaterTransform.position.y - 0.2f)
		{
			this._targetY = PlayerController.Instance.skaterController.skaterTransform.position.y - 0.2f;
		}
		this._targetY = Mathf.Clamp(this._targetY, this._lowestY, this._targetY);
		this._currentY = Mathf.Lerp(this._currentY, this._targetY, Time.fixedDeltaTime * 10f);
		vector31.y = this._currentY;
		this._camTransform.position = Vector3.MoveTowards(this._camTransform.position, vector31, Time.fixedDeltaTime * 10f);
		this._projectedVelocity = Vector3.ProjectOnPlane(PlayerController.Instance.skaterController.skaterRigidbody.velocity, Vector3.up);
		if (this._projectedVelocity.magnitude > 0.3f)
		{
			this._forwardTarget = PlayerController.Instance.boardController.boardTransform.position + (this._projectedVelocity * 10f);
			if (PlayerController.Instance.IsGrounded())
			{
				Quaternion rotation = Quaternion.FromToRotation(this._camTransform.forward, this._projectedVelocity);
				rotation *= this._camTransform.rotation;
				this._camTransform.rotation = Quaternion.Slerp(this._camTransform.rotation, rotation, Time.fixedDeltaTime * 10f);
			}
			Quaternion quaternion = Quaternion.FromToRotation(this._camTransform.up, Vector3.up);
			quaternion *= this._camTransform.rotation;
			this._camTransform.rotation = Quaternion.Slerp(this._camTransform.rotation, quaternion, Time.fixedDeltaTime * 10f);
		}
		if (PlayerController.Instance.inputController.player.GetAxis("DPadX") < 0f)
		{
			this._right = false;
		}
		else if (PlayerController.Instance.inputController.player.GetAxis("DPadX") > 0f)
		{
			this._right = true;
		}
		if (this._right)
		{
			this._actualCam.position = Vector3.Lerp(this._actualCam.position, this._rightTopPos.position, Time.fixedDeltaTime * 4f);
			this._actualCam.rotation = Quaternion.Slerp(this._actualCam.rotation, this._rightTopPos.rotation, Time.fixedDeltaTime * 4f);
			return;
		}
		this._actualCam.position = Vector3.Lerp(this._actualCam.position, this._leftTopPos.position, Time.fixedDeltaTime * 4f);
		this._actualCam.rotation = Quaternion.Slerp(this._actualCam.rotation, this._leftTopPos.rotation, Time.fixedDeltaTime * 4f);
	}

	public void ResetAllCamera()
	{
		this._targetY = PlayerController.Instance.boardController.boardTransform.position.y + this._initialYPos;
		this._currentY = this._targetY;
	}
}