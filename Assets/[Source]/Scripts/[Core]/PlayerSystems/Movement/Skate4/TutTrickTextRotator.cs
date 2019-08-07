using System;
using UnityEngine;
using UnityEngine.UI;

public class TutTrickTextRotator : MonoBehaviour
{
	public string[] labels;

	public Text label;

	public int i;

	public TutTrickTextRotator()
	{
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.C))
		{
			this.i++;
			if (this.i == (int)this.labels.Length)
			{
				this.i = 0;
				Debug.Log("reset");
			}
			this.label.text = this.labels[this.i];
		}
	}
}