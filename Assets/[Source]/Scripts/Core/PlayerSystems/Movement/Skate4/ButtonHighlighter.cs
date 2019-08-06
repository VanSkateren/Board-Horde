using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHighlighter : MonoBehaviour
{
	private Button previousButton;

	[SerializeField]
	private float scaleAmount = 1.4f;

	[SerializeField]
	private GameObject defaultButton;

	public ButtonHighlighter()
	{
	}

	private void HighlightButton(Button butt)
	{
		butt.transform.localScale = new Vector3(this.scaleAmount, this.scaleAmount, this.scaleAmount);
	}

	private void OnDisable()
	{
		if (this.previousButton != null)
		{
			this.UnHighlightButton(this.previousButton);
		}
	}

	private void Start()
	{
		if (this.defaultButton != null)
		{
			EventSystem.current.SetSelectedGameObject(this.defaultButton);
		}
	}

	private void UnHighlightButton(Button butt)
	{
		butt.transform.localScale = new Vector3(1f, 1f, 1f);
	}

	private void Update()
	{
		GameObject gameObject = EventSystem.current.currentSelectedGameObject;
		if (gameObject == null)
		{
			return;
		}
		Button component = gameObject.GetComponent<Button>();
		if (component != null && component != this.previousButton && component.transform.name != "PauseButton")
		{
			this.HighlightButton(component);
		}
		if (this.previousButton != null && this.previousButton != component)
		{
			this.UnHighlightButton(this.previousButton);
		}
		this.previousButton = component;
	}
}