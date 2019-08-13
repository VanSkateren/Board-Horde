using UnityEngine;
using CommonGames.Utilities;

namespace Adrenak.Tork
{
	public class CenterOfMass : Singleton<CenterOfMass>
	{
		public Transform Point { get; set; }

		private Rigidbody _rigidbody;

		private void Start ()
		{
			_rigidbody = GetComponent<Rigidbody>();
			
			if (_rigidbody == null) return;
			
			_rigidbody.centerOfMass = Point.localPosition;
		}
	
	#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.red;

			if(Point != null)
				Gizmos.DrawSphere(Point.position, .1f);
		}
	#endif
	}
}
