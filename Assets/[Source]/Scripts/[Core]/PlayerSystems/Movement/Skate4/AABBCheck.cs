using System;
using UnityEngine;

public class AABBCheck : MonoBehaviour
{
	private Rigidbody _board;

	private Rigidbody _backTruck;

	private Rigidbody _frontTruck;

	private Vector3 _lastBoardPos = Vector3.zero;

	private Vector3 _lastBoardLocalPos = Vector3.zero;

	private Vector3 _lastBoardVelocity = Vector3.zero;

	private Vector3 _lastBoardAngularVelocity = Vector3.zero;

	private Vector3 _lastBackTruckPos = Vector3.zero;

	private Vector3 _lastBackTruckLocalPos = Vector3.zero;

	private Vector3 _lastBackTruckVelocity = Vector3.zero;

	private Vector3 _lastBackTruckAngularVelocity = Vector3.zero;

	private Vector3 _lastFrontTruckPos = Vector3.zero;

	private Vector3 _lastFrontTruckLocalPos = Vector3.zero;

	private Vector3 _lastFrontTruckVelocity = Vector3.zero;

	private Vector3 _lastFrontTruckAngularVelocity = Vector3.zero;

	public AABBCheck()
	{
	}

	private void FixedUpdate()
	{
	}

	private void LateUpdate()
	{
	}

	private void NaNCheck()
	{
		if (!Mathd.Vector3IsInfinityOrNan(this._board.position))
		{
			this._lastBoardPos = this._board.position;
		}
		else
		{
			Debug.LogError("Board Position is NaN");
			this.RestorLastValues();
		}
		if (!Mathd.Vector3IsInfinityOrNan(this._board.transform.localPosition))
		{
			this._lastBoardLocalPos = this._board.transform.localPosition;
		}
		else
		{
			Debug.LogError("Board Local Position is NaN");
			this.RestorLastValues();
		}
		if (!Mathd.Vector3IsInfinityOrNan(this._board.velocity))
		{
			this._lastBoardVelocity = this._board.velocity;
		}
		else
		{
			Debug.LogError("Board Velocity is NaN");
			this.RestorLastValues();
		}
		if (!Mathd.Vector3IsInfinityOrNan(this._board.angularVelocity))
		{
			this._lastBoardAngularVelocity = this._board.angularVelocity;
		}
		else
		{
			Debug.LogError("Board Angular Velocity is NaN");
			this.RestorLastValues();
		}
		if (!Mathd.Vector3IsInfinityOrNan(this._backTruck.position))
		{
			this._lastBackTruckPos = this._backTruck.position;
		}
		else
		{
			Debug.LogError("Back Truck Position is NaN");
			this.RestorLastValues();
		}
		if (!Mathd.Vector3IsInfinityOrNan(this._backTruck.transform.localPosition))
		{
			this._lastBackTruckLocalPos = this._backTruck.transform.localPosition;
		}
		else
		{
			Debug.LogError("Back Truck Local Position is NaN");
			this.RestorLastValues();
		}
		if (!Mathd.Vector3IsInfinityOrNan(this._backTruck.velocity))
		{
			this._lastBackTruckVelocity = this._backTruck.velocity;
		}
		else
		{
			Debug.LogError("Back Truck Velocity is NaN");
			this.RestorLastValues();
		}
		if (!Mathd.Vector3IsInfinityOrNan(this._backTruck.angularVelocity))
		{
			this._lastBackTruckAngularVelocity = this._backTruck.angularVelocity;
		}
		else
		{
			Debug.LogError("Back Truck Angular Velocity is NaN");
			this.RestorLastValues();
		}
		if (!Mathd.Vector3IsInfinityOrNan(this._frontTruck.position))
		{
			this._lastFrontTruckPos = this._frontTruck.position;
		}
		else
		{
			Debug.LogError("Front Truck Position is NaN");
			this.RestorLastValues();
		}
		if (!Mathd.Vector3IsInfinityOrNan(this._frontTruck.transform.localPosition))
		{
			this._lastFrontTruckLocalPos = this._frontTruck.transform.localPosition;
		}
		else
		{
			Debug.LogError("Front Truck Local Position is NaN");
			this.RestorLastValues();
		}
		if (!Mathd.Vector3IsInfinityOrNan(this._frontTruck.velocity))
		{
			this._lastFrontTruckVelocity = this._frontTruck.velocity;
		}
		else
		{
			Debug.LogError("Front Truck Velocity is NaN");
			this.RestorLastValues();
		}
		if (Mathd.Vector3IsInfinityOrNan(this._frontTruck.angularVelocity))
		{
			Debug.LogError("Front Truck Angular Velocity is NaN");
			this.RestorLastValues();
			return;
		}
		this._lastFrontTruckAngularVelocity = this._frontTruck.angularVelocity;
	}

	private void RestorLastValues()
	{
		Debug.LogError("Restored Values");
		this._board.position = this._lastBoardPos;
		this._board.transform.localPosition = this._lastBoardLocalPos;
		this._board.velocity = this._lastBoardVelocity;
		this._board.angularVelocity = this._lastBoardAngularVelocity;
		this._backTruck.position = this._lastBackTruckPos;
		this._backTruck.transform.localPosition = this._lastBackTruckLocalPos;
		this._backTruck.velocity = this._lastBackTruckVelocity;
		this._backTruck.angularVelocity = this._lastBackTruckAngularVelocity;
		this._frontTruck.position = this._lastFrontTruckPos;
		this._frontTruck.transform.localPosition = this._lastFrontTruckLocalPos;
		this._frontTruck.velocity = this._lastFrontTruckVelocity;
		this._frontTruck.angularVelocity = this._lastFrontTruckAngularVelocity;
	}

	private void Start()
	{
		this._board = PlayerController.Instance.boardController.boardRigidbody;
		this._backTruck = PlayerController.Instance.boardController.backTruckRigidbody;
		this._frontTruck = PlayerController.Instance.boardController.frontTruckRigidbody;
	}

	private void Update()
	{
	}
}