using System;
using UnityEngine;

[ExecuteInEditMode]
public class DistanceTool : MonoBehaviour
{
	public string distanceToolName = "";

	public Color lineColor = Color.yellow;

	public bool initialized;

	public string initialName = "Distance Tool";

	public Vector3 startPoint = Vector3.zero;

	public Vector3 endPoint = new Vector3(0f, 1f, 0f);

	public float distance;

	public float gizmoRadius = 0.1f;

	public bool scaleToPixels;

	public int pixelPerUnit = 128;

	public DistanceTool()
	{
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = this.lineColor;
		Gizmos.DrawWireSphere(this.startPoint, this.gizmoRadius);
		Gizmos.DrawWireSphere(this.endPoint, this.gizmoRadius);
		Gizmos.DrawLine(this.startPoint, this.endPoint);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = this.lineColor;
		Gizmos.DrawWireSphere(this.startPoint, this.gizmoRadius);
		Gizmos.DrawWireSphere(this.endPoint, this.gizmoRadius);
		Gizmos.DrawLine(this.startPoint, this.endPoint);
	}

	private void OnEnable()
	{
	}
}