using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpAbility : BaseAbility
{
    #region Variables
    
    [SerializeField] private float forceMin = 5;
    [SerializeField] private float forceMax = 15;
    [SerializeField] private float forceChargeRate = 10;

    [Space]
    
    private float _currentCharge = 0;
    private bool _jumpPressed;

    private CpMain _cpMain;
    
    #endregion

    #region Methods

    private void Awake()
    {
        _cpMain = GetComponent<CpMain>();
    }

    private void Update()
    {
        CheckInput();
    }

    private void FixedUpdate()
    {
        DoAbility();
    }

    public override void CheckInput()
    {
        if (Input.GetKey(abilityButton))
        {
            _currentCharge += forceChargeRate * Time.deltaTime;
            _currentCharge = Mathf.Clamp(_currentCharge, forceMin, forceMax);
        }
        if (Input.GetKeyUp(abilityButton))
        {
            _jumpPressed = true;
        }
    }

    public override void DoAbility()
    {
        if (!_jumpPressed) return;

        _jumpPressed = false;

        //Remove if you want air jumps
        if (!_cpMain.wheelData.grounded) return;

        Rigidbody rb = _cpMain.rb;
        
        rb.AddForceAtPosition(_currentCharge * rb.transform.up, rb.position, ForceMode.Impulse);
        
        _currentCharge = 0;
        
    }
    
    #endregion

}
