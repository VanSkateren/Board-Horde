using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace NWH.WheelController3D
{
    [Serializable]
    public partial class WheelController : MonoBehaviour
    {
        [SerializeField]
        public Wheel wheel;

        [SerializeField, HideInInspector]
        public Spring spring;

        [SerializeField, HideInInspector]
        public Damper damper;

        /// <summary>
        /// Forward (longitudinal) friction info.
        /// </summary>
        [SerializeField, HideInInspector]
        public Friction fFriction;

        /// <summary>
        /// Side (lateral) friction info.
        /// </summary>
        [SerializeField, HideInInspector]
        public Friction sFriction;

        /// <summary>
        /// Array of rays and related data that are shot each frame to detect surface features.
        /// Contains offsets, hit points, normals, etc. of each point.
        /// </summary>
        [SerializeField]
        private WheelHit[] wheelHits;

        [SerializeField]
        private LayerMask scanIgnoreLayers = Physics.IgnoreRaycastLayer;

        /// <summary>
        /// Number of raycasts in the forward / longitudinal direction.
        /// </summary>
        [SerializeField]
        private int forwardScanResolution = 8; // resolution of the first scan pass

        /// <summary>
        /// Number of raycasts in the side / lateral direction.
        /// </summary>
        [SerializeField]
        private int sideToSideScanResolution = 3; // number of scan planes (side-to-side)

        /// <summary>
        /// True if wheel touching ground.
        /// </summary>
        [SerializeField]
        private bool hasHit = true;
        [SerializeField]
        private bool prevHasHit = true;

        // If set to true draws hit points and related data.
        public bool debug;

        /// <summary>
        /// Root object of the vehicle.
        /// </summary>
        [SerializeField]
        public GameObject parent;
        private Rigidbody _parentRigidbody;

        /// <summary>
        /// If enabled mesh collider mimicking the shape of rim and wheel will be positioned so that wheel can not pass through objects in case raycast does not detect the surface in time.
        /// </summary>
        public bool useRimCollider = true;

        /// <summary>
        /// Side of the vehicle.
        /// </summary>
        public enum Side
        {
            Left = -1,
            Right = 1,
            Center = 0,
            Auto = 2
        }

        /// <summary>
        /// Side the wheel is on.
        /// </summary>
        [SerializeField]
        private Side vehicleSide = Side.Auto;

        /// <summary>
        /// Current active preset enum value.
        /// </summary>
        public FrictionPreset.FrictionPresetEnum activeFrictionPresetEnum;

        /// <summary>
        /// Current active friction preset.
        /// </summary>
        public FrictionPreset activeFrictionPreset;

        /// <summary>
        /// Contains point in which wheel touches ground. Not valid if !isGrounded.
        /// </summary>
        public WheelHit wheelHit = new WheelHit();

        public bool singleRay = false;
        public WheelHit singleWheelHit = new WheelHit();

        /// <summary>
        /// Enables some wheel behaviors specific to tracked vehicles, specifically the fact that there is no wheel spin.
        /// </summary>
        [HideInInspector]
        public bool trackedVehicle = false;

        /// <summary>
        /// Only for tracked vehicles that use NVP wheel enlargement.
        /// Determines how much larger the wheel radius virtually is.
        /// </summary>
        [HideInInspector]
        public float trackedOffset = 0;

        // Wheel rotation
        private Quaternion _steerQuaternion;
        private Quaternion _camberQuaternion;
        private Quaternion _totalRotation;

        private float _boundsX, _boundsY, _boundsZ, _boundsW;
        private float _stepX, _stepY;
        private float _rayLength;
        private int _minDistRayIndex;

        // Weighted average
        private WheelHit _wheelRay;
        private float _n;
        private float _minWeight = Mathf.Infinity;
        private float _maxWeight = 0f;
        private float _weightSum = 0f;
        private int _validCount = 0;

        [NonSerialized]
        private Vector3 _hitPointSum = Vector3.zero;
        [NonSerialized]
        private Vector3 _normalSum = Vector3.zero;
        [NonSerialized]
        private Vector3 _point = new Vector3();
        [NonSerialized]
        private Vector3 _normal = new Vector3();
        private float _weight = 0;

        private float _forwardSum = 0;
        private float _sideSum = 0;
        private float _angleSum = 0;
        private float _offsetSum = 0;

        private Vector3 _transformUp;
        private Vector3 _transformForward;
        private Vector3 _transformRight;
        private Vector3 _transformPosition;
        private Quaternion _transformRotation;

        public bool applyForceToOthers = false;
        public float maxPutDownForce;

        private Vector3 _origin;
        private Vector3 _alternateForwardNormal;
        private Vector3 _totalForce;
        private Vector3 _forcePoint;
        private Vector3 _hitDir;
        private Vector3 _predictedDistance;
        private Vector3 _wheelDown;
        private Vector3 _offsetPrecalc;
        private float _prevForwardSpeed;
        private float _prevFreeRollingAngularVelocity;

        private Vector3 _projectedNormal;
        private Vector3 _projectedAltNormal;

        // Cache transform 
        private Transform _trans;
        private Transform _visualTrans;

        // Raycast command
        private NativeArray<RaycastHit> _raycastHits;
        private NativeArray<RaycastCommand> _raycastCommands;
        private JobHandle _raycastJobHandle;

        private WheelHit _wr;
        private RaycastCommand _rc = new RaycastCommand();
        private RaycastHit _tmpHit;


        private void Awake()
        {
            // Cache transform
            _trans = transform;

            // Invert layers so that all the other layers are detected except for the ignore ones.
            scanIgnoreLayers = scanIgnoreLayers | (1 << 2);
            scanIgnoreLayers = ~scanIgnoreLayers;

            // Fill in necessary values and generate curves if needed
            Initialize();

            // Set the world position to the position of the wheel
            if (wheel.visual != null)
            {
                _visualTrans = wheel.visual.transform;
                wheel.worldPosition = _visualTrans.position;
                wheel.up = _visualTrans.up;
                wheel.forward = _visualTrans.forward;
                wheel.right = _visualTrans.right;
            }

            if (wheel.nonRotating != null)
            {
                wheel.nonRotatingPostionOffset = _trans.InverseTransformDirection(wheel.nonRotating.transform.position - _visualTrans.position);
            }

            // Initialize the wheel params
            wheel.Initialize(this);

            InitializeScanParams();

            // Find parent
            _parentRigidbody = parent.GetComponent<Rigidbody>();

            // Initialize spring length to starting value.
            spring.length = spring.maxLength * 0.5f;
        }


        public void InitializeScanParams()
        {
            // Scan start point
            _boundsX = -wheel.width / 2f;
            _boundsY = -wheel.tireRadius;

            // Scan end point
            _boundsZ = wheel.width / 2f + 0.000001f;
            _boundsW = wheel.tireRadius + 0.000001f;

            // Increment
            _stepX = sideToSideScanResolution == 1 ? 1 : (wheel.width) / (sideToSideScanResolution - 1);
            _stepY = forwardScanResolution == 1 ? 1 : (wheel.tireRadius * 2f) / (forwardScanResolution - 1);

            // Initialize wheel rays
            int __n = forwardScanResolution * sideToSideScanResolution;
            wheelHits = new WheelHit[__n];

            int __w = 0;
            for (float __x = _boundsX; __x <= _boundsZ; __x += _stepX)
            {
                int __h = 0;
                for (float __y = _boundsY; __y <= _boundsW; __y += _stepY)
                {
                    int __index = __w * forwardScanResolution + __h;

                    WheelHit __wr = new WheelHit();
                    __wr.angleForward = Mathf.Asin(__y / (wheel.tireRadius + 0.000001f));
                    __wr.curvatureOffset = Mathf.Cos(__wr.angleForward) * wheel.tireRadius;

                    float __xOffset = __x;
                    if (sideToSideScanResolution == 1) __xOffset = 0;
                    __wr.offset = new Vector2(__xOffset, __y);
                    wheelHits[__index] = __wr;

                    __h++;
                }
                __w++;
            }

            if (_raycastCommands.Length > 0) _raycastCommands.Dispose();
            if (_raycastHits.Length > 0) _raycastHits.Dispose();
            _raycastCommands = new NativeArray<RaycastCommand>(__n, Allocator.Persistent);
            _raycastHits = new NativeArray<RaycastHit>(__n, Allocator.Persistent);
        }


        public void FixedUpdate()
        {
            prevHasHit = hasHit;

            _transformPosition = _trans.position;
            _transformRotation = _trans.rotation;
            _transformForward = _trans.forward;
            _transformRight = _trans.right;
            _transformUp = _trans.up;

            if (!_parentRigidbody.IsSleeping())
            {
                // Find contact point with ground
                HitUpdate();
                SuspensionUpdate();
                CalculateWheelDirectionsAndRotations();
                WheelUpdate();
                FrictionUpdate();
                UpdateForces();
            }
        }


        private void OnDestroy()
        {
            if (_raycastCommands != null) _raycastCommands.Dispose();
            if (_raycastHits != null) _raycastHits.Dispose();
        }


        private void CalculateWheelDirectionsAndRotations()
        {
            _steerQuaternion = Quaternion.AngleAxis(wheel.steerAngle, _transformUp);
            _camberQuaternion = Quaternion.AngleAxis(-(int)vehicleSide * wheel.camberAngle, _transformForward);
            _totalRotation = _steerQuaternion * _camberQuaternion;

            wheel.up = _totalRotation * _transformUp;
            wheel.forward = _totalRotation * _transformForward;
            wheel.right = _totalRotation * _transformRight;
            wheel.inside = wheel.right * -(int)vehicleSide;
        }


        /// <summary>
        /// Searches for wheel hit point by iterating WheelScan() function to the requested scan depth.
        /// </summary>
        private void HitUpdate()
        {
            // Hit flag     
            float __minDistance = Mathf.Infinity;
            _wheelDown = -wheel.up;

            float __distanceThreshold = spring.maxLength - spring.length;
            _rayLength = wheel.tireRadius * 2.1f + __distanceThreshold;

            _offsetPrecalc = _transformPosition - _transformUp * spring.length + wheel.up * wheel.tireRadius - wheel.inside * wheel.rimOffset;

            int __validHitCount = 0;
            _minDistRayIndex = -1;
            hasHit = false;

            if (singleRay)
            {
                singleWheelHit.valid = false;

                bool __grounded = Physics.Raycast(_offsetPrecalc, _wheelDown, out singleWheelHit.raycastHit, _rayLength + wheel.tireRadius, scanIgnoreLayers);

                if (__grounded)
                {
                    float __distanceFromTire = singleWheelHit.raycastHit.distance - wheel.tireRadius - wheel.tireRadius;
                    if (__distanceFromTire > __distanceThreshold) return;
                    singleWheelHit.valid = true;
                    hasHit = true;
                    singleWheelHit.distanceFromTire = __distanceFromTire;

                    wheelHit.raycastHit = singleWheelHit.raycastHit;
                    wheelHit.angleForward = singleWheelHit.angleForward;
                    wheelHit.distanceFromTire = singleWheelHit.distanceFromTire;
                    wheelHit.offset = singleWheelHit.offset;
                    wheelHit.weight = singleWheelHit.weight;
                    wheelHit.curvatureOffset = singleWheelHit.curvatureOffset;

                    wheelHit.groundPoint = wheelHit.raycastHit.point;
                    wheelHit.raycastHit.point += wheel.up * wheel.tireRadius;
                    wheelHit.curvatureOffset = wheel.tireRadius;
                }
            }
            else
            {
                int __n = wheelHits.Length;
                for (int __i = 0; __i < __n; __i++)
                {
                    _wr = wheelHits[__i];
                    _wr.valid = false;

                    _origin.x = wheel.forward.x * _wr.offset.y + wheel.right.x * _wr.offset.x + _offsetPrecalc.x;
                    _origin.y = wheel.forward.y * _wr.offset.y + wheel.right.y * _wr.offset.x + _offsetPrecalc.y;
                    _origin.z = wheel.forward.z * _wr.offset.y + wheel.right.z * _wr.offset.x + _offsetPrecalc.z;

                    // Raycast command is a struct so exploit that to avoid GC
                    _rc.from = _origin;
                    _rc.direction = _wheelDown;
                    _rc.distance = _rayLength + _wr.curvatureOffset;
                    _rc.layerMask = scanIgnoreLayers;
                    _rc.maxHits = 1;
                    _raycastCommands[__i] = _rc;
                }

                _raycastJobHandle = RaycastCommand.ScheduleBatch(_raycastCommands, _raycastHits, 4);
                _raycastJobHandle.Complete();


                for (int __i = 0; __i < __n; __i++)
                {
                    _tmpHit = _raycastHits[__i];

                    _wr = wheelHits[__i];
                    _wr.valid = false;

                    if (_raycastHits[__i].distance > 0)
                    {
                        float __distanceFromTire = _tmpHit.distance - _wr.curvatureOffset - wheel.tireRadius;

                        if (__distanceFromTire > __distanceThreshold) continue;

                        _wr.valid = true;
                        _wr.raycastHit = _tmpHit;
                        _wr.distanceFromTire = __distanceFromTire;

                        __validHitCount++;

                        if (__distanceFromTire < __minDistance)
                        {
                            __minDistance = __distanceFromTire;
                            _minDistRayIndex = __i;
                        }
                    }

                    wheelHits[__i] = _wr;
                }              

                CalculateAverageWheelHit();
            }

            // Friction force directions
            if (hasHit)
            {
                wheelHit.forwardDir = Vector3.Normalize(Vector3.Cross(wheelHit.Normal, -wheel.right));
                wheelHit.sidewaysDir = Quaternion.AngleAxis(90f, wheelHit.Normal) * wheelHit.forwardDir;
            }
        }


        private void CalculateAverageWheelHit()
        {
            int __count = 0;

            _n = wheelHits.Length;

            _minWeight = Mathf.Infinity;
            _maxWeight = 0f;
            _weightSum = 0f;
            _validCount = 0;

            _hitPointSum = Vector3.zero;
            _normalSum = Vector3.zero;
            _weight = 0;

            _forwardSum = 0;
            _sideSum = 0;
            _angleSum = 0;
            _offsetSum = 0;
            _validCount = 0;

            for (int __i = 0; __i < _n; __i++)
            {
                _wheelRay = wheelHits[__i];
                if (_wheelRay.valid)
                {
                    _weight = wheel.tireRadius - _wheelRay.distanceFromTire;
                    _weight = _weight * _weight * _weight * _weight * _weight;

                    if (_weight < _minWeight) _minWeight = _weight;
                    else if (_weight > _maxWeight) _maxWeight = _weight;

                    _weightSum += _weight;
                    _validCount++;

                    _normal = _wheelRay.raycastHit.normal;
                    _point = _wheelRay.raycastHit.point;

                    _hitPointSum.x += _point.x * _weight;
                    _hitPointSum.y += _point.y * _weight;
                    _hitPointSum.z += _point.z * _weight;

                    _normalSum.x += _normal.x * _weight;
                    _normalSum.y += _normal.y * _weight;
                    _normalSum.z += _normal.z * _weight;

                    _forwardSum += _wheelRay.offset.y * _weight;
                    _sideSum += _wheelRay.offset.x * _weight;
                    _angleSum += _wheelRay.angleForward * _weight;
                    _offsetSum += _wheelRay.curvatureOffset * _weight;

                    __count++;
                }
            }

            if (_validCount == 0 || _minDistRayIndex < 0)
            {
                hasHit = false;
                return;
            }

            wheelHit.raycastHit = wheelHits[_minDistRayIndex].raycastHit;
            wheelHit.raycastHit.point = _hitPointSum / _weightSum;
            wheelHit.offset.y = _forwardSum / _weightSum;
            wheelHit.offset.x = _sideSum / _weightSum;
            wheelHit.angleForward = _angleSum / _weightSum;
            wheelHit.raycastHit.normal = Vector3.Normalize(_normalSum / _weightSum);
            wheelHit.curvatureOffset = _offsetSum / _weightSum;
            wheelHit.raycastHit.point += wheel.up * wheelHit.curvatureOffset;
            wheelHit.groundPoint = wheelHit.raycastHit.point - wheel.up * wheelHit.curvatureOffset;

            hasHit = true;
        }


        private void SuspensionUpdate()
        {
            spring.prevOverflow = spring.overflow;
            spring.overflow = 0f;
            if (hasHit && Vector3.Dot(wheelHit.raycastHit.normal, _transformUp) > 0.1f)
            {
                spring.bottomedOut = spring.overExtended = false;

                // Calculate spring length from ground hit, position of the wheel and transform position.     
                if (singleRay)
                {
                    spring.targetPoint = wheelHit.raycastHit.point - wheel.right * wheel.rimOffset * (int)vehicleSide;
                }
                else
                {
                    spring.targetPoint = wheelHit.raycastHit.point
                         + wheel.up * wheel.tireRadius * 0.03f
                        - wheel.forward * wheelHit.offset.y
                        - wheel.right * wheelHit.offset.x
                        - wheel.right * wheel.rimOffset * (int)vehicleSide;
                }

                spring.length = -_trans.InverseTransformPoint(spring.targetPoint).y;

                // If the spring is overcompressed remember the value for later force calculation and set spring to 0.
                // If the spring is overcompresset hit has not actually happened since the wheel is in the air.
                if (spring.length < 0f)
                {
                    spring.overflow = -spring.length;
                    spring.length = 0f;
                    spring.bottomedOut = true;
                }
                else if (spring.length > spring.maxLength)
                {
                    hasHit = false;
                    spring.length = spring.maxLength;
                    spring.overExtended = true;
                }
            }
            else
            {
                // If the wheel suddenly gets in the air smoothly extend it.
                spring.length = Mathf.Lerp(spring.length, spring.maxLength, Time.fixedDeltaTime * 8f);
            }

            spring.velocity = (spring.length - spring.prevLength) / Time.fixedDeltaTime;
            spring.compressionPercent = (spring.maxLength - spring.length) / spring.maxLength;
            spring.force = spring.maxForce * spring.forceCurve.Evaluate(spring.compressionPercent);

            // If spring has bottomed out add bottoming out force and if functioning normally add damper force.
            spring.overflowVelocity = 0f;
            if (spring.overflow > 0)
            {
                spring.overflowVelocity = (spring.overflow - spring.prevOverflow) / Time.fixedDeltaTime;
                spring.bottomOutForce = _parentRigidbody.mass * -Physics.gravity.y * Mathf.Clamp(spring.overflowVelocity, 0f, Mathf.Infinity) * 0.011f;
                _parentRigidbody.AddForceAtPosition(spring.bottomOutForce * _transformUp, _transformPosition, ForceMode.Impulse);
            }
            else
            {
                damper.maxForce = spring.length < spring.prevLength ? damper.unitBumpForce : damper.unitReboundForce;
                if (spring.length <= spring.prevLength)
                    damper.force = damper.unitBumpForce * damper.dampingCurve.Evaluate(Mathf.Abs(spring.velocity));
                else
                    damper.force = -damper.unitReboundForce * damper.dampingCurve.Evaluate(Mathf.Abs(spring.velocity));
            }

            spring.prevLength = spring.length;
        }


        private void WheelUpdate()
        {
            wheel.prevWorldPosition = wheel.worldPosition;
            wheel.worldPosition = _transformPosition - _transformUp * spring.length - wheel.inside * wheel.rimOffset;

            wheel.prevVelocity = wheel.velocity;
            wheel.velocity = _parentRigidbody.GetPointVelocity(wheel.worldPosition);
            wheel.acceleration = (wheel.velocity - wheel.prevVelocity) / Time.fixedDeltaTime;

            // Calculate camber based on spring travel
            wheel.camberAngle = wheel.camberCurve.Evaluate(spring.length / spring.maxLength);

            // Tire load calculated from spring and damper force for wheelcollider compatibility
            wheel.tireLoad = Mathf.Clamp(spring.force + damper.force, 0.0f, Mathf.Infinity);
            if (hasHit) wheelHit.force = wheel.tireLoad;

            // Calculate visual rotation angle between 0 and 2PI radians.
            wheel.rotationAngle = (wheel.rotationAngle % 360.0f) + (wheel.angularVelocity * Mathf.Rad2Deg * Time.fixedDeltaTime);

            var __axleRotation = Quaternion.AngleAxis(wheel.rotationAngle, _transformRight);

            // Set rotation   
            wheel.worldRotation = _totalRotation * __axleRotation * _transformRotation;

            // Apply rotation and position to visuals if assigned
            if (wheel.visual != null)
            {
                _visualTrans.rotation = wheel.worldRotation;

                if (trackedVehicle)
                {
                    _visualTrans.position = wheel.worldPosition - _transformUp * trackedOffset;
                }
                else
                {
                    _visualTrans.position = wheel.worldPosition;
                }

            }

            // Apply rotation and position to the non-rotationg objects if assigned
            if (wheel.nonRotating != null)
            {
                wheel.nonRotating.transform.rotation = _totalRotation * _transformRotation;

                if (trackedVehicle)
                {
                    wheel.nonRotating.transform.position = wheel.worldPosition + _trans.TransformDirection(_totalRotation * wheel.nonRotatingPostionOffset) - _transformUp * trackedOffset;
                }
                else
                {
                    wheel.nonRotating.transform.position = wheel.worldPosition + _trans.TransformDirection(_totalRotation * wheel.nonRotatingPostionOffset);
                }
            }

            // Apply rotation to rim collider 
            if (useRimCollider)
            {
                wheel.rim.transform.position = wheel.worldPosition;
                wheel.rim.transform.rotation = _steerQuaternion * _camberQuaternion * _transformRotation;
            }
        }


        /// <summary>
        /// Does lateral and longitudinal slip and force calculations.
        /// </summary>
        private void FrictionUpdate()
        {
            _prevForwardSpeed = fFriction.speed;
            Vector3 __contactVelocity = _parentRigidbody.GetPointVelocity(wheelHit.raycastHit.point);

            if (hasHit)
            {
                fFriction.speed = Vector3.Dot(__contactVelocity, wheelHit.forwardDir);
                sFriction.speed = Vector3.Dot(__contactVelocity, wheelHit.sidewaysDir);
            }
            else
            {
                fFriction.speed = sFriction.speed = 0;
            }

            float __lowerLimit = 3f - Mathf.Clamp(__contactVelocity.magnitude, 0f, 3f);
            float __wheelForwardSpeed = wheel.angularVelocity * wheel.tireRadius;
            float __clampedWheelForwardSpeed = Mathf.Clamp(Mathf.Abs(__wheelForwardSpeed), __lowerLimit, Mathf.Infinity);

            //*******************
            // Side slip
            //*******************

            sFriction.slip = 0f;
            sFriction.force = 0f;

            if (hasHit)
            {
                if (trackedVehicle)
                    SetActiveFrictionPreset(FrictionPreset.Tracks);

                sFriction.slip = fFriction.speed == 0 ? 0 : (Mathf.Atan(sFriction.speed / __clampedWheelForwardSpeed) * Mathf.Rad2Deg) / 80.0f;
                sFriction.force = Mathf.Sign(sFriction.slip) * activeFrictionPreset.Curve.Evaluate(Mathf.Abs(sFriction.slip)) * wheel.tireLoad * sFriction.forceCoefficient * 1.3f;
            }

            //*******************
            // Forward slip
            //*******************
            wheel.freeRollingAngularVelocity = fFriction.speed / wheel.tireRadius;

            float __inertia = wheel.mass * wheel.tireRadius * wheel.tireRadius;
            float __motorForce = wheel.motorTorque / wheel.tireRadius;
            float __brakeForce = Mathf.Abs(wheel.brakeTorque / wheel.tireRadius);

            // Calculate wheel slip
            fFriction.slip = 0;
            if (hasHit)
            {
                float __fClampedForwardSpeed = Mathf.Clamp(Mathf.Abs(fFriction.speed), 0.22f, Mathf.Infinity);
                fFriction.slip = __fClampedForwardSpeed == 0 ? 0 : (((wheel.angularVelocity * wheel.tireRadius) - fFriction.speed) / __fClampedForwardSpeed) * fFriction.slipCoefficient;
            }

            float __clampedSlip = Mathf.Clamp(Mathf.Abs(fFriction.slip), 0.05f, Mathf.Infinity);

            // Calculate maximum force that wheel can put down before it starts to spin
            if (!trackedVehicle)
            {
                maxPutDownForce = activeFrictionPreset.Curve.Evaluate(__clampedSlip) * wheel.tireLoad * fFriction.forceCoefficient * 1.3f;
            }
            else
            {
                maxPutDownForce = wheel.tireLoad * fFriction.forceCoefficient * 1.3f;
            }

            // Reduce residual angular velocity by the unused force
            float __decelerationForce = Mathf.Sign(__motorForce) * Mathf.Clamp(maxPutDownForce - Mathf.Abs(__motorForce), 0f, Mathf.Infinity);
            float __decelerationDelta = __inertia == 0 ? 0 : ((__decelerationForce * wheel.tireRadius) / __inertia) * Time.fixedDeltaTime;

            // Increase residual angular velocity by the motor force that could not be put down
            float __accelerationForce = Mathf.Sign(__motorForce) * Mathf.Clamp((Mathf.Abs(__motorForce) - maxPutDownForce), 0f, Mathf.Infinity);
            float __accelerationDelta = __inertia == 0 ? 0 : ((__accelerationForce * wheel.tireRadius) / __inertia) * Time.fixedDeltaTime;

            // Calculate residual angular velocity
            wheel.residualAngularVelocity += __accelerationDelta - __decelerationDelta;

            // Limit angular velocity so that brakes can slow down the rotation only until wheel is fully stopped.
            if (__motorForce >= 0)
                wheel.residualAngularVelocity = Mathf.Clamp(wheel.residualAngularVelocity, 0f, Mathf.Infinity);
            else
                wheel.residualAngularVelocity = Mathf.Clamp(wheel.residualAngularVelocity, -Mathf.Infinity, 0f);

            // Continue spinning even after leaving groundW
            if (!hasHit && prevHasHit)
            {
                wheel.residualAngularVelocity = _prevFreeRollingAngularVelocity;
            }
            wheel.angularVelocity = wheel.freeRollingAngularVelocity + wheel.residualAngularVelocity;

            // Calculate brakes
            float __angularDeceleration = __inertia == 0 ? 0 : -Mathf.Sign(wheel.angularVelocity) * ((__brakeForce * wheel.tireRadius) / __inertia) * Time.fixedDeltaTime;

            // Limit angular velocity after applying brakes so that brakes can slow down the rotation only until wheel is fully stopped.
            if (wheel.angularVelocity < 0)
                wheel.angularVelocity = Mathf.Clamp(wheel.angularVelocity + __angularDeceleration, -Mathf.Infinity, 0f);
            else
                wheel.angularVelocity = Mathf.Clamp(wheel.angularVelocity + __angularDeceleration, 0f, Mathf.Infinity);

            // Limit how much residual velocity a wheel can have. Too much will cause wheel to spin for long time after motor force is no longer applied.
            // Physically this would be more accurate but can be irritating (default wheelcollider does not limit this).
            wheel.residualAngularVelocity = Mathf.Sign(wheel.residualAngularVelocity) * Mathf.Clamp(Mathf.Abs(wheel.residualAngularVelocity), 0f, 1000f);

            // Make wheels free roll when slight braking is applied, but not enough to lock the wheel.
            if (hasHit && __brakeForce != 0 && Mathf.Abs(__motorForce) < __brakeForce && __brakeForce < maxPutDownForce)
                wheel.angularVelocity = wheel.freeRollingAngularVelocity;

            // No wheel spin for tracked vehicles
            if (trackedVehicle)
                wheel.angularVelocity = wheel.freeRollingAngularVelocity;

            // Calculate force that will be put down to the surface
            if (hasHit)
            {
                float __smoothSpeed = fFriction.speed;
                if (__contactVelocity.magnitude < 1f)
                {
                    __smoothSpeed = Mathf.SmoothStep(_prevForwardSpeed, fFriction.speed, Time.fixedDeltaTime * 2f);
                }
                fFriction.force = Mathf.Clamp(__motorForce - Mathf.Sign(fFriction.speed) * Mathf.Clamp01(Mathf.Abs(__smoothSpeed)) * __brakeForce,
                    -maxPutDownForce, maxPutDownForce) * fFriction.forceCoefficient;
            }
            else
            {
                fFriction.force = 0f;
            }

            // Convert angular velocity to RPM
            wheel.rpm = wheel.angularVelocity * 9.55f;

            // Limit side and forward force if needed, useful for drift vehicles in arcade games
            if (fFriction.maxForce > 0)
            {
                fFriction.force = Mathf.Clamp(fFriction.force, -fFriction.maxForce, fFriction.maxForce);
            }
            if (sFriction.maxForce > 0)
            {
                sFriction.force = Mathf.Clamp(sFriction.force, -sFriction.maxForce, sFriction.maxForce);
            }

            // Fill in WheelHit info for Unity wheelcollider compatibility
            if (hasHit)
            {
                wheelHit.forwardSlip = fFriction.slip;
                wheelHit.sidewaysSlip = sFriction.slip;
            }

            _prevFreeRollingAngularVelocity = wheel.freeRollingAngularVelocity;
        }


        /// <summary>
        /// Updates force values, calculates force vector and applies it to the rigidbody.
        /// </summary>
        private void UpdateForces()
        {
            if (hasHit)
            {
                // Use alternate normal when encountering obstracles that have sharp edges in which case raycastHit.normal will alwyas point up.
                // Alternate normal cannot be used when on flat surface because of inaccuracies which cause vehicle to creep forward or in reverse.
                // Sharp edge detection is done via dot product of bot normals, if it differs it means that raycasHit.normal in not correct.

                // Cache most used values
                Vector3 __wheelHitPoint = wheelHit.Point;
                Vector3 __raycastHitNormal = wheelHit.raycastHit.normal;

                // Hit direction
                _hitDir.x = wheel.worldPosition.x - __wheelHitPoint.x;
                _hitDir.y = wheel.worldPosition.y - __wheelHitPoint.y;
                _hitDir.z = wheel.worldPosition.z - __wheelHitPoint.z;

                // Alternate normal
                float __distance = Mathf.Sqrt(_hitDir.x * _hitDir.x + _hitDir.y * _hitDir.y + _hitDir.z * _hitDir.z);
                _alternateForwardNormal.x = _hitDir.x / __distance;
                _alternateForwardNormal.y = _hitDir.y / __distance;
                _alternateForwardNormal.z = _hitDir.z / __distance;

                if (Vector3.Dot(__raycastHitNormal, _transformUp) > 0.1f)
                {
                    // Spring force
                    float __suspensionForceMagnitude = Mathf.Clamp(spring.force + damper.force, 0.0f, Mathf.Infinity);

                    // Obstracle force
                    float __obstracleForceMagnitude = 0f;

                    // Abs speed
                    float __absSpeed = fFriction.speed;
                    if (__absSpeed < 0) __absSpeed = -__absSpeed;

                    if (__absSpeed < 8f)
                    {
                        // Dot between normal and alternate normal
                        _projectedNormal = Vector3.ProjectOnPlane(wheelHit.Normal, wheel.right);
                        float __distace = Mathf.Sqrt(_projectedNormal.x * _projectedNormal.x + _projectedNormal.y * _projectedNormal.y + _projectedNormal.z * _projectedNormal.z);
                        _projectedNormal.x /= __distace;
                        _projectedNormal.y /= __distace;
                        _projectedNormal.z /= __distace;

                        _projectedAltNormal = Vector3.ProjectOnPlane(_alternateForwardNormal, wheel.right);
                        __distace = Mathf.Sqrt(_projectedAltNormal.x * _projectedAltNormal.x + _projectedAltNormal.y * _projectedAltNormal.y + _projectedAltNormal.z * _projectedAltNormal.z);
                        _projectedAltNormal.x /= __distace;
                        _projectedAltNormal.y /= __distace;
                        _projectedAltNormal.z /= __distace;

                        float __dot = Vector3.Dot(_projectedNormal, _projectedAltNormal);

                        // Abs dot
                        if (__dot < 0) __dot = -__dot;

                        __obstracleForceMagnitude = (1f - __dot) * __suspensionForceMagnitude * -Mathf.Sign(wheelHit.angleForward);
                    }

                    _totalForce.x = __obstracleForceMagnitude * wheel.forward.x
                        + __suspensionForceMagnitude * __raycastHitNormal.x
                        + wheelHit.sidewaysDir.x * -sFriction.force
                        + wheelHit.forwardDir.x * fFriction.force;

                    _totalForce.y = __obstracleForceMagnitude * wheel.forward.y
                        + __suspensionForceMagnitude * __raycastHitNormal.y
                        + wheelHit.sidewaysDir.y * -sFriction.force
                        + wheelHit.forwardDir.y * fFriction.force;

                    _totalForce.z = __obstracleForceMagnitude * wheel.forward.z
                        + __suspensionForceMagnitude * __raycastHitNormal.z
                        + wheelHit.sidewaysDir.z * -sFriction.force
                        + wheelHit.forwardDir.z * fFriction.force;

                    _forcePoint.x = (__wheelHitPoint.x * 3 + spring.targetPoint.x) / 4f;
                    _forcePoint.y = (__wheelHitPoint.y * 3 + spring.targetPoint.y) / 4f;
                    _forcePoint.z = (__wheelHitPoint.z * 3 + spring.targetPoint.z) / 4f;

                    _parentRigidbody.AddForceAtPosition(_totalForce, _forcePoint);

                    if (!applyForceToOthers) return;
                    
                    if (wheelHit.raycastHit.rigidbody)
                    {
                        wheelHit.raycastHit.rigidbody.AddForceAtPosition(-_totalForce, _forcePoint);
                    }
                }
            }
        }


        #region Classes
        /*****************************/
        /* CLASSES                   */
        /*****************************/

        /// <summary>
        /// All info related to longitudinal force calculation.
        /// </summary>
        [System.Serializable]
        public class Friction
        {
            public float forceCoefficient = 1.1f;
            public float slipCoefficient = 1;
            public float maxForce;
            public float slip;
            public float speed;
            public float force;
        }


        /// <summary>
        /// Suspension part.
        /// </summary>
        [System.Serializable]
        public class Damper
        {
            public AnimationCurve dampingCurve = null;
            public float unitBumpForce = 800.0f;
            public float unitReboundForce = 1000.0f;
            public float force;
            public float maxForce;
        }


        /// <summary>
        /// Suspension part.
        /// </summary>
        [System.Serializable]
        public class Spring
        {
            public float maxLength = 0.3f;
            public AnimationCurve forceCurve = null;
            public float maxForce = 22000.0f;

            public float length;
            public float prevLength;
            public float compressionPercent;
            public float force;
            public float velocity;
            public Vector3 targetPoint;

            public float overflow;
            public float prevOverflow;
            public float overflowVelocity;
            public float bottomOutForce;

            public bool bottomedOut;
            public bool overExtended;
        }


        /// <summary>
        /// Contains everything wheel related, including rim and tire.
        /// </summary>
        [System.Serializable]
        public class Wheel
        {
            public float mass = 25.0f;
            public float rimOffset = 0f;
            public float tireRadius = 0.4f;
            public float width = 0.25f;

            public float rpm;

            public Vector3 prevWorldPosition;
            public Vector3 worldPosition;
            public Vector3 prevGroundPoint;
            public Quaternion worldRotation;

            public AnimationCurve camberCurve = null;
            public float camberAngle;

            public float inertia;

            public float angularVelocity;
            public float freeRollingAngularVelocity;
            public float residualAngularVelocity;
            public float steerAngle;
            public float rotationAngle;
            public GameObject visual;
            public GameObject nonRotating;
            public GameObject rim;
            public Transform rimCollider;

            public Vector3 up;
            public Vector3 inside;
            public Vector3 forward;
            public Vector3 right;
            public Vector3 velocity;
            public Vector3 prevVelocity;
            public Vector3 acceleration;

            public float tireLoad;

            public float motorTorque;
            public float brakeTorque;

            public Vector3 nonRotatingPostionOffset;

            /// <summary>
            /// Calculation of static parameters and creation of rim collider.
            /// </summary>
            public void Initialize(WheelController wc)
            {
                // Precalculate wheel variables
                inertia = 0.5f * mass * (tireRadius * tireRadius + tireRadius * tireRadius);

                // Instantiate rim (prevent ground passing through the side of the wheel)
                rim = new GameObject();
                rim.name = "RimCollider";
                rim.transform.position = wc.transform.position + wc.transform.right * rimOffset * (int)wc.vehicleSide;
                rim.transform.parent = wc.transform;
                rim.layer = LayerMask.NameToLayer("Ignore Raycast");

                if (wc.useRimCollider && visual != null)
                {
                    MeshFilter __mf = rim.AddComponent<MeshFilter>();
                    __mf.name = "Rim Mesh Filter";
                    __mf.mesh = wc.GenerateRimColliderMesh(visual.transform);
                    __mf.mesh.name = "Rim Mesh";

                    MeshCollider __mc = rim.AddComponent<MeshCollider>();
                    __mc.name = "Rim MeshCollider";
                    __mc.convex = true;

                    PhysicMaterial __material = new PhysicMaterial();
                    __material.staticFriction = 0f;
                    __material.dynamicFriction = 0f;
                    __material.bounciness = 0.3f;
                    __mc.material = __material;

                    wc.wheel.rimCollider = rim.transform;
                }
            }

            public void GenerateCamberCurve(float camberAtBottom, float camberAtTop)
            {
                AnimationCurve __ac = new AnimationCurve();
                __ac.AddKey(0.0f, camberAtBottom);
                __ac.AddKey(1.0f, camberAtTop);
                camberCurve = __ac;
            }
        }


        /// <summary>
        /// Contains RaycastHit and extended hit data.
        /// </summary>
        [System.Serializable]
        public class WheelHit
        {
            [SerializeField]
            public RaycastHit raycastHit;
            public float angleForward;
            public float distanceFromTire;
            public Vector2 offset;

            [HideInInspector]
            public float weight;
            public bool valid = false;
            public float curvatureOffset;
            public Vector3 groundPoint;

            public WheelHit() { }

            /// <summary>
            /// The point of contact between the wheel and the ground.
            /// </summary>
            public Vector3 Point => groundPoint;

            /// <summary>
            /// The normal at the point of contact 
            /// </summary>
            public Vector3 Normal => raycastHit.normal;

            /// <summary>
            /// The direction the wheel is pointing in.
            /// </summary>
            public Vector3 forwardDir;

            /// <summary>
            /// Tire slip in the rolling direction.
            /// </summary>
            public float forwardSlip;

            /// <summary>
            /// The sideways direction of the wheel.
            /// </summary>
            public Vector3 sidewaysDir;

            /// <summary>
            /// The slip in the sideways direction.
            /// </summary>
            public float sidewaysSlip;

            /// <summary>
            /// The magnitude of the force being applied for the contact. [N]
            /// </summary>
            public float force;

            // WheelCollider compatibility variables
            public Collider Collider => raycastHit.collider;
        }
        #endregion


        #region Functions
        /*****************************/
        /* FUNCTIONS                 */
        /*****************************/

        public void Initialize()
        {
            // Objects
            if (parent == null) parent = FindParent();
            if (wheel == null) wheel = new Wheel();
            if (spring == null) spring = new Spring();
            if (damper == null) damper = new Damper();
            if (fFriction == null) fFriction = new Friction();
            if (sFriction == null) sFriction = new Friction();

            // Curves
            if (springCurve == null || springCurve.keys.Length == 0) springCurve = GenerateDefaultSpringCurve();
            if (damperCurve == null || damperCurve.keys.Length == 0) damperCurve = GenerateDefaultDamperCurve();
            if (wheel.camberCurve == null || wheel.camberCurve.keys.Length == 0) wheel.GenerateCamberCurve(0, 0);
            if (activeFrictionPreset == null) activeFrictionPreset = FrictionPreset.TarmacDry;

            //Other
            if (vehicleSide == Side.Auto && parent != null) vehicleSide = DetermineSide(transform.position, parent.transform);
        }


        private GameObject FindParent()
        {
            Transform __t = transform;
            while (__t != null)
            {
                if (__t.GetComponent<Rigidbody>())
                {
                    return __t.gameObject;
                }
                else
                {
                    __t = __t.parent;
                }
            }
            return null;
        }

        private AnimationCurve GenerateDefaultSpringCurve()
        {
            AnimationCurve __ac = new AnimationCurve();
            __ac.AddKey(0.0f, 0.0f);
            __ac.AddKey(1.0f, 1.0f);
            return __ac;
        }


        private AnimationCurve GenerateDefaultDamperCurve()
        {
            AnimationCurve __ac = new AnimationCurve();
            __ac.AddKey(0f, 0f);
            __ac.AddKey(100f, 400f);
            return __ac;
        }


        public Mesh GenerateRimColliderMesh(Transform rt)
        {
            Mesh __mesh = new Mesh();
            List<Vector3> __vertices = new List<Vector3>();
            List<int> __triangles = new List<int>();

            var __halfWidth = wheel.width / 1.6f;
            float __theta = 0.0f;
            float __startAngleOffset = Mathf.PI / 18.0f;
            float __x = tireRadius * 0.5f * Mathf.Cos(__theta);
            float __y = tireRadius * 0.5f * Mathf.Sin(__theta);
            Vector3 __pos = rt.InverseTransformPoint(wheel.worldPosition + wheel.up * __y + wheel.forward * __x);
            Vector3 __newPos = __pos;

            int __vertexIndex = 0;
            for (__theta = __startAngleOffset; __theta <= Mathf.PI * 2 + __startAngleOffset; __theta += Mathf.PI / 12.0f)
            {
                if (__theta <= Mathf.PI - __startAngleOffset)
                {
                    __x = tireRadius * 0.93f * Mathf.Cos(__theta);
                    __y = tireRadius * 0.93f * Mathf.Sin(__theta);
                }
                else
                {
                    __x = tireRadius * 0.1f * Mathf.Cos(__theta);
                    __y = tireRadius * 0.1f * Mathf.Sin(__theta);
                }

                __newPos = rt.InverseTransformPoint(wheel.worldPosition + wheel.up * __y + wheel.forward * __x);

                // Left Side
                Vector3 __p0 = __pos - rt.InverseTransformDirection(wheel.right) * __halfWidth;
                Vector3 __p1 = __newPos - rt.InverseTransformDirection(wheel.right) * __halfWidth;

                // Right side
                Vector3 __p2 = __pos + rt.InverseTransformDirection(wheel.right) * __halfWidth;
                Vector3 __p3 = __newPos + rt.InverseTransformDirection(wheel.right) * __halfWidth;

                __vertices.Add(__p0);
                __vertices.Add(__p1);
                __vertices.Add(__p2);
                __vertices.Add(__p3);

                // Triangles (double sided)
                // 013
                __triangles.Add(__vertexIndex + 3);
                __triangles.Add(__vertexIndex + 1);
                __triangles.Add(__vertexIndex + 0);

                // 023
                __triangles.Add(__vertexIndex + 0);
                __triangles.Add(__vertexIndex + 2);
                __triangles.Add(__vertexIndex + 3);

                __pos = __newPos;
                __vertexIndex += 4;
            }

            __mesh.vertices = __vertices.ToArray();
            __mesh.triangles = __triangles.ToArray();
            __mesh.RecalculateBounds();
            __mesh.RecalculateNormals();
            __mesh.RecalculateTangents();
            return __mesh;
        }


        /// <summary>
        /// Average of multiple Vector3's
        /// </summary>
        private Vector3 Vector3Average(List<Vector3> vectors)
        {
            Vector3 __sum = Vector3.zero;
            foreach (Vector3 __v in vectors)
            {
                __sum += __v;
            }
            return __sum / vectors.Count;
        }


        /// <summary>
        /// Calculates an angle between two vectors in relation a normal.
        /// </summary>
        /// <param name="v1">First Vector.</param>
        /// <param name="v2">Second Vector.</param>
        /// <param name="n">Angle around this vector.</param>
        /// <returns>Angle in degrees.</returns>
        private float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
        {
            return Mathf.Atan2(
                Vector3.Dot(n, Vector3.Cross(v1, v2)),
                Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Determines on what side of the vehicle a point is. 
        /// </summary>
        /// <param name="pointPosition">Position of the point in question.</param>
        /// <param name="referenceTransform">Position of the reference transform.</param>
        /// <returns>Enum Side [Left,Right] (int)[-1,1]</returns>
        public Side DetermineSide(Vector3 pointPosition, Transform referenceTransform)
        {
            Vector3 __relativePoint = referenceTransform.InverseTransformPoint(pointPosition);

            if (__relativePoint.x < 0.0f)
            {
                return WheelController.Side.Left;
            }
            else
            {
                return WheelController.Side.Right;
            }
        }

        /// <summary>
        /// Determines if layer is in layermask.
        /// </summary>
        public static bool IsInLayerMask(int layer, LayerMask layermask)
        {
            return layermask == (layermask | (1 << layer));
        }


        #endregion

    }
}
