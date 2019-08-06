using System;
using UnityEngine;

public class CanvasAlphaTween : MonoBehaviour
{
	public CanvasGroup canv;

	public float tweenSpeed;

	public CanvasAlphaTween()
	{
	}

	private void Start()
	{
		if (this.canv != null)
		{
			LeanTween.alphaCanvas(this.canv, 0.3f, this.tweenSpeed).setLoopPingPong();
		}
	}

	private void Update()
	{
	}
}