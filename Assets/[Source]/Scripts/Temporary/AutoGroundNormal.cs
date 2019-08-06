using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Core.PlayerSystems.Movement;

using CommonGames.Utilities.CGTK;

namespace Temporary
{
    public class AutoGroundNormal : MonoBehaviour
    {
        public Core.PlayerSystems.Movement.PlayerController playerController = null;
        
        private Vector3 _gravityDirection = Vector3.zero;
        
        private void Update()
        {
            Transform __playerRoot = playerController.meshRoot; 
            
            Ray __ray = new Ray(__playerRoot.position, -__playerRoot.up);

            if (Physics.Raycast(__ray, out RaycastHit __hitInfo))
            {
                _gravityDirection = -__hitInfo.normal * 30;

                CGDebug.DrawRay(__hitInfo.point, -_gravityDirection);
            }
            else
            {
                _gravityDirection = playerController.configs.gravity;
            }
            
            playerController._gravity = _gravityDirection;
        }
    }
}