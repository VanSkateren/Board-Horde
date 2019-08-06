namespace Temporary
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using KinematicCharacterController;
    using KinematicCharacterController.Examples;
    using Core.PlayerSystems.Movement;

    public class Player : MonoBehaviour
    {
        public bool usePlayerController = false;

        [Sirenix.OdinInspector.ShowIf("usePlayerController")]
        public PlayerController playerController;

        [Sirenix.OdinInspector.HideIf("usePlayerController")]
        public ExampleCharacterController characterController;

        public ExampleCharacterCamera CharacterCamera;

        private const string MouseXInput = "Mouse X";
        private const string MouseYInput = "Mouse Y";
        private const string MouseScrollInput = "Mouse ScrollWheel";
        private const string HorizontalInput = "Horizontal";
        private const string VerticalInput = "Vertical";

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;

            CharacterCamera.SetFollowTransform(usePlayerController
                ? playerController.cameraFollowPoint
                : characterController.CameraFollowPoint);

            // Ignore the character's collider(s) for camera obstruction checks
            if (characterController == null) return;

            CharacterCamera.IgnoredColliders.Clear();
            CharacterCamera.IgnoredColliders.AddRange(characterController.GetComponentsInChildren<Collider>());
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            HandleCameraInput();

            if (usePlayerController) return;

            HandleCharacterInput();
        }

        private void HandleCameraInput()
        {
            // Create the look input vector for the camera
            float __mouseLookAxisUp = Input.GetAxisRaw(MouseYInput);
            float __mouseLookAxisRight = Input.GetAxisRaw(MouseXInput);
            Vector3 __lookInputVector = new Vector3(__mouseLookAxisRight, __mouseLookAxisUp, 0f);

            // Prevent moving the camera while the cursor isn't locked
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                __lookInputVector = Vector3.zero;
            }

            // Input for zooming the camera (disabled in WebGL because it can cause problems)
            float __scrollInput = -Input.GetAxis(MouseScrollInput);

            #if UNITY_WEBGL
                scrollInput = 0f;
            #endif

            // Apply inputs to the camera
            CharacterCamera.UpdateWithInput(Time.deltaTime, __scrollInput, __lookInputVector);

            // Handle toggling zoom level
            if (Input.GetMouseButtonDown(1))
            {
                CharacterCamera.TargetDistance =
                    (CharacterCamera.TargetDistance == 0f) ? CharacterCamera.DefaultDistance : 0f;
            }
        }

        private void HandleCharacterInput()
        {
            PlayerCharacterInputs __characterInputs = new PlayerCharacterInputs
            {
                MoveAxisForward = Input.GetAxisRaw(VerticalInput),
                MoveAxisRight = Input.GetAxisRaw(HorizontalInput),
                CameraRotation = CharacterCamera.Transform.rotation,
                JumpDown = Input.GetKeyDown(KeyCode.Space),
                CrouchDown = Input.GetKeyDown(KeyCode.C),
                CrouchUp = Input.GetKeyUp(KeyCode.C)
            };

            // Build the CharacterInputs struct

            // Apply inputs to character
            characterController.SetInputs(ref __characterInputs);
        }
    }
}