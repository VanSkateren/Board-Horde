﻿using UnityEngine.Rendering;
using UnityEngine;

[RequireComponent(typeof(Light))]
[ExecuteInEditMode()]
public class NGSS_Local : MonoBehaviour
{
    [Tooltip("Check this option to disable this component from receiving updates calls at runtime or when you hit play in Editor.\nUseful when you have lot of lights in your scene and you don't want that many update calls.")]
    public bool NGSS_DISABLE_ON_PLAY = false;

    [Tooltip("Check this option if you don't need to update shadows variables at runtime, only once when scene loads.\nUseful when you have lot of lights in your scene and you don't want that many update calls.")]
    public bool NGSS_NO_UPDATE_ON_PLAY = false;

    [Tooltip("If enabled, this component will manage GLOBAL SETTINGS for all Local shadows.\nEnable this option only in one of your scene local lights to avoid multiple lights fighting for global tweaks.\nLOCAL SETTINGS are not affected by this option.")]
    public bool NGSS_MANAGE_GLOBAL_SETTINGS = false;

    [Header("GLOBAL SETTINGS")]
    [Space]
    [Tooltip("Used to test blocker search and early bail out algorithms. Keep it as low as possible, might lead to noise artifacts if too low.\nRecommended values: Mobile = 8, Consoles & VR = 16, Desktop = 24")]
    [Range(4, 32)]
    public int NGSS_SAMPLING_TEST = 16;

    [Tooltip("Number of samplers per pixel used for PCF and PCSS shadows algorithms.\nRecommended values: Mobile = 12, Consoles & VR = 24, Desktop Med = 32, Desktop High = 48, Desktop Ultra = 64")]
    [Range(4, 64)]
    public int NGSS_SAMPLING_FILTER = 32;

    [Space]
    [Tooltip("If zero = 100% noise.\nIf one = 100% dithering.\nUseful when fighting banding.")]
    [Range(0, 1)]
    public int NGSS_NOISE_TO_DITHERING_SCALE = 0;
    [Tooltip("If you set the noise scale value to something less than 1 you need to input a noise texture.\nRecommended noise textures are blue noise signals.")]
    public Texture2D NGSS_NOISE_TEXTURE;

    [Space]
    [Tooltip("Number of samplers per pixel used for PCF and PCSS shadows algorithms.\nRecommended values: Mobile = 12, Consoles & VR = 24, Desktop Med = 32, Desktop High = 48, Desktop Ultra = 64")]
    [Range(0f, 1f)]
    public float NGSS_SHADOWS_OPACITY = 1f;

    //[Header("BIAS")]
    //[Tooltip("This estimates receiver slope using derivatives and tries to tilt the filtering kernel along it.\nHowever, when doing it in screenspace from the depth texture can leads to shadow artifacts.\nThus it is disabled by default.")]
    //public bool NGSS_SLOPE_BASED_BIAS = false;
    //[Tooltip("Minimal fractional error for the receiver plane bias algorithm.")]
    //[Range(0f, 0.1f)]
    //public float NGSS_RECEIVER_PLANE_MIN_FRACTIONAL_ERROR = 0.01f;

    [Header("LOCAL SETTINGS")]
#if !UNITY_5
    //[Header("PCSS")]
    [Tooltip("PCSS Requires inline sampling and SM3.5.\nProvides Area Light soft-shadows.\nDisable it if you are looking for PCF filtering (uniform soft-shadows) which runs with SM3.0.")]
    public bool NGSS_PCSS_ENABLED = true;

    [Tooltip("How soft shadows are when close to caster. Low values means sharper shadows.")]
    [Range(0f, 2f)]
    public float NGSS_PCSS_SOFTNESS_NEAR = 0f;

    [Tooltip("How soft shadows are when far from caster. Low values means sharper shadows.")]
    [Range(0f, 2f)]
    public float NGSS_PCSS_SOFTNESS_FAR = 1f;//

    [Tooltip("Value to fix blocker search bias artefacts. Be careful with extreme values, can lead to false self-shadowing.")]
    [Range(0f, 1f)]
    public float NGSS_PCSS_BLOCKER_BIAS = 0f;

    //[Tooltip("How soft shadows are when close to caster.")]
    //[Range(0f, 2f)]
    //public float NGSS_PCSS_SOFTNESS_MIN = 1f;

    //[Tooltip("How soft shadows are when far from caster.")]
    //[Range(0f, 2f)]
    //public float NGSS_PCSS_SOFTNESS_MAX = 1f;
#endif
    [Space]
    [Tooltip("Defines the Penumbra size of this shadows.")]
    [Range(0f, 1f)]
    public float NGSS_SHADOWS_SOFTNESS = 1f;

    public enum ShadowMapResolution { UseQualitySettings = 256, VeryLow = 512, Low = 1024, Med = 2048, High = 4096, Ultra = 8192 }    
    [Tooltip("Shadows resolution.\nUseQualitySettings = From Quality Settings, SuperLow = 512, Low = 1024, Med = 2048, High = 4096, Ultra = 8192.")]
    public ShadowMapResolution NGSS_SHADOWS_RESOLUTION = ShadowMapResolution.UseQualitySettings;

