using System;
using UnityEngine;

public class TurnOffTimer : MonoBehaviour
{
	private float startTime;

	private bool startTimer;

	private float timer;

	public float duration = 3f;

	public GameObject target;

	private bool didSet;

	public TurnOffTimer()
	{
	}

	public void StartTimer()
	{
		this.startTimer = true;
		this.startTime = Time.time;
	}

	private void Update()
	{
		this.timer += Time.deltaTime;
		if (this.startTimer && !this.didSet && this.timer - this.startTime > this.duration && PlayerPrefs.GetInt("PlaySessions") < 2)
		{
			this.target.SetActive(true);
			this.didSet = true;
		}
	}
}