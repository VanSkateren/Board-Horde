using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.Serialization;
#endif

using CommonGames.Utilities;
using CommonGames.Utilities.Extensions;
using CommonGames.Utilities.CGTK;

using JetBrains.Annotations;

using Curve;

namespace Core.PlayerSystems.Movement.Abilities
{
    //using Utilities.Extensions;

    public class GrindAbility : BaseAbility
    {
        #region Variables
        
        [HideInInspector]
        public TubeGenerator bar;
        
        //public EndOfPathInstruction endOfPathInstruction;
        [SerializeField] private float speed = 0.1f;
    
        private float _distanceTravelled;

        private Matrix4x4 _worldMatrix, _localMatrix;

        private bool _onGrindBar = false;
        
        //for example 10m/s on a bar of 100m would be 0.1/s, because 10 is 10% of 100.
        private float _calculatedSpeed = 0f;

        private bool _readyToYeet = false;
        
        #endregion

        #region Methods

        private void Awake()
        {
            abilityAction.started +=
                ctx => { _readyToYeet = true; };
            
            abilityAction.canceled +=
                ctx => { _readyToYeet = false; };
        }

        public override void CheckInput()
        {
            
        }

        public override void DoAbility()
        {

        }

        private void Update()
        {
            if(!_onGrindBar) return;
            
            //Debug.Log("Following GrindBar");
            if(_readyToYeet)
            {
                YeetFromBar();
                return;
            }
            
            FollowBar();
        }

        [PublicAPI]
        public void AttachToBar(in TubeGenerator bar)
        {
            if(_onGrindBar) return;

            this.bar = bar;
            
            _worldMatrix = bar.transform.WorldMatrix();
            _localMatrix = bar.transform.LocalMatrix();
        
            Vector3 __relativePositionToTube = transform.position.GetRelativePositionTo(_localMatrix);

            float __startPoint = bar.GetStartPoint(__relativePositionToTube);

            _distanceTravelled = __startPoint;
            
            _calculatedSpeed = (speed / bar.Length);

            _vehicle.mayMove++;

            ToggleRigidBody();
            StopSecondaryMovement();

            _onGrindBar = true;
        }
        [PublicAPI]
        public void YeetFromBar()
        {
            _onGrindBar = false;
            
            ToggleRigidBody();
            
            _vehicle.mayMove--;
        }
        
        private void FollowBar()
        {
            //if(!_onGrindBar) return;
            
            _distanceTravelled += _calculatedSpeed * Time.deltaTime;

            if(_distanceTravelled >= 1 || _distanceTravelled.Approximately(1))
            {
                YeetFromBar();
                return;
            }

            Vector3 __posRelativeToBar = bar.Curve.GetPointAt(_distanceTravelled);
        
            Vector3 __posRelativeToWorld = __posRelativeToBar.GetRelativePositionFrom(_worldMatrix);

            CGDebug.DrawWireSphere(__posRelativeToWorld, radius: 1f).Color(Color.red);

            //__worldPos.y += 1f;
        
            transform.position = __posRelativeToWorld;
        }

        private void StopSecondaryMovement()
        {
            Rigidbody __rigidbody = _vehicle.rigidbody;
            
            __rigidbody.ResetVelocity();
        }

        private void ToggleRigidBody()
        {
            Rigidbody __rigidbody = _vehicle.rigidbody;

            __rigidbody.constraints = __rigidbody.constraints == RigidbodyConstraints.None 
                ? RigidbodyConstraints.FreezeAll 
                : RigidbodyConstraints.None;

            __rigidbody.useGravity = !__rigidbody.useGravity;

            _vehicle.rigidbody = __rigidbody;
        }

        #endregion
    
    }
}