    /****************************************************************/

    //public Texture noiseTexture;
    private bool isInitialized = false;
    private Light _LocalLight;
    private Light LocalLight
    {
        get
        {
            if (_LocalLight == null) { _LocalLight = GetComponent<Light>(); }
            return _LocalLight;
        }
    }
    
    void OnDisable()
    {
        isInitialized = false;
    }

    void OnEnable()
    {
        if (IsNotSupported())
        {
            Debug.LogWarning("Unsupported graphics API, NGSS requires at least SM3.0 or higher and DX9 is not supported.", this);
            enabled = false;
            return;
        }

        Init();
    }

    void Init()
    {
        if (isInitialized) { return; }

#if !UNITY_5
        LocalLight.shadows = NGSS_PCSS_ENABLED ? LightShadows.Soft : LightShadows.Hard;
        Shader.SetGlobalFloat("NGSS_PCSS_FILTER_LOCAL_MIN", NGSS_PCSS_SOFTNESS_NEAR);
        Shader.SetGlobalFloat("NGSS_PCSS_FILTER_LOCAL_MAX", NGSS_PCSS_SOFTNESS_FAR);
#endif

        SetProperties(NGSS_MANAGE_GLOBAL_SETTINGS);

        if (NGSS_NOISE_TEXTURE == null) { NGSS_NOISE_TEXTURE = Resources.Load<Texture2D>("BlueNoise_R8_8"); }
        Shader.SetGlobalTexture("_BlueNoiseTexture", NGSS_NOISE_TEXTURE);

        isInitialized = true;
    }

    bool IsNotSupported()
    {
#if UNITY_2018_1_OR_NEWER
        return (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2);
#elif UNITY_2017_4_OR_EARLIER
        return (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2 || SystemInfo.graphicsDeviceType == GraphicsDeviceType.PlayStationVita || SystemInfo.graphicsDeviceType == GraphicsDeviceType.N3DS);
#else
        return (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D9 || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2 || SystemInfo.graphicsDeviceType == GraphicsDeviceType.PlayStationMobile || SystemInfo.graphicsDeviceType == GraphicsDeviceType.PlayStationVita || SystemInfo.graphicsDeviceType == GraphicsDeviceType.N3DS);
#endif
    }

    void Update()
    {
        if (LocalLight.shadows == LightShadows.None) { return; }

        if (Application.isPlaying) { if (NGSS_DISABLE_ON_PLAY) { enabled = false; return; } if (NGSS_NO_UPDATE_ON_PLAY) { return; } }

        SetProperties(NGSS_MANAGE_GLOBAL_SETTINGS);
    }

    void SetProperties(bool setLocalAndGlobalProperties)
    {
        //Local
        LocalLight.shadowStrength = NGSS_SHADOWS_SOFTNESS;
        if (NGSS_SHADOWS_RESOLUTION == ShadowMapResolution.UseQualitySettings)
            LocalLight.shadowResolution = LightShadowResolution.FromQualitySettings;
        else
            LocalLight.shadowCustomResolution = (int)NGSS_SHADOWS_RESOLUTION;

        //Global
        if (setLocalAndGlobalProperties == false) { return; }

        NGSS_SAMPLING_TEST = Mathf.Clamp(NGSS_SAMPLING_TEST, 4, NGSS_SAMPLING_FILTER);
        Shader.SetGlobalFloat("NGSS_TEST_SAMPLERS", NGSS_SAMPLING_TEST);

#if !UNITY_5
        LocalLight.shadows = NGSS_PCSS_ENABLED ? LightShadows.Soft : LightShadows.Hard;
        Shader.SetGlobalFloat("NGSS_PCSS_FILTER_LOCAL_MIN", NGSS_PCSS_SOFTNESS_NEAR);
        Shader.SetGlobalFloat("NGSS_PCSS_FILTER_LOCAL_MAX", NGSS_PCSS_SOFTNESS_FAR);
        Shader.SetGlobalFloat("NGSS_PCSS_LOCAL_BLOCKER_BIAS", NGSS_PCSS_BLOCKER_BIAS * 0.01f);
#endif
        Shader.SetGlobalFloat("NGSS_NOISE_TO_DITHERING_SCALE", NGSS_NOISE_TO_DITHERING_SCALE);
        Shader.SetGlobalFloat("NGSS_FILTER_SAMPLERS", NGSS_SAMPLING_FILTER);
        Shader.SetGlobalFloat("NGSS_GLOBAL_OPACITY", 1f - NGSS_SHADOWS_OPACITY);        

        //Shader.SetGlobalFloat("NGSS_RECEIVER_PLANE_MIN_FRACTIONAL_ERROR_LOCAL", NGSS_RECEIVER_PLANE_MIN_FRACTIONAL_ERROR);
    }
}
