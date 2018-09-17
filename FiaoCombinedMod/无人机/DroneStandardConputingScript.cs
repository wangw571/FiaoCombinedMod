﻿using System;
using UnityEngine;

namespace FiaoCombinedMod
{

    public class DroneStandardConputingScript : Modding.BlockScript
    {
        //public DroneControlBlockBehavior Parent;
        protected GameObject Shooter;
        protected CanonBlock CB;
        protected int iterativeCount = 0;
        public GameObject currentTarget;
        public Vector3 targetPoint;
        protected Vector3 targetVeloRecorder;
        protected Vector3 targetVeloAveraged;
        protected float 炮弹速度;
        protected Vector3 前一帧速度;
        protected float 目标前帧速度Mag;
        protected float MyPrecision;
        public int MySize;
        public float 精度;
        public float size;
        protected GameObject IncomingDetection;
        protected IncomingDetectionScript IDS;
        public bool IgnoreIncoming = false;
        protected Rigidbody rigidBody;
        protected bool NotEvenHavingAJoint = false; 
        protected bool NotEvenHavingAFireTag = false;

        public float HitPoints;
        public Vector3[] IncomingVectors;
        public Vector3 VelocityRecorder;
        protected Vector3 PreviousPosition;

        public override void SafeAwake()
        {
            base.SafeAwake();
        }

        protected Vector2 formulaProjectile(float X, float Y, float V, float G)
        {
            if (G == 0)
            {
                float THETA = Mathf.Atan(Y / X);
                float T = (Y / Mathf.Sin(THETA)) / V;
                return (new Vector2(THETA, T));
            }
            else
            {
                float DELTA = Mathf.Pow(V, 4) - G * (G * X * X - 2 * Y * V * V);
                if (DELTA < 0)
                {
                    return Vector2.zero;
                }
                float THETA1 = Mathf.Atan((-(V * V) + Mathf.Sqrt(DELTA)) / (G * X));
                float THETA2 = Mathf.Atan((-(V * V) - Mathf.Sqrt(DELTA)) / (G * X));
                if (THETA1 > THETA2)
                    THETA1 = THETA2;
                float T = X / (V * Mathf.Cos(THETA1));
                return new Vector2(THETA1, T);
            }
        }

        protected Vector3 formulaTarget(float VT, Vector3 PT, Vector3 DT, float TT)
        {
            Vector3 newPosition = PT + DT * (VT * TT);
            return newPosition;
        }

