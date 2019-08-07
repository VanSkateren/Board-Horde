using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class FixFirstSelected : MonoBehaviour
{
	public GameObject selected;

	private EventSystem es;

	private bool doSet;

	public FixFirstSelected()
	{
	}

	private void OnDisable()
	{
		if (this.es.currentSelectedGameObject != null)
		{
			this.selected = this.es.currentSelectedGameObject;
		}
	}

	private void OnEnable()
	{
		this.es = GameObject.Find("EventSystem").GetComponent<EventSystem>();
		this.es.SetSelectedGameObject(null);
		if (this.selected == null)
		{
			this.selected = this.es.firstSelectedGameObject;
		}
		this.doSet = true;
	}

	private void Update()
	{
		if (this.doSet)
		{
			this.es.SetSelectedGameObject(this.selected);
			Debug.Log(this.selected.name);
		}
		this.doSet = false;
	}
}