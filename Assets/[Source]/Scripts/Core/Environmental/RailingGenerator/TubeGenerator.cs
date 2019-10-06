using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UltEvents;

using CommonGames;
using CommonGames.Utilities;
using CommonGames.Utilities.Extensions;
using CommonGames.Utilities.CGTK;

#if UNITY_EDITOR
using UnityEditor;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using Sirenix.Serialization.Editor;
#endif

using Debug = CommonGames.Utilities.CGTK.CGDebug;

#endif

namespace Curve
{
	using CommonGames.Utilities.Extensions;

	public enum CurveType
	{
		CatmullRom
	};

	public partial class TubeGenerator : MonoBehaviour 
	{
		public UltEvent refresh;
		
		public List<Vector3> Points => points;

		[SerializeField] protected CurveType type = CurveType.CatmullRom;
		
		[SerializeField] protected List<Vector3> points;
		
		[SerializeField] protected float radiusSize = 0.1f;
		
		[SerializeField] protected bool 
			pointDebug = true, 
			tangentDebug = true, 
			frameDebug = false,
			indexDebug = true;

		public Curve Curve
		{
			get;
			private set;
		}
		
		private List<FrenetFrame> _frames;

		private void OnEnable() 
		{
			if(points.Count <= 0) 
			{
				points = new List<Vector3>() 
				{
					Vector3.zero,
					Vector3.up,
					Vector3.right
				};
			}
		}

		private void Start() => Initialize();

		public void AddPoint(Vector3 point) => points.Add(point);

		public void Initialize() => Curve = Build();

		public Curve Build() 
		{
			Curve __curve = default(Curve);
			switch(type) 
			{
				case CurveType.CatmullRom:
					__curve = new CatmullRomCurve(points);
					break;
				default:
					Debug.LogWarning("CurveType is not defined.");
					break;
			}

			Debug.Log($"Points Combined Distance = {points.CombinedDistanceInOrder().ToString()}");
			
			return __curve;
		}

		private void OnDrawGizmos() 
		{
			if(Curve == null) 
			{
				Initialize();
			}
			DrawGizmos();
		}

		private void DrawGizmos() 
		{
			const float __DELTA = 0.01f;
			float __deltaUnit = radiusSize * 2f;
			int __count = Mathf.FloorToInt(1f / __DELTA);

			if (_frames == null) 
			{
				_frames = Curve.ComputeFrenetFrames(__count, false);
			}

			Gizmos.matrix = transform.localToWorldMatrix;
			
			for (int __index = 0; __index < __count; __index++)
			{
				float __t = __index * __DELTA;
				Vector3 __point = Curve.GetPointAt(__t);

				if (pointDebug) 
				{
					Gizmos.color = Color.white;
					Gizmos.DrawSphere(__point, radiusSize);
				}

				if (tangentDebug) 
				{
					Vector3 __t1 = Curve.GetTangentAt(__t);
					Vector3 __n1 = (__t1 + Vector3.one) * 0.5f;
					
					Gizmos.color = new Color(__n1.x, __n1.y, __n1.z);
					Gizmos.DrawLine(__point, __point + __t1 * __deltaUnit);
				}

				if (indexDebug)
				{
					#if UNITY_EDITOR
					Handles.matrix = transform.localToWorldMatrix;
					Handles.color = Color.black;
					Handles.Label(__point, __index.ToString());
					#endif
				}

				if (!frameDebug) continue;
				
				FrenetFrame __frame = _frames[__index];

				Gizmos.color = Color.blue;
				Gizmos.DrawLine(__point, __point + __frame.Tangent * __deltaUnit);

				Gizmos.color = Color.green;
				Gizmos.DrawLine(__point, __point + __frame.Normal * __deltaUnit);

				Gizmos.color = Color.red;
				Gizmos.DrawLine(__point, __point + __frame.Binormal * __deltaUnit);
			}
		}

	}

	[CustomEditor (typeof(TubeGenerator))]
	public class TubeGeneratorEditor : OdinEditor 
	{
		private Quaternion _handle;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI ();

			if (!GUILayout.Button("Add")) return;
			
			TubeGenerator __tester = target as TubeGenerator;
			
			if (__tester == null) return;
			
			Vector3 __last = __tester.Points[__tester.Points.Count - 1];
			__tester.AddPoint(__last);
		}

		private void OnSceneGUI()
		{
			TubeGenerator __target = target as TubeGenerator;
			
			if (__target == null) return;
			
			List<Vector3> __points = __target.Points;

			Transform __testerTransform = __target.transform;
			
			_handle = Tools.pivotRotation == PivotRotation.Local ? __testerTransform.rotation : Quaternion.identity;
			
			Transform __transform = __testerTransform;

			for(int __index = 0, __n = __points.Count; __index < __n; __index++) 
			{
				Vector3 __point = __transform.TransformPoint(__points[__index]);
				
				EditorGUI.BeginChangeCheck();
				
				__point = Handles.DoPositionHandle(__point, _handle);

				if (!EditorGUI.EndChangeCheck()) continue;
				
				Undo.RecordObject(__target, "Point");
				
				__target.refresh?.Invoke();
				
				EditorUtility.SetDirty(__target);
				__points[__index] = __transform.InverseTransformPoint(__point);

				__target.Initialize();
			}
		}

	}
	
}