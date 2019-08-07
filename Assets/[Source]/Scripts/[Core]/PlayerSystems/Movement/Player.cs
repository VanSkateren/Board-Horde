using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonGames.Utilities;
using CommonGames.Utilities.Extensions;
using CommonGames.Utilities.CGTK;

using Sirenix.OdinInspector;
using Sirenix.Serialization;

using JetBrains.Annotations;

using EaseType = CommonGames.Utilities.CGTK.Greasy.EaseType;
using Greasy = CommonGames.Utilities.CGTK.Greasy.Greasy;
using ReadOnlyAttribute = Sirenix.OdinInspector.ReadOnlyAttribute;

namespace Core.PlayerSystems.Movement
{
	public class Player : Singleton<Player>
	{
		#region Variables

		#region Serialized
		
		[BoxGroup("References", showLabel: false)]
		
		[BoxGroup("References/SceneReferences", showLabel: false)]
		[Required]
		public Transform playerCamera;
		
		[BoxGroup("References/SceneReferences")]
		[Required]
		[SerializeField] public Skateboard skateboard;

		[BoxGroup("References/SceneReferences")]
        [Required]
        public Transform meshRoot, cameraFollowPoint;
		
		[BoxGroup("References")]
		[Required] [InlineEditor]
		public PlayerConfigs configs = null;
		
		#endregion
		
		#region Non-Serialized

		private const OrientationMethod ORIENTATION_METHOD = OrientationMethod.TowardsMovement;

		private readonly Collider[] _probedColliders = new Collider[8];

		[NonSerialized] public Vector3 _gravity;
		
		private Vector3
			_moveInputVector,
			_lookInputVector;
		
		private bool 
			_jumpRequested = false,
			_jumpConsumed = false,
			_jumpedThisFrame = false;
		
		private float
			_timeSinceJumpRequested = Mathf.Infinity,
			_timeSinceLastAbleToJump = 0f;
		
		private Vector3 _internalVelocityAdd = Vector3.zero;

		private Vector3 _lastInnerNormal = Vector3.zero;
		private Vector3 _lastOuterNormal = Vector3.zero;

		
		[NonSerialized] // Don't serialize this so the value is lost on an editor script recompile.
		// ReSharper disable once MemberCanBePrivate.Global
		public bool initialized;

		[SerializeField] [ReadOnly] 
		private PlayerCharacterInputs characterInputs;
		
		#endregion
			
		#region Local Classes
		
		public enum CharacterState
		{
			Default,
		}

		private enum OrientationMethod
		{
			TowardsCamera,
			TowardsMovement,
		}

		[Serializable]
		public struct PlayerCharacterInputs
		{
			public float moveAxisForward;
			public float moveAxisRight;
			public Quaternion cameraRotation;
			public bool jumpDown;
		}
		
		#endregion
		
		#region Consts
		
		private const string LOOK_HORIZONTAL = "Look Horizontal";
		private const string LOOK_VERTICAL = "Look Vertical";
		private const string MOVE_HORIZONTAL = "Horizontal";
		private const string MOVE_VERTICAL = "Vertical";
		private const string JUMP_KEY = "Jump";
		
		#endregion

		#endregion

