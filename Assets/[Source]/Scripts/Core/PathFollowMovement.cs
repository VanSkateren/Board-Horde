using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using CommonGames;
using CommonGames.Utilities;
using CommonGames.Utilities.Extensions;
using CommonGames.Utilities.CGTK;


using Curve;

public class PathFollowMovement : MonoBehaviour
{
    public TubeGenerator tube;
    //public EndOfPathInstruction endOfPathInstruction;
    public float speed = 0.5f;
    
    private float _distanceTravelled;

    private Matrix4x4 _matrix;

    private void Start()
    {
        _matrix = tube.transform.WorldMatrix();
    }

    private void Update()
    {
        if (tube == null) return;
        
        _distanceTravelled += speed * Time.deltaTime;

        
        //Gizmos.matrix = transform.localToWorldMatrix;
        
        Vector3 __localPos = tube.Curve.GetPointAt(_distanceTravelled);

        Vector3 __worldPos = __localPos.GetRelativePositionFrom(_matrix);

        transform.position = __worldPos;

        //transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
        //transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
    }

    /*
    // If the path changes during the game, update the distance travelled so that the follower's position on the new path
    // is as close as possible to its position on the old path
    void OnPathChanged() {
        distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
    }
    */
}
