using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonGames.Utilities;
using CommonGames.Utilities.Extensions;
using CommonGames.Utilities.CGTK;

using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace Core.PlayerSystems.Movement
{
    public class VehicleCore : Singleton<VehicleCore>
    {
        [OdinSerialize] public PlayerInputs Input { get; set; }
        [OdinSerialize] public VehicleSpeed SpeedData { get; set; }
        public WheelData wheelData;

        [HideInInspector] public new Rigidbody rigidbody;
        public Vector3 averageColliderSurfaceNormal;

        private bool _prevGroundedState;
        public static event Action<VehicleCore> OnLeavingGround = vehicleCore => { };
        public static event Action<VehicleCore> OnLanding = vehicleCore => { };

        private void Start()
        {
            rigidbody = GetComponentInChildren<Rigidbody>();
        }

        private void Update()
        {
            switch (_prevGroundedState)
            {
                case false when wheelData.grounded:
                    OnLanding(this);
                    break;
                case true when !wheelData.grounded:
                    OnLeavingGround(this);
                    break;
            }

            _prevGroundedState = wheelData.grounded;
        }
    }
}