		/*
		#region Methods

		#region Base
		
		private void Reset()
			=> skateboard = skateboard ? skateboard : GetComponentInChildren<Skateboard>();

		private void Awake()
		{
			//Initial state.
			TransitionToState(CharacterState.Default);

			skateboard = skateboard ? skateboard : GetComponentInChildren<Skateboard>();

			skateboard.Player = this;

			_gravity = configs.gravity;
		}

		private void Update()
	  	{		   
		   if(!initialized)
		   {
			   Initialize();
		   }

		   PlayerCharacterInputs __charInputs = CharacterInputs;
		   SetCharacterInputs(ref __charInputs);
	   }
		
		private void Initialize()
			=> initialized = true;
		
		#endregion

		#region Input Handling

		private PlayerCharacterInputs CharacterInputs
		{
			get
			{
				characterInputs.moveAxisForward = Input.GetAxisRaw(MOVE_VERTICAL);
				characterInputs.moveAxisRight = Input.GetAxisRaw(MOVE_HORIZONTAL);
				characterInputs.cameraRotation = playerCamera.transform.rotation;
				characterInputs.jumpDown = Input.GetButtonDown(JUMP_KEY);

				return characterInputs;
			}
		}
		 
		#endregion

		#region Character Controller

		[PublicAPI]
		public CharacterState CurrentCharacterState { get; private set; }
	
		#region State Handling
		
		///<summary> Handles movement state transitions and enter/exit callbacks </summary>
		[PublicAPI]
		public void TransitionToState(CharacterState newState)
		{
			CharacterState __tmpInitialState = CurrentCharacterState;
			OnStateExit(__tmpInitialState, newState);
			CurrentCharacterState = newState;
			OnStateEnter(newState, __tmpInitialState);
		}
		
		///<summary> Event when entering a state </summary>
		[PublicAPI]
		public void OnStateEnter(CharacterState state, CharacterState fromState)
		{
			switch (state)
			{
				case CharacterState.Default:
				{
					break;
				}
				default:
					throw new ArgumentOutOfRangeException(nameof(state), state, null);
			}
		}
		
		///<summary> Event when exiting a state </summary>
		[PublicAPI]
		public void OnStateExit(CharacterState state, CharacterState toState)
		{
			switch (state)
			{
				case CharacterState.Default:
				{
					break;
				}
				default:
					throw new ArgumentOutOfRangeException(nameof(state), state, null);
			}
		}
		
		#endregion
		
		#region Update Loops
		
		/// <inheritdoc />
		/// <summary>
		/// (Called by KinematicCharacterMotor during its update cycle)
		/// This is called before the character begins its movement update
		/// </summary>
		[PublicAPI]
		public void BeforeCharacterUpdate(float deltaTime)
		{
		}
		
		/// <inheritdoc />
		/// <summary>
		/// (Called by KinematicCharacterMotor during its update cycle)
		/// This is called after the character has finished its movement update
		/// </summary>
		[PublicAPI]
		public void AfterCharacterUpdate(float deltaTime)
		{
			switch (CurrentCharacterState)
			{
				case CharacterState.Default:
				{
					// Handle jumping pre-ground grace period
					if (_jumpRequested && _timeSinceJumpRequested > configs.jumpPreGroundingGraceTime)
					{
						_jumpRequested = false;
					}
					
					
					//if (configs.allowJumpingWhenSliding ? skateboard.GroundingStatus.FoundAnyGround : skateboard.GroundingStatus.IsStableOnGround)
					//{
					//	// If we're on a ground surface, reset jumping values
						//if (!_jumpedThisFrame)
						//{
							//_jumpConsumed = false;
						//}
						
						//_timeSinceLastAbleToJump = 0f;
					//}
					//else
					//{
						// Keep track of time since we were last able to jump (for grace period)
					//	_timeSinceLastAbleToJump += deltaTime;
					//}
					//
					
					break;
				}
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		
		
		private float _animationTimePosition;
		
		/// <inheritdoc />
		/// <summary>
		/// (Called by KinematicCharacterMotor during its update cycle)
		/// This is where you tell your character what its rotation should be right now. 
		/// This is the ONLY place where you should set the character's rotation
		/// </summary>
		public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
		{
			switch (CurrentCharacterState)
			{
				case CharacterState.Default:
				{
					if (_lookInputVector.sqrMagnitude > 0f && configs.orientationSharpness > 0f)
					{
						__HandleDefaultCharacterRotation(ref currentRotation);
					}

					if (configs.orientTowardsGravity)
					{
						// Rotate from current up to invert gravity
						currentRotation = Quaternion.FromToRotation((currentRotation * Vector3.up), -_gravity) * currentRotation;
					}

					break;
				}
				default:
					throw new ArgumentOutOfRangeException();
			}

			#region Handling

			void __HandleDefaultCharacterRotation(ref Quaternion currentRotationDefault)
			{

				//Quaternion __fromRotation = Quaternion.LookRotation(skateboard.CharacterForward, skateboard.CharacterUp),
				//			 __toRotation = Quaternion.LookRotation(_lookInputVector, skateboard.CharacterUp);

				//currentRotationDefault = Quaternion.RotateTowards(from: __fromRotation, to: __toRotation, maxDegreesDelta: configs.orientationSharpness * Time.deltaTime);
			}
			
			#endregion
		}
		
		/// <inheritdoc />
		/// <summary>
		/// (Called by KinematicCharacterMotor during its update cycle)
		/// This is where you tell your character what its velocity should be right now. 
		/// This is the ONLY place where you can set the character's velocity
		/// </summary>
		[PublicAPI]
		public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
		{
			switch (CurrentCharacterState)
			{
				case CharacterState.Default:
				{
					// Ground movement
					if (skateboard.GroundingStatus.IsStableOnGround)
					{
						__HandleDefaultGroundMovement(ref currentVelocity);
					}
					else
					{
						__HandleDefaultAirMovement(ref currentVelocity);
					}

					__HandleDefaultJumping(ref currentVelocity);
					break;
				}
				default:
					throw new ArgumentOutOfRangeException();
			}

			#region Handling
			
			void __HandleDefaultGroundMovement(ref Vector3 currentVelocityGround)
			{	
				float __currentVelocityMagnitude = currentVelocityGround.magnitude;
				
				Vector3 __effectiveGroundNormal = skateboard.GroundingStatus.GroundNormal;
				
				if (__currentVelocityMagnitude > 0f && skateboard.GroundingStatus.SnappingPrevented)
				{
					// Take the normal from where we're coming from
					Vector3 __groundPointToCharacter = skateboard.TransientPosition - skateboard.GroundingStatus.GroundPoint;
					
					__effectiveGroundNormal = (Vector3.Dot(currentVelocityGround, __groundPointToCharacter) >= 0f) 
						? skateboard.GroundingStatus.OuterGroundNormal 
						: skateboard.GroundingStatus.InnerGroundNormal;
				}
				
				// Reorient velocity on slope
				currentVelocityGround = skateboard.GetDirectionTangentToSurface(currentVelocityGround, __effectiveGroundNormal) * __currentVelocityMagnitude;

				// Calculate target velocity
				Vector3 __inputRight = Vector3.Cross(_moveInputVector, skateboard.CharacterUp);
				Vector3 __reorientedInput = Vector3.Cross(__effectiveGroundNormal, __inputRight).normalized * _moveInputVector.magnitude;
				Vector3 __targetMovementVelocity = __reorientedInput * configs.maxGroundMoveSpeed;
				
				// Smooth movement Velocity
				currentVelocityGround = Vector3.Lerp(currentVelocityGround, __targetMovementVelocity, 1f - Mathf.Exp(-configs.groundMovementSharpness * deltaTime));
			}
			
			void __HandleDefaultAirMovement(ref Vector3 currentVelocityAir)
			{
				// Add move input
				if (_moveInputVector.sqrMagnitude > 0f)
				{
					Vector3 __addedVelocity = _moveInputVector * configs.airAccelerationSpeed * deltaTime;
				
					// Prevent air movement from making you move up steep sloped walls
					if (skateboard.GroundingStatus.FoundAnyGround)
					{
						Vector3 __perpendicularObstructionNormal = Vector3.Cross(Vector3.Cross(skateboard.CharacterUp, skateboard.GroundingStatus.GroundNormal), skateboard.CharacterUp).normalized;
						__addedVelocity = Vector3.ProjectOnPlane(__addedVelocity, __perpendicularObstructionNormal);
					}
				
					// Limit air movement from inputs to a certain maximum, without limiting the total air move speed from momentum, gravity or other forces
					Vector3 __resultingVelOnInputsPlane = Vector3.ProjectOnPlane(currentVelocityAir + __addedVelocity, skateboard.CharacterUp);
					
					if(__resultingVelOnInputsPlane.magnitude > configs.maxAirMoveSpeed && Vector3.Dot(_moveInputVector, __resultingVelOnInputsPlane) >= 0f)
					{
						__addedVelocity = Vector3.zero;
					}
					else
					{
						Vector3 __velOnInputsPlane = Vector3.ProjectOnPlane(currentVelocityAir, skateboard.CharacterUp);
						Vector3 __clampedResultingVelOnInputsPlane = Vector3.ClampMagnitude(__resultingVelOnInputsPlane, configs.maxAirMoveSpeed);
						__addedVelocity = __clampedResultingVelOnInputsPlane - __velOnInputsPlane;
					}
				
					currentVelocityAir += __addedVelocity;
				}
				
				// Gravity
				currentVelocityAir += _gravity * deltaTime;
				
				// Drag
				currentVelocityAir *= (1f / (1f + (configs.drag * deltaTime)));
			}

			void __HandleDefaultJumping(ref Vector3 currentVelocityJumping)
			{
				_jumpedThisFrame = false;
				_timeSinceJumpRequested += deltaTime;
					
				if (_jumpRequested)
				{
					// See if we actually are allowed to jump
					if (!_jumpConsumed && ((configs.allowJumpingWhenSliding 
											  ? skateboard.GroundingStatus.FoundAnyGround 
											  : skateboard.GroundingStatus.IsStableOnGround) 
										  || _timeSinceLastAbleToJump <= configs.jumpPostGroundingGraceTime))
					{
						// Calculate jump direction before un-grounding
						Vector3 __jumpDirection = skateboard.CharacterUp;
						if (skateboard.GroundingStatus.FoundAnyGround && !skateboard.GroundingStatus.IsStableOnGround)
						{
							__jumpDirection = skateboard.GroundingStatus.GroundNormal;
						}
				
						// Makes the character skip ground probing/snapping on its next update. 
						skateboard.ForceUnground();
				
						// Add to the return velocity and reset jump state
						currentVelocityJumping += (__jumpDirection * configs.jumpUpSpeed) - Vector3.Project(currentVelocityJumping, skateboard.CharacterUp);
						currentVelocityJumping += (_moveInputVector * configs.jumpScalableForwardSpeed);
						_jumpRequested = false;
						_jumpConsumed = true;
						_jumpedThisFrame = true;
					}
				}
				
				// Take into account additive velocity
				if (!(_internalVelocityAdd.sqrMagnitude > 0f)) return;
				
				currentVelocityJumping += _internalVelocityAdd;
				_internalVelocityAdd = Vector3.zero;
			}
			
			#endregion
		}
		
		[PublicAPI]
		public void PostGroundingUpdate(float deltaTime)
		{
			// Handle landing and leaving ground
			if (skateboard.GroundingStatus.IsStableOnGround && !skateboard.LastGroundingStatus.IsStableOnGround)
			{
				 //OnLanded();
			}
			else if (!skateboard.GroundingStatus.IsStableOnGround && skateboard.LastGroundingStatus.IsStableOnGround)
			{
				 //OnLeaveStableGround();
			}
		}
		
		#endregion
			
		///<summary> This is called every frame by the PlayerController in order to tell the character what its inputs are. </summary>
		[PublicAPI]
		public void SetCharacterInputs(ref PlayerCharacterInputs inputs)
		{
			Vector3 __moveInputVector = Vector3.ClampMagnitude(new Vector3(inputs.moveAxisRight, 0f, inputs.moveAxisForward), 1f);
			Vector3 __cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.cameraRotation * Vector3.forward, skateboard.CharacterUp).normalized;

			//Use the up Vector instead of forward. 
			__cameraPlanarDirection = (__cameraPlanarDirection.sqrMagnitude.Approximately(0f))
				? Vector3.ProjectOnPlane(inputs.cameraRotation * Vector3.up, skateboard.CharacterUp).normalized
				: __cameraPlanarDirection;
			
			Quaternion __cameraPlanarRotation = Quaternion.LookRotation(__cameraPlanarDirection, skateboard.CharacterUp);
			
			switch (CurrentCharacterState)
			{
				case CharacterState.Default:
				{
					_moveInputVector = __cameraPlanarRotation * __moveInputVector;

					_lookInputVector = _moveInputVector.normalized;

					if (inputs.jumpDown)
					{
						_timeSinceJumpRequested = 0f;
						_jumpRequested = true;
					}

					break;
				}
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		
		[PublicAPI]
		public bool IsColliderValidForCollisions(Collider coll)
		{
			if (configs.ignoredColliders.Count == 0) return true;
		
			return !configs.ignoredColliders.Contains(coll);
		}
		
		[PublicAPI]
		public void AddVelocity(Vector3 velocity)
		{
			switch (CurrentCharacterState)
			{
				case CharacterState.Default:
				{
					_internalVelocityAdd += velocity;
					break;
				}
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#endregion

		#region Debugging

		private void Debug()
		{
			//CGDebug.DrawRay(meshRoot.position, _gravity).Color(Color.green);
			
			//CGDebug.DrawRay()
		}

		#endregion
		
		#endregion
		*/
	}
}