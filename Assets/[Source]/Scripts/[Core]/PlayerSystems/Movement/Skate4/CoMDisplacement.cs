using System;
using UnityEngine;

public class CoMDisplacement : MonoBehaviour
{
	[SerializeField]
	private AnimationCurve _displacementCurve;

	[SerializeField]
	private float _targetCoMHeight = 1.13956f;

	[SerializeField]
	private float _lowestCoMHeight = 0.7370192f;

	[SerializeField]
	private float _curveScalar = 1f;

	private float _lastValue;

	private float _currentValue;

	public float sum;

	public CoMDisplacement()
	{
	}

	public float GetDisplacement(float p_timeStep)
	{
		this._lastValue = this._currentValue;
		this._currentValue = this._displacementCurve.Evaluate(p_timeStep);
		this.sum = this.sum + (this._currentValue - this._lastValue) * this._curveScalar;
		return (this._currentValue - this._lastValue) * this._curveScalar;
	}

	public void ScaleDisplacementCurve(float p_skaterHeight)
	{
		p_skaterHeight = Mathf.Clamp(p_skaterHeight, this._lowestCoMHeight, this._targetCoMHeight);
		this._curveScalar = 1f - (p_skaterHeight - this._lowestCoMHeight) / (this._targetCoMHeight - this._lowestCoMHeight);
		this._lastValue = 0f;
		this._currentValue = 0f;
		this._curveScalar = Mathf.Clamp(this._curveScalar, 0f, 1f);
		this.sum = 0f;
	}
}