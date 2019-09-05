using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostAbility : BaseAbility
{
    #region Variables
    
    [SerializeField] private  float boostForce = 20;
    //public AnimationCurve BoostCurve;
    [SerializeField] private  float boostTimeMax = 3;
    [SerializeField] private float boostRechargeRate = 0.5f;
    
    private float currentBoost = 0;
    private float currentBoostTimeLeft = 0;

    private CpMain _cpMain;
    private CpAcceleration _cpAcceleration;
    
    public bool IsBoosting { get; private set; }

    #endregion

    #region Methods
    
    private void Start()
    {
        _cpMain = GetComponent<CpMain>();
        _cpAcceleration = GetComponentInChildren<CpAcceleration>();
        currentBoostTimeLeft = boostTimeMax;
    }

    private void Update()
    {
        CheckInput();
        UpdateAbility();
    }

    private void FixedUpdate()
    {
        DoAbility();
    }

    public override void CheckInput()
    {
        IsBoosting = Input.GetKey(abilityButton);
    }

    private void UpdateAbility()
    {
        if (IsBoosting && currentBoostTimeLeft > 0)
        {
            currentBoostTimeLeft -= Time.deltaTime;
            currentBoost = boostForce;
        }
        else if (currentBoostTimeLeft < boostTimeMax && !IsBoosting)
        {
            currentBoostTimeLeft += boostRechargeRate * Time.deltaTime;
            currentBoostTimeLeft = Mathf.Clamp(currentBoostTimeLeft, 0, boostTimeMax);
        }
        else
        {
            currentBoost = 0;
        }
    }

    public override void DoAbility()
    {
        if (!_cpMain.wheelData.grounded) return;

        //Note sign has been accounted for when calculating acceleration
        Vector3 __force = Time.fixedDeltaTime * currentBoost * _cpMain.rb.transform.forward; 
        _cpMain.rb.AddForce(__force, ForceMode.Impulse);
    }

    #endregion
}