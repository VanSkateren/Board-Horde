using System;
using UnityEngine;

public class DeckSounds : MonoBehaviour
{
	public float rollFilterRamp;

	public float grindVolMult;

	public float grindHitVolMult;

	public float grindVolMax;

	public AudioLowPassFilter mainListenerFilter;

	private static DeckSounds _instance;

	public Rigidbody board;

	public AudioClip musicClip;

	public AudioClip[] bumps;

	public AudioClip[] ollieScooped;

	public AudioClip[] ollieSlow;

	public AudioClip[] ollieFast;

	public AudioClip[] boardLand;

	public AudioClip[] boardImpacts;

	public AudioClip tutorialBoardImpact;

	public AudioClip[] bearingSounds;

	public AudioClip[] shoesBoardBackImpacts;

	public AudioClip[] shoesImpactGroundSole;

	public AudioClip[] shoesImpactGroundUpper;

	public AudioClip[] shoesMovementShort;

	public AudioClip[] shoesMovementLong;

	public AudioClip[] shoesPivotHeavy;

	public AudioClip[] shoesPivotLight;

	public AudioClip[] shoesPushImpact;

	public AudioClip[] shoesPushOff;

	public AudioClip[] concreteGrindGeneralStart;

	public AudioClip[] concreteGrindGeneralLoop;

	public AudioClip[] concreteGrindGeneralEnd;

	public AudioClip[] metalGrindGeneralStart;

	public AudioClip[] metalGrindGeneralLoop;

	public AudioClip[] metalGrindGeneralEnd;

	public AudioClip[] woodGrindGeneralStart;

	public AudioClip[] woodGrindGeneralLoop;

	public AudioClip[] woodGrindGeneralEnd;

	public AudioClip grassRollLoop;

	public AudioClip rollingSoundSlow;

	public AudioClip rollingSoundFast;

	public AudioSource musicSource;

	public AudioSource deckSource;

	public AudioSource shoesBoardHitSource;

	public AudioSource leftShoeHitSource;

	public AudioSource rightShoeHitSource;

	public AudioSource shoesScrapeSource;

	public AudioSource grindHitSource;

	public AudioSource grindLoopSource;

	public AudioSource bearingSource;

	public AudioSource wheelHitSource;

	public AudioSource wheelRollingLoopLowSource;

	public AudioSource wheelRollingLoopHighSource;

	public AudioLowPassFilter wheelRollingLoopLowFilter;

	public AudioLowPassFilter wheelRollingLoopHighFilter;

	private AudioSource[] _allSources;

	private bool _isMuted;

	public float landVolMult;

	private bool _boardImpactAllowed = true;

	private bool _wheelImpactAllowed = true;

	private bool _shoeBoardBackImpactAllowed = true;

	private bool _shoeGroundImpactAllowed = true;

	public float scoopMult;

	public DeckSounds.GrindState grindState;

	public float pushOffVolLow;

	public float pushOffVolHigh;

	public float _muteLerp;

	private float _rollingLowVolCache;

	private float _rollingHighVolCache;

	public float muteTimeStep = 0.02f;

	public static DeckSounds Instance
	{
		get
		{
			return DeckSounds._instance;
		}
	}

	static DeckSounds()
	{
	}

	public DeckSounds()
	{
	}

	private void Awake()
	{
		if (DeckSounds._instance == null)
		{
			DeckSounds._instance = this;
		}
		this._allSources = base.GetComponentsInChildren<AudioSource>();
	}

	public void DoBoardImpactGround(float vol)
	{
		if (this._boardImpactAllowed && vol > 0.01f)
		{
			if (vol < 0.1f)
			{
				vol = 0.1f;
			}
			if (vol > 0.6f)
			{
				vol = 0.6f;
			}
			this.PlayRandomOneShotFromArray(this.boardImpacts, this.deckSource, vol);
			this._boardImpactAllowed = false;
			base.Invoke("ResetBoardImpactAllowed", 0.3f);
		}
	}

	public void DoLandingSound(float vol)
	{
		vol *= this.landVolMult;
		this.PlayRandomOneShotFromArray(this.boardLand, this.deckSource, vol);
	}

