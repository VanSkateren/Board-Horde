using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CvBoostPostProcessingEffect : MonoBehaviour
{
    [SerializeField] public PostProcessProfile profile;
    
    private ChromaticAberration _chromaticAberration;
    private LensDistortion _lensDistortion;

    private CpMain _cpMain;
    private BoostAbility _caBoost;

    private void Start()
    {
        Transform __parent = transform.parent;
        
        _cpMain = __parent.GetComponent<CpMain>();
        _caBoost = __parent.GetComponent<BoostAbility>();

        profile.TryGetSettings<ChromaticAberration>(out _chromaticAberration); 
        //GetSetting<ChromaticAberration>();
        _lensDistortion = profile.GetSetting<LensDistortion>();
    }

    private void Update()
    {
        if (_caBoost.IsBoosting)
        {
            if(_chromaticAberration != null)
            {
                _chromaticAberration.intensity.value =
                Mathf.Lerp(_chromaticAberration.intensity.value, 1, Time.deltaTime * 5);
            }

            if (_lensDistortion != null)
            {
                _lensDistortion.intensity.value = Mathf.Lerp(_lensDistortion.intensity.value, -20f, Time.deltaTime * 5);
            }
        }
        else
        {
            if (_chromaticAberration != null)
            {
                _chromaticAberration.intensity.value =
                    Mathf.Lerp(_chromaticAberration.intensity.value, 0, Time.deltaTime * 10);
            }

            if (_lensDistortion != null)
            {
                _lensDistortion.intensity.value = Mathf.Lerp(_lensDistortion.intensity.value, 0f, Time.deltaTime * 10);
            }
        }
    }
}
