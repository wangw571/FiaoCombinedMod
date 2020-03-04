using System;
using Modding;
using Modding.Blocks;
using Modding.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FiaoCombinedMod
{
    public class ModifiedTurret : BasicTrackingComputerBehavior
    {
        public bool UseChinese = true;

        public MKey Key1;
        public MKey Key2;
        public MKey Key25;
        public MSlider 炮力;
        public MSlider 精度;
        public MSlider 计算间隔;
        public MSlider 警戒度;
        public MSlider 镜头哪里;
        public MToggle 自动索敌;
        public MSlider 自动索敌友方范围;
        public MToggle FireOnMouseClick;
        public MMenu 模式;
        public MToggle 不聪明模式;
        public MToggle DisableHTracking;
        public MToggle DisableVTracking;
        public MSlider KnockBackStablizierAdjuster;
        public MToggle LockConnectionWhenNoTarget;
        public MSlider TorqueMultiplier;

        private List<Guid> FriendlyBlockGUID;


        private AudioSource Audio;

        public int FUCounter;
        private float size;
        private float RotatingSpeed = 0.5f;
        public float 记录器 = 0;

        public Vector3 前一帧速度 = Vector3.zero;
        private bool shoott = false;

        public Vector3 AimPos { get; private set; }


        public override void SafeAwake() 
        {

            //LocalMovieMode = FiaoCombinedMod.MovieMode;
            Init init = new Init(EnglishInitialize);
            init();

            //if (!spaar.ModLoader.Configuration.DoesKeyExist("MovieMode"))
            //{
            //    spaar.ModLoader.Configuration.SetBool("MovieMode", false);
            //    spaar.ModLoader.Configuration.Save();
            //}
            //IsInMovieMode = spaar.ModLoader.Configuration.GetBool("MovieMode", false); IsInMovieMode = LocalMovieMode.Value;

            //UseLockingWindow.Toggled += CheckIfAvailablePilotPanelExists;
        }
        //void ChineseInitialize()
        //{
        //    Key1 = AddKey("获取目标", //按键信息
        //                         "Locked",           //名字
        //                         KeyCode.T);       //默认按键

        //    Key2 = AddKey("解除锁定", //按键信息2
        //                         "RLocked",           //名字
        //                         KeyCode.Slash);       //默认按键

        //    Key25 = AddKey("下一目标", //按键信息2
        //                         "NTar",           //名字
        //                         KeyCode.Slash);       //默认按键

        //    炮力 = AddSlider("炮力",       //滑条信息
        //                            "strength",       //名字
        //                            1f,            //默认值
        //                            0f,          //最小值
        //                            2f);           //最大值

        //    精度 = AddSlider("精度",       //滑条信息
        //                            "Precision",       //名字
        //                            0.5f,            //默认值
        //                            0.01f,          //最小值
        //                            10f);           //最大值

        //    计算间隔 = AddSlider("每秒计算频率",       //滑条信息 
        //                            "CalculationPerSecond",       //名字
        //                            100f,            //默认值
        //                            1f,          //最小值
        //                            100f);           //最大值

        //    警戒度 = AddSlider("自动索敌警戒度",       //滑条信息
        //                            "Aggressivity",       //名字
        //                            0.5f,            //默认值
        //                            0.01f,          //最小值
        //                            1f);           //最大值

        //    镜头哪里 = AddSlider("距离相机的距离", "Dist", 1500, 1, 900000);

        //    自动索敌 = AddToggle("启用\n自动索敌\n功能", "USE", false);

        //    自动索敌友方范围 = AddSlider("友方模块判定范围", "FriendDist", 10, 0, 500);

        //    模式 = AddMenu("Menu", 0, new List<string> { "锁定模式", "跟随鼠标模式" });

        //    不聪明模式 = AddToggle("不计算弹道",   //toggle信息
        //                               "NoCL",       //名字
        //                               false);             //默认状态

        //    FireOnMouseClick = AddToggle("在鼠标点击时开火", "FOC", true);

        //    //DisableHTracking = AddToggle("Disable Horizontal Tracking", "DHT", false);
        //    DisableVTracking = AddToggle("关闭垂直方向的计算", "DVT", false);

        //    KnockBackStablizierAdjuster = AddSlider("后坐力/过载 调整", "ADJ", 95, 0, 95);

        //    LockConnectionWhenNoTarget = AddToggle("当没有目标时\n锁定连接点", "LOCKConnection", false);

        //    UseLockingWindow = AddToggle("使用自动锁定窗口", "LockWindow", false);
        //}
        void EnglishInitialize()
        {
            Key1 = AddKey("Lock On", //按键信息
                                 "Locked",           //名字
                                 KeyCode.T);       //默认按键

            Key2 = AddKey("Release Lock", //按键信息2
                                 "RLocked",           //名字
                                 KeyCode.Slash);       //默认按键

            Key25 = AddKey("Switch to Next Target", //按键信息2
                                "NTar",           //名字
                                KeyCode.Slash);       //默认按键

            炮力 = AddSlider("Cannon Power Slider",       //滑条信息
                                    "strength",       //名字
                                    1f,            //默认值
                                    0f,          //最小值
                                    2f);           //最大值

            精度 = AddSlider("Precision",       //滑条信息
                                    "Precision",       //名字
                                    0.5f,            //默认值
                                    0.01f,          //最小值
                                    20f);           //最大值

            计算间隔 = AddSlider("Calculations per second",       //滑条信息
                                    "CalculationPerSecond",       //名字
                                    100f,            //默认值
                                    0.01f,          //最小值
                                    100f);           //最大值

            警戒度 = AddSlider("Auto lock Aggressivity",       //滑条信息
                                    "Aggressivity",       //名字
                                    0.5f,            //默认值
                                    0.01f,          //最小值
                                    1f);           //最大值

            镜头哪里 = AddSlider("Calibration Distance From Camera", "Dist", 1500, 1, 900000);

            自动索敌 = AddToggle("Auto Lock", "USE", false);
            自动索敌.DisplayInMapper = false;
            自动索敌友方范围 = AddSlider("Friendly Blocks Range", "FriendDist", 10, 0, 500);
            模式 = AddMenu("Menu", 0, new List<string> { "Lock Mode", "Mouse Mode" });
            //模式 = AddMenu("Menu", 0, new List<string> { "Lock Mode"});

            不聪明模式 = AddToggle("Disable Calculation",   //toggle信息
                                       "NoCL",       //名字
                                       false);             //默认状态

            FireOnMouseClick = AddToggle("Fire On Mouse Click", "FOC", true);

            //DisableHTracking = AddToggle("Disable Horizontal Tracking", "DHT", false);
            DisableVTracking = AddToggle("Disable Vertical Tracking", "DVT", false);

            KnockBackStablizierAdjuster = AddSlider("Knockback Damper Effecitvity", "ADJ", 95, 0, 95);

            LockConnectionWhenNoTarget = AddToggle("Lock the joint\nwhen having no target", "LOCKConnection", false);

            TorqueMultiplier = AddSlider("Torque Multiplier", "torqMult", 1, 0.5f, 20f);

            //UseLockingWindow = AddToggle("Enable Autolock window", "LockWindow", false);
        }

        public override void BuildingUpdate()
        {
            if (LockConnectionWhenNoTarget == null) return;
            自动索敌.DisplayInMapper = 模式.Value == 0;

            Key1.DisplayInMapper = 模式.Value != 1 && !自动索敌.IsActive;
            Key2.DisplayInMapper = 模式.Value != 1 && !自动索敌.IsActive;
            Key25.DisplayInMapper = 模式.Value != 1 && 自动索敌.IsActive;

            if (模式.Value == 1)
            {
                自动索敌.IsActive = false;
            }
            警戒度.DisplayInMapper = 自动索敌.IsActive && 模式.Value == 0;
            自动索敌友方范围.DisplayInMapper = 自动索敌.IsActive && 模式.Value == 0;

            镜头哪里.DisplayInMapper = 模式.Value == 1;
            不聪明模式.DisplayInMapper = 模式.Value == 1;
            计算间隔.DisplayInMapper = !(不聪明模式.IsActive && 模式.Value == 1);
            精度.DisplayInMapper = !(不聪明模式.IsActive && 模式.Value == 1);
            炮力.DisplayInMapper = !(不聪明模式.IsActive && 模式.Value == 1);

            FireOnMouseClick.DisplayInMapper = 模式.Value == 1;

            //自动索敌.DisplayInMapper = false;
        }

        public override void OnSimulateStart()
        {
            currentTarget = null;
            currentLocking = null;
            LockingTimer = -1;
            炮弹速度 = 炮力.Value * 54;
            SetUpAudioAndVis();
            if (!StatMaster.isClient) ConfigJointSetup();
            //conf.angularZMotion = ConfigurableJointMotion.Locked;
        }

        //protected override void OnSimulateFixedStart()
        //{
        //    if (自动索敌.IsActive && 自动索敌.DisplayInMapper)
        //    {
        //        FUCounter = 1;
        //        FriendlyBlockGUID = new List<Guid>();
        //        float range = 自动索敌友方范围.Value * 自动索敌友方范围.Value;
        //        foreach (Transform FriendlyOr in Machine.SimulationMachine)
        //        {
        //            if (range >= this.transform.InverseTransformPoint(FriendlyOr.transform.position).sqrMagnitude)
        //            {
        //                FriendlyBlockGUID.Add(FriendlyOr.GetComponent<BlockBehaviour>().Guid);
        //            }

        //        }
        //    }
        //}


        public override void SimulateUpdateClient()
        {
            //下个目标
            if (Machine.Player != Player.GetLocalPlayer()) return;
            if (Key25.IsPressed && 自动索敌.IsActive)
            {
                LockingTimer = currentLocking ? 0 : -1;
            }

            //烧坏
            if (HasBurnedOut && !StatMaster.GodTools.UnbreakableMode)
            {
                currentLocking = null; LockingTimer = -1;
                currentTarget = null;
                return;
            }

            if (自动索敌.IsActive)
            {
                return;
            }

            //鼠标瞄准模式
            if (模式.Value == 1)
            {
                Ray raay = Camera.main.ScreenPointToRay(Input.mousePosition);
                Message lockmessage = Messages.TrackingComputerLock.CreateMessage(Block.From(this), raay.origin, raay.direction);
                ModNetworking.SendToHost(lockmessage);
                if (FireOnMouseClick.IsActive && 模式.Value == 1 && Input.GetMouseButtonDown(0))
                {
                    Message shotmess = Messages.ModTrackingComputerShot.CreateMessage(Block.From(this));
                    ModNetworking.SendToHost(shotmess);
                }
            }
            //按键瞄准模式
            if (Key1.IsPressed && !HasBurnedOut && 模式.Value == 0)
            {
                Ray raay = Camera.main.ScreenPointToRay(Input.mousePosition);
                Message lockmessage = Messages.TrackingComputerLock.CreateMessage(Block.From(this), raay.origin, raay.direction);
                ModNetworking.SendToHost(lockmessage);
            }

            //按键瞄准模式-取消
            if (Key2.IsPressed && 模式.Value == 0)
            {
                currentLocking = null; LockingTimer = -1;
                currentTarget = null;
            }
        }

        public override void SimulateUpdateAlways()
        {
            if (StatMaster.isClient) return;
            if (Key25.IsPressed && 自动索敌.IsActive)
            {
                currentLocking = MyTargetSelector();
                LockingTimer = currentLocking ? 0 : -1;
            }

            //烧坏
            if (HasBurnedOut && !StatMaster.GodTools.UnbreakableMode)
            {
                currentLocking = null; LockingTimer = -1;
                currentTarget = null;
            }

            if (自动索敌.IsActive)
            {
                return;
            }

            //鼠标瞄准模式
            if (模式.Value == 1 && !不聪明模式.IsActive)
            {
                AcquireTarget(Camera.main.ScreenPointToRay(Input.mousePosition));
            }
            //按键瞄准模式
            if (Key1.IsPressed && !HasBurnedOut && 模式.Value == 0)
            {
                AcquireTarget(Camera.main.ScreenPointToRay(Input.mousePosition));
            }

            //按键瞄准模式-取消
            if (Key2.IsPressed && 模式.Value == 0)
            {
                currentLocking = null; LockingTimer = -1;
                currentTarget = null;
            }

            //后坐力
            if (VisualController == null) return;
            if (VisualController.Block == null) return;
            if (VisualController.Block.jointsToMe == null) return;
            foreach (Joint Jo in VisualController.Block.jointsToMe)
            {
                if (VisualController == null) return;
                if (VisualController.Block == null) return;
                if (VisualController.Block.jointsToMe == null) return;
                if (Jo.GetComponentInParent<CanonBlock>())
                {
                    CanonBlock cb = Jo.GetComponentInParent<CanonBlock>();
                    cb.knockbackSpeed = 8000 * ((100 - KnockBackStablizierAdjuster.Value) / 100);
                    cb.randomDelay = 0.000001f;
                    shoott = StatMaster.isMP ? shoott : Input.GetMouseButtonDown(0);
                    if (FireOnMouseClick.IsActive && 模式.Value == 1 && shoott) { cb.Shoot(); }
                }
            }
            shoott = false;
        }

        public void shot()
        {
            shoott = true;
        }

        public override void AcquireTarget(Ray ray)
        {
            if (模式.Value == 1 && 不聪明模式.IsActive)
            {
                AimPos = ray.origin + ray.direction * 镜头哪里.Value;
            }
            else
            {
                foreach (RaycastHit Hito in Physics.RaycastAll(ray))
                {
                    if (!Hito.collider.isTrigger)
                    {
                        if (!HasBurnedOut && 模式.Value == 0)
                        {
                            if (Hito.transform.position != this.transform.position && Hito.collider.attachedRigidbody != null)
                            {
                                currentLocking = Hito.transform.gameObject;
                                LockingTimer = /*spaar.ModLoader.Configuration.GetFloat("LockingTimer", 4);*/ 4;
                                LockingTimer = 0;
                            }
                        }
                        else if (模式.Value == 1)
                        {
                            if (!不聪明模式.IsActive)
                            {
                                if (Hito.transform.position != this.transform.position)
                                {
                                    currentTarget = Hito.transform.gameObject;
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void SimulateFixedUpdateAlways()
        {
            if (StatMaster.isClient) return;

            LockingTimer -= Time.deltaTime * 3;
            size = 1 * this.transform.localScale.x * this.transform.localScale.y * this.transform.localScale.z;
            Rigidbody.mass = 2f * size;
            FireProg = this.IsBurning ? FireProg + 0.01f : FireProg;
            if (currentLocking != null) 
            {
                if (LockConnectionWhenNoTarget.IsActive)
                {
                    this.GetComponent<ConfigurableJoint>().angularXMotion = ConfigurableJointMotion.Free;
                    this.GetComponent<ConfigurableJoint>().angularYMotion = ConfigurableJointMotion.Free;
                }
            }
            else if (LockConnectionWhenNoTarget.IsActive)
            {
                this.GetComponent<ConfigurableJoint>().angularXMotion = ConfigurableJointMotion.Locked;
                this.GetComponent<ConfigurableJoint>().angularYMotion = ConfigurableJointMotion.Locked;
            }

            记录器 += (计算间隔.Value / 100) * (1 - FireProg);
            if (currentTarget != null && 模式.Value != 1)
            {
                if (currentTarget.GetComponent<Rigidbody>())
                {
                    NonMouseMode(0);
                }
            }
            else if (自动索敌.IsActive && 自动索敌.DisplayInMapper)
            {
                Auto(0);
            }
            else if (模式.Value == 1 && !HasBurnedOut)
            {
                MouseMode(0);
            }

            累计质量 = this.Rigidbody.mass;

            if (VisualController == null) return;
            if (VisualController.Block == null) return;
            if (VisualController.Block.jointsToMe == null) return;
            foreach (Joint aRB in VisualController.Block.jointsToMe)
            {
                if (aRB != null && aRB.gameObject != null && aRB.gameObject.GetComponentInParent<BlockBehaviour>() != null && aRB.gameObject.GetComponentInParent<BlockBehaviour>().Rigidbody != null)
                {
                    aRB.gameObject.GetComponentInParent<BlockBehaviour>().Rigidbody.centerOfMass = (Vector3)aRB.gameObject?.GetComponentInParent<BlockBehaviour>()?.gameObject?.transform?.InverseTransformPoint(this.transform.position);
                    累计质量 += aRB.gameObject.GetComponentInParent<BlockBehaviour>().Rigidbody.mass;
                }
            }

        }
        void NonMouseMode(float FireProg)
        {
            //float Random = 1 + (0.5f - UnityEngine.Random.value) * 0.15f;
            float Random = 1;
            if (currentTarget.transform.position != this.transform.position)
            {
                Vector3 LocalTargetDirection = currentTarget.transform.position;
                if (炮力.Value != 0 && 记录器 >= 1)
                {
                    Rigidbody ctRgdBdy = currentTarget.GetComponent<Rigidbody>();
                    float targetVelo = ctRgdBdy.velocity.magnitude;
                    记录器 = 0;
                    LocalTargetDirection = calculateNoneLinearTrajectory(
                        炮弹速度 * Random,
                        0.2f,
                        transform.position,
                        targetVelo,
                        currentTarget.transform.position,
                        ctRgdBdy.velocity.normalized,
                            calculateLinearTrajectory(
                                炮弹速度 * Random,
                                transform.position,
                                targetVelo,
                                currentTarget.transform.position,
                                ctRgdBdy.velocity.normalized
                            ),
                            Physics.gravity.y,
                            size * 精度.Value + 10 * size * FireProg,
                            float.PositiveInfinity
                            );
                }
                //this.transform.rotation.SetFromToRotation(this.transform.right, LocalTargetDirection); 
                Vector3 rooo = Vector3.RotateTowards(this.transform.eulerAngles, new Vector3(LocalTargetDirection.z, LocalTargetDirection.x, LocalTargetDirection.y), 1, 1); //RotatingSpeed * size, RotatingSpeed * size);

                //Debug.Log(LocalTargetDirection + "and" + this.transform.up + "and" + rooo);
                //this.transform.rotation = Quaternion.LookRotation(rooo);
                //LocalTargetDirection = new Vector3(LocalTargetDirection.x, LocalTargetDirection.y - this.transform.position.y, LocalTargetDirection.z);
                /*if(DisableHTracking.IsActive)
                {
                    LocalTargetDirection = new Vector3(this.transform.right.x, LocalTargetDirection.y, this.transform.right.z);
                }*/

                if (DisableVTracking.IsActive)
                {
                    LocalTargetDirection = new Vector3(LocalTargetDirection.x, this.transform.right.y, LocalTargetDirection.z);
                }
                float Difference = Vector3.Angle(transform.right, LocalTargetDirection - this.transform.position * 1);
                //if (Difference > 精度.Value)
                //{
                //    this.GetComponent<Rigidbody>().angularVelocity = (getCorrTorque(this.transform.right, LocalTargetDirection - this.transform.position * 1, this.GetComponent<Rigidbody>(), 0.01f * size) * Mathf.Rad2Deg).normalized * RotatingSpeed;
                //}
                //else
                //{
                //    this.GetComponent<Rigidbody>().angularVelocity = (getCorrTorque(this.transform.right, LocalTargetDirection - this.transform.position * 1, this.GetComponent<Rigidbody>(), 0.01f * size) * Mathf.Rad2Deg);
                //}

                float valuees = Mathf.Sign((this.transform.right - (LocalTargetDirection - this.transform.position).normalized).y) == -1 ? 1 + Math.Min(1 / (转速乘子 * 100), 0.005f) : 0.2f;
                转速乘子 = Math.Max(Math.Max(转速乘子 * valuees, 25), 转速乘子 / 2);
                CorrTorq = getCorrTorque(
                                    this.transform.right,
                                    LocalTargetDirection - this.transform.position * 1,
                                    Rigidbody, 0.01f / size
                                    )
                                * Mathf.Rad2Deg;

                ApplyAngularVelo();

                float mag = (this.transform.right.normalized - LocalTargetDirection.normalized).magnitude;
                if (Vector3.Angle(transform.right, LocalTargetDirection - this.transform.position * 1) > 0.01f * 精度.Value)
                {
                    //this.GetComponent<Rigidbody>().freezeRotation = false;
                    Audio.maxDistance = 40;
                    Audio.minDistance = 10;
                    Audio.Play();
                }
                else
                {
                    //this.GetComponent<Rigidbody>().freezeRotation = true;
                    Audio.Stop();
                }
            }
        }

        private void ApplyAngularVelo()
        {
            float 后坐力替换转速 = (1 - (KnockBackStablizierAdjuster.Value / 95));
            //Try MoveRotation
            this.Rigidbody.angularVelocity = TorqueMultiplier.Value * (
                MultiplyXAndZ(CorrTorq.normalized, 转速乘子)
                * RotatingSpeed * (累计质量 / this.Rigidbody.mass) * (1 + 后坐力替换转速 * 5));
        }

        void MouseMode(float FireProg)
        {
            //float Random = 1 + (0.5f - UnityEngine.Random.value * 0.1f);
            float Random = 1;
            if (StatMaster.isMP && Machine.Player.IsHost)
            {
                AimPos = GameObject.Find("Main Camera").GetComponent<Camera>().ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 镜头哪里.Value));
            }
            if (!StatMaster.isMP)
            {
                AimPos = GameObject.Find("Main Camera").GetComponent<Camera>().ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 镜头哪里.Value));
            }
            if (AimPos != this.transform.position)
            {
                Vector3 LocalTargetDirection = AimPos;
                if (炮力.Value != 0 && 记录器 >= 1 && !不聪明模式.IsActive)
                {
                    记录器 = 0;
                    LocalTargetDirection = calculateNoneLinearTrajectory(
                        炮弹速度 + Random,
                        0.2f,
                        this.transform.position,
                        0,
                        AimPos,
                        Vector3.zero,
                            AimPos,
                            Physics.gravity.y,
                            size * 精度.Value + 10 * size * FireProg,
                            float.PositiveInfinity
                            );
                }
                //this.transform.rotation.SetFromToRotation(this.transform.right, LocalTargetDirection);
                Vector3 rooo = Vector3.RotateTowards(this.transform.right, LocalTargetDirection - this.transform.position, RotatingSpeed * size, RotatingSpeed * size);
                //Debug.Log(LocalTargetDirection + "and" + this.transform.up + "and" + rooo);
                //this.transform.rotation = Quaternion.LookRotation(rooo);
                //LocalTargetDirection = new Vector3(LocalTargetDirection.x, LocalTargetDirection.y - this.transform.position.y, LocalTargetDirection.z);
                /*if (DisableHTracking.IsActive)
                {
                    LocalTargetDirection = new Vector3(this.transform.right.x, LocalTargetDirection.y, this.transform.right.z);
                }*/
                if (DisableVTracking.IsActive)
                {
                    LocalTargetDirection = new Vector3(LocalTargetDirection.x, this.transform.right.y, LocalTargetDirection.z);
                }
                float Difference = Vector3.Angle(transform.forward, LocalTargetDirection - this.transform.position * 1);
                转速乘子 *= Mathf.Sign((this.transform.right - (LocalTargetDirection - this.transform.position).normalized).y) == -1 ? 1 + Math.Min(1 / (转速乘子 * 100), 0.005f) : 0.2f;
                转速乘子 = Math.Max(1, 转速乘子);

                CorrTorq = getCorrTorque(
                                this.transform.right,
                                LocalTargetDirection - this.transform.position * 1,
                                this.GetComponent<Rigidbody>(), 0.01f * size
                                )
                            * Mathf.Rad2Deg;

                ApplyAngularVelo();


                float mag = (this.transform.right.normalized - LocalTargetDirection.normalized).magnitude;
                if (Vector3.Angle(transform.right, LocalTargetDirection - this.transform.position * 1) > 0.01f * 精度.Value)
                {
                    //this.GetComponent<Rigidbody>().freezeRotation = false;
                    Audio.volume = mag * 0.2f * Math.Max((10 / (Vector3.Distance(this.transform.position, GameObject.Find("Main Camera").transform.position))), 1);
                    Audio.Play();
                }
                else
                {
                    //this.GetComponent<Rigidbody>().freezeRotation = true;
                    Audio.Stop();
                }
            }
        }

        void Auto(float FireProg)
        {

            ++FUCounter;
            if (FUCounter == 20)
            {
                if (自动索敌.IsActive && 自动索敌.DisplayInMapper)
                {
                    FriendlyBlockGUID = new List<Guid>();
                    float range = 自动索敌友方范围.Value * 自动索敌友方范围.Value;
                    foreach (Transform FriendlyOr in Machine.SimulationMachine)
                    {
                        if (range >= this.transform.InverseTransformPoint(FriendlyOr.transform.position).sqrMagnitude)
                        {
                            FriendlyBlockGUID.Add(FriendlyOr.GetComponent<BlockBehaviour>().Guid);
                        }

                    }
                }
            }
            else if (FUCounter < 20)
            {
                return;
            }

            Vector3 LocalTargetDirection;

            Vector3 TargetDirection;
            if (currentTarget != null)
            {
                if (currentTarget.GetComponent<Rigidbody>() && currentTarget.transform.position != this.transform.position)
                {
                    float targetVelo = currentTarget.GetComponent<Rigidbody>().velocity.magnitude;
                    LocalTargetDirection = calculateNoneLinearTrajectory(
                        炮弹速度,
                        0.2f,
                        this.transform.position,
                        targetVelo,
                        currentTarget.transform.position,
                        currentTarget.GetComponent<Rigidbody>().velocity.normalized,
                            calculateLinearTrajectory(
                                炮弹速度,
                                this.transform.position,
                                targetVelo,
                                currentTarget.transform.position,
                                currentTarget.GetComponent<Rigidbody>().velocity.normalized
                            ),
                            Physics.gravity.y,
                            size * 精度.Value + 10 * size * FireProg,
                            float.PositiveInfinity
                            );

                    TargetDirection = (getCorrTorque(this.transform.forward, LocalTargetDirection - this.transform.position * 1, this.GetComponent<Rigidbody>(), 0.01f * size) * Mathf.Rad2Deg).normalized;

                    GetComponent<Rigidbody>().angularVelocity = (TargetDirection * RotatingSpeed * TorqueMultiplier.Value);

                    if (FUCounter % 30 == 0 && Vector3.Angle(transform.forward, LocalTargetDirection - this.transform.position * 1) < 15)
                    {
                        //if (DroneAIType.Value == 1)
                        //{
                        if (!NotEvenHavingAJoint)
                        {
                            if (!currentTarget.GetComponent<ConfigurableJoint>())
                            {
                                currentTarget = null;
                                LockingTimer = -1;
                            }
                        }
                        if (!NotEvenHavingAFireTag)
                        {
                            if (currentTarget.GetComponent<FireTag>().burning)
                            {
                                currentTarget = null;
                                LockingTimer = -1;
                            }
                        }
                        //}
                        //else
                        //{
                        //    if (currentTarget.GetComponent<EntityAI>())
                        //    {
                        //        if (currentTarget.GetComponent<EntityAI>().isDead == true)
                        //        {
                        //            currentTarget = null;
                        //        }
                        //    }
                        //    else if (currentTarget.GetComponent<EnemyAISimple>())
                        //    {
                        //        if (currentTarget.GetComponent<EnemyAISimple>().isDead == true)
                        //        {
                        //            currentTarget = null;
                        //        }
                        //    }
                        //}
                    }
                }
            }
            else if (FUCounter % 50 == 0)
            {
                currentLocking = MyTargetSelector();
                LockingTimer = currentLocking ? 0 : -1;
            }

        }

        GameObject MyTargetSelector()
        {
            List<Block> BBList = new List<Block>();
            List<int> ThreatMultiplier = new List<int>();
            bool onlyOneTeam = false;
            if (StatMaster.isMP)
            {
                onlyOneTeam = DetectIfOnlyOneTeamExists();
            }
            foreach (Player playyer in Player.GetAllPlayers())
            {
                if (playyer.Team == Machine.Player.Team && !onlyOneTeam)
                {
                    p.l("Same Team Encountered!");
                    continue;
                }
                foreach (Block BB in playyer.Machine.SimulationBlocks)
                {
                    if (!StatMaster.isMP)
                    {
                        if (FriendlyBlockGUID.Contains(BB.Guid))
                        {
                            continue;
                        }
                    }
                    NotEvenHavingAFireTag = BB.SimBlock.GameObject.GetComponent<FireTag>() == null;
                    int BBID = BB.Prefab.Type;
                    bool? burningN = (BB?.SimBlock?.GameObject?.GetComponent<FireTag>()?.burning);
                    bool NotBurning = (burningN == null ? false : !(bool)burningN);
                    if (BBID == 23 || BBID == 525 || BBID == 526 || BBID == 540 || BBID == 519)//Bombs, Tracking Computers
                    {
                        if (NotEvenHavingAFireTag)
                        {
                            BBList.Add(BB);
                            ThreatMultiplier.Add(15);
                        }
                        else if (NotBurning)
                        {
                            BBList.Add(BB);
                            ThreatMultiplier.Add(14);
                        }
                    }
                    else if (BBID == 59 || BBID == 54 || BBID == 43 || BBID == 61)//Rocket & Grenade
                    {
                        BBList.Add(BB);
                        ThreatMultiplier.Add(12);
                    }
                    else if (BBID == 14 || BBID == 2 || BBID == 46 || BBID == 39)//Locomotion && Proplusion
                    {
                        if (BB?.SimBlock?.GameObject?.GetComponent<ConfigurableJoint>())
                        {
                            if (NotEvenHavingAFireTag)
                            {
                                BBList.Add(BB);
                                ThreatMultiplier.Add(10);
                            }
                            else if (NotBurning)
                            {
                                BBList.Add(BB);
                                ThreatMultiplier.Add(11);
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
                        if (BB?.SimBlock?.GameObject?.GetComponent<ConfigurableJoint>())
                        {
                            if (NotEvenHavingAFireTag)
                            {
                                BBList.Add(BB);
                                ThreatMultiplier.Add(4);
                            }
                            else if (NotBurning)
                            {
                                BBList.Add(BB);
                                ThreatMultiplier.Add(6);
                            }
                        }
                    }
                    else if (BBID == 35 || BBID == 16 || BBID == 42 /**/ || BBID == 40 || BBID == 60 || BBID == 38 || BBID == 51 /**/ || BBID == 1 || BBID == 15 || BBID == 41 || BBID == 5)//Structure Block
                    {
                        if (BB?.SimBlock?.GameObject?.GetComponent<ConfigurableJoint>())
                        {
                            if (NotEvenHavingAFireTag)
                            {
                                BBList.Add(BB);
                                ThreatMultiplier.Add(1);
                            }
                            else if (NotBurning == false)
                            {
                                BBList.Add(BB);
                                ThreatMultiplier.Add(2);
                            }
                        }
                    }

                }
            }
            foreach (Block BB2 in BBList.ToArray())
            {
                //删除不符合条件的结果
                int RemoveId = BBList.IndexOf(BB2);
                if ((float)ThreatMultiplier[RemoveId] <= 14f - (14f * 警戒度.Value))
                {
                    BBList.RemoveAt(RemoveId);
                    ThreatMultiplier.RemoveAt(RemoveId);
                    continue;
                }
            }

            if (BBList.Count == 0)
            {
                p.l("No Hostile Target!");
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
                //foreach (MyBlockInfo BNB in BBList)
                for (int Index = UnityEngine.Random.Range(0, BBList.Count - 1); Index < BBList.Count; ++Index)
                {
                    if (ThreatMultiplier.Count == Index + 1)
                    {
                        NotEvenHavingAJoint = !BBList[Index].SimBlock.GameObject.GetComponent<ConfigurableJoint>();
                        NotEvenHavingAFireTag = !BBList[Index].SimBlock.GameObject.GetComponent<FireTag>();
                        if (!NotEvenHavingAJoint)
                        {
                            BBList[Index].SimBlock.GameObject.GetComponent<ConfigurableJoint>().breakForce = Mathf.Min(BBList[Index].SimBlock.GameObject.GetComponent<ConfigurableJoint>().breakForce, 45000);
                        }
                        return (BBList[Index].SimBlock.GameObject);
                    }
                    if (ThreatMultiplier[Index + 1] <= ThreatMultiplier[Index])
                    {
                        ThreatMultiplier.RemoveAt(Index + 1);
                        BBList.RemoveAt(Index + 1);
                        continue;
                    }
                    NotEvenHavingAJoint = !BBList[Index].SimBlock.GameObject.GetComponent<ConfigurableJoint>();
                    NotEvenHavingAFireTag = !BBList[Index].SimBlock.GameObject.GetComponent<FireTag>();
                    if (!NotEvenHavingAJoint)
                    {
                        BBList[Index].SimBlock.GameObject.GetComponent<ConfigurableJoint>().breakForce = Mathf.Min(BBList[Index].SimBlock.GameObject.GetComponent<ConfigurableJoint>().breakForce, 45000);
                    }
                    return (BBList[Index].SimBlock.GameObject);
                }
                p.l("No appropriate target!");
                return null;
            }
        }

        private bool DetectIfOnlyOneTeamExists()
        {
            bool onlyOneTeam = true;
            foreach (Player playyer in Player.GetAllPlayers())
            {
                if ((playyer.Team != Machine.Player.Team))
                {
                    onlyOneTeam = false;
                    break;
                }
            }
            return onlyOneTeam;
        }

        public override void SetUpAudioAndVis()
        {
            Audio = this.gameObject.AddComponent<AudioSource>();
            Audio.clip = Modding.ModResource.GetAudioClip("FiaoCombinedMod/炮台旋转音效.ogg");
            Audio.loop = false;
            Audio.volume = 0.2f;
        }
    }

}
