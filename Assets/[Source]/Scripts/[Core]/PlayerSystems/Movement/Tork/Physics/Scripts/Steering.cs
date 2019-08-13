using UnityEngine;

namespace Adrenak.Tork
{
	[RequireComponent(typeof(Ackermann))]
	public class Steering : MonoBehaviour
	{
		public float range = 35;
		public float value;
		public float rate = 45;

		public Ackermann Ackermann { get; private set; }
		private float currentAngle;

		private void Awake()
		{
			Ackermann = GetComponent<Ackermann>();
		}

		private void Update()
		{
			float __destination = value * range;
			
			currentAngle = Mathf.MoveTowards(currentAngle, __destination, Time.deltaTime * rate);		
			currentAngle = Mathf.Clamp(currentAngle, -range, range);
			
			Ackermann.SetAngle(currentAngle);
		}
	}
}
