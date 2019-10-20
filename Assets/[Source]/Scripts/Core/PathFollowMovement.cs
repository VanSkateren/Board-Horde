using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using CommonGames;
using CommonGames.Utilities;
using CommonGames.Utilities.Extensions;
using CommonGames.Utilities.CGTK;

using Sirenix.OdinInspector;

using Curve;

public class PathFollowMovement : MonoBehaviour
{
    [Required]
    public TubeGenerator tube;
    //public EndOfPathInstruction endOfPathInstruction;
    public float speed = 0.1f;
    
    private float _distanceTravelled;

    private Matrix4x4 _worldMatrix, _localMatrix;

    private bool onGrindBar = false;
    
    private void Start()
    {
        _worldMatrix = tube.transform.WorldMatrix();
        _localMatrix = tube.transform.LocalMatrix();
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        if(!onGrindBar) return;

        _distanceTravelled += speed * Time.deltaTime;

        if(_distanceTravelled >= 1 || _distanceTravelled.Approximately(1))
        {
            onGrindBar = false;
            return;
        }

        Vector3 __localPos = tube.Curve.GetPointAt(_distanceTravelled);
        
        Vector3 __worldPos = __localPos.GetRelativePositionFrom(_worldMatrix);
        
        transform.position = __worldPos;
    }

    [Button]
    public void DoTheThing()
    {
        if(onGrindBar) return;
        
        Vector3 __relativePositionToTube = transform.position.GetRelativePositionTo(_localMatrix);

        float __startPoint = tube.GetStartPoint(__relativePositionToTube);

        _distanceTravelled = __startPoint;

        onGrindBar = true;
    }
}
