using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CvBoostTrails : MonoBehaviour
{
    [SerializeField] private TrailRenderer leftLightTrail;
    [SerializeField] private TrailRenderer rightLightTrail;
    
    private BoostAbility _caBoost;
    private CpMain _cpMain;
    
    private void Start()
    {
        _caBoost = transform.parent.GetComponent<BoostAbility>();
        _cpMain = transform.parent.GetComponent<CpMain>();
    }

    private void Update()
    {
        if (_caBoost == null) return;
        
        leftLightTrail.emitting = _caBoost.IsBoosting || _cpMain.speedData.ForwardSpeedPercent>1;
        rightLightTrail.emitting = _caBoost.IsBoosting|| _cpMain.speedData.ForwardSpeedPercent>1;
    }
}
