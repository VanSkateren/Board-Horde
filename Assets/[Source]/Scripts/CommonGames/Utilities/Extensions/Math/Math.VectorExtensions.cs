using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CommonGames.Utilities.CGTK;

namespace CommonGames.Utilities.Extensions
{
	public static partial class Math
	{
		//TODO: Vector4
		
		#region Vector3
			
		//public static Vector3 MaxValue(this Vector3 vector) => new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);	
		
		public static Vector3 Closest(this IEnumerable<Vector3> vectors, Vector3 comparer)
		{
			Vector3 __closestVector = Vector3.positiveInfinity;
			float __closestDistance = float.MaxValue;

			foreach (Vector3 __vector in vectors)
			{
				float __distance = __vector.DistanceTo(comparer);

				if (!(__distance <= __closestDistance)) continue;
				
				__closestDistance = __distance;
				__closestVector = __vector;
			}
			
			return __closestVector;
		}
		
		public static float CombinedDistanceInOrder(this IEnumerable<Vector3> vectors)
		{
			float __combinedDistance = 0f;

			Vector3? __lastVector = null;
			foreach(Vector3 __vector in vectors) //Normal for loop wasn't possible because we use an IEnumerable instead of an array or list.
			{
				if(__lastVector == null)
				{
					__lastVector = __vector;
					continue;
				}

				__combinedDistance += __vector.DistanceTo((Vector3)__lastVector);

				__lastVector = __vector;
			}
			
			return __combinedDistance;
		}
		
		//public static Vector3 GetClosest(this List<Vector3> vectors, Vector3 comparer) => vectors.OrderBy(vector => Math.Abs(vector - comparer)).First();
		
		public static float DistanceTo(this Vector3 from, Vector3 to)
			=> Vector3.Distance(from, to);
		
		#region Rounding
		
		public static void Round(this Vector3 v) => v.Rounded();
		
		public static void Round(this Vector3 v, CGMath.RoundingMode roundingMode) => v.Rounded(roundingMode);

		public static void Floor(this Vector3 v) => v.Floored();

		public static void Ceil(this Vector3 v) => v.Ceiled();
		
		public static Vector3 Rounded(this Vector3 v) 
			=> new Vector3 {x = v.x.Round(), y = v.y.Round(), z = v.z.Round()};
		
		public static Vector3 Rounded(this Vector3 v, CGMath.RoundingMode roundingMode) 
			=> new Vector3 {x = v.x.Round(roundingMode), y = v.y.Round(roundingMode), z = v.z.Round(roundingMode)};
		
		public static Vector3Int RoundedToVector3Int(this Vector3 v)
			=> new Vector3Int {x = v.x.RoundToInt(), y = v.y.RoundToInt(), z = v.z.RoundToInt()};
		
		public static Vector3Int RoundedToVector3Int(this Vector3 v, CGMath.RoundingMode roundingMode)
			=> new Vector3Int {x = v.x.RoundToInt(), y = v.y.RoundToInt(), z = v.z.RoundToInt()};

		public static Vector3 Floored(this Vector3 v)
			=> new Vector3 {x = v.x.Floor(), y = v.y.Floor(), z = v.z.Floor()};
		
		public static Vector3Int FlooredToVector3Int(this Vector3 v)
			=> new Vector3Int {x = v.x.FloorToInt(), y = v.y.FloorToInt(), z = v.z.FloorToInt()};
		
		public static Vector3 Ceiled(this Vector3 v)
			=> new Vector3 {x = v.x.Ceil(), y = v.y.Ceil(), z = v.z.Ceil()};
		
		public static Vector3Int CeiledToVector3Int(this Vector3 v)
			=> new Vector3Int {x = v.x.CeilToInt(), y = v.y.CeilToInt(), z = v.z.CeilToInt()};
		
		#endregion
		
		public static Vector3 GetRelativePositionFrom(this Vector3 position, Matrix4x4 from)
			=> from.MultiplyPoint(position);

		public static Vector3 GetRelativePositionTo(this Vector3 position, Matrix4x4 to)
			=> to.inverse.MultiplyPoint(position);

		public static Vector3 GetRelativeDirectionFrom(this Vector3 direction, Matrix4x4 from)
			=> from.MultiplyVector(direction);