        protected Vector3 calculateNoneLinearTrajectory(float gunVelocity, float AirDrag, Vector3 gunPosition, float TargetVelocity, Vector3 TargetPosition, Vector3 TargetDirection, Vector3 hitPoint, float Grav, float accuracy, float diff)
        {
            iterativeCount++;
            if (iterativeCount > 128) { iterativeCount = 0; return hitPoint; }
            if (hitPoint == Vector3.zero || gunVelocity < 1)
            {
                return TargetPosition;
            }
            Vector3 gunDirection = new Vector3(hitPoint.x, gunPosition.y, hitPoint.z) - gunPosition;
            Quaternion gunRotation = Quaternion.FromToRotation(gunDirection, Vector3.forward);
            Vector3 localHitPoint = gunRotation * (hitPoint - gunPosition);
            float currentCalculatedDistance = (hitPoint - gunPosition).magnitude;

            float b2M4ac = gunVelocity * gunVelocity - 4 * AirDrag * currentCalculatedDistance;
            if (b2M4ac < 0) { return TargetPosition; }
            float V = (float)Math.Sqrt(b2M4ac);
            float X = localHitPoint.z;//z为前方
            float Y = localHitPoint.y;
            Vector2 TT = formulaProjectile(X, Y, V, Grav);
            if (TT == Vector2.zero)
            {
                iterativeCount = 0;
                return TargetPosition;
            }
            float VT = TargetVelocity;
            Vector3 PT = TargetPosition;
            Vector3 DT = TargetDirection;
            float T = TT.y;
            Vector3 newHitPoint = formulaTarget(VT, PT, DT, T);
            float diff1 = (newHitPoint - hitPoint).magnitude;
            if (diff1 > diff)
            {
                iterativeCount = 0;
                return TargetPosition;
            }
            if (diff1 < accuracy)
            {
                gunRotation = Quaternion.Inverse(gunRotation);
                Y = Mathf.Tan(TT.x) * X;
                newHitPoint = gunRotation * new Vector3(0, Y, X) + gunPosition;
                iterativeCount = 0;
                return newHitPoint;
            }
            return calculateNoneLinearTrajectory(gunVelocity, AirDrag, gunPosition, TargetVelocity, TargetPosition, TargetDirection, newHitPoint, Grav, accuracy, diff1);
        }
        protected Vector3 calculateNoneLinearTrajectoryWithAccelerationPrediction(float gunVelocity, float AirDrag, Vector3 gunPosition, float TargetVelocity, float targetAcceleration, Vector3 TargetPosition, Vector3 TargetDirection, Vector3 hitPoint, float G, float accuracy, float diff)
        {
            iterativeCount++;
            if (iterativeCount > 512) { iterativeCount = 0; return TargetPosition; }
            if (hitPoint == Vector3.zero || gunVelocity < 1)
            {
                return TargetPosition;
            }
            Vector3 gunDirection = new Vector3(hitPoint.x, gunPosition.y, hitPoint.z) - gunPosition;
            Quaternion gunRotation = Quaternion.FromToRotation(gunDirection, Vector3.forward);
            Vector3 localHitPoint = gunRotation * (hitPoint - gunPosition);
            float currentCalculatedDistance = (hitPoint - gunPosition).magnitude;

            float b2M4ac = gunVelocity * gunVelocity - 4 * AirDrag * currentCalculatedDistance;
            if (b2M4ac < 0) { return currentTarget.transform.position; }
            float V = (float)Math.Sqrt(b2M4ac);
            float X = localHitPoint.z;//z为前方
            float Y = localHitPoint.y;
            Vector2 TT = formulaProjectile(X, Y, V, G);
            if (TT == Vector2.zero)
            {
                iterativeCount = 0;
                return TargetPosition;
            }
            float VT = TargetVelocity + targetAcceleration * currentCalculatedDistance;
            Vector3 PT = TargetPosition;
            Vector3 DT = TargetDirection;
            float T = TT.y;
            Vector3 newHitPoint = formulaTarget(VT, PT, DT, T);
            float diff1 = (newHitPoint - hitPoint).magnitude;
            if (diff1 > diff)
            {
                iterativeCount = 0;
                return TargetPosition;
            }
            if (diff1 < accuracy)
            {
                gunRotation = Quaternion.Inverse(gunRotation);
                Y = Mathf.Tan(TT.x) * X;
                newHitPoint = gunRotation * new Vector3(0, Y, X) + gunPosition;
                iterativeCount = 0;
                return newHitPoint;
            }
            return calculateNoneLinearTrajectory(gunVelocity, AirDrag, gunPosition, TargetVelocity, TargetPosition, TargetDirection, newHitPoint, G, accuracy, diff1);
        }
        protected Vector3 calculateLinearTrajectory(float gunVelocity, Vector3 gunPosition, float TargetVelocity, Vector3 TargetPosition, Vector3 TargetDirection)
        {

            Vector3 hitPoint = Vector3.zero;

            if (TargetVelocity != 0)
            {
                Vector3 D = gunPosition - TargetPosition;
                float THETA = Vector3.Angle(D, TargetDirection) * Mathf.Deg2Rad;
                float DD = D.magnitude;

                float A = 1 - Mathf.Pow((gunVelocity / TargetVelocity), 2);
                float B = -(2 * DD * Mathf.Cos(THETA));
                float C = DD * DD;
                float DELTA = B * B - 4 * A * C;

                if (DELTA < 0)
                {
                    return Vector3.zero;
                }

                float F1 = (-B + Mathf.Sqrt(B * B - 4 * A * C)) / (2 * A);
                float F2 = (-B - Mathf.Sqrt(B * B - 4 * A * C)) / (2 * A);

                if (F1 < F2)
                    F1 = F2;
                hitPoint = TargetPosition + TargetDirection * F1;
            }
            else
            {
                hitPoint = TargetPosition;
            }
            return hitPoint;
        }
        protected Vector3 calculateLinearTrajectoryWithAccelerationPrediction(float gunVelocity, Vector3 gunPosition, float TargetVelocity, float TargetAcceleration, Vector3 TargetPosition, Vector3 TargetDirection, Vector3 PredictedPoint, float Precision)
        {

            Vector3 hitPoint = Vector3.zero;

            iterativeCount++;
            if (iterativeCount > 512) { iterativeCount = 0; return calculateLinearTrajectory(gunVelocity, gunPosition, TargetVelocity, TargetPosition, TargetDirection); }

            if (TargetVelocity != 0)
            {
                Vector3 D = gunPosition - TargetPosition;
                float THETA = Vector3.Angle(D, TargetDirection) * Mathf.Deg2Rad;
                float DD = D.magnitude;

                float A = 1 - Mathf.Pow((gunVelocity / TargetVelocity + (TargetAcceleration * (PredictedPoint.magnitude / gunVelocity))), 2);
                float B = -(2 * DD * Mathf.Cos(THETA));
                float C = DD * DD;
                float DELTA = B * B - 4 * A * C;

                if (DELTA < 0)
                {
                    return calculateLinearTrajectory(gunVelocity, gunPosition, TargetVelocity, TargetPosition, TargetDirection);
                }

                float F1 = (-B + Mathf.Sqrt(B * B - 4 * A * C)) / (2 * A);
                float F2 = (-B - Mathf.Sqrt(B * B - 4 * A * C)) / (2 * A);

                if (F1 < F2 && F1 >= 0)
                    F1 = F2;
                hitPoint = TargetPosition + TargetDirection * F1;
            }
            else
            {
                hitPoint = TargetPosition;
            }
            if ((hitPoint - PredictedPoint).sqrMagnitude < Precision * Precision)
            {
                return hitPoint;
            }
            else
            {
                return calculateLinearTrajectoryWithAccelerationPrediction(gunVelocity, gunPosition, TargetVelocity, TargetAcceleration, TargetPosition, TargetDirection, hitPoint, Precision);
            }
        }