	public void DoOllie(float scoop)
	{
		scoop = scoop * this.scoopMult / 30f;
		scoop = Mathf.Abs(scoop);
		float single = scoop;
		float single1 = 1f - single;
		this.PlayRandomOneShotFromArray(this.ollieScooped, this.deckSource, single * 0.7f);
		this.PlayRandomOneShotFromArray(this.ollieSlow, this.deckSource, single1 * 0.7f);
	}

	public void DoShoeFlipSlide()
	{
		this.PlayRandomFromArray(this.shoesMovementShort, this.shoesScrapeSource);
		this.shoesScrapeSource.volume = 0f;
	}

	public void DoShoeImpactBoardBack(float vol, bool isLeftShoe)
	{
		if (this._shoeBoardBackImpactAllowed && vol > 0.01f)
		{
			if (vol < 0.1f)
			{
				vol = 0.1f;
			}
			if (vol > 0.6f)
			{
				vol = 0.6f;
			}
			this.PlayRandomOneShotFromArray(this.shoesBoardBackImpacts, (isLeftShoe ? this.leftShoeHitSource : this.rightShoeHitSource), vol);
			this._shoeBoardBackImpactAllowed = false;
			base.Invoke("ResetShoeBoardBackImpactAllowed", 0.1f);
		}
	}

	public void DoShoeImpactGround(float vol, bool isLeftShoe)
	{
		if (this._shoeGroundImpactAllowed && vol > 0.01f)
		{
			if (vol < 0.05f)
			{
				vol = 0.05f;
			}
			if (vol > 0.45f)
			{
				vol = 0.45f;
			}
			this.PlayRandomOneShotFromArray(this.shoesImpactGroundSole, (isLeftShoe ? this.leftShoeHitSource : this.rightShoeHitSource), vol);
			this._shoeGroundImpactAllowed = false;
			base.Invoke("ResetShoeGroundImpactAllowed", 0.1f);
		}
	}

	public void DoShoesImpactBoardBack(float vol)
	{
		this.shoesBoardHitSource.volume = 1f;
		this.PlayRandomOneShotFromArray(this.shoesBoardBackImpacts, this.shoesBoardHitSource, vol);
	}

	private void DoSmoothMuteRolling()
	{
		this._muteLerp = this._muteLerp - this.muteTimeStep * 2.85f;
		if (this._muteLerp < 0f)
		{
			this._muteLerp = 0f;
		}
		this.wheelRollingLoopLowSource.volume = this._muteLerp * this._rollingLowVolCache;
		this.wheelRollingLoopHighSource.volume = this._muteLerp * this._rollingLowVolCache;
		if (this._muteLerp == 0f)
		{
			base.CancelInvoke();
			this.wheelRollingLoopLowSource.mute = true;
			this.wheelRollingLoopHighSource.mute = true;
		}
	}

	private void DoSmoothUnMuteRolling()
	{
		this._muteLerp += this.muteTimeStep;
		if (this._muteLerp > 1f)
		{
			this._muteLerp = 1f;
		}
		this.wheelRollingLoopLowSource.volume = this._muteLerp * this._rollingLowVolCache;
		this.wheelRollingLoopHighSource.volume = this._muteLerp * this._rollingHighVolCache;
		if (this._muteLerp == 1f)
		{
			base.CancelInvoke();
		}
	}

	public void DoTutorialBoardImpact(float vol)
	{
		this.deckSource.PlayOneShot(this.tutorialBoardImpact, vol);
	}

	public void DoWheelImpactGround(float vol)
	{
		if (this._wheelImpactAllowed && vol > 0.01f)
		{
			if (vol < 0.1f)
			{
				vol = 0.1f;
			}
			if (vol > 0.6f)
			{
				vol = 0.6f;
			}
			this.PlayRandomOneShotFromArray(this.boardImpacts, this.deckSource, vol);
			this._wheelImpactAllowed = false;
			base.Invoke("ResetWheelImpactAllowed", 0.1f);
		}
	}

	private void FixedUpdate()
	{
	}

	public void MuteAll()
	{
		AudioSource[] audioSourceArray = this._allSources;
		for (int i = 0; i < (int)audioSourceArray.Length; i++)
		{
			audioSourceArray[i].mute = true;
		}
		this._isMuted = true;
	}

