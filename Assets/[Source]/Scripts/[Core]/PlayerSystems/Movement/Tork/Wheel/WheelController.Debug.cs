using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace NWH.WheelController3D
{
    public partial class WheelController : MonoBehaviour
    {
        /// <summary>
        /// Visual representation of the wheel and it's more important Vectors.
        /// </summary>
        private void OnDrawGizmos()
        {
            if(!Application.isPlaying) _transformPosition = transform.position;

            // Draw spring travel
            Gizmos.color = Color.green;
            var forwardOffset = transform.forward * 0.07f;
            var springOffset = transform.up * spring.maxLength;
            Gizmos.DrawLine(_transformPosition - forwardOffset, _transformPosition + forwardOffset);
            Gizmos.DrawLine(_transformPosition - springOffset - forwardOffset, _transformPosition - springOffset + forwardOffset);
            Gizmos.DrawLine(_transformPosition, _transformPosition - springOffset);

            Vector3 interpolatedPos = Vector3.zero;

            // Set dummy variables when in inspector.
            if (!Application.isPlaying)
            {
                if (wheel.visual != null)
                {
                    wheel.worldPosition = wheel.visual.transform.position;
                    wheel.up = wheel.visual.transform.up;
                    wheel.forward = wheel.visual.transform.forward;
                    wheel.right = wheel.visual.transform.right;
                }
            }

            Gizmos.DrawSphere(wheel.worldPosition, 0.02f);

            // Draw wheel
            Gizmos.color = Color.green;
            DrawWheelGizmo(wheel.tireRadius, wheel.width, wheel.worldPosition, wheel.up, wheel.forward, wheel.right);

            if (debug && Application.isPlaying)
            {
                // Draw wheel anchor normals
                Gizmos.color = Color.red;
                Gizmos.DrawRay(new Ray(wheel.worldPosition, wheel.up));
                Gizmos.color = Color.green;
                Gizmos.DrawRay(new Ray(wheel.worldPosition, wheel.forward));
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(new Ray(wheel.worldPosition, wheel.right));
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(new Ray(wheel.worldPosition, wheel.inside));

                // Draw axle location
                if (spring.length < 0.01f) Gizmos.color = Color.red;
                else if (spring.length > spring.maxLength - 0.01f) Gizmos.color = Color.yellow;
                else Gizmos.color = Color.green;

                if (hasHit)
                {
                    // Draw hit points
                    float weightSum = 0f;
                    float minWeight = Mathf.Infinity;
                    float maxWeight = 0f;

                    foreach (WheelHit hit in wheelHits)
                        {
                            weightSum += hit.weight;
                            if (hit.weight < minWeight) minWeight = hit.weight;
                            if (hit.weight > maxWeight) maxWeight = hit.weight;
                        }

                    foreach (WheelHit hit in wheelHits)
                        {
                            float t = (hit.weight - minWeight) / (maxWeight - minWeight);
                            Gizmos.color = Color.Lerp(Color.black, Color.white, t);
                            Gizmos.DrawSphere(hit.Point, 0.04f);
                            Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
                            Gizmos.DrawLine(hit.Point, hit.Point + wheel.up * hit.distanceFromTire);
                        }

                    //Draw hit forward and right
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawRay(new Ray(wheelHit.Point, wheelHit.forwardDir));
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawRay(new Ray(wheelHit.Point, wheelHit.sidewaysDir));

                    // Draw ground point
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(wheelHit.Point, 0.04f);
                    Gizmos.DrawLine(wheelHit.Point, wheelHit.Point + wheelHit.Normal * 1f);

                    // Force point
                    Gizmos.DrawSphere(_forcePoint, 0.06f);

                    Gizmos.color = Color.yellow;
                    Vector3 alternateNormal = (wheel.worldPosition - wheelHit.Point).normalized;
                    Gizmos.DrawLine(wheelHit.Point, wheelHit.Point + alternateNormal * 1f);

                    // Spring travel point
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawCube(spring.targetPoint, new Vector3(0.1f, 0.1f, 0.04f));
                }
            }
        }

        /// <summary>
        /// Draw a wheel radius on both side of the wheel, interconected with lines perpendicular to wheel axle.
        /// </summary>
        private void DrawWheelGizmo(float radius, float width, Vector3 position, Vector3 up, Vector3 forward, Vector3 right)
        {
            var halfWidth = width / 2.0f;
            float theta = 0.0f;
            float x = radius * Mathf.Cos(theta);
            float y = radius * Mathf.Sin(theta);
            Vector3 pos = position + up * y + forward * x;
            Vector3 newPos = pos;

            for (theta = 0.0f; theta <= Mathf.PI * 2; theta += Mathf.PI / 12.0f)
            {
                x = radius * Mathf.Cos(theta);
                y = radius * Mathf.Sin(theta);
                newPos = position + up * y + forward * x;

                // Left line
                Gizmos.DrawLine(pos - right * halfWidth, newPos - right * halfWidth);

                // Right line
                Gizmos.DrawLine(pos + right * halfWidth, newPos + right * halfWidth);

                // Center Line
                Gizmos.DrawLine(pos - right * halfWidth, pos + right * halfWidth);

                // Diagonal
                Gizmos.DrawLine(pos - right * halfWidth, newPos + right * halfWidth);

                pos = newPos;
            }
        }
    }
}