        protected Vector3 getCorrTorque(Vector3 from, Vector3 to, Rigidbody rb, float SpeedPerSecond)
        {
            try
            {
                Vector3 x = Vector3.Cross(from.normalized, to.normalized);                // axis of rotation
                float theta = Mathf.Asin(x.magnitude);                                    // angle between from & to
                Vector3 w = x.normalized * theta / SpeedPerSecond;                        // scaled angular acceleration
                Vector3 w2 = w - rb.angularVelocity;                                      // need to slow down at a point
                Quaternion q = rb.rotation * rb.inertiaTensorRotation;                    // transform inertia tensor
                return q * Vector3.Scale(rb.inertiaTensor, (Quaternion.Inverse(q) * w2)); // calculate final torque
            }
            catch { return Vector3.zero; }
        }

        public override void OnSimulateStart()
        {
            /*AD = this.gameObject.AddComponent<BxialDrag>();
            InstantPropeller = GameObject.CreatePrimitive(PrimitiveType.Plane);
            DestroyImmediate(InstantPropeller.GetComponent<MeshCollider>());
            DestroyImmediate(InstantPropeller.GetComponent<MeshRenderer>());
            InstantPropeller.transform.position = new Vector3(0, 0, 1.5f);
            InstantPropeller.transform.rotation = new Quaternion(0, 0, 0, 1);
            AD.GB = this;
            AD.upTransform = InstantPropeller.transform;
            AD.AxisDrag = new Vector3(0, 0.2f, 0.2f);
            AD.velocityCap = 30;
            AD.block = this;
            AD.dragAmount = 1;*/
            MyPrecision = MySize * 5;
            rigidBody = this.GetComponent<Rigidbody>();
        }
        protected Vector3 CalculateTarget(Vector3 LocalTargetDirection, float FireProg)
        {
            炮弹速度 = this.GetComponent<Rigidbody>().velocity.magnitude;
            float targetVelo = currentTarget.GetComponent<Rigidbody>().velocity.magnitude;
            //Debug.Log((currentTarget.GetComponent<Rigidbody>().velocity - 前一帧速度).magnitude);
            LocalTargetDirection = calculateNoneLinearTrajectoryWithAccelerationPrediction(
                炮弹速度 + 0.001f,
                (this.GetComponent<Rigidbody>().velocity - 前一帧速度).magnitude,
                transform.position,
                targetVelo,
                目标前帧速度Mag - targetVelo,
                currentTarget.transform.position,
                currentTarget.GetComponent<Rigidbody>().velocity.normalized,
                    calculateLinearTrajectoryWithAccelerationPrediction(
                        炮弹速度,
                        transform.position,
                        targetVelo,
                        targetVelo - 目标前帧速度Mag,
                        currentTarget.transform.position,
                        currentTarget.GetComponent<Rigidbody>().velocity.normalized,
                        calculateLinearTrajectory(
                            炮弹速度,
                            transform.position,
                            targetVelo,
                            currentTarget.transform.position,
                            currentTarget.GetComponent<Rigidbody>().velocity.normalized),
                        MyPrecision
                    ),
                    Physics.gravity.y,
                    MyPrecision,
                    float.PositiveInfinity
                    );
            目标前帧速度Mag = targetVelo;

            if (LocalTargetDirection.y == float.NaN)
            {
                LocalTargetDirection = currentTarget.transform.position;
            }
            前一帧速度 = GetComponent<Rigidbody>().velocity;
            return LocalTargetDirection;
        }


