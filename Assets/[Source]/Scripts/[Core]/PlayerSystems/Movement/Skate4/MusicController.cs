using System;
using UnityEngine;

public class MusicController : MonoBehaviour
{
	public AudioClip[] tracks;

	public AudioSource audioSource;

	private float startingVol;

	private static MusicController _instance;

	public static MusicController Instance
	{
		get
		{
			return MusicController._instance;
		}
	}

	public MusicController()
	{
	}

	private void Awake()
	{
		if (!(MusicController._instance != null) || !(MusicController._instance != this))
		{
			MusicController._instance = this;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		this.startingVol = this.audioSource.volume;
	}

	public void PlayGameMusic()
	{
		this.audioSource.clip = this.tracks[1];
		this.audioSource.Play();
		this.audioSource.volume = 0f;
		Debug.Log("play");
	}

	public void PlayTitleTrack()
	{
		this.audioSource.clip = this.tracks[0];
		this.audioSource.Play();
		this.audioSource.volume = this.startingVol;
	}
}