using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.PlayerSystems.Movement
{
    [Serializable]
    public class Drag : VehicleBehaviour
    {
        [Header("Drag")] public float linearDrag;
        public float freeWheelDrag;
        public float brakingDrag;
        public float angularDrag;

        public bool linearDragCheck;
        public bool brakingDragCheck;
        public bool freeWheelDragCheck;

        protected override void Start()
        {
            base.Start();
            
            _vehicle.rigidbody.angularDrag = angularDrag;
        }

        private void Update()
        {
            UpdateDrag(
                _vehicle.rigidbody,
                _vehicle.wheelData.grounded,
                _vehicle.Input,
                _vehicle.SpeedData
            );

        }

        private void UpdateDrag(Rigidbody rb, bool grounded, PlayerInputs input, VehicleSpeed speedData)
        {
            linearDragCheck = Mathf.Abs(input.accelInput) < 0.05 || grounded;
            float __linearDragToAdd = linearDragCheck ? linearDrag : 0;

            brakingDragCheck = input.accelInput < 0 && speedData.forwardSpeed > 0;
            float __brakingDragToAdd = brakingDragCheck ? brakingDrag : 0;

            freeWheelDragCheck = Math.Abs(input.accelInput) < 0.02f && grounded;
            float __freeWheelDragToAdd = freeWheelDragCheck ? freeWheelDrag : 0;

            rb.drag = __linearDragToAdd + __brakingDragToAdd + __freeWheelDragToAdd;
        }
    }
}