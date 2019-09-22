using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CvSpeedCameraShake : MonoBehaviour
{
    private CpMain _cpMain;
    private CameraSystem _cameraSystem;

    private void Awake()
    {
        _cpMain = transform.parent.GetComponent<CpMain>();
        _cameraSystem = transform.parent.GetComponent<CameraSystem>();
    }

    private void Update()
    {
        _cameraSystem.currentNoise.m_FrequencyGain = (_cpMain.speedData.SpeedPercent - 0.3f) * 15f;
        _cameraSystem.currentNoise.m_AmplitudeGain = Mathf.Clamp01(_cpMain.speedData.SpeedPercent - 0.98f);
    }
}