	public void OnMetalGrind(float impactForce)
	{
		if (this.grindState == DeckSounds.GrindState.none)
		{
			Vector3 vector3 = Vector3.ProjectOnPlane(PlayerController.Instance.boardController.boardRigidbody.velocity, Vector3.up);
			this.SetGrindingVolFromBoardSpeed(vector3.magnitude);
			this.PlayRandomOneShotFromArray(this.metalGrindGeneralStart, this.grindHitSource, impactForce / 10f);
			this.PlayRandomFromArray(this.metalGrindGeneralLoop, this.grindLoopSource);
			this.grindState = DeckSounds.GrindState.metal;
		}
	}

	public void OnPivot()
	{
		this.PlayRandomOneShotFromArray(this.shoesPivotLight, this.shoesBoardHitSource, 1f);
	}

	public void OnPushImpact()
	{
		this.PlayRandomOneShotFromArray(this.shoesPushImpact, this.grindHitSource, UnityEngine.Random.Range(0.1f, 0.6f));
	}

	public void OnPushOff(float pushSpeed)
	{
		if (this.shoesScrapeSource.isPlaying)
		{
			this.ShoePushOffSetMinVol(pushSpeed / 15f);
			return;
		}
		this.shoesScrapeSource.volume = 0f;
		this.PlayRandomFromArray(this.shoesMovementShort, this.shoesScrapeSource);
		this.ShoePushOffSetMinVol(UnityEngine.Random.Range(this.pushOffVolLow, this.pushOffVolHigh));
	}

	public void OnSmoothConcreteGrind(float impactForce)
	{
		if (this.grindState == DeckSounds.GrindState.none)
		{
			Vector3 vector3 = Vector3.ProjectOnPlane(PlayerController.Instance.boardController.boardRigidbody.velocity, Vector3.up);
			this.SetGrindingVolFromBoardSpeed(vector3.magnitude);
			this.PlayRandomOneShotFromArray(this.concreteGrindGeneralStart, this.grindHitSource, impactForce / 10f);
			this.PlayRandomFromArray(this.concreteGrindGeneralLoop, this.grindLoopSource);
			this.grindState = DeckSounds.GrindState.concrete;
		}
	}

	public void OnWoodGrind(float impactForce)
	{
		if (this.grindState == DeckSounds.GrindState.none)
		{
			Vector3 vector3 = Vector3.ProjectOnPlane(PlayerController.Instance.boardController.boardRigidbody.velocity, Vector3.up);
			this.SetGrindingVolFromBoardSpeed(vector3.magnitude);
			this.PlayRandomOneShotFromArray(this.woodGrindGeneralStart, this.grindHitSource, impactForce / 10f);
			this.PlayRandomFromArray(this.woodGrindGeneralLoop, this.grindLoopSource);
			this.grindState = DeckSounds.GrindState.wood;
		}
	}

	private void PlayRandomFromArray(AudioClip[] array, AudioSource source)
	{
		int num = UnityEngine.Random.Range(0, (int)array.Length);
		source.clip = array[num];
		source.Play();
	}

	private void PlayRandomOneShotFromArray(AudioClip[] array, AudioSource source, float vol)
	{
		if (array == null || array.Length == 0)
		{
			return;
		}
		int num = UnityEngine.Random.Range(0, (int)array.Length);
		source.PlayOneShot(array[num], vol);
	}

	private void ResetBoardImpactAllowed()
	{
		this._boardImpactAllowed = true;
	}

	private void ResetShoeBoardBackImpactAllowed()
	{
		this._shoeBoardBackImpactAllowed = true;
	}

	private void ResetShoeGroundImpactAllowed()
	{
		this._shoeGroundImpactAllowed = true;
	}

	private void ResetWheelImpactAllowed()
	{
		this._wheelImpactAllowed = true;
	}

	public void SetGrindingVolFromBoardSpeed(float p_velocity)
	{
		float pVelocity = p_velocity * this.grindVolMult;
		if (pVelocity > this.grindVolMax)
		{
			pVelocity = this.grindVolMax;
		}
		this.grindLoopSource.volume = pVelocity;
	}

