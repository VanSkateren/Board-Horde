using System;
using UnityEngine;

public class PromptController : MonoBehaviour
{
	public GameObject catchPrompt;

	public int catchCount;

	public int StateChangeReleaseToBailCount;

	public int ReleaseToBailTriggerCount;

	private bool doPause;

	private bool doUnPause;

	//public Menu menuthing;

	private static PromptController _instance;

	public static PromptController Instance
	{
		get
		{
			return PromptController._instance;
		}
	}

	public PromptController()
	{
	}

	public void ActionTakenCatch()
	{
		this.catchCount++;
	}

	private void Awake()
	{
		if (!(PromptController._instance != null) || !(PromptController._instance != this))
		{
			PromptController._instance = this;
			return;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void DoPopupDelayed()
	{
		this.catchPrompt.SetActive(true);
		Time.timeScale = 0f;
		this.doPause = false;
		InputController.Instance.controlsActive = true;
	}

	public void DoUnPause()
	{
		if (Time.timeScale == 0f)
		{
			this.doPause = false;
			this.doUnPause = true;
			Time.timeScale = 0.05f;
		}
	}

	public void FixedUpdate()
	{
		if (this.doPause)
		{
			if (Time.timeScale > 0.1f)
			{
				Time.timeScale = Time.timeScale - Time.deltaTime * 6f;
				if (Time.timeScale < 0.1f)
				{
					Time.timeScale = 0.1f;
				}
			}
			else
			{
				base.Invoke("DoPopupDelayed", 0.005f);
				Time.timeScale = 0.1f;
			}
		}
		if (this.doUnPause)
		{
			Time.timeScale = Time.timeScale + Time.deltaTime * 6f;
			if (Time.timeScale >= 1f)
			{
				this.doUnPause = false;
				Time.timeScale = 1f;
				//this.menuthing.enabled = true;
			}
		}
	}

	public void StateChangePopToRelease()
	{
		this.StateChangeReleaseToBailCount++;
		int stateChangeReleaseToBailCount = this.StateChangeReleaseToBailCount;
		int releaseToBailTriggerCount = this.ReleaseToBailTriggerCount;
	}

	public void StateChangeReleaseToBail()
	{
	}
}