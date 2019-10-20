using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static CommonGames.Utilities.Extensions.Constants;

namespace CommonGames.Utilities.Extensions
{
    public static partial class Physics
    {
        public static void ResetVelocity(this Rigidbody rigidbody)
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
    }
}