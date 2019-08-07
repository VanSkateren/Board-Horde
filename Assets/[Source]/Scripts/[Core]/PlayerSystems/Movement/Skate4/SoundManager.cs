using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
	private static SoundManager _instance;

	public DeckSounds deckSounds;

	public PhysicMaterial mat;

	public float wheelRadius = 0.0275f;

	public Transform wheel1;

	public Transform wheel2;

	public Transform wheel3;

	public Transform wheel4;

	private float _rps;

	public static SoundManager Instance
	{
		get
		{
			return SoundManager._instance;
		}
	}

	public SoundManager()
	{
	}

	private void Awake()
	{
		if (!(SoundManager._instance != null) || !(SoundManager._instance != this))
		{
			SoundManager._instance = this;
			return;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void PlayBoardImpactGround(float p_vol)
	{
		this.deckSounds.DoBoardImpactGround(p_vol);
	}

	public void PlayCatchSound()
	{
		this.deckSounds.DoShoesImpactBoardBack(1f);
	}

	public void PlayGrindSound(int p_grindType, float p_velocity)
	{
		switch (p_grindType)
		{
			case 0:
			{
				this.deckSounds.OnSmoothConcreteGrind(p_velocity);
				return;
			}
			case 1:
			{
				this.deckSounds.OnWoodGrind(p_velocity);
				return;
			}
			case 2:
			{
				this.deckSounds.OnMetalGrind(p_velocity);
				return;
			}
			default:
			{
				return;
			}
		}
	}

	public void PlayLandingSound(float p_vol)
	{
		this.deckSounds.DoLandingSound(p_vol);
	}

	public void PlayPopSound(float p_scoopSpeed)
	{
		this.deckSounds.DoOllie(p_scoopSpeed);
	}

	public void PlayPushOff(float p_vel)
	{
		this.deckSounds.OnPushOff(p_vel);
		this.deckSounds.OnPushImpact();
	}

	public void PlayWheelImpactGround(float p_vol)
	{
		this.deckSounds.DoWheelImpactGround(p_vol);
	}

	private void RollWheels(float _rotationsPerSecond)
	{
		if (Vector3.Angle(PlayerController.Instance.boardController.boardTransform.forward, PlayerController.Instance.boardController.boardRigidbody.velocity.normalized) > 90f)
		{
			_rotationsPerSecond = -_rotationsPerSecond;
		}
		Quaternion quaternion = Quaternion.Euler(_rotationsPerSecond, 0f, 0f);
		this.wheel1.rotation = this.wheel1.rotation * quaternion;
		Quaternion quaternion1 = Quaternion.Euler(_rotationsPerSecond, 0f, 0f);
		this.wheel2.rotation = this.wheel2.rotation * quaternion1;
		Quaternion quaternion2 = Quaternion.Euler(_rotationsPerSecond, 0f, 0f);
		this.wheel3.rotation = this.wheel3.rotation * quaternion2;
		Quaternion quaternion3 = Quaternion.Euler(_rotationsPerSecond, 0f, 0f);
		this.wheel4.rotation = this.wheel4.rotation * quaternion3;
	}

	public void SetGrindVolume(float p_velocity)
	{
		this.deckSounds.SetGrindingVolFromBoardSpeed(p_velocity);
	}

	public void SetRollingVolumeFromRPS(PhysicMaterial p_mat, float p_vel)
	{
		this._rps = p_vel / (6.28318548f * this.wheelRadius);
		this.deckSounds.SetRollingVolFromRPS(p_mat, p_vel / (6.28318548f * this.wheelRadius));
		this.RollWheels(this._rps);
	}

	public void StartBearingSound(float p_vol)
	{
		this.deckSounds.StartBearingSound(p_vol / (6.28318548f * this.wheelRadius));
	}

	public void StopBearingSound()
	{
		this.deckSounds.StopBearingSounds();
	}

	public void StopGrindSound(float p_exitVelocity)
	{
		this.deckSounds.StopGrind(p_exitVelocity);
	}
}