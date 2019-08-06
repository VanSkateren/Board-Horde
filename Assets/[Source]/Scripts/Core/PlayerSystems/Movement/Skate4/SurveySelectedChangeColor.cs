using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SurveySelectedChangeColor : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	public Text text;

	private Color startColor;

	public Color newColor;

	private Texture startTexture;

	public Texture selectedArrow;

	public SurveySelectedChangeColor()
	{
	}

	public void OnDeselect(BaseEventData eventData)
	{
		this.text.color = this.startColor;
		eventData.selectedObject.GetComponent<RawImage>().texture = this.startTexture;
	}

	void UnityEngine.EventSystems.ISelectHandler.OnSelect(BaseEventData eventData)
	{
		this.startTexture = eventData.selectedObject.GetComponent<RawImage>().texture;
		eventData.selectedObject.GetComponent<RawImage>().texture = this.selectedArrow;
		this.startColor = this.text.color;
		this.text.color = this.newColor;
	}
}