		public static Vector3 GetRelativeDirectionTo(this Vector3 direction, Matrix4x4 to)
			=> to.inverse.MultiplyVector(direction);
		
		/// <summary>
		/// Mirrors a Vector in desired  Axis
		/// </summary>
		/// <returns></returns>
		public static Vector3 Mirror(this Vector3 vector, Vector3 axis) //TODO: Edit to *actually* use an axis.
		{
			if (axis == Vector3.right) { vector.x *= -1f; }

			if (axis == Vector3.up) { vector.y *= -1f; }

			if (axis == Vector3.forward) { vector.z *= -1f; }

			return vector;
		}

		public static Vector3 ToAbs(ref this Vector3 vector)
		{
			vector.x.ToAbs();
			vector.y.ToAbs();
			vector.z.ToAbs();
			
			return vector;
		}
		
		public static Vector3 Abs(this Vector3 vector) 
			=> new Vector3(vector.x.ToAbs(), vector.y.ToAbs(), vector.z.ToAbs());

		public static bool Approximately(this Vector3 position, Vector3 comparer)
			=> position.x.Approximately(comparer.x) && position.y.Approximately(comparer.y) && position.z.Approximately(comparer.z);

		/*
		public static Vector3 Random(this Vector3 target, Vector3 minRange, Vector3 maxRange)
		{
			minRange = -minRange.Abs();
			minRange = maxRange.Abs();
			
			return new Vector3(CGRandom.RandomRange(minRange.x, maxRange.x), CGRandom.RandomRange(minRange.y, maxRange.y),CGRandom.RandomRange(minRange.z, maxRange.z));
		}
		*/
		
		//public static Vector3 Shake
		
		#endregion
		
		#region Vector2
		
		#region Rounding
		
		public static void Round(this Vector2 v) => v.Rounded();
		
		public static void Round(this Vector2 v, CGMath.RoundingMode roundingMode) => v.Rounded(roundingMode);

		public static void Floor(this Vector2 v) => v.Floored();

		public static void Ceil(this Vector2 v) => v.Ceiled();
		
		public static Vector2 Rounded(this Vector2 v) 
			=> new Vector2 {x = v.x.Round(), y = v.y.Round()};
		
		public static Vector2 Rounded(this Vector2 v, CGMath.RoundingMode roundingMode) 
			=> new Vector3 {x = v.x.Round(roundingMode), y = v.y.Round(roundingMode)};
		
		public static Vector2Int RoundedToVector3Int(this Vector2 v) 
			=> new Vector2Int {x = v.x.RoundToInt(), y = v.y.RoundToInt()};
		
		public static Vector2Int RoundedToVector3Int(this Vector2 v, CGMath.RoundingMode roundingMode) 
			=> new Vector2Int {x = v.x.RoundToInt(), y = v.y.RoundToInt()};

		public static Vector2 Floored(this Vector2 v) 
			=> new Vector2 {x = v.x.Floor(), y = v.y.Floor()};
		
		public static Vector2Int FlooredToVector2Int(this Vector2 v) 
			=> new Vector2Int {x = v.x.FloorToInt(), y = v.y.FloorToInt()};
		
		public static Vector2 Ceiled(this Vector2 v) 
			=> new Vector2 {x = v.x.Ceil(), y = v.y.Ceil()};
	
		public static Vector2Int CeiledToVector2Int(this Vector2 v) 
			=> new Vector2Int {x = v.x.CeilToInt(), y = v.y.CeilToInt()};
		
		#endregion
		
		public static Vector2 GetRelativePositionFrom(this Vector2 position, Matrix4x4 from) 
			=> from.MultiplyPoint(position);

		public static Vector2 GetRelativePositionTo(this Vector2 position, Matrix4x4 to) 
			=> to.inverse.MultiplyPoint(position);

		public static Vector2 GetRelativeDirectionFrom(this Vector2 direction, Matrix4x4 from) 
			=> from.MultiplyVector(direction);

		public static Vector2 GetRelativeDirectionTo(this Vector2 direction, Matrix4x4 to) 
			=> to.inverse.MultiplyVector(direction);
		
		#endregion
	}
}