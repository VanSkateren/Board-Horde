using System;
using UnityEngine;

using CommonGames.Utilities.Extensions;

public class CpAcceleration : MonoBehaviour
{
    [SerializeField] private AnimationCurve velocityTimeCurve;

    private float _accelerationToApply;
    private float _currentTimeValue;
    private float _nextTimeValue;
    private float _nextVelocityMagnitude;

    private CpMain _cpMain;

    private void Awake()
    {
        _cpMain = transform.parent.GetComponent<CpMain>();
    }

    private void Start()
    {
        _cpMain.speedData = new VehicleSpeed(velocityTimeCurve.keys[velocityTimeCurve.length - 1].value);
    }

    private void Update()
    {
        CalculateSpeedData(_cpMain.rb, _cpMain.speedData);
        _accelerationToApply = GetAccelerationFromVelocityTimeCurve(velocityTimeCurve, _cpMain.input, _cpMain.speedData);
    }

    private void FixedUpdate()
    {
        float __inputScaledAccel = Mathf.Abs(_cpMain.input.accelInput) * _accelerationToApply;
        ApplyAcceleration(__inputScaledAccel, _cpMain.rb, _cpMain.wheelData.grounded);
    }

    private static void CalculateSpeedData(Rigidbody rb, VehicleSpeed speedData)
    {
        speedData.sideSpeed = Vector3.Dot(rb.transform.right, rb.velocity);
        speedData.forwardSpeed = Vector3.Dot(rb.transform.forward, rb.velocity);
        speedData.speed = rb.velocity.magnitude;
    }

    /// <summary>
    /// An Alternative to the Vel-Time Curve approach
    /// This works by adjusting the force applied according to how fast the car is moving
    /// Top speed is defined on the curve by the value of the first key
    /// </summary>
    private float GetForceFromVelocityForceCurve(AnimationCurve velocityForceCurve, PlayerInputs input,
        VehicleSpeed speedData)
    {
        if (input.accelInput.Abs() < 0.05f) return 0;

        float __curveTopSpeed = velocityForceCurve.keys[0].value;
        float __velocityForceCurveEvaluation = velocityForceCurve.Evaluate(speedData.forwardSpeed / __curveTopSpeed);

        return __velocityForceCurveEvaluation * __curveTopSpeed;
    }

    //This process works using reverse evaluation of a Velocity-Time curve
    //Binary search using current forward speed to find the time value on the graph
    //Add one time step onto that time value and evaluate the graph to get the new velocity
    //Calculate a = (Vf - Vi)/deltaTime
    //Return the new force to apply (Must use ForceMode.Acceleration to ignore mass)
    //timeScaler is the scale of the time buckets used in the binary search as it uses Ints
    private float GetAccelerationFromVelocityTimeCurve(AnimationCurve velocityTime, PlayerInputs input,
        VehicleSpeed speedData)
    {
        if (speedData.forwardSpeed > velocityTime.keys[velocityTime.length - 1].value) return 0;

        float __speedClamped = Mathf.Clamp(
            speedData.forwardSpeed,
            velocityTime.keys[0].value,
            velocityTime.keys[velocityTime.length - 1].value);

        _currentTimeValue = BinarySearchDisplay(velocityTime, __speedClamped);

        if (_currentTimeValue.Approximately(-1)) return 0;
        
        //TODO: Edit
        float __inputDir = (input.accelInput > 0) ? 1 : -1;
        
        _nextTimeValue = _currentTimeValue + __inputDir * Time.fixedDeltaTime;
        _nextTimeValue = Mathf.Clamp(
            value: _nextTimeValue,
            min:velocityTime.keys[0].time, 
            max:velocityTime.keys[velocityTime.length - 1].time);

        _nextVelocityMagnitude = velocityTime.Evaluate(_nextTimeValue);
        float __accelMagnitude = (_nextVelocityMagnitude - speedData.forwardSpeed) / (Time.fixedDeltaTime);

        return __accelMagnitude;
    }

    private static float BinarySearchDisplay(AnimationCurve velTimeCurve, float currentVel)
    {
        const int __TIME_SCALE = 10000;
        
        int __minTime = (int)(velTimeCurve.keys[0].time * __TIME_SCALE);
        int __maxTime = (int)(velTimeCurve.keys[velTimeCurve.length - 1].time * __TIME_SCALE);

        while (__minTime <= __maxTime)
        {
            int __mid = (__minTime + __maxTime) / 2;

            float __scaledMid = (float) __mid / __TIME_SCALE;
            
            if (Mathf.Abs(velTimeCurve.Evaluate(__scaledMid) - currentVel) <= 0.01f)
            {
                return (float)__mid/__TIME_SCALE;
            }
            
            if (currentVel < velTimeCurve.Evaluate(__scaledMid))
            {
                __maxTime = __mid - 1;
            }
            else
            {
                __minTime = __mid + 1;
            }
        }
        
        return -1;
    }

    private static void ApplyAcceleration(float accelToApply, Rigidbody rb, bool grounded)
    {
        if (!grounded) return;

        //Note sign has been accounted for when calculating acceleration
        Vector3 __force = rb.transform.forward * accelToApply; 
        rb.AddForce(__force, ForceMode.Acceleration);
    }
}

[Serializable]
public class VehicleSpeed
{
    public float speed;
    public float forwardSpeed;
    public float sideSpeed;
    public float topSpeed;
    
    public float ForwardSpeedPercent => Mathf.Abs(forwardSpeed / topSpeed);
    public float SideSpeedPercent => Mathf.Abs(sideSpeed / topSpeed);
    public float SpeedPercent => Mathf.Abs(speed / topSpeed);

    public VehicleSpeed(float topSpeed)
    {
        this.topSpeed = topSpeed;
    }
}