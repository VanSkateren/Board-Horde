using Dreamteck.Splines;
using FSMHelper;
using System;
using UnityEngine;

public class TriggerManager : MonoBehaviour
{
	public GrindDetection grindDetection;

	public GrindCollisions boardCollision;

	public GrindCollisions backTruckCollision;

	public GrindCollisions frontTruckCollision;

	public Transform grindContactSplinePosition;

	public Transform grindOffset;

	public SplineComputer spline;

	public SplineComputer currentSpline;

	public Transform playerOffset;

	public Vector3 grindContactPoint = Vector3.zero;

	public bool canOllie;

	public bool canNollie;

	public bool enteredFromRight;

	public bool swapped;

	public TriggerManager.SideEnteredGrind sideEnteredGrind = TriggerManager.SideEnteredGrind.Center;

	public bool grindContact;

	[SerializeField]
	private bool _isColliding;

	public bool wasColliding;

	[SerializeField]
	private GrindTrigger[] _grindTriggers = new GrindTrigger[5];

	public bool[] activeGrinds = new bool[5];

	public Vector3 grindDirection;

	public Vector3 grindUp;

	public Vector3 grindRight;

	public float corrrectiveForce = 10f;

	[SerializeField]
	private Transform _tailLimit;

	[SerializeField]
	private Transform _backTruckLeftLimit;

	[SerializeField]
	private Transform _backTruckRightLimit;

	[SerializeField]
	private Transform _frontTruckLeftLimit;

	[SerializeField]
	private Transform _frontTruckRightLimit;

	[SerializeField]
	private Transform _noseLimit;

	public bool[] _contactLimitIsRight = new bool[6];

	private float _stallTimer;

	private float _maxStallTime = 0.3f;

	[SerializeField]
	private bool _boardCollision;

	[SerializeField]
	private bool _frontTruckCollision;

	[SerializeField]
	private bool _backTruckCollision;

	private bool _collidingThisFrame;

	private float _grindAngle;

	public bool IsColliding
	{
		get
		{
			return this._isColliding;
		}
		set
		{
			this._isColliding = value;
		}
	}

	public TriggerManager()
	{
	}

	private void DetectCollisionPoint()
	{
		if (this.boardCollision.isColliding || this.backTruckCollision.isColliding || this.frontTruckCollision.isColliding)
		{
			this._grindAngle = Vector3.Angle(Vector3.ProjectOnPlane(PlayerController.Instance.boardController.boardTransform.forward, this.grindUp), this.grindDirection);
			if (this._grindAngle > 70f && this._grindAngle < 110f)
			{
				if (this.boardCollision.isColliding)
				{
					this.grindContactPoint = this.boardCollision.lastCollision;
					this.grindContact = true;
				}
				else if (this.frontTruckCollision.isColliding)
				{
					this.grindContactPoint = this.frontTruckCollision.lastCollision;
					this.grindContact = true;
				}
				else if (this.backTruckCollision.isColliding)
				{
					this.grindContactPoint = this.backTruckCollision.lastCollision;
					this.grindContact = true;
				}
			}
			else if (!this.backTruckCollision.isColliding && !this.frontTruckCollision.isColliding)
			{
				this.grindContactPoint = this.boardCollision.lastCollision;
				this.grindContact = true;
			}
			else if (this.backTruckCollision.isColliding && this.frontTruckCollision.isColliding)
			{
				this.grindContactPoint = (this.backTruckCollision.lastCollision + this.frontTruckCollision.lastCollision) / 2f;
				this.grindContact = true;
			}
			else if (this.frontTruckCollision.isColliding)
			{
				this.grindContactPoint = this.frontTruckCollision.lastCollision;
				this.grindContact = true;
			}
			else if (this.backTruckCollision.isColliding)
			{
				this.grindContactPoint = this.backTruckCollision.lastCollision;
				this.grindContact = true;
			}
		}
		else
		{
			this.grindContact = false;
		}
		if (!this.boardCollision.isColliding)
		{
			this._boardCollision = false;
		}
		else
		{
			this._boardCollision = true;
		}
		if (!this.frontTruckCollision.isColliding)
		{
			this._frontTruckCollision = false;
		}
		else
		{
			this._frontTruckCollision = true;
		}
		if (this.backTruckCollision.isColliding)
		{
			this._backTruckCollision = true;
			return;
		}
		this._backTruckCollision = false;
	}

	public void GrindTriggerCheck()
	{
		this._collidingThisFrame = false;
		for (int i = 0; i < 5; i++)
		{
			if (this._grindTriggers[i].Colliding)
			{
				this.grindDirection = this._grindTriggers[i].GrindDirection;
				this.grindUp = this._grindTriggers[i].GrindUp;
				this.grindRight = this._grindTriggers[i].GrindRight;
				this.wasColliding = false;
				this._collidingThisFrame = true;
				this.activeGrinds[i] = true;
			}
		}
		if (!this._collidingThisFrame)
		{
			for (int j = 0; j < 5; j++)
			{
				if (this._grindTriggers[j].WasColliding)
				{
					this.wasColliding = false;
					this._collidingThisFrame = true;
				}
			}
		}
		if (this.wasColliding && !this.IsColliding && !this._collidingThisFrame)
		{
			for (int k = 0; k < (int)this.activeGrinds.Length; k++)
			{
				this.activeGrinds[k] = false;
			}
			this.canOllie = false;
			this.canNollie = false;
			PlayerController.Instance.playerSM.OnGrindEndedSM();
			this.swapped = false;
			this.wasColliding = false;
		}
		if (!this.IsColliding && this._collidingThisFrame)
		{
			PlayerController.Instance.playerSM.OnGrindDetectedSM();
			PlayerController.Instance.playerSM.OnGrindStaySM();
			PlayerController.Instance.playerSM.SetSplineSM(this.spline);
		}
		if (this.IsColliding && this._collidingThisFrame)
		{
			PlayerController.Instance.playerSM.OnGrindStaySM();
			if (PlayerController.Instance.boardController.boardRigidbody.velocity.magnitude >= 0.2f)
			{
				this._stallTimer = 0f;
			}
			else
			{
				this._stallTimer += Time.deltaTime;
				if (this._stallTimer > this._maxStallTime)
				{
					PlayerController.Instance.ForceBail();
				}
			}
		}
		if (this.IsColliding && !this._collidingThisFrame)
		{
			this.wasColliding = true;
		}
		this.IsColliding = this._collidingThisFrame;
		this.DetectCollisionPoint();
		if (this.IsColliding)
		{
			this.grindDetection.DetectGrind(this.activeGrinds[0], this.activeGrinds[1], this.activeGrinds[2], this.activeGrinds[3], this.activeGrinds[4], this.grindUp, this.grindDirection, this.grindRight, ref this.canOllie, ref this.canNollie, this._backTruckCollision, this._frontTruckCollision, this._boardCollision);
		}
	}

	public enum SideEnteredGrind
	{
		Left,
		Right,
		Center
	}
}