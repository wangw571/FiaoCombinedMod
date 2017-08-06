using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using spaar.ModLoader;

namespace FiaoCombinedMod
{
    public class BasicTrackingComputerBehavior : BlockScript
    {
        protected GameObject currentTarget;
        protected GameObject currentLocking;

        protected MToggle UseLockingWindow;

        public int OnScreenCloseEnoughDistSqr = 900;

        protected PilotPanelScript ParentPilotPanel;

        //protected Vector3 targetPoint;
        protected Vector3 CorrTorq;
        public int iterativeCount = 0;
        public float LockingTimer = -1;
        public bool IsInMovieMode = false;
        protected bool NoSign = false;
        protected float 转速乘子 = 1f;

        protected bool NotEvenHavingAFireTag = false;
        protected bool NotEvenHavingAJoint = false;

        public bool UsingGUITargetAcquier = false;

        protected float 累计质量;

        public static SettingsButton MovieMode;

        protected delegate void Init();

        public Vector2 formulaProjectile(float X, float Y, float V, float G)
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

        public Vector3 formulaTarget(float VT, Vector3 PT, Vector3 DT, float TT)
        {
            Vector3 newPosition = PT + DT * (VT * TT);
            return newPosition;
        }

        public Vector3 calculateNoneLinearTrajectory(float gunVelocity, float AirDrag, Vector3 gunPosition, float TargetVelocity, Vector3 TargetPosition, Vector3 TargetDirection, Vector3 hitPoint, float G, float accuracy, float diff)
        {
            iterativeCount++;
            if (iterativeCount > 512) { iterativeCount = 0; return hitPoint; }
            if (hitPoint == Vector3.zero || gunVelocity < 1)
            {
                return currentTarget.transform.position;
            }
            Vector3 gunDirection = new Vector3(hitPoint.x, gunPosition.y, hitPoint.z) - gunPosition;
            Quaternion gunRotation = Quaternion.FromToRotation(gunDirection, Vector3.forward);
            Vector3 localHitPoint = gunRotation * (hitPoint - gunPosition);
            float currentCalculatedDistance = (hitPoint - gunPosition).magnitude;

            float b2M4ac = gunVelocity * gunVelocity - 4 * AirDrag * currentCalculatedDistance;
            if (b2M4ac < 0) { /*Debug.Log("Nan!!!" + (gunVelocity * gunVelocity - 2 * AirDrag * currentCalculatedDistance));*/ return currentTarget.transform.position; }
            float V = (float)Math.Sqrt(b2M4ac);
            float X = localHitPoint.z;//z为前方
            float Y = localHitPoint.y;
            Vector2 TT = formulaProjectile(X, Y, V, G);
            if (TT == Vector2.zero)
            {
                iterativeCount = 0;
                return currentTarget.transform.position;
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
                return currentTarget.transform.position;
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
        public Vector3 calculateNoneLinearTrajectoryWithAccelerationPrediction(float gunVelocity, float AirDrag, Vector3 gunPosition, float TargetVelocity, float targetAcceleration, Vector3 TargetPosition, Vector3 TargetDirection, Vector3 hitPoint, float G, float accuracy, float diff)
        {
            iterativeCount++;
            if (iterativeCount > 512) { iterativeCount = 0; return TargetPosition; }
            if (hitPoint == Vector3.zero || gunVelocity < 1)
            {
                return currentTarget.transform.position;
            }
            Vector3 gunDirection = new Vector3(hitPoint.x, gunPosition.y, hitPoint.z) - gunPosition;
            Quaternion gunRotation = Quaternion.FromToRotation(gunDirection, Vector3.forward);
            Vector3 localHitPoint = gunRotation * (hitPoint - gunPosition);
            float currentCalculatedDistance = (hitPoint - gunPosition).magnitude;

            float b2M4ac = gunVelocity * gunVelocity - 4 * AirDrag * currentCalculatedDistance;
            if (b2M4ac < 0) { /*Debug.Log("Nan!!!" + (gunVelocity * gunVelocity - 2 * AirDrag * currentCalculatedDistance));*/ return currentTarget.transform.position; }
            float V = (float)Math.Sqrt(b2M4ac);
            float X = localHitPoint.z;//z为前方
            float Y = localHitPoint.y;
            Vector2 TT = formulaProjectile(X, Y, V, G);
            if (TT == Vector2.zero)
            {
                iterativeCount = 0;
                return currentTarget.transform.position;
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
                return currentTarget.transform.position;
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
        public Vector3 calculateLinearTrajectory(float gunVelocity, Vector3 gunPosition, float TargetVelocity, Vector3 TargetPosition, Vector3 TargetDirection)
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
        public Vector3 calculateLinearTrajectoryWithAccelerationPrediction(float gunVelocity, Vector3 gunPosition, float TargetVelocity, float TargetAcceleration, Vector3 TargetPosition, Vector3 TargetDirection, Vector3 PredictedPoint, float Precision)
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

        public Vector3 getCorrTorque(Vector3 from, Vector3 to, Rigidbody rb, float SpeedPerSecond)
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

        protected void OnGUI()
        {
            //RegularSphereScan(Vector3.zero, 15, 15);
            int size = 30;
            float SettingLockingTimer = spaar.ModLoader.Configuration.GetFloat("LockingTimer", 4);
            float SizeMultiplier = LockingTimer / (SettingLockingTimer + 0.000001f) * 4;

            if (NoSign) return;
            if (LockingTimer >= SettingLockingTimer * 3 / 4)
            {
                if (Camera.main.WorldToScreenPoint(currentLocking.transform.position).z > 0)
                {
                    GUI.DrawTexture(
                           new Rect(
                           new Vector2(Camera.main.WorldToScreenPoint(currentLocking.transform.position).x - SizeMultiplier * size / 2, Screen.height - (Camera.main.WorldToScreenPoint(currentLocking.transform.position).y) - SizeMultiplier * size / 2),
                           new Vector2(SizeMultiplier * size, SizeMultiplier * size))
                           , resources["Target 0.png"].texture
                           );
                }
            }
            else if (LockingTimer >= SettingLockingTimer / 2)
            {
                if (Camera.main.WorldToScreenPoint(currentLocking.transform.position).z > 0)
                {
                    GUI.DrawTexture(
                   new Rect(
                   new Vector2(Camera.main.WorldToScreenPoint(currentLocking.transform.position).x - SizeMultiplier * size / 2, Screen.height - (Camera.main.WorldToScreenPoint(currentLocking.transform.position).y) - SizeMultiplier * size / 2),
                   new Vector2(SizeMultiplier * size, SizeMultiplier * size))
                   , resources["Target 1.png"].texture
                   );
                }
            }
            else if (LockingTimer >= SettingLockingTimer / 4)
            {
                if (Camera.main.WorldToScreenPoint(currentLocking.transform.position).z > 0)
                {
                    GUI.DrawTexture(
           new Rect(
           new Vector2(Camera.main.WorldToScreenPoint(currentLocking.transform.position).x - SizeMultiplier * size / 2, Screen.height - (Camera.main.WorldToScreenPoint(currentLocking.transform.position).y) - SizeMultiplier * size / 2),
           new Vector2(SizeMultiplier * size, SizeMultiplier * size))
           , resources["Target 2.png"].texture
           );
                }
            }
            else if (LockingTimer >= 0)
            {
                if (Camera.main.WorldToScreenPoint(currentLocking.transform.position).z > 0)
                {
                    GUI.DrawTexture(
                    new Rect(
                     new Vector2(Camera.main.WorldToScreenPoint(currentLocking.transform.position).x - size / 2, Screen.height - (Camera.main.WorldToScreenPoint(currentLocking.transform.position).y) - size / 2),
                     new Vector2(size, size)),

                    resources["Target 3.png"].texture
                   );
                }
            }
            else if (currentLocking != null)
            {
                if (Camera.main.WorldToScreenPoint(currentLocking.transform.position).z > 0)
                {
                    GUI.DrawTexture(
                        new Rect(
                            new Vector2(Camera.main.WorldToScreenPoint(currentLocking.transform.position).x - size / 2, Screen.height - (Camera.main.WorldToScreenPoint(currentLocking.transform.position).y) - size / 2),
                            new Vector2(size, size)),

                        resources["Targeted.png"].texture
                        );
                }
                currentTarget = currentLocking;
            }
        }

        void RegularSphereScan(Vector3 StartPoint, float HorizontalDegreePrecision, float VerticalDegreePrecision)
        {
            for (float Vi = VerticalDegreePrecision; Vi < 360; Vi += VerticalDegreePrecision)
            {
                for (float Hi = 0; Hi < 360; Hi += HorizontalDegreePrecision)
                {
                    float elevation = Hi * Mathf.Deg2Rad;
                    float heading = Vi * Mathf.Deg2Rad;
                    GUI.DrawTexture(
                           new Rect(
                           new Vector2(
                               Camera.main.WorldToScreenPoint(new Vector3(Mathf.Cos(elevation) * Mathf.Sin(heading), Mathf.Sin(elevation), Mathf.Cos(elevation) * Mathf.Cos(heading)) * 20).x - 7.5f,
                               Screen.height - (Camera.main.WorldToScreenPoint(new Vector3(Mathf.Cos(elevation) * Mathf.Sin(heading), Mathf.Sin(elevation), Mathf.Cos(elevation) * Mathf.Cos(heading)) * 20).y) - 7.5f),
                           new Vector2(15, 15))
                           , resources["Target 0.png"].texture
                           );
                }
            }
        }

        protected Vector3 EulerDifference(Vector3 From, Vector3 To)
        {
            return new Vector3(Math.Abs(From.x) - Math.Abs(To.x), Math.Abs(From.y) - Math.Abs(To.y), Math.Abs(From.z) - Math.Abs(To.z));
        }

        protected Vector3 MultiplyXAndZ(Vector3 vector3, float Multiplier)
        {
            return new Vector3(vector3.x * Multiplier, vector3.y, vector3.z * Multiplier);
        }

        protected void LogHo()
        {
            Debug.Log("Ho!");
        }

        protected GameObject TargetSelector()
        {
            List<MachineTrackerMyId> BBList = new List<MachineTrackerMyId>();
            List<int> ThreatMultiplier = new List<int>();
            if (UsingGUITargetAcquier)
            {
                foreach (MachineTrackerMyId BB in FindObjectsOfType<MachineTrackerMyId>())
                {
                    if (!ParentPilotPanel.AutoLockRange.Contains(Camera.main.WorldToScreenPoint(BB.transform.position)) || (BB.gameObject.GetComponent<TargetPainter>() && BB.gameObject.GetComponent<TargetPainter>().OnMe != null))
                    {
                        continue;
                    }
                    NotEvenHavingAFireTag = BB.gameObject.GetComponent<FireTag>() == null;
                    int BBID = BB.myId;

                    if (BBID == 23 || BBID == 525 || BBID == 526 || BBID == 540 || BBID == 519)//Bombs, Tracking Computers
                    {
                        if (NotEvenHavingAFireTag)
                        {
                            BBList.Add(BB);
                            ThreatMultiplier.Add(15);
                        }
                        else if (!BB.gameObject.GetComponent<FireTag>().burning)
                        {
                            BBList.Add(BB);
                            ThreatMultiplier.Add(15);
                        }
                    }
                    else if (BBID == 59 || BBID == 54 || BBID == 43 || BBID == 61)//Rocket & Grenade
                    {
                        BBList.Add(BB);
                        ThreatMultiplier.Add(12);
                    }
                    else if (BBID == 14 || BBID == 2 || BBID == 46 || BBID == 39)//Locomotion && Proplusion
                    {
                        if (BB.gameObject.GetComponent<ConfigurableJoint>() != null)
                        {
                            if (NotEvenHavingAFireTag)
                            {
                                BBList.Add(BB);
                                ThreatMultiplier.Add(10);
                            }
                            else if (!BB.gameObject.GetComponent<FireTag>().burning)
                            {
                                BBList.Add(BB);
                                ThreatMultiplier.Add(10);
                            }
                        }
                    }
                    else if (BBID == 26 || BBID == 55 || BBID == 52)//Propellers and balloon
                    {
                        BBList.Add(BB);
                        ThreatMultiplier.Add(8);
                    }
                    else if (BBID == 34 || BBID == 25  /**/  || BBID == 28 || BBID == 4 || BBID == 18 || BBID == 27 || BBID == 3 || BBID == 20)//Large Aero Blocks/Mechanic Blocks
                    {
                        if (BB.gameObject.GetComponent<ConfigurableJoint>() != null)
                        {
                            if (NotEvenHavingAFireTag)
                            {
                                BBList.Add(BB);
                                ThreatMultiplier.Add(4);
                            }
                            else if (!BB.gameObject.GetComponent<FireTag>().burning)
                            {
                                BBList.Add(BB);
                                ThreatMultiplier.Add(4);
                            }
                        }
                    }
                    else if (BBID == 35 || BBID == 16 || BBID == 42 /**/ || BBID == 40 || BBID == 60 || BBID == 38 || BBID == 51 /**/ || BBID == 1 || BBID == 15 || BBID == 41 || BBID == 5)//Structure Block
                    {
                        if (BB.gameObject.GetComponent<ConfigurableJoint>() != null)
                        {
                            if (NotEvenHavingAFireTag)
                            {
                                BBList.Add(BB);
                                ThreatMultiplier.Add(1);
                            }
                            else if (!BB.gameObject.GetComponent<FireTag>().burning)
                            {
                                BBList.Add(BB);
                                ThreatMultiplier.Add(1);
                            }
                        }
                    }
                    break;
                }
                foreach (MachineTrackerMyId BB2 in BBList.ToArray())
                {
                    //删除不符合条件的结果
                    int RemoveId;
                    if (!ParentPilotPanel.AutoLockRange.Contains(Camera.main.WorldToScreenPoint(BB2.transform.position)) || (BB2.gameObject.GetComponent<TargetPainter>() && BB2.gameObject.GetComponent<TargetPainter>().OnMe != null))
                    {
                        RemoveId = BBList.IndexOf(BB2);
                        BBList.RemoveAt(RemoveId);
                        ThreatMultiplier.RemoveAt(RemoveId);
                        continue;
                    }
                }
                if (BBList.Count == 0)
                {
                    return null;
                    //targetPoint = this.transform.TransformPoint(EulerToDirection(this.transform.eulerAngles.x, 45) * 200);
                    //if ((this.transform.position - targetPoint).sqrMagnitude <= 100 || targetPoint.y <= 45)
                    //{
                    //    this.targetPoint = new Vector3(UnityEngine.Random.value * 1400 - 700, 500, UnityEngine.Random.value * 1400 - 700);
                    //}
                    //Debug.Log(targetPoint);
                    //IAmEscapingOrReturning = true;
                }
                else
                {
                    Camera MainC = Camera.main;
                    foreach (MachineTrackerMyId BNB in BBList)
                    {
                        int Index = BBList.IndexOf(BNB);
                        if (ThreatMultiplier.Count == Index + 1)
                        {
                            return (BNB.gameObject);
                        }

                        while (ThreatMultiplier[Index + 1] <= ThreatMultiplier[Index])
                        {
                            ThreatMultiplier.RemoveAt(Index + 1);
                            BBList.RemoveAt(Index + 1);
                        }
                        Vector3 Diff = MainC.WorldToScreenPoint(BNB.transform.position) - MainC.WorldToScreenPoint(BBList[Index + 1].transform.position);
                        if (new Vector3(Diff.x, Diff.y, 0).sqrMagnitude < OnScreenCloseEnoughDistSqr)
                        {
                            ThreatMultiplier.RemoveAt(Index);
                            BBList.RemoveAt(Index);
                            continue;
                        }
                        else
                        {
                            NotEvenHavingAJoint = BNB.gameObject.GetComponent<ConfigurableJoint>() == null;
                            NotEvenHavingAFireTag = BNB.gameObject.GetComponent<FireTag>() == null;
                            if (BNB.gameObject.GetComponent<ConfigurableJoint>() != null)
                            {
                                BNB.gameObject.GetComponent<ConfigurableJoint>().breakForce = Mathf.Min(currentTarget.GetComponent<ConfigurableJoint>().breakForce, 45000);
                            }
                            return (BNB.gameObject);
                        }
                    }
                    return null;
                }
            }
            return null;
        }

        //protected void CheckIfAvailablePilotPanelExists(bool IS)
        //{
        //    UseLockingWindow.IsActive = false;
        //    if (!IS) return;
        //    foreach (PilotPanelScript PPS in Machine.Active().BuildingMachine.GetComponentsInChildren<PilotPanelScript>())
        //    {
        //        if (PPS.UseLockWindow.IsActive)
        //        {
        //            UseLockingWindow.IsActive = true;
        //            return;
        //        }
        //    }
        //}

    }
    public class TrackingComputer : BasicTrackingComputerBehavior
    {

        //public bool UseChinese = true;
        protected MKey Key1;
        protected MKey Key2;
        protected MSlider 炮力;
        protected MSlider 精度;
        protected MSlider 计算间隔;
        protected MMenu 模式;
        protected MMenu MissileGuidanceMode;
        protected MKey MissileGuidanceModeSwitchButton;
        protected MSlider MissileHeightController;
        protected MSlider MissileTorqueSlider;
        //protected MToggle 不聪明模式;

        private bool IsOverLoaded = false;
        private AudioSource Audio;

        public float 炮弹速度;
        public Vector3 前一帧速度;
        private float size;
        private float RotatingSpeed = 4f;
        private float TurretRotatingSpeed = 0.3f;
        public float 记录器 = 0;
        public Vector3 默认朝向;
        public Transform 只有一门炮也是没有问题的;
        public bool 但是有两门炮 = false;
        public float 目标前帧速度Mag = 0;

        public int MissileGuidanceModeInt;
        //public float MaxAcceleration = 0;

        public override void SafeAwake()
        {
            MovieMode = FiaoCombinedMod.MovieMode;
            IsInMovieMode = MovieMode.Value;
            Init init = Configuration.GetBool("UseChinese", false) ? new Init(ChineseInitialize) : new Init(EnglishInitialize);
            init();

            if (!spaar.ModLoader.Configuration.DoesKeyExist("MovieMode"))
            {
                spaar.ModLoader.Configuration.SetBool("MovieMode", IsInMovieMode);
                spaar.ModLoader.Configuration.Save();
            }
            IsInMovieMode = spaar.ModLoader.Configuration.GetBool("MovieMode", false);

            //UseLockingWindow.Toggled += CheckIfAvailablePilotPanelExists;
        }

        void ChineseInitialize()
        {
            Key1 = AddKey("获取目标", //按键信息
                                 "Locked",           //名字
                                 KeyCode.T);       //默认按键

            Key2 = AddKey("解除锁定", //按键信息2
                                 "RLocked",           //名字
                                 KeyCode.Slash);       //默认按键
            List<string> chaos = new List<String> { "炮塔索敌计算机", "导弹导引计算机", "相机跟踪计算机" };
            模式 = AddMenu("Mode", 0, chaos);

            炮力 = AddSlider("炮力",       //滑条信息
                                        "strength",       //名字
                                        1f,            //默认值
                                        0f,          //最小值
                                        2f);           //最大值

            精度 = AddSlider("精度",       //滑条信息
                                        "Precision",       //名字
                                        0.5f,            //默认值
                                        0.01f,          //最小值
                                        10f);           //最大值

            计算间隔 = AddSlider("每秒计算频率",       //滑条信息 
                                        "CalculationPerSecond",       //名字
                                        100f,            //默认值
                                        1f,          //最小值
                                        100f);           //最大值

            MissileGuidanceMode = AddMenu("MissileMode", 0, new List<string> { "预测目标运动", "直接跟随", "攻顶", "攻底" });

            MissileGuidanceModeSwitchButton = AddKey("切换导引模式", "GuideModeSwitch", KeyCode.RightControl);

            MissileHeightController = AddSlider("目标上方高度", "Height", 100, 0, 1500);

            MissileTorqueSlider = AddSlider("导弹扭矩乘子", "Torque", 4, 0.1f, 10f);

            UseLockingWindow = AddToggle("使用自动锁定窗口", "LockWindow", false);
        }
        void EnglishInitialize()
        {
            Key1 = AddKey("Lock On", //按键信息
                                 "Locked",           //名字
                                 KeyCode.T);       //默认按键

            Key2 = AddKey("Release Lock", //按键信息2
                                 "RLocked",           //名字
                                 KeyCode.Slash);       //默认按键
            List<string> chaos = new List<String> { "Turret Tracking \nComputer", "Missile Guidance \nComputer", "Camera Tracking \nComputer" };
            模式 = AddMenu("Mode", 0, chaos);

            炮力 = AddSlider("Cannon Slider",       //滑条信息
                                    "strength",       //名字
                                    1f,            //默认值
                                    0f,          //最小值
                                    2f);           //最大值

            精度 = AddSlider("Precision",       //滑条信息
                                    "Precision",       //名字
                                    0.5f,            //默认值
                                    0.01f,          //最小值
                                    10f);           //最大值

            计算间隔 = AddSlider("Calculation per second",       //滑条信息 
                                    "CalculationPerSecond",       //名字
                                    100f,            //默认值
                                    1f,          //最小值
                                    100f);           //最大值

            MissileGuidanceMode = AddMenu("MissileMode", 0, new List<string> { "Calculate \nTarget Speed", "Directly Follow", "From Top", "From Bottom" });

            MissileGuidanceModeSwitchButton = AddKey("Switch Guide Mode", "GuideModeSwitch", KeyCode.RightControl);

            MissileHeightController = AddSlider("Height above target", "Height", 100, 0, 1500);

            MissileTorqueSlider = AddSlider("Torque Multiplier", "Torque", 4, 0.1f, 10f);

            UseLockingWindow = AddToggle("Enable Autolock window", "LockWindow", false);
        }
        protected virtual IEnumerator UpdateMapper()
        {
            if (BlockMapper.CurrentInstance == null)
                yield break;
            while (Input.GetMouseButton(0))
                yield return null;
            BlockMapper.CurrentInstance.Copy();
            BlockMapper.CurrentInstance.Paste();
            yield break;
        }
        public override void OnSave(XDataHolder data)
        {
            SaveMapperValues(data);
        }
        public override void OnLoad(XDataHolder data)
        {
            LoadMapperValues(data);
            if (data.WasSimulationStarted) return;
        }



        protected override void BuildingUpdate()
        {

            UseLockingWindow.DisplayInMapper = Machine.Active().BuildingMachine.GetComponentInChildren<PilotPanelScript>() && 模式.Value != 2;

            if (!spaar.ModLoader.Configuration.DoesKeyExist("MovieMode"))
            {
                spaar.ModLoader.Configuration.SetBool("MovieMode", false);
                spaar.ModLoader.Configuration.Save();
            }
            IsInMovieMode = spaar.ModLoader.Configuration.GetBool("MovieMode", false);

            bool IsMissileMode = 模式.Value == 1;

            炮力.DisplayInMapper = !IsMissileMode;
            计算间隔.DisplayInMapper = 模式.Value != 3;
            精度.DisplayInMapper = 模式.Value != 3;
            MissileGuidanceMode.DisplayInMapper = IsMissileMode;
            MissileGuidanceModeSwitchButton.DisplayInMapper = IsMissileMode;
            MissileTorqueSlider.DisplayInMapper = IsMissileMode;

            if (MissileGuidanceMode.Value == 2)
            {
                MissileHeightController.DisplayInMapper = true;
                MissileHeightController.DisplayName = Configuration.GetBool("UseChinese", false) ? "目标上方高度" : "Height Above Target";
            }
            else if (MissileGuidanceMode.Value == 3)
            {
                MissileHeightController.DisplayInMapper = true;
                MissileHeightController.DisplayName = Configuration.GetBool("UseChinese", false) ? "高度" : "Height Above Ground";
            }
            else
            {
                MissileHeightController.DisplayInMapper = false;
            }
            MissileGuidanceModeInt = MissileGuidanceMode.Value;
            ModelReplacer();
            MissileColorApplier();
        }


        protected override void OnSimulateStart()
        {
            currentTarget = null;
            炮弹速度 = 炮力.Value * 58;
            MissileGuidanceModeInt = MissileGuidanceMode.Value;
            Audio = this.gameObject.AddComponent<AudioSource>();
            Audio.clip = resources["炮台旋转音效.ogg"].audioClip;
            Audio.loop = false;
            Audio.volume = 0.2f;
            ConfigurableJoint conf = this.GetComponent<ConfigurableJoint>();
            conf.breakForce = Mathf.Infinity;
            conf.breakTorque = Mathf.Infinity;
            conf.angularZMotion = ConfigurableJointMotion.Locked;
            try
            {
                默认朝向 = this.transform.forward;
            }
            catch { 默认朝向 = Vector3.zero; }

            ModelReplacer();
            MissileColorApplier();

            //this.GetComponent<Rigidbody>().angularDrag = 20;
            //this.GetComponent<Rigidbody>().maxAngularVelocity = 2f;
        }
        protected override void OnSimulateUpdate()
        {
            IsInMovieMode = MovieMode.Value;

            //Trail.GetComponent<TrailRenderer>().material.color = Color.white;
            if (Key1.IsPressed && !HasBurnedOut())
            {
                foreach (RaycastHit hitt in Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition)))
                {
                    if (hitt.transform.position != this.transform.position && hitt.collider.attachedRigidbody != null && !hitt.collider.isTrigger)
                    {
                        currentLocking = hitt.transform.gameObject;

                        if (模式.Value == 2)
                        {
                            NoSign = true;
                            currentTarget = hitt.transform.gameObject;
                            CameraTrackingComputerMode();
                        }

                        LockingTimer = spaar.ModLoader.Configuration.GetFloat("LockingTimer", 4);
                        LockingTimer = 0;
                        if (!IsInMovieMode)
                        {
                            if (currentLocking.GetComponentInParent<MachineTrackerMyId>() || this.name.Contains(("IsCloaked")))
                            {
                                if (currentLocking.GetComponentInParent<MachineTrackerMyId>().gameObject.name.Contains("IsCloaked") || this.name.Contains(("IsCloaked")))
                                {
                                    currentLocking = null;
                                    LockingTimer = -1;
                                }
                            }
                        }
                    }
                }
            }

            if (Key2.IsPressed)
            {
                currentLocking = null; LockingTimer = -1;
                currentTarget = null;
            }
            if (MissileGuidanceModeSwitchButton.IsPressed && currentLocking == null)
            {
                MissileGuidanceMode.Value++;
                if (MissileGuidanceMode.Value > MissileGuidanceMode.Items.Count - 1) { MissileGuidanceMode.Value = 0; }
                MissileGuidanceModeInt = MissileGuidanceMode.Value;
            }
            MissileColorApplier();

            if (HasBurnedOut())
            {
                currentLocking = null; LockingTimer = -1;
                currentTarget = null;
            }

            if (模式.Value == 2)
            {
                NoSign = true;
                CameraTrackingComputerMode();
            }
            else if (模式.Value == 0)
            {
                foreach (Joint Jo in this.GetComponent<BlockBehaviour>().jointsToMe)
                {
                    if (Jo.GetComponentInParent<CanonBlock>())
                    {
                        CanonBlock cb = Jo.GetComponentInParent<CanonBlock>();
                        if (!IsOverLoaded)
                        {
                            cb.knockbackSpeed = 4250;
                        }
                        else { cb.knockbackSpeed = 8000; }
                        if (jointsToMe.Count == 1)
                        {
                            只有一门炮也是没有问题的 = cb.transform;
                        }
                    }
                    但是有两门炮 = 只有一门炮也是没有问题的 == null;
                }
            }

        }
        protected override void OnSimulateFixedUpdate()
        {
            LockingTimer -= Time.deltaTime;

            if (HasBurnedOut() && !StatMaster.GodTools.UnbreakableMode) return;
            if (currentLocking != null)
                if (!IsInMovieMode)
                {
                    if (currentLocking.GetComponentInParent<MachineTrackerMyId>())
                    {
                        if (currentLocking.GetComponentInParent<MachineTrackerMyId>().gameObject.name.Contains("IsCloaked") || this.name.Contains(("IsCloaked")))
                        {
                            currentLocking = null;
                            LockingTimer = -1;
                        }
                    }
                    else if (currentLocking.gameObject.name == "FieldDetector")
                    {
                        foreach (Transform block in Machine.Active().SimulationMachine)
                        {
                            if (block.name.Contains("Improved") && (block.position - currentLocking.transform.position).sqrMagnitude < 1)
                            {
                                currentLocking = block.gameObject;
                                break;
                            }
                            else
                            {
                                currentLocking = null;
                                LockingTimer = -1;
                            }
                        }
                    }
                }
            累计质量 = this.rigidbody.mass;

            foreach (Joint aRB in this.GetComponent<BlockBehaviour>().jointsToMe)
            {
                if (aRB != null)
                {
                    aRB.gameObject.GetComponentInParent<BlockBehaviour>().Rigidbody.centerOfMass = (Vector3)aRB.gameObject?.GetComponentInParent<BlockBehaviour>()?.gameObject?.transform?.InverseTransformPoint(this.transform.position);
                    累计质量 += aRB.gameObject.GetComponentInParent<BlockBehaviour>().Rigidbody.mass;
                }
            }

            if (!IsInMovieMode)
            {
                if (!IsOverLoaded && !StatMaster.GodTools.UnbreakableMode)
                {
                    bool aha;
                    aha = ((前一帧速度 - this.rigidbody.velocity).sqrMagnitude >= 12500f && 模式.Value == 0) || (模式.Value == 1 && (前一帧速度 - this.rigidbody.velocity).sqrMagnitude >= 1000f);
                    if (aha)
                    {
                        OverLoadExplosion();
                        this.GetComponent<FireTag>().Ignite();
                        IsOverLoaded = true;
                    }
                    /*else
                    {
                        MaxAcceleration = Math.Max(MaxAcceleration, (前一帧速度 - this.rigidbody.velocity).sqrMagnitude);
                        Debug.Log(MaxAcceleration);
                    }*/
                }
                else
                {
                    spaar.ModLoader.ModConsole.AddMessage(LogType.Warning, "Overloaded! Tracking Computer CPU Damaged!");
                    Destroy(GetComponent<TrackingComputer>());
                }
            }
            if (模式.Value == 0 && !IsOverLoaded)
            {
                TurretTrackingComputerMode();
            }
            else if (模式.Value == 1 && !IsOverLoaded)
            {
                MissileGuidanceComputerMode();
            }


        }
        void TurretTrackingComputerMode()
        {
            float Random = 1 + (0.5f - UnityEngine.Random.value * 0.1f);
            size = 1 * this.transform.localScale.x * this.transform.localScale.y * this.transform.localScale.z;
            this.GetComponent<Rigidbody>().mass = 2f * size;
            float FireProg = this.GetComponentInChildren<FireController>().fireProgress;
            if (只有一门炮也是没有问题的 && !但是有两门炮)
            {
                炮弹速度 = 只有一门炮也是没有问题的.GetComponent<BlockBehaviour>().Sliders[0].Value * 55;
            }
            记录器 += (计算间隔.Value / 100) * (1 - FireProg);
            Vector3 LocalTargetDirection = this.transform.position + 默认朝向 * 2;

            if (currentTarget)
            {
                if (currentTarget.GetComponent<Rigidbody>() && currentTarget.transform.position != this.transform.position)
                {
                    LocalTargetDirection = currentTarget.transform.position;
                    if (炮力.Value != 0 && 记录器 >= 1)
                    {
                        float targetVelo = currentTarget.GetComponent<Rigidbody>().velocity.magnitude;
                        记录器 = 0;
                        LocalTargetDirection = calculateNoneLinearTrajectory(
                            炮弹速度 + Random,
                            0.2f,
                            this.transform.position,
                            targetVelo,
                            currentTarget.transform.position,
                            currentTarget.GetComponent<Rigidbody>().velocity.normalized,
                                calculateLinearTrajectory(
                                    炮弹速度 + Random,
                                    this.transform.position,
                                    targetVelo,
                                    currentTarget.transform.position,
                                    currentTarget.GetComponent<Rigidbody>().velocity.normalized
                                ),
                                Physics.gravity.y,
                                size * 精度.Value + 10 * size * FireProg,
                                float.PositiveInfinity
                                );
                    }
                }
            }
            //this.transform.rotation.SetFromToRotation(this.transform.forward, LocalTargetDirection);
            Vector3 rooo = Vector3.RotateTowards(this.transform.forward, LocalTargetDirection - this.transform.position, TurretRotatingSpeed * size, TurretRotatingSpeed * size);
            //Debug.Log(LocalTargetDirection + "and" + this.transform.up + "and" + rooo);
            //this.transform.rotation = Quaternion.LookRotation(rooo);
            //LocalTargetDirection = new Vector3(LocalTargetDirection.x, LocalTargetDirection.y - this.transform.position.y, LocalTargetDirection.z);
            float Difference = Vector3.Angle(transform.forward, LocalTargetDirection - this.transform.position * 1);
            //if (Difference > 精度.Value)
            //{
            //    calculated = (getCorrTorque(this.transform.forward, LocalTargetDirection - this.transform.position * 1, this.GetComponent<Rigidbody>(), 0.01f * size) * Mathf.Rad2Deg).normalized * TurretRotatingSpeed;
            //}
            //else
            //{
            //    calculated = (getCorrTorque(this.transform.forward, LocalTargetDirection - this.transform.position * 1, this.GetComponent<Rigidbody>(), 0.01f * size) * Mathf.Rad2Deg);
            //}
            //Vector3 Thera = EulerDifference((MultiplyXAndZ(CorrTorq.normalized, 转速乘子) * TurretRotatingSpeed * (累计质量 / this.rigidbody.mass)), this.Rigidbody.angularVelocity);
            //转速乘子 -= Math.Min(Thera.x, Thera.z) * Mathf.Deg2Rad;

            //Debug.Log(Math.Round(转速乘子, 5) + " " + (LocalTargetDirection - this.transform.position).normalized + " " + Math.Min(1 / ((转速乘子 * 100)), 0.005f));
            转速乘子 *= Mathf.Sign((this.transform.forward - (LocalTargetDirection - this.transform.position).normalized).y) == -1 ? 1 + Math.Min(1 / (转速乘子 * 100), 0.005f) : 0.2f;
            转速乘子 = Math.Max(1, 转速乘子);
            CorrTorq = getCorrTorque(
                                this.transform.forward,
                                LocalTargetDirection - this.transform.position * 1,
                                this.GetComponent<Rigidbody>(), 0.01f * size
                                )
                            * Mathf.Rad2Deg;




            this.Rigidbody.angularVelocity = (
                MultiplyXAndZ(CorrTorq.normalized, 转速乘子)
                * TurretRotatingSpeed * (累计质量 / this.rigidbody.mass));


            float mag = (this.transform.forward.normalized - LocalTargetDirection.normalized).magnitude;
            //Debug.Log(Vector3.Angle(transform.forward, LocalTargetDirection - this.transform.position * 1));
            if (Vector3.Angle(transform.forward, LocalTargetDirection - this.transform.position * 1) > 精度.Value)
            {

                Audio.volume = mag * 0.2f * Math.Max((10 / (Vector3.Distance(this.transform.position, GameObject.Find("Main Camera").transform.position))), 1);
                Audio.Play();
            }
            else
            {
                //if (炮力.Value != 0) this.GetComponent<Rigidbody>().freezeRotation = true;
                Audio.Stop();
            }
            前一帧速度 = this.rigidbody.velocity;

        }
        void MissileGuidanceComputerMode()
        {
            size = 1 * this.transform.localScale.x * this.transform.localScale.y * this.transform.localScale.z;
            this.GetComponent<Rigidbody>().mass = 2f * size;

            /*if (this.GetComponent<ConfigurableJoint>())
            {
                this.GetComponent<ConfigurableJoint>().angularXMotion = ConfigurableJointMotion.Locked;
                this.GetComponent<ConfigurableJoint>().angularYMotion = ConfigurableJointMotion.Locked;
                this.GetComponent<ConfigurableJoint>().angularZMotion = ConfigurableJointMotion.Locked;
            }*/

            this.GetComponentInChildren<BoxCollider>().size = new Vector3(0.7f, 0.7f, 1.3f);
            this.GetComponentInChildren<BoxCollider>().center = new Vector3(0f, 0f, 0.8f);

            float FireProg = this.GetComponentInChildren<FireController>().fireProgress;
            记录器 += (计算间隔.Value / 100) * (1 - FireProg);
            if (currentTarget)
            {
                if (currentTarget.GetComponent<Rigidbody>() && currentTarget.transform.position != this.transform.position)
                {
                    Vector3 LocalTargetDirection = currentTarget.transform.position;
                    if (MissileGuidanceModeInt == 2)
                    {
                        Vector2 HorizontalDistance = new Vector2(currentTarget.transform.position.x - this.transform.position.x, currentTarget.transform.position.z - this.transform.position.z);
                        if (this.transform.position.y > currentTarget.transform.position.y + MissileHeightController.Value)
                        {
                            LocalTargetDirection = currentTarget.transform.position + new Vector3(HorizontalDistance.x / (this.rigidbody.velocity.x + 0.0001f), currentTarget.transform.position.y + MissileHeightController.Value * 1.1f, HorizontalDistance.y / (this.rigidbody.velocity.z + 0.0001f));
                        }
                        else
                        {
                            LocalTargetDirection = this.transform.position + Vector3.up * (currentTarget.transform.position.y + MissileHeightController.Value * 1.1f);
                        }
                        //Debug.Log(HorizontalDistance);
                        if (HorizontalDistance.sqrMagnitude < 100 * 炮弹速度)
                        {
                            MissileGuidanceModeInt = 0;
                        }
                    }
                    else if (MissileGuidanceModeInt == 3)
                    {
                        Vector2 HorizontalDistance = new Vector2(currentTarget.transform.position.x - this.transform.position.x, currentTarget.transform.position.z - this.transform.position.z);
                        if (this.transform.position.y < MissileHeightController.Value)
                        {
                            LocalTargetDirection = new Vector3(transform.position.x, MissileHeightController.Value * 1.5f, transform.position.z);
                        }
                        else if (this.transform.position.y > MissileHeightController.Value * 2)
                        {
                            LocalTargetDirection = new Vector3(transform.position.x + HorizontalDistance.normalized.x * 1.5f, MissileHeightController.Value, transform.position.z + +HorizontalDistance.normalized.y);
                        }
                        else
                        {
                            LocalTargetDirection = currentTarget.transform.position + new Vector3(HorizontalDistance.x / (this.rigidbody.velocity.x + 0.0001f), MissileHeightController.Value * 1f, HorizontalDistance.y / (this.rigidbody.velocity.z + 0.0001f));
                        }

                        if (HorizontalDistance.sqrMagnitude < 100 * 炮弹速度)
                        {
                            MissileGuidanceModeInt = 0;
                        }
                    }
                    else
                    {
                        if (MissileGuidanceModeInt == 0)
                        {
                            LocalTargetDirection = currentTarget.transform.position;
                            LocalTargetDirection = MissileMode0(LocalTargetDirection, FireProg);
                        }
                    }
                    //this.transform.rotation.SetFromToRotation(this.transform.forward, LocalTargetDirection);
                    //Vector3 rooo = Vector3.RotateTowards(this.transform.forward, LocalTargetDirection - this.transform.position, RotatingSpeed * size, RotatingSpeed * size);
                    //Debug.Log(LocalTargetDirection + "and" + this.transform.up + "and" + rooo);
                    //this.transform.rotation = Quaternion.LookRotation(rooo);
                    //LocalTargetDirection = new Vector3(LocalTargetDirection.x, LocalTargetDirection.y - this.transform.position.y, LocalTargetDirection.z);
                    //float mag = (LocalTargetDirection.normalized - transform.forward.normalized).magnitude;
                    Vector3 TargetDirection = (getCorrTorque(this.transform.forward, LocalTargetDirection - this.transform.position * 1, this.GetComponent<Rigidbody>(), 0.01f * size) * Mathf.Rad2Deg).normalized;
                    if ((Vector3.Angle(transform.forward, LocalTargetDirection - this.transform.position * 1) < 105 && !IsInMovieMode) || MissileGuidanceMode.Value != 0 || MissileGuidanceMode.Value != 1)
                    {
                        GetComponent<Rigidbody>().angularVelocity = (TargetDirection * RotatingSpeed * MissileTorqueSlider.Value);
                    }
                    else { Debug.Log("Target Lost!"); }
                }
            }
            前一帧速度 = this.GetComponent<Rigidbody>().velocity;
        }
        void CameraTrackingComputerMode()
        {
            if (currentTarget)
            {
                this.transform.LookAt(currentTarget.transform.position);
                try
                {
                    this.GetComponent<Rigidbody>().mass = 0.00001f;
                }
                catch { }
            }

        }

        Vector3 MissileMode0(Vector3 LocalTargetDirection, float FireProg)
        {
            if (记录器 >= 1)
            {
                炮弹速度 = this.GetComponent<Rigidbody>().velocity.magnitude;
                float targetVelo = currentTarget.GetComponent<Rigidbody>().velocity.magnitude;
                记录器 = 0;
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
                            size * 精度.Value + 10 * size * FireProg
                        ),
                        Physics.gravity.y,
                        size * 精度.Value + 10 * size * FireProg,
                        float.PositiveInfinity
                        );
                目标前帧速度Mag = targetVelo;
            }
            if (LocalTargetDirection.y == float.NaN)
            {
                LocalTargetDirection = currentTarget.transform.position;
            }
            前一帧速度 = GetComponent<Rigidbody>().velocity;
            return LocalTargetDirection;
        }

