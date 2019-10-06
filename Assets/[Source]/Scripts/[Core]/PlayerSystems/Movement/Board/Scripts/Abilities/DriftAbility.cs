using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriftAbility : BaseAbility
{
    [SerializeField] private AnimationCurve driftCurve;
    
    [SerializeField] private float 
        driftInDuration = 0.5f,
        driftOutDuration = 1f;

    private float _currentDriftFactor;
    private bool _isDrifting;
    
    private float _currentDriftTime;
    
    
    private CpMain _cpMain;
    private CpLateralFriction _lateralFriction;

    private void Awake()
    {
        _cpMain = GetComponent<CpMain>();
        _lateralFriction = GetComponentInChildren<CpLateralFriction>();
    }

    private void Update()
    {
        CheckInput();
        UpdateAbility();
    }
    
    public override void CheckInput()
    {
        if (_cpMain.wheelData.grounded)
        {
            _isDrifting = Input.GetKey(abilityButton);
        }
    }

    private void UpdateAbility()
    {
        if (_isDrifting)
        {
            _currentDriftTime += Time.deltaTime * 1 / driftInDuration;
        }
        else if (_currentDriftTime > 0)
        {
            _currentDriftTime -= Time.deltaTime * 1 / driftOutDuration;
        }

        _currentDriftTime = Mathf.Clamp01(_currentDriftTime);
        _currentDriftFactor = driftCurve.Evaluate(_currentDriftTime);
        
        _lateralFriction.currentTireStickiness =_lateralFriction.baseTireStickiness * _currentDriftFactor;
    }

    public override void DoAbility()
    {
        bool __belowBaseTireStickiness = (_lateralFriction.currentTireStickiness < _lateralFriction.baseTireStickiness);
    
        if (__belowBaseTireStickiness && 
            !_isDrifting && 
            _cpMain.wheelData.grounded
            )
        {
            //This is to try recover some lost speed while drifting
            _cpMain.rb.AddForce(Mathf.Abs(_lateralFriction.slidingFrictionForceAmount) * _cpMain.rb.transform.forward, ForceMode.Acceleration);
        }
    }
}
