using Dreamteck.Splines;
using System;
using System.Collections.Generic;

namespace FSMHelper
{
	public class FSMStateMachineLogic
	{
		private BaseFSMState m_State;

		private FSMStateMachine m_OwnerSM;

		private FSMStateMachineLogic m_Parent;

		private List<FSMStateMachineLogic> m_ChildSMs = new List<FSMStateMachineLogic>();

		private Type m_StateClass;

		private FSMStateType m_StateType;

		private List<Type> m_ChildrenTypes;

		public FSMStateMachineLogic(FSMStateType stateType, List<Type> childrenTypes, FSMStateMachine ownerSM, FSMStateMachineLogic parent)
		{
			this.m_StateClass = null;
			this.m_StateType = stateType;
			this.m_ChildrenTypes = childrenTypes;
			this.m_Parent = parent;
			this.m_OwnerSM = ownerSM;
		}

		public FSMStateMachineLogic(Type stateClass, FSMStateMachine ownerSM, FSMStateMachineLogic parent)
		{
			this.m_ChildrenTypes = new List<Type>();
			this.m_StateClass = stateClass;
			this.m_OwnerSM = ownerSM;
			this.m_Parent = parent;
		}

		public void BothTriggersReleased(InputController.TurningMode turningMode)
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].BothTriggersReleased(turningMode);
			}
			if (this.m_State != null)
			{
				this.m_State.BothTriggersReleased(turningMode);
			}
		}

		public void BroadcastMessage(object[] args)
		{
			this.m_OwnerSM.BroadcastMessage(args);
		}

		public bool CanGrind()
		{
			bool flag = false;
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				flag = this.m_ChildSMs[i].CanGrind();
			}
			if (this.m_State != null)
			{
				flag = this.m_State.CanGrind();
			}
			return flag;
		}

		public bool CapsuleEnabled()
		{
			bool flag = false;
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				flag = this.m_ChildSMs[i].CapsuleEnabled();
			}
			if (this.m_State != null)
			{
				flag = this.m_State.CapsuleEnabled();
			}
			return flag;
		}

		public bool DoTransition(Type nextState, object[] args)
		{
			if (this.m_Parent == null)
			{
				return false;
			}
			return this.m_Parent.RequestChildTransition(this, nextState, args);
		}

		public void Enter(object[] args)
		{
			if (this.m_StateClass != null)
			{
				this.m_State = (BaseFSMState)Activator.CreateInstance(this.m_StateClass, args);
				this.m_State._InternalSetOwnerLogic(this);
				this.m_State.SetupDefinition(ref this.m_StateType, ref this.m_ChildrenTypes);
				this.m_State.Enter();
			}
			for (int i = 0; i < this.m_ChildrenTypes.Count; i++)
			{
				FSMStateMachineLogic fSMStateMachineLogic = new FSMStateMachineLogic(this.m_ChildrenTypes[i], this.m_OwnerSM, this);
				this.m_ChildSMs.Add(fSMStateMachineLogic);
				fSMStateMachineLogic.Enter(null);
				if (this.m_StateType == FSMStateType.Type_OR)
				{
					break;
				}
			}
		}

		public void Exit()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].Exit();
			}
			if (this.m_State != null)
			{
				this.m_State.Exit();
			}
			this.m_OwnerSM = null;
			this.m_Parent = null;
			this.m_State = null;
			this.m_ChildSMs.Clear();
		}

		public void FixedUpdate()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].FixedUpdate();
			}
			if (this.m_State != null)
			{
				this.m_State.FixedUpdate();
			}
		}

		public string GetActiveStateTreeText(int level)
		{
			string str = "";
			if (this.m_State != null)
			{
				for (int i = 0; i < level * 4; i++)
				{
					str = string.Concat(str, " ");
				}
				str = string.Concat(str, this.m_State.ToString());
				str = string.Concat(str, "\n");
			}
			for (int j = 0; j < this.m_ChildSMs.Count; j++)
			{
				str = string.Concat(str, this.m_ChildSMs[j].GetActiveStateTreeText(level + 1));
			}
			return str;
		}

		public float GetAugmentedAngle(StickInput p_stick)
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				float augmentedAngle = this.m_ChildSMs[i].GetAugmentedAngle(p_stick);
				if (augmentedAngle != 0f)
				{
					return augmentedAngle;
				}
			}
			if (this.m_State == null)
			{
				return 0f;
			}
			return this.m_State.GetAugmentedAngle(p_stick);
		}

		public BaseFSMState GetParentState()
		{
			if (this.m_Parent == null)
			{
				return null;
			}
			return this.m_Parent.m_State;
		}

		public StickInput GetPopStick()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				StickInput popStick = this.m_ChildSMs[i].GetPopStick();
				if (popStick)
				{
					return popStick;
				}
			}
			if (this.m_State == null)
			{
				return null;
			}
			return this.m_State.GetPopStick();
		}

		public FSMStateMachine GetStateMachine()
		{
			return this.m_OwnerSM;
		}

		public bool IsCurrentSpline(SplineComputer p_spline)
		{
			int num = 0;
			if (num < this.m_ChildSMs.Count)
			{
				return this.m_ChildSMs[num].IsCurrentSpline(p_spline);
			}
			if (this.m_State == null)
			{
				return false;
			}
			return this.m_State.IsCurrentSpline(p_spline);
		}

		public bool IsGrinding()
		{
			bool flag = false;
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				flag = this.m_ChildSMs[i].IsGrinding();
			}
			if (this.m_State != null)
			{
				flag = this.m_State.IsGrinding();
			}
			return flag;
		}

		public bool IsInImpactState()
		{
			bool flag = false;
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				flag = this.m_ChildSMs[i].IsInImpactState();
			}
			if (this.m_State != null)
			{
				flag = this.m_State.IsInImpactState();
			}
			return flag;
		}

		public bool IsInState(Type state)
		{
			if (this.m_StateClass == state)
			{
				return true;
			}
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				if (this.m_ChildSMs[i].IsInState(state))
				{
					return true;
				}
			}
			return false;
		}

		public bool IsOnGroundState()
		{
			bool flag = false;
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				flag = this.m_ChildSMs[i].IsOnGroundState();
			}
			if (this.m_State != null)
			{
				flag = this.m_State.IsOnGroundState();
			}
			return flag;
		}

		public bool IsPushing()
		{
			bool flag = false;
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				flag = this.m_ChildSMs[i].IsPushing();
			}
			if (this.m_State != null)
			{
				flag = this.m_State.IsPushing();
			}
			return flag;
		}

		public bool LeftFootOff()
		{
			bool flag = false;
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				flag = this.m_ChildSMs[i].LeftFootOff();
			}
			if (this.m_State != null)
			{
				flag = this.m_State.LeftFootOff();
			}
			return flag;
		}

		public void LeftTriggerHeld(float value, InputController.TurningMode turningMode)
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].LeftTriggerHeld(value, turningMode);
			}
			if (this.m_State != null)
			{
				this.m_State.LeftTriggerHeld(value, turningMode);
			}
		}

		public void LeftTriggerPressed()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].LeftTriggerPressed();
			}
			if (this.m_State != null)
			{
				this.m_State.LeftTriggerPressed();
			}
		}

		public void LeftTriggerReleased()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].LeftTriggerReleased();
			}
			if (this.m_State != null)
			{
				this.m_State.LeftTriggerReleased();
			}
		}

		public void OnAllWheelsDown()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnAllWheelsDown();
			}
			if (this.m_State != null)
			{
				this.m_State.OnAllWheelsDown();
			}
		}

		public void OnAnimatorUpdate()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnAnimatorUpdate();
			}
			if (this.m_State != null)
			{
				this.m_State.OnAnimatorUpdate();
			}
		}

		public void OnBailed()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnBailed();
			}
			if (this.m_State != null)
			{
				this.m_State.OnBailed();
			}
		}

		public void OnBoardSeparatedFromTarget()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnBoardSeparatedFromTarget();
			}
			if (this.m_State != null)
			{
				this.m_State.OnBoardSeparatedFromTarget();
			}
		}

		public void OnBrakeHeld()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnBrakeHeld();
			}
			if (this.m_State != null)
			{
				this.m_State.OnBrakeHeld();
			}
		}

		public void OnBrakePressed()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnBrakePressed();
			}
			if (this.m_State != null)
			{
				this.m_State.OnBrakePressed();
			}
		}

		public void OnBrakeReleased()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnBrakeReleased();
			}
			if (this.m_State != null)
			{
				this.m_State.OnBrakeReleased();
			}
		}

		public void OnCanManual()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnCanManual();
			}
			if (this.m_State != null)
			{
				this.m_State.OnCanManual();
			}
		}

		public void OnCollisionEnterEvent()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnCollisionEnterEvent();
			}
			if (this.m_State != null)
			{
				this.m_State.OnCollisionEnterEvent();
			}
		}

		public void OnCollisionExitEvent()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnCollisionExitEvent();
			}
			if (this.m_State != null)
			{
				this.m_State.OnCollisionExitEvent();
			}
		}

		public void OnCollisionStayEvent()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnCollisionStayEvent();
			}
			if (this.m_State != null)
			{
				this.m_State.OnCollisionStayEvent();
			}
		}

		public void OnEndImpact()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnEndImpact();
			}
			if (this.m_State != null)
			{
				this.m_State.OnEndImpact();
			}
		}

		public void OnFirstWheelDown()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnFirstWheelDown();
			}
			if (this.m_State != null)
			{
				this.m_State.OnFirstWheelDown();
			}
		}

		public void OnFirstWheelUp()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnFirstWheelUp();
			}
			if (this.m_State != null)
			{
				this.m_State.OnFirstWheelUp();
			}
		}

		public void OnFlipStickCentered()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnFlipStickCentered();
			}
			if (this.m_State != null)
			{
				this.m_State.OnFlipStickCentered();
			}
		}

		public void OnFlipStickUpdate()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnFlipStickUpdate();
			}
			if (this.m_State != null)
			{
				this.m_State.OnFlipStickUpdate();
			}
		}

		public void OnForcePop()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnForcePop();
			}
			if (this.m_State != null)
			{
				this.m_State.OnForcePop();
			}
		}

		public void OnGrindDetected()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnGrindDetected();
			}
			if (this.m_State != null)
			{
				this.m_State.OnGrindDetected();
			}
		}

		public void OnGrindEnded()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnGrindEnded();
			}
			if (this.m_State != null)
			{
				this.m_State.OnGrindEnded();
			}
		}

		public void OnGrindStay()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnGrindStay();
			}
			if (this.m_State != null)
			{
				this.m_State.OnGrindStay();
			}
		}

		public void OnImpactUpdate()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnImpactUpdate();
			}
			if (this.m_State != null)
			{
				this.m_State.OnImpactUpdate();
			}
		}

		public void OnLeftStickCenteredUpdate()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnLeftStickCenteredUpdate();
			}
			if (this.m_State != null)
			{
				this.m_State.OnLeftStickCenteredUpdate();
			}
		}

		public void OnManualEnter(StickInput popStick, StickInput flipStick)
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnManualEnter(popStick, flipStick);
			}
			if (this.m_State != null)
			{
				this.m_State.OnManualEnter(popStick, flipStick);
			}
		}

		public void OnManualExit()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnManualExit();
			}
			if (this.m_State != null)
			{
				this.m_State.OnManualExit();
			}
		}

		public void OnManualUpdate(StickInput popStick, StickInput flipStick)
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnManualUpdate(popStick, flipStick);
			}
			if (this.m_State != null)
			{
				this.m_State.OnManualUpdate(popStick, flipStick);
			}
		}

		public void OnNextState()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnNextState();
			}
			if (this.m_State != null)
			{
				this.m_State.OnNextState();
			}
		}

		public void OnNoseManualEnter(StickInput popStick, StickInput flipStick)
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnNoseManualEnter(popStick, flipStick);
			}
			if (this.m_State != null)
			{
				this.m_State.OnNoseManualEnter(popStick, flipStick);
			}
		}

		public void OnNoseManualExit()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnNoseManualExit();
			}
			if (this.m_State != null)
			{
				this.m_State.OnNoseManualExit();
			}
		}

		public void OnNoseManualUpdate(StickInput popStick, StickInput flipStick)
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnNoseManualUpdate(popStick, flipStick);
			}
			if (this.m_State != null)
			{
				this.m_State.OnNoseManualUpdate(popStick, flipStick);
			}
		}

		public void OnPopStickCentered()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnPopStickCentered();
			}
			if (this.m_State != null)
			{
				this.m_State.OnPopStickCentered();
			}
		}

		public void OnPopStickUpdate()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnPopStickUpdate();
			}
			if (this.m_State != null)
			{
				this.m_State.OnPopStickUpdate();
			}
		}

		public void OnPredictedCollisionEvent()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnPredictedCollisionEvent();
			}
			if (this.m_State != null)
			{
				this.m_State.OnPredictedCollisionEvent();
			}
		}

		public void OnPreLandingEvent()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnPreLandingEvent();
			}
			if (this.m_State != null)
			{
				this.m_State.OnPreLandingEvent();
			}
		}

		public void OnPush()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnPush();
			}
			if (this.m_State != null)
			{
				this.m_State.OnPush();
			}
		}

		public void OnPushButtonHeld(bool mongo)
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnPushButtonHeld(mongo);
			}
			if (this.m_State != null)
			{
				this.m_State.OnPushButtonHeld(mongo);
			}
		}

		public void OnPushButtonPressed(bool mongo)
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnPushButtonPressed(mongo);
			}
			if (this.m_State != null)
			{
				this.m_State.OnPushButtonPressed(mongo);
			}
		}

		public void OnPushButtonReleased()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnPushButtonReleased();
			}
			if (this.m_State != null)
			{
				this.m_State.OnPushButtonReleased();
			}
		}

		public void OnPushEnd()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnPushEnd();
			}
			if (this.m_State != null)
			{
				this.m_State.OnPushEnd();
			}
		}

		public void OnPushLastCheck()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnPushLastCheck();
			}
			if (this.m_State != null)
			{
				this.m_State.OnPushLastCheck();
			}
		}

		public void OnRespawn()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnRespawn();
			}
			if (this.m_State != null)
			{
				this.m_State.OnRespawn();
			}
		}

		public void OnRightStickCenteredUpdate()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnRightStickCenteredUpdate();
			}
			if (this.m_State != null)
			{
				this.m_State.OnRightStickCenteredUpdate();
			}
		}

		public void OnStickFixedUpdate(StickInput stick1, StickInput stick2)
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnStickFixedUpdate(stick1, stick2);
			}
			if (this.m_State != null)
			{
				this.m_State.OnStickFixedUpdate(stick1, stick2);
			}
		}

		public void OnStickPressed(bool right)
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnStickPressed(right);
			}
			if (this.m_State != null)
			{
				this.m_State.OnStickPressed(right);
			}
		}

		public void OnStickUpdate(StickInput stick1, StickInput stick2)
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnStickUpdate(stick1, stick2);
			}
			if (this.m_State != null)
			{
				this.m_State.OnStickUpdate(stick1, stick2);
			}
		}

		public void OnWheelsLeftGround()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].OnWheelsLeftGround();
			}
			if (this.m_State != null)
			{
				this.m_State.OnWheelsLeftGround();
			}
		}

		public bool Popped()
		{
			bool flag = false;
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				flag = this.m_ChildSMs[i].Popped();
			}
			if (this.m_State != null)
			{
				flag = this.m_State.Popped();
			}
			return flag;
		}

		public void ReceiveMessage(object[] args)
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].ReceiveMessage(args);
			}
			if (this.m_State != null)
			{
				this.m_State.ReceiveMessage(args);
			}
		}

		public bool RequestChildTransition(FSMStateMachineLogic child, Type nextState, object[] args)
		{
			if (this.m_StateType == FSMStateType.Type_AND)
			{
				return false;
			}
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				if (this.m_ChildSMs[i] == child)
				{
					for (int j = 0; j < this.m_ChildrenTypes.Count; j++)
					{
						if (this.m_ChildrenTypes[j] == nextState)
						{
							this.m_ChildSMs[i].Exit();
							this.m_ChildSMs[i] = new FSMStateMachineLogic(nextState, this.m_OwnerSM, this);
							this.m_ChildSMs[i].Enter(args);
							return true;
						}
					}
					return false;
				}
			}
			return false;
		}

		public bool RightFootOff()
		{
			bool flag = false;
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				flag = this.m_ChildSMs[i].RightFootOff();
			}
			if (this.m_State != null)
			{
				flag = this.m_State.RightFootOff();
			}
			return flag;
		}

		public void RightTriggerHeld(float value, InputController.TurningMode turningMode)
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].RightTriggerHeld(value, turningMode);
			}
			if (this.m_State != null)
			{
				this.m_State.RightTriggerHeld(value, turningMode);
			}
		}

		public void RightTriggerPressed()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].RightTriggerPressed();
			}
			if (this.m_State != null)
			{
				this.m_State.RightTriggerPressed();
			}
		}

		public void RightTriggerReleased()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].RightTriggerReleased();
			}
			if (this.m_State != null)
			{
				this.m_State.RightTriggerReleased();
			}
		}

		public void SendEventBeginPop()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].SendEventBeginPop();
			}
			if (this.m_State != null)
			{
				this.m_State.SendEventBeginPop();
			}
		}

		public void SendEventEndFlipPeriod()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].SendEventEndFlipPeriod();
			}
			if (this.m_State != null)
			{
				this.m_State.SendEventEndFlipPeriod();
			}
		}

		public void SendEventExtend(float value)
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].SendEventExtend(value);
			}
			if (this.m_State != null)
			{
				this.m_State.SendEventExtend(value);
			}
		}

		public void SendEventPop(float value)
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].SendEventPop(value);
			}
			if (this.m_State != null)
			{
				this.m_State.SendEventPop(value);
			}
		}

		public void SendEventReleased()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].SendEventReleased();
			}
			if (this.m_State != null)
			{
				this.m_State.SendEventReleased();
			}
		}

		public void SetSpline(SplineComputer p_spline)
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].SetSpline(p_spline);
			}
			if (this.m_State != null)
			{
				this.m_State.SetSpline(p_spline);
			}
		}

		public void Update()
		{
			for (int i = 0; i < this.m_ChildSMs.Count; i++)
			{
				this.m_ChildSMs[i].Update();
			}
			if (this.m_State != null)
			{
				this.m_State.Update();
			}
		}
	}
}