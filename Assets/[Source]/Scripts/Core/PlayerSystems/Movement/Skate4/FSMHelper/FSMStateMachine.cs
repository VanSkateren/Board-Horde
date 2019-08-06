using Dreamteck.Splines;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FSMHelper
{
	public class FSMStateMachine
	{
		private FSMStateMachineLogic m_Logic;

		public FSMStateMachine()
		{
		}

		public string ActiveStateTreeString()
		{
			string str = this.ToString();
			str = string.Concat(str, this.m_Logic.GetActiveStateTreeText(0));
			str = str.Remove(0, 34);
			str = str.Substring(0, str.Length - 1);
			return str;
		}

		public virtual void BothTriggersReleasedSM(InputController.TurningMode turningMode)
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.BothTriggersReleased(turningMode);
			}
		}

		public void BroadcastMessage(object[] args)
		{
			this.m_Logic.ReceiveMessage(args);
		}

		public virtual bool CanGrindSM()
		{
			if (this.m_Logic == null)
			{
				return false;
			}
			return this.m_Logic.CanGrind();
		}

		public virtual bool CapsuleEnabledSM()
		{
			if (this.m_Logic == null)
			{
				return false;
			}
			return this.m_Logic.CapsuleEnabled();
		}

		public virtual void FixedUpdateSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.FixedUpdate();
			}
		}

		public virtual float GetAugmentedAngleSM(StickInput p_stick)
		{
			if (this.m_Logic == null)
			{
				return 0f;
			}
			return this.m_Logic.GetAugmentedAngle(p_stick);
		}

		public virtual StickInput GetPopStickSM()
		{
			if (this.m_Logic == null)
			{
				return null;
			}
			return this.m_Logic.GetPopStick();
		}

		public virtual bool IsCurrentSplineSM(SplineComputer p_spline)
		{
			if (this.m_Logic == null)
			{
				return false;
			}
			return this.m_Logic.IsCurrentSpline(p_spline);
		}

		public virtual bool IsGrindingSM()
		{
			if (this.m_Logic == null)
			{
				return false;
			}
			return this.m_Logic.IsGrinding();
		}

		public virtual bool IsInImpactStateSM()
		{
			if (this.m_Logic == null)
			{
				return false;
			}
			return this.m_Logic.IsInImpactState();
		}

		public bool IsInState(Type state)
		{
			return this.m_Logic.IsInState(state);
		}

		public virtual bool IsOnGroundStateSM()
		{
			if (this.m_Logic == null)
			{
				return false;
			}
			return this.m_Logic.IsOnGroundState();
		}

		public virtual bool IsPushingSM()
		{
			if (this.m_Logic == null)
			{
				return false;
			}
			return this.m_Logic.IsPushing();
		}

		public virtual bool LeftFootOffSM()
		{
			if (this.m_Logic == null)
			{
				return false;
			}
			return this.m_Logic.LeftFootOff();
		}

		public virtual void LeftTriggerHeldSM(float value, InputController.TurningMode turningMode)
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.LeftTriggerHeld(value, turningMode);
			}
		}

		public virtual void LeftTriggerPressedSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.LeftTriggerPressed();
			}
		}

		public virtual void LeftTriggerReleasedSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.LeftTriggerReleased();
			}
		}

		public virtual void OnAllWheelsDownSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnAllWheelsDown();
			}
		}

		public virtual void OnAnimatorUpdateSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnAnimatorUpdate();
			}
		}

		public virtual void OnBailedSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnBailed();
			}
		}

		public virtual void OnBoardSeparatedFromTargetSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnBoardSeparatedFromTarget();
			}
		}

		public virtual void OnBrakeHeldSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnBrakeHeld();
			}
		}

		public virtual void OnBrakePressedSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnBrakePressed();
			}
		}

		public virtual void OnBrakeReleasedSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnBrakeReleased();
			}
		}

		public virtual void OnCanManualSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnCanManual();
			}
		}

		public virtual void OnCollisionEnterEventSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnCollisionEnterEvent();
			}
		}

		public virtual void OnCollisionExitEventSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnCollisionExitEvent();
			}
		}

		public virtual void OnCollisionStayEventSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnCollisionStayEvent();
			}
		}

		public virtual void OnEndImpactSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnEndImpact();
			}
		}

		public virtual void OnFirstWheelDownSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnFirstWheelDown();
			}
		}

		public virtual void OnFirstWheelUpSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnFirstWheelUp();
			}
		}

		public virtual void OnFlipStickCenteredSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnFlipStickCentered();
			}
		}

		public virtual void OnFlipStickUpdateSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnFlipStickUpdate();
			}
		}

		public virtual void OnForcePopSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnForcePop();
			}
		}

		public virtual void OnGrindDetectedSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnGrindDetected();
			}
		}

		public virtual void OnGrindEndedSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnGrindEnded();
			}
		}

		public virtual void OnGrindStaySM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnGrindStay();
			}
		}

		public virtual void OnImpactUpdateSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnImpactUpdate();
			}
		}

		public virtual void OnLeftStickCenteredUpdateSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnLeftStickCenteredUpdate();
			}
		}

		public virtual void OnManualEnterSM(StickInput popStick, StickInput flipStick)
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnManualEnter(popStick, flipStick);
			}
		}

		public virtual void OnManualExitSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnManualExit();
			}
		}

		public virtual void OnManualUpdateSM(StickInput popStick, StickInput flipStick)
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnManualUpdate(popStick, flipStick);
			}
		}

		public virtual void OnNextStateSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnNextState();
			}
		}

		public virtual void OnNoseManualEnterSM(StickInput popStick, StickInput flipStick)
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnNoseManualEnter(popStick, flipStick);
			}
		}

		public virtual void OnNoseManualExitSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnNoseManualExit();
			}
		}

		public virtual void OnNoseManualUpdateSM(StickInput popStick, StickInput flipStick)
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnNoseManualUpdate(popStick, flipStick);
			}
		}

		public virtual void OnPopStickCenteredSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnPopStickCentered();
			}
		}

		public virtual void OnPopStickUpdateSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnPopStickUpdate();
			}
		}

		public virtual void OnPredictedCollisionEventSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnPredictedCollisionEvent();
			}
		}

		public virtual void OnPreLandingEventSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnPreLandingEvent();
			}
		}

		public virtual void OnPushButtonHeldSM(bool mongo)
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnPushButtonHeld(mongo);
			}
		}

		public virtual void OnPushButtonPressedSM(bool mongo)
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnPushButtonPressed(mongo);
			}
		}

		public virtual void OnPushButtonReleasedSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnPushButtonReleased();
			}
		}

		public virtual void OnPushEndSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnPushEnd();
			}
		}

		public virtual void OnPushLastCheckSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnPushLastCheck();
			}
		}

		public virtual void OnPushSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnPush();
			}
		}

		public virtual void OnRespawnSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnRespawn();
			}
		}

		public virtual void OnRightStickCenteredUpdateSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnRightStickCenteredUpdate();
			}
		}

		public virtual void OnStickFixedUpdateSM(StickInput stick1, StickInput stick2)
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnStickFixedUpdate(stick1, stick2);
			}
		}

		public virtual void OnStickPressedSM(bool right)
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnStickPressed(right);
			}
		}

		public virtual void OnStickUpdateSM(StickInput stick1, StickInput stick2)
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnStickUpdate(stick1, stick2);
			}
		}

		public virtual void OnWheelsLeftGroundSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.OnWheelsLeftGround();
			}
		}

		public virtual bool PoppedSM()
		{
			if (this.m_Logic == null)
			{
				return false;
			}
			return this.m_Logic.Popped();
		}

		public void PrintActiveStateTree()
		{
			Debug.Log(string.Concat(string.Concat(this.ToString(), "\n"), this.m_Logic.GetActiveStateTreeText(0)));
		}

		public virtual bool RightFootOffSM()
		{
			if (this.m_Logic == null)
			{
				return false;
			}
			return this.m_Logic.RightFootOff();
		}

		public virtual void RightTriggerHeldSM(float value, InputController.TurningMode turningMode)
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.RightTriggerHeld(value, turningMode);
			}
		}

		public virtual void RightTriggerPressedSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.RightTriggerPressed();
			}
		}

		public virtual void RightTriggerReleasedSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.RightTriggerReleased();
			}
		}

		public virtual void SendEventBeginPopSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.SendEventBeginPop();
			}
		}

		public virtual void SendEventEndFlipPeriodSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.SendEventEndFlipPeriod();
			}
		}

		public virtual void SendEventExtendSM(float value)
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.SendEventExtend(value);
			}
		}

		public virtual void SendEventPopSM(float value)
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.SendEventPop(value);
			}
		}

		public virtual void SendEventReleasedSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.SendEventReleased();
			}
		}

		public virtual void SetSplineSM(SplineComputer p_spline)
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.SetSpline(p_spline);
			}
		}

		public virtual void SetupDefinition(ref FSMStateType stateType, ref List<Type> children)
		{
		}

		public virtual void StartSM()
		{
			FSMStateType fSMStateType = FSMStateType.Type_OR;
			List<Type> types = new List<Type>();
			this.SetupDefinition(ref fSMStateType, ref types);
			this.m_Logic = new FSMStateMachineLogic(fSMStateType, types, this, null);
			this.m_Logic.Enter(null);
		}

		public virtual void StopSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.Exit();
				this.m_Logic = null;
			}
		}

		public virtual void UpdateSM()
		{
			if (this.m_Logic != null)
			{
				this.m_Logic.Update();
			}
		}
	}
}