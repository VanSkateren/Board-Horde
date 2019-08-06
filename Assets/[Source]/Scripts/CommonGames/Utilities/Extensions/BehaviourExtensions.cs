using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace CommonGames.Utilities.Extensions
{	
	public static partial class GeneralExtensions
	{
		#region ToggleInTime

		public static void ToggleInTime(this Behaviour obj, bool state, float time = DEFAULT_TOGGLE_TIME)
			=> CoroutineHandler.StartCoroutine(ToggleInTimeCoroutine(obj, state, time.ToAbs()));
		
		private static IEnumerator ToggleInTimeCoroutine(Behaviour obj, bool state, float time = DEFAULT_TOGGLE_TIME)
		{
			yield return (time.Approximately(default) ? DefaultWait : new WaitForSeconds(time));
			
			obj.enabled = state;
		}
		
		#endregion
	}
}