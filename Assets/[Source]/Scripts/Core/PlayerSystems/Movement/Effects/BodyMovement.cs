﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.PlayerSystems.Movement
{
    public class BodyMovement : VehicleBehaviour
    {
        public Transform modelBase;
        public Vector3 modelBaseOffset;

        public BodyAxisMovement roll;
        public BodyAxisMovement pitch;

        private void Update()
        {
            if(_vehicle.mayMove <= 0)
            {
                UpdateCarBodyTransform();
            }
        }

        private void UpdateCarBodyTransform()
        {
            Transform __transform = transform;
            __transform.position = _vehicle.rigidbody.position;
            __transform.rotation = _vehicle.rigidbody.rotation;

            //Debug.Log(_vehicle.SpeedData.ForwardSpeedPercent.ToString());
            
            //CarBody Roll - Steering and Side Speed
            roll.currentAngle = Mathf.Lerp(roll.currentAngle, roll.inputMaxAngle * _vehicle.InputData.steeringInput,Time.deltaTime * 10);
            //(Time.deltaTime * (10 * _vehicle.SpeedData.SpeedPercent)));
            
            float __currentBodyRoll = (roll.currentAngle - _vehicle.SpeedData.SideSpeedPercent * roll.speedMaxAngle) * _vehicle.SpeedData.SpeedPercent;

            
            /*
            pitch.currentAngle = Mathf.Lerp(
                pitch.currentAngle,=
                (_vehicle.InputData.accelInput * pitch.inputMaxAngle),
                (10 * Time.deltaTime));
            */
            
            float __currentBodyPitch = pitch.currentAngle + (Mathf.Clamp01(_vehicle.SpeedData.ForwardSpeedPercent) * pitch.speedMaxAngle);

            modelBase.rotation = _vehicle.rigidbody.rotation * Quaternion.Euler(__currentBodyPitch, 0, __currentBodyRoll);
        }
    }

    [System.Serializable]
    public class BodyAxisMovement
    {
        public float currentAngle;
        public float inputMaxAngle;
        public float speedMaxAngle;
    }
}