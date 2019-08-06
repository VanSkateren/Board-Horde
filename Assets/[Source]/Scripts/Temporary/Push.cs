using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Push : MonoBehaviour
{

    public KinematicCharacterController.KinematicCharacterMotor motor = null;

    public float maximum = 15f, minumum = 89f;

    private void Update()
    {
        motor.MaxStableSlopeAngle = Input.GetKey(KeyCode.R) ? minumum : maximum;
    }
}