        void MissileColorApplier()
        {
            Transform Vis = transform.Find("Vis/Vis");
            if (MissileGuidanceModeInt == 0)
            {
                Vis.GetComponent<MeshRenderer>().material.color = Color.white;
            }
            else if (MissileGuidanceModeInt == 1)
            {
                Vis.GetComponent<MeshRenderer>().material.color = Color.black;
            }
            else if (MissileGuidanceModeInt == 2)
            {
                Vis.GetComponent<MeshRenderer>().material.color = Color.Lerp(Color.red, Color.white, 0.5f);
            }
            else if (MissileGuidanceModeInt == 3)
            {
                Vis.GetComponent<MeshRenderer>().material.color = Color.yellow;
            }
        }
        void ModelReplacer()
        {
            Transform Vis = transform.Find("Vis/Vis");
            if (模式.Value == 1)
            {
                Vis.GetComponentInChildren<MeshFilter>().mesh = resources["MissileModule.obj"].mesh;
                Vis.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));
                MissileColorApplier();
                this.GetComponentInChildren<BoxCollider>().size = new Vector3(0.7f, 0.7f, 1.3f);
                this.GetComponentInChildren<BoxCollider>().center = new Vector3(0f, 0f, 0.8f);
            }
            else
            {
                Vis.GetComponentInChildren<MeshFilter>().mesh = resources["turret.obj"].mesh;
                Vis.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));
                MissileColorApplier();
                this.GetComponentInChildren<BoxCollider>().size = new Vector3(1f, 1f, 1.2f);
                this.GetComponentInChildren<BoxCollider>().center = new Vector3(0f, 0f, 0.6f);
            }
        }
        void OverLoadExplosion()
        {
            GameObject explo = (GameObject)GameObject.Instantiate(PrefabMaster.BlockPrefabs[59].gameObject, this.transform.position, this.transform.rotation);
            explo.transform.localScale = Vector3.one * 0.01f;
            TimedRocket ac = explo.GetComponent<TimedRocket>();
            ac.SetSlip(Color.white);
            ac.radius = 0.00001f;
            ac.power = 0.00001f;
            ac.randomDelay = 0.000001f;
            ac.upPower = 0;
            ac.StartCoroutine(ac.Explode(0.01f));
            explo.AddComponent<TimedSelfDestruct>();
        }
    }
}


