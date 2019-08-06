using Dreamteck.Splines;
using FSMHelper;
using System;
using UnityEngine;

public class GrindTrigger : TriggerManager
{
	public TriggerManager triggerManager;

	public SplineComputer localSpline;

	[SerializeField]
	public GrindTrigger.TriggerType triggerType;

	private bool _colliding;

	private bool _wasJustColliding;

	[SerializeField]
	private Vector3 _grindDirection;

	[SerializeField]
	private Vector3 _grindUp;

	[SerializeField]
	private Vector3 _grindRightVector;

	public Vector3 closestPoint = Vector3.zero;

	private Quaternion _newUp = Quaternion.identity;

	private float _maxVelocity;

	private float _timer;

	private bool _assist;

	private double percent;

	private bool _forward;

	private float _velocityMagnitude;

	private bool _ignore;

	private SplineResult _splineResult;

	private float _steepness;

	public bool Colliding
	{
		get
		{
			return this._colliding;
		}
	}

	public Vector3 GrindDirection
	{
		get
		{
			return this._grindDirection;
		}
	}

	public Vector3 GrindRight
	{
		get
		{
			return this._grindRightVector;
		}
	}

	public Vector3 GrindUp
	{
		get
		{
			return this._grindUp;
		}
	}

	public bool WasColliding
	{
		get
		{
			return this._wasJustColliding;
		}
	}

	public GrindTrigger()
	{
	}

	private void CorrectVelocity(Rigidbody p_rb, float p_magnitude, bool p_addGravity)
	{
		p_magnitude = Mathf.Clamp(p_magnitude, 0f, this._velocityMagnitude + this._steepness);
		if (!Mathd.Vector3IsInfinityOrNan(this._grindDirection) && !Mathd.IsInfinityOrNaN(p_magnitude))
		{
			if (p_addGravity)
			{
				p_rb.velocity = (this._grindDirection * p_magnitude) + (Physics.gravity * Time.fixedDeltaTime);
				return;
			}
			p_rb.velocity = this._grindDirection * p_magnitude;
		}
	}

	private void FixedUpdate()
	{
		this._wasJustColliding = this._colliding;
	}