	public void SetRollingVolFromRPS(PhysicMaterial mat, float rps)
	{
		if (mat != null)
		{
			if (this.wheelRollingLoopLowSource.clip != this.rollingSoundSlow)
			{
				this.wheelRollingLoopLowSource.clip = this.rollingSoundSlow;
				this.wheelRollingLoopLowSource.Play();
			}
			if (this.wheelRollingLoopHighSource.clip != this.rollingSoundFast)
			{
				this.wheelRollingLoopHighSource.clip = this.rollingSoundFast;
				this.wheelRollingLoopHighSource.Play();
			}
		}
		float single = 0f;
		float single1 = 5f;
		float single2 = rps / 30f;
		float single3 = single2 * 0.4f;
		float single4 = 0f;
		if (single2 < 1f)
		{
			single4 = 0f;
		}
		else if (single2 >= single && single2 < single + single1)
		{
			single4 = (single2 - single) / single1;
		}
		else if (single2 >= single)
		{
			single4 = 1f;
		}
		this.wheelRollingLoopLowSource.volume = single3 * (1f - single4);
		this.wheelRollingLoopHighSource.volume = single3 * single4;
		this.wheelRollingLoopLowFilter.cutoffFrequency = single3 * 22000f * this.rollFilterRamp;
		this.wheelRollingLoopHighFilter.cutoffFrequency = this.wheelRollingLoopLowFilter.cutoffFrequency;
	}

	public void ShoeFlipFinish()
	{
		this.shoesScrapeSource.Stop();
		this.shoesScrapeSource.volume = 1f;
	}

	public void ShoeFlipSlideSetMinVol(float newVol)
	{
		if (newVol > this.shoesScrapeSource.volume)
		{
			this.shoesScrapeSource.volume = Mathf.Clamp(newVol, 0f, 0.7f);
		}
	}

	public void ShoePushOffFinish()
	{
		this.shoesScrapeSource.volume = 0f;
	}

	private void ShoePushOffSetMinVol(float newVol)
	{
		if (newVol > this.shoesScrapeSource.volume)
		{
			this.shoesScrapeSource.volume = Mathf.Clamp(newVol, 0f, 0.2f);
		}
	}

	public void SmoothMuteRolling()
	{
		this._rollingLowVolCache = this.wheelRollingLoopLowSource.volume;
		this._rollingHighVolCache = this.wheelRollingLoopHighSource.volume;
		base.CancelInvoke();
		base.InvokeRepeating("DoSmoothMuteRolling", 0f, this.muteTimeStep);
	}

	public void SmoothUnMuteRolling()
	{
		base.CancelInvoke();
		this.wheelRollingLoopLowSource.mute = false;
		this.wheelRollingLoopHighSource.mute = false;
		base.InvokeRepeating("DoSmoothUnMuteRolling", 0f, this.muteTimeStep);
	}

	public void StartBearingSound(float rps)
	{
		float single = rps / 30f * 0.4f;
		this.PlayRandomOneShotFromArray(this.bearingSounds, this.bearingSource, Mathf.Clamp(single, 0.03f, 0.6f));
	}

	public void StopBearingSounds()
	{
		this.bearingSource.Stop();
	}

	public void StopGrind(float exitForce)
	{
		this.grindLoopSource.Stop();
		float single = 1f;
		if (single > this.grindVolMax)
		{
			single = this.grindVolMax;
		}
		switch (this.grindState)
		{
			case DeckSounds.GrindState.wood:
			{
				this.PlayRandomOneShotFromArray(this.woodGrindGeneralEnd, this.grindHitSource, single * 0.6f);
				break;
			}
			case DeckSounds.GrindState.metal:
			{
				this.PlayRandomOneShotFromArray(this.metalGrindGeneralEnd, this.grindHitSource, single * 0.6f);
				break;
			}
			case DeckSounds.GrindState.concrete:
			{
				this.PlayRandomOneShotFromArray(this.concreteGrindGeneralEnd, this.grindHitSource, single * 0.6f);
				break;
			}
		}
		this.grindState = DeckSounds.GrindState.none;
	}

	public void UnMuteAll()
	{
		AudioSource[] audioSourceArray = this._allSources;
		for (int i = 0; i < (int)audioSourceArray.Length; i++)
		{
			audioSourceArray[i].mute = false;
		}
		this._isMuted = false;
	}

	public void Update()
	{
	}

	public enum GrindState
	{
		none,
		wood,
		metal,
		concrete
	}
}