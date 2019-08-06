using System;
using UnityEngine;

public class PlayerPrefsManager : MonoBehaviour
{
	private static PlayerPrefsManager _instance;

	public static PlayerPrefsManager Instance
	{
		get
		{
			return PlayerPrefsManager._instance;
		}
	}

	public PlayerPrefsManager()
	{
	}

	private void Awake()
	{
		if (!(PlayerPrefsManager._instance != null) || !(PlayerPrefsManager._instance != this))
		{
			PlayerPrefsManager._instance = this;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		this.IncrementPlaySessions();
	}

	private void IncrementPlaySessions()
	{
		PlayerPrefs.SetInt("PlaySessions", PlayerPrefs.GetInt("PlaySessions", 0) + 1);
		PlayerPrefs.SetInt("PixelSessions", PlayerPrefs.GetInt("PixelSessions", 0) + 1);
	}

	private void OnApplicationQuit()
	{
		if (PlayerPrefs.GetInt("PixelSessions") == 1)
		{
			Application.OpenURL("https://skaterxl.com/ownermailinglist");
		}
	}
}