        protected Vector3 DroneDirectionIndicator(Vector3 CurrentTargetPoistion, float CalculationSpeed)
        {
            float targetVelo = targetVeloAveraged.magnitude;
            Vector3 TargetDirection = targetVeloAveraged.normalized;

            //Debug.Log((currentTarget.GetComponent<Rigidbody>().velocity - 前一帧速度).magnitude);
            /*LocalTargetDirection = calculateNoneLinearTrajectoryWithAccelerationPrediction(
                炮弹速度 + 0.001f,
                (this.GetComponent<Rigidbody>().velocity - 前一帧速度).magnitude,
                transform.position,
                targetVelo,
                目标前帧速度Mag - targetVelo,
                currentTarget.transform.position,
                currentTarget.GetComponent<Rigidbody>().velocity.normalized,
                    calculateLinearTrajectoryWithAccelerationPrediction(
                        炮弹速度,
                        transform.position,
                        targetVelo,
                        targetVelo - 目标前帧速度Mag,
                        currentTarget.transform.position,
                        currentTarget.GetComponent<Rigidbody>().velocity.normalized,
                        calculateLinearTrajectory(
                            炮弹速度,
                            transform.position,
                            targetVelo,
                            currentTarget.transform.position,
                            currentTarget.GetComponent<Rigidbody>().velocity.normalized),
                        size * 精度 + 10 * size
                    ),
                    Physics.gravity.y,
                    size * 精度 + 10 * size,
                    float.PositiveInfinity
                    );*/
            Vector3 LocalTargetDirection = calculateNoneLinearTrajectory(
                CalculationSpeed,
                0.2f,
                this.transform.position,
                targetVelo,
                CurrentTargetPoistion,
                TargetDirection,
                    calculateLinearTrajectory(
                        CalculationSpeed,
                        this.transform.position,
                        targetVelo,
                        CurrentTargetPoistion,
                        TargetDirection
                    ),
                    Physics.gravity.y,
                    size * 精度 + 10 * size,
                    float.PositiveInfinity
                    );
            目标前帧速度Mag = targetVelo;

            if (LocalTargetDirection.y == float.NaN && currentTarget)
            {
                LocalTargetDirection = currentTarget.transform.position;
            }
            前一帧速度 = rigidBody.velocity;
            return LocalTargetDirection;
        }
        protected Vector3 RelativeAverageOfPoints(Vector3[] Vector3s, float SphereSize)
        {
            Vector3 V3 = this.transform.forward * SphereSize / 2;
            if (Vector3s.Length > 0)
            {
                foreach (Vector3 VT3 in Vector3s)
                {
                    Vector3 RealVT3 = (this.transform.InverseTransformPoint(VT3));
                    RealVT3 = RealVT3.normalized * (SphereSize * SphereSize - RealVT3.sqrMagnitude);
                    V3 = Vector3.Lerp(V3, RealVT3, 0.5f);
                }
            }
            return V3;
        }
        public void SetUpHP(float BaseAcceleration)
        {
            this.HitPoints = BaseAcceleration * BaseAcceleration;
        }
        protected void HPCalculation(float MinimumCalcAcceleration)
        {
            Vector3 VelocityNow = this.GetComponent<Rigidbody>().velocity;
            float AccelerationAmountSqr = (VelocityNow - VelocityRecorder).sqrMagnitude;
            if (AccelerationAmountSqr >= MinimumCalcAcceleration * MinimumCalcAcceleration)
            {
                HitPoints -= AccelerationAmountSqr;
            }
            VelocityRecorder = VelocityNow;
        }
        public Vector3 EulerToDirection(float Elevation, float Heading)
        {
            float elevation = Elevation * Mathf.Deg2Rad;
            float heading = Heading * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(elevation) * Mathf.Sin(heading), Mathf.Sin(elevation), Mathf.Cos(elevation) * Mathf.Cos(heading));
        }
        protected void LogHo()
        {
            Debug.Log("ho");
        }
    }
}
