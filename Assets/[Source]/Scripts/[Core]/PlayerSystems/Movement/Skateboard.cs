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
	public class Skateboard : Singleton<Skateboard>
	{
	    #region Variables

		[BoxGroup("Wheels")]
		
		[SerializeField] private Transform[] wheelPositions;
		
	    public Player Player { get; set; }
		
		/// <summary>
		/// The Transform of the skateboard
		/// </summary>
		[PublicAPI]
		public Transform Transform { get; }

		/// <summary>
		/// The Transform of the character motor
		/// </summary>
		[PublicAPI]
		public Rigidbody Rigidbody { get; }

		/// <summary>
		/// The character's goal position in its movement calculations (always up-to-date during the character update phase)
		/// </summary>
		[PublicAPI]
		public Vector3 TransientPosition { get; private set; }

		/// <summary>
		/// The character's up direction (always up-to-date during the character update phase)
		/// </summary>
		[PublicAPI]
		public Vector3 CharacterUp { get; }

		/// <summary>
		/// The character's forward direction (always up-to-date during the character update phase)
		/// </summary>
		[PublicAPI]
		public Vector3 CharacterForward { get; }

		/// <summary>
		/// The character's right direction (always up-to-date during the character update phase)
		/// </summary>
		[PublicAPI]
		public Vector3 CharacterRight { get; }

		#region State Info
		
		/// <summary>
		/// Represents the entire state of a character motor that is pertinent for simulation.
		/// Use this to save state or revert to past state
		/// </summary>
		[System.Serializable]
		public struct State
		{
			public Vector3 position;
			public Quaternion rotation;
			public Vector3 baseVelocity;

			public bool mustUnground;
			public float mustUngroundTime;
			public bool lastMovementIterationFoundAnyGround;
			public TransientGroundingReport groundingStatus;

			public Rigidbody attachedRigidbody;
			public Vector3 attachedRigidbodyVelocity;
		}

		/// <summary>
		/// Contains all the information for the motor's grounding status
		/// </summary>
		public struct GroundingReport
		{
			public bool foundAnyGround;
			public bool isStableOnGround;
			public bool snappingPrevented;
			public Vector3 groundNormal;
			public Vector3 innerGroundNormal;
			public Vector3 outerGroundNormal;

			public Collider groundCollider;
			public Vector3 groundPoint;

			public void CopyFrom(TransientGroundingReport transientGroundingReport)
			{
				foundAnyGround = transientGroundingReport.foundAnyGround;
				isStableOnGround = transientGroundingReport.isStableOnGround;
				snappingPrevented = transientGroundingReport.snappingPrevented;
				groundNormal = transientGroundingReport.groundNormal;
				innerGroundNormal = transientGroundingReport.innerGroundNormal;
				outerGroundNormal = transientGroundingReport.outerGroundNormal;

				groundCollider = null;
				groundPoint = Vector3.zero;
			}
		}
		
		/// <summary>
		/// Contains the simulation-relevant information for the motor's grounding status
		/// </summary>
		public struct TransientGroundingReport
		{
			public bool foundAnyGround;
			public bool isStableOnGround;
			public bool snappingPrevented;
			public Vector3 groundNormal;
			public Vector3 innerGroundNormal;
			public Vector3 outerGroundNormal;

			public void CopyFrom(GroundingReport groundingReport)
			{
				foundAnyGround = groundingReport.foundAnyGround;
				isStableOnGround = groundingReport.isStableOnGround;
				snappingPrevented = groundingReport.snappingPrevented;
				groundNormal = groundingReport.groundNormal;
				innerGroundNormal = groundingReport.innerGroundNormal;
				outerGroundNormal = groundingReport.outerGroundNormal;
			}
		}
		
		/// <summary>
		/// Contains the current grounding information
		/// </summary>
		[NonSerialized]
		public GroundingReport GroundingStatus = new GroundingReport();
		/// <summary>
		/// Contains the previous grounding information
		/// </summary>
		[NonSerialized]
		public TransientGroundingReport LastGroundingStatus = new TransientGroundingReport();
		
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
	    
	    #endregion
		
		#region Methods

		#region Public API

		/*
		public void AddPushForce(float force)
        {
        	if (this.boardRigidbody.velocity.magnitude < PlayerController.Instance.topSpeed)
        	{
        		if (this.boardRigidbody.velocity.magnitude >= 0.15f)
        		{
        			Rigidbody rigidbody = this.boardRigidbody;
        			Vector3 vector3 = this.boardRigidbody.velocity;
        			rigidbody.AddForce(vector3.normalized * p_value, ForceMode.Impulse);
        		}
        		else if (Vector3.Angle(PlayerController.Instance.PlayerForward(), Camera.main.transform.forward) >= 90f)
        		{
        			this.boardRigidbody.AddForce((-PlayerController.Instance.PlayerForward() * p_value) * 1.4f, ForceMode.Impulse);
        		}
        		else
        		{
        			this.boardRigidbody.AddForce((PlayerController.Instance.PlayerForward() * p_value) * 1.4f, ForceMode.Impulse);
        		}
        	}
        	SoundManager.Instance.PlayPushOff(0.01f);
        }
        */


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

					break;
				}
				default:
					throw new ArgumentOutOfRangeException();
			}

			#region Handling

			void __HandleDefaultCharacterRotation(ref Quaternion currentRotationDefault)
			{

				//Quaternion __fromRotation = Quaternion.LookRotation(motor.CharacterForward, motor.CharacterUp),
				//			 __toRotation = Quaternion.LookRotation(_lookInputVector, motor.CharacterUp);

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
					if (GroundingStatus.isStableOnGround)
					{
						//__HandleDefaultGroundMovement(ref currentVelocity);
					}
					else
					{
						//__HandleDefaultAirMovement(ref currentVelocity);
					}

					//__HandleDefaultJumping(ref currentVelocity);
					break;
				}
				default:
					throw new ArgumentOutOfRangeException();
			}

			/*
			
			#region Handling
			
			void __HandleDefaultGroundMovement(ref Vector3 currentVelocityGround)
			{	
				float __currentVelocityMagnitude = currentVelocityGround.magnitude;
				
				Vector3 __effectiveGroundNormal = GroundingStatus.groundNormal;
				
				if (__currentVelocityMagnitude > 0f && GroundingStatus.snappingPrevented)
				{
					// Take the normal from where we're coming from
					Vector3 __groundPointToCharacter = TransientPosition - GroundingStatus.groundPoint;
					
					__effectiveGroundNormal = (Vector3.Dot(currentVelocityGround, __groundPointToCharacter) >= 0f) 
						? motor.GroundingStatus.OuterGroundNormal 
						: motor.GroundingStatus.InnerGroundNormal;
				}
				
				// Reorient velocity on slope
				currentVelocityGround = GetDirectionTangentToSurface(currentVelocityGround, __effectiveGroundNormal) * __currentVelocityMagnitude;

				// Calculate target velocity
				Vector3 __inputRight = Vector3.Cross(_moveInputVector, motor.CharacterUp);
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
					if (motor.GroundingStatus.FoundAnyGround)
					{
						Vector3 __perpendicularObstructionNormal = Vector3.Cross(Vector3.Cross(motor.CharacterUp, motor.GroundingStatus.GroundNormal), motor.CharacterUp).normalized;
						__addedVelocity = Vector3.ProjectOnPlane(__addedVelocity, __perpendicularObstructionNormal);
					}
				
					// Limit air movement from inputs to a certain maximum, without limiting the total air move speed from momentum, gravity or other forces
					Vector3 __resultingVelOnInputsPlane = Vector3.ProjectOnPlane(currentVelocityAir + __addedVelocity, motor.CharacterUp);
					
					if(__resultingVelOnInputsPlane.magnitude > configs.maxAirMoveSpeed && Vector3.Dot(_moveInputVector, __resultingVelOnInputsPlane) >= 0f)
					{
						__addedVelocity = Vector3.zero;
					}
					else
					{
						Vector3 __velOnInputsPlane = Vector3.ProjectOnPlane(currentVelocityAir, motor.CharacterUp);
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
											  ? motor.GroundingStatus.FoundAnyGround 
											  : motor.GroundingStatus.IsStableOnGround) 
										  || _timeSinceLastAbleToJump <= configs.jumpPostGroundingGraceTime))
					{
						// Calculate jump direction before un-grounding
						Vector3 __jumpDirection = motor.CharacterUp;
						if (motor.GroundingStatus.FoundAnyGround && !motor.GroundingStatus.IsStableOnGround)
						{
							__jumpDirection = motor.GroundingStatus.GroundNormal;
						}
				
						// Makes the character skip ground probing/snapping on its next update. 
						motor.ForceUnground();
				
						// Add to the return velocity and reset jump state
						currentVelocityJumping += (__jumpDirection * configs.jumpUpSpeed) - Vector3.Project(currentVelocityJumping, motor.CharacterUp);
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
			
			*/
		}
		
		[PublicAPI]
		public void PostGroundingUpdate(float deltaTime)
		{
			// Handle landing and leaving ground
			if (GroundingStatus.isStableOnGround && !LastGroundingStatus.isStableOnGround)
			{
				 //OnLanded();
			}
			else if (!GroundingStatus.isStableOnGround && LastGroundingStatus.isStableOnGround)
			{
				 //OnLeaveStableGround();
			}
		}
		
		#endregion

		[PublicAPI]
		public bool IsColliderValidForCollisions(Collider coll)
		{
			if (Player.configs.ignoredColliders.Count == 0) return true;
		
			return !Player.configs.ignoredColliders.Contains(coll);
		}
		
		[PublicAPI]
		public void AddVelocity(Vector3 velocity)
		{
			switch (CurrentCharacterState)
			{
				case CharacterState.Default:
				{
					//_internalVelocityAdd += velocity;
					break;
				}
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		
		/// <summary>
		/// Sets the character's position directly
		/// </summary>
		public void SetPosition(Vector3 position, bool bypassInterpolation = true)
		{
			Transform.position = position;
			//_initialSimulationPosition = position;
			TransientPosition = position;

			if (bypassInterpolation)
			{
				//InitialTickPosition = position;
			}
		}
		/// <summary>
		/// Sets the character's rotation directly
		/// </summary>
		public void SetRotation(Quaternion rotation, bool bypassInterpolation = true)
		{
			Transform.rotation = rotation;
			//_initialSimulationRotation = rotation;
			//TransientRotation = rotation;

			if (bypassInterpolation)
			{
				//InitialTickRotation = rotation;
			}
		}
		/// <summary>
		/// Sets the character's position and rotation directly
		/// </summary>
		public void SetPositionAndRotation(Vector3 position, Quaternion rotation, bool bypassInterpolation = true)
		{
			Transform.SetPositionAndRotation(position, rotation);
			//_initialSimulationPosition = position;
			//_initialSimulationRotation = rotation;
			TransientPosition = position;
			//TransientRotation = rotation;

			if (bypassInterpolation)
			{
				//InitialTickPosition = position;
				//InitialTickRotation = rotation;
			}
		}

		#endregion
		
		#endregion
	}
}