	private void OnTriggerEnter(Collider p_other)
	{
		if (p_other.gameObject.layer == LayerMask.NameToLayer("Grindable") && (PlayerController.Instance.playerSM.CanGrindSM() || PlayerController.Instance.GetPopped() && !PlayerController.Instance.playerSM.IsGrindingSM()))
		{
			PlayerController.Instance.boardController.grindTag = p_other.gameObject.tag;
			if (!this.triggerManager.IsColliding)
			{
				this.triggerManager.spline = p_other.GetComponent<SplineComputer>();
				if (this.triggerManager.spline == null)
				{
					this.triggerManager.spline = p_other.gameObject.GetComponentInParent<SplineComputer>();
				}
			}
			if (!this._colliding)
			{
				this._assist = true;
				this._timer = 0f;
			}
			this._ignore = false;
			this._colliding = true;
			if (this.triggerManager.spline != null)
			{
				this.percent = this.triggerManager.spline.Project((this.grindContact ? this.grindContactPoint : base.transform.position), 3, 0, 1);
				this._splineResult = this.triggerManager.spline.Evaluate(this.percent);
				float single = Vector3.Angle(Vector3.ProjectOnPlane(PlayerController.Instance.boardController.boardRigidbody.velocity, this._splineResult.normal), this._splineResult.direction);
				if (single < 89f)
				{
					this._forward = true;
					this._grindDirection = this._splineResult.direction.normalized;
				}
				else if (single > 91f)
				{
					this._forward = false;
					this._grindDirection = -this._splineResult.direction.normalized;
				}
				if (!this.VelocityCheck(this._grindDirection))
				{
					PlayerController.Instance.playerSM.OnGrindEndedSM();
					this._colliding = false;
					this._ignore = true;
				}
				if (!this._ignore)
				{
					if (PlayerController.Instance.boardController.triggerManager.IsColliding)
					{
						this._grindUp = Vector3.Lerp(this._grindUp, this._splineResult.normal, Time.fixedDeltaTime * 4f);
						if (!this._forward)
						{
							this._grindRightVector = Vector3.Lerp(this._grindRightVector, -this._splineResult.right, Time.fixedDeltaTime * 20f);
						}
						else
						{
							this._grindRightVector = Vector3.Lerp(this._grindRightVector, this._splineResult.right, Time.fixedDeltaTime * 20f);
						}
					}
					else
					{
						this._grindUp = this._splineResult.normal;
						if (!this._forward)
						{
							this._grindRightVector = -this._splineResult.right;
						}
						else
						{
							this._grindRightVector = this._splineResult.right;
						}
					}
					PlayerController.Instance.boardController.GroundY = this._splineResult.position.y - 0.3f;
					Vector3 vector3 = Vector3.Project(PlayerController.Instance.boardController.boardRigidbody.velocity, this._grindDirection);
					this._velocityMagnitude = vector3.magnitude;
					this.CorrectVelocity(PlayerController.Instance.boardController.boardRigidbody, this._velocityMagnitude, true);
					this.CorrectVelocity(PlayerController.Instance.boardController.backTruckRigidbody, this._velocityMagnitude, true);
					this.CorrectVelocity(PlayerController.Instance.boardController.frontTruckRigidbody, this._velocityMagnitude, true);
					if (!Mathd.Vector3IsInfinityOrNan(this._splineResult.position))
					{
						this.triggerManager.grindContactSplinePosition.position = this._splineResult.position;
					}
					if (PlayerController.Instance.boardController.triggerManager.IsColliding)
					{
						this.triggerManager.grindContactSplinePosition.rotation = Quaternion.Slerp(this.triggerManager.grindContactSplinePosition.rotation, Quaternion.LookRotation(this._grindDirection, this._grindUp), Time.fixedDeltaTime * 10f);
					}
					else
					{
						this.triggerManager.grindContactSplinePosition.rotation = Quaternion.LookRotation(this._grindDirection, this._grindUp);
					}
					this.triggerManager.playerOffset.position = PlayerController.Instance.skaterController.skaterTransform.position;
					this.triggerManager.playerOffset.rotation = PlayerController.Instance.skaterController.skaterTransform.rotation;
					PlayerController.Instance.boardOffsetRoot.rotation = (PlayerController.Instance.GetBoardBackwards() ? PlayerController.Instance.skaterController.physicsBoardBackwardsTransform.rotation : PlayerController.Instance.skaterController.physicsBoardTransform.rotation);
					PlayerController.Instance.PlayerGrindRotation = PlayerController.Instance.playerRotationReference.rotation;
				}
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("Grindable"))
		{
			this._colliding = false;
		}
	}

	private void OnTriggerStay(Collider p_other)
	{
		Vector3 instance;
		if (p_other.gameObject.layer == LayerMask.NameToLayer("Grindable") && PlayerController.Instance.playerSM.CanGrindSM())
		{
			if (this._assist)
			{
				this._timer += Time.deltaTime;
				if (this._timer > 0.1f)
				{
					this._assist = false;
				}
			}
			this._colliding = true;
			this._ignore = false;
			if (this.triggerManager.spline != null)
			{
				this.percent = this.triggerManager.spline.Project((this.grindContact ? this.grindContactPoint : base.transform.position), 3, 0, 1);
				this._splineResult = this.triggerManager.spline.Evaluate(this.percent);
				if (!this._forward)
				{
					this._grindDirection = -this._splineResult.direction.normalized;
				}
				else
				{
					this._grindDirection = this._splineResult.direction.normalized;
				}
				if (!this.VelocityCheck(this._grindDirection))
				{
					PlayerController.Instance.playerSM.OnGrindEndedSM();
					this._colliding = false;
					this._ignore = true;
				}
				if (!this._ignore)
				{
					if (Vector3.Angle(this._grindDirection, Vector3.up) <= 94f)
					{
						this._steepness = 0f;
					}
					else
					{
						this._steepness = 5f;
					}
					if (!this._forward)
					{
						this._grindRightVector = Vector3.Lerp(this._grindRightVector, -this._splineResult.right, Time.fixedDeltaTime * 20f);
					}
					else
					{
						this._grindRightVector = Vector3.Lerp(this._grindRightVector, this._splineResult.right, Time.fixedDeltaTime * 20f);
					}
					this._grindUp = Vector3.Lerp(this._grindUp, this._splineResult.normal, Time.fixedDeltaTime * 4f);
					if (!Mathd.Vector3IsInfinityOrNan(this._splineResult.position))
					{
						this.triggerManager.grindContactSplinePosition.position = this._splineResult.position;
					}
					this.triggerManager.grindContactSplinePosition.rotation = Quaternion.Slerp(this.triggerManager.grindContactSplinePosition.rotation, Quaternion.LookRotation(this._grindDirection, this._grindUp), Time.fixedDeltaTime * 10f);
					PlayerController.Instance.boardController.GroundY = this._splineResult.position.y - 0.3f;
					this._newUp = Quaternion.FromToRotation(PlayerController.Instance.playerRotationReference.up, this._grindUp);
					this._newUp *= PlayerController.Instance.playerRotationReference.rotation;
					PlayerController.Instance.playerRotationReference.rotation = Quaternion.Slerp(PlayerController.Instance.playerRotationReference.rotation, this._newUp, Time.fixedDeltaTime * 10f);
					PlayerController.Instance.PlayerGrindRotation = PlayerController.Instance.playerRotationReference.rotation;
					if (!PlayerController.Instance.GetPopped())
					{
						if (!this._assist)
						{
							Rigidbody rigidbody = PlayerController.Instance.boardController.boardRigidbody;
							instance = PlayerController.Instance.boardController.boardRigidbody.velocity;
							this.CorrectVelocity(rigidbody, instance.magnitude, true);
							return;
						}
						Rigidbody instance1 = PlayerController.Instance.boardController.boardRigidbody;
						instance = PlayerController.Instance.boardController.boardRigidbody.velocity;
						this.CorrectVelocity(instance1, instance.magnitude, true);
						Rigidbody rigidbody1 = PlayerController.Instance.boardController.backTruckRigidbody;
						instance = PlayerController.Instance.boardController.backTruckRigidbody.velocity;
						this.CorrectVelocity(rigidbody1, instance.magnitude, true);
						Rigidbody instance2 = PlayerController.Instance.boardController.frontTruckRigidbody;
						instance = PlayerController.Instance.boardController.frontTruckRigidbody.velocity;
						this.CorrectVelocity(instance2, instance.magnitude, true);
						return;
					}
				}
			}
		}
		else if (p_other.gameObject.layer == LayerMask.NameToLayer("Grindable") && !PlayerController.Instance.IsGrounded() && PlayerController.Instance.boardController.boardRigidbody.velocity.magnitude < 0.2f && !PlayerController.Instance.playerSM.IsGrindingSM())
		{
			bool flag = PlayerController.Instance.respawn.bail.bailed;
		}
	}

	private bool VelocityCheck(Vector3 _grindDirection)
	{
		Vector3 vector3 = Vector3.ProjectOnPlane(_grindDirection, Vector3.up);
		if (Vector3.Angle(Vector3.ProjectOnPlane(PlayerController.Instance.VelocityOnPop, Vector3.up), vector3) < 60f)
		{
			return true;
		}
		return false;
	}

	public enum TriggerType
	{
		Board,
		Nose,
		Tail,
		BackTruck,
		FrontTruck
	}
}