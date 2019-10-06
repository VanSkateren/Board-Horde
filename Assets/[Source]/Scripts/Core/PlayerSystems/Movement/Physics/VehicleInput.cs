using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using Sirenix.OdinInspector;

namespace Core.PlayerSystems.Movement
{
    public class VehicleInput : VehicleBehaviour
    {
        #region Variables
        
        public InputAction moveAction;
        //public InputAction lookAction;

        [Space] 
        [ShowInInspector] private float accelerateAxis;
        [ShowInInspector] private float brakingAxis;

        private Vector2 moveInput;
        
        #endregion

        #region Methods

        public void OnEnable()
        {
            moveAction.Enable();
        }

        public void OnDisable()
        {
            moveAction.Disable();
        }

        protected override void Start()
        {
            base.Start();
            _vehicle.Input = new PlayerInputs();
        }
        
        private void Update()
        {
            HandleInputs();
        }

        private void HandleInputs()
        {
            moveInput = moveAction.ReadValue<Vector2>();
            
            //Forward/Reverse
            //accelerateAxis = Input.GetAxis("Vertical");
            _vehicle.Input.accelInput = moveInput.y;

            //Steering
            _vehicle.Input.steeringInput = moveInput.x;
        }
        
        #endregion
    }

    [Serializable]
    public class PlayerInputs
    {
        public float accelInput;
        public float steeringInput;
    }
}
