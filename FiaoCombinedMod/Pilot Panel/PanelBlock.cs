using Modding;
using Modding.Blocks;
using Modding.Common;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace FiaoCombinedMod
{
    public class PilotPanelScript : Modding.BlockScript
    {
        public Rect AutoLockRange;

        private MKey ActivateTargetIndicator;
        private MToggle HideThisPanel, AdvTI;
        private MSlider AdvTIS;
        private MSlider MiniMapLerpValue, MiniMapHeight;
        private MSlider LockerHeight, LockerWidth;
        private MColourSlider StartColour, EndColour;
        private MKey ActiveHUD;
        private MKey HidePanel;

        private MToggle ReduceCameraShake;

        public MToggle UseLockWindow;

        private MToggle Pitch, Center, Direction, MapCenter, Height, IceFreezeIndicator, GroundIndicator, OneThousandIndicator, HeightIndicator, MiniMap;

        private MMenu ConfigMenu;
        private MMenu HUDConfigMenu;

        public bool HidePanelBool = false;
        public bool HUD_Activated = false;

        public GameObject ShowHUD;
        public GameObject MiniMapGO;
        public Camera MiniMapCamera;
        public RenderTexture RTCamera;
        public Rect LockerRect;


        private GameObject BombDrop;
        private LineRenderer AdvBombDrop;

        public Vector3 displacement, velocity, rotation, direction, horizontal, bombposition, vel1, vel0;
        public float T1, dt, T_hitground, alt = 0, climbrate = 0;
        private float acce, overload, yaw, pitch, roll = 0;

        private float row1, row2, row3, tic, ticc, toc = 0;



        private float num_alt = 1;
        private float num_vel = 1;
        private float num_time = 1;
        private bool disp = true;
        private bool disp2 = false;


        private string label_row1 = "Speed/m/s: ";
        private string label_row2 = "Altitude/m: ";
        private string label_row3 = "Time/s: ";

        private Texture 俯仰刻度, 机体准星, 正00纹理, 负00纹理, 冰层纹理, 地面那一条杠杠滴纹理, 现时高度指示纹理, 一千杠杠, 北罗盘纹理, 南罗盘纹理
            , 西罗盘纹理
            , 东罗盘纹理
            , 小罗盘纹理, 小地图纹理,
            锁定窗口;

        //private GameObject 冲刺效果 = GameObject.CreatePrimitive(PrimitiveType.Plane);

        public bool 高度计_渐变中 = false;
        public int 高度计状态 = 0; //-1 地底   0 冰层下   1 1000下   2 1000上
        public int 比较用高度计状态 = 0; //-1 地底   0 冰层下   1 1000下   2 1000上
        public Vector2 pos = new Vector2(Screen.width / 2, Screen.height / 2);
        public float 调试函数 = 0;
        //private Key LCF1 = new Key(KeyCode.LeftControl, KeyCode.F2);
        //private Key RCF1 = new Key(KeyCode.RightControl, KeyCode.F2);
        private float CurrentCameraSpeed;
        private float 速度标记位置 = 0;
        public bool 强制关闭 = false;
        public float 渐变高度计使用的临时函数 = 0;

        private Vector3 PreviousMiniMapLookAtPosition;

        private float LastFUveloMag;

        private delegate void Init();

        private MouseOrbit MainMo;

        public override void SafeAwake()
        {
            Init init = /*Configuration.GetBool("UseChinese", false)*/ false ? new Init(ChineseInit) : new Init(EnglishInit);
            init();

        }

        private void ChineseInit()
        {
            ActivateTargetIndicator = AddKey("投弹指示光斑", //按键信息
                                 "TI",           //名字
                                 KeyCode.C);       //默认按键

            HideThisPanel = AddToggle("隐藏本模块的信息显示器",   //toggle信息
                                       "Hide",       //名字
                                       false);             //默认状态

            //HidePanel = AddKey("Hide Panel", "Panel", KeyCode.Backspace);

            AdvTI = AddToggle("高级投弹指示器",   //toggle信息
                                       "AdvTI",       //名字
                                       false);             //默认状态
            AdvTIS = AddSlider("投弹指示器线段数量", "LineAmt", 20f, 2f, 45f);

            StartColour = AddColourSlider("投弹指示器起始色", "SColor", Color.red, true);

            EndColour = AddColourSlider("投弹指示器结束色", "EColor", Color.yellow, true);


            ConfigMenu = AddMenu("ConfigMenu", 0, new List<string> { "飞行信息设置", "HUD设置" });

            HUDConfigMenu = AddMenu("HUDConfigMenu", 0, new List<string> { "中心", "高度", "俯仰", "罗盘", "地图中心", "小地图" });

            ActiveHUD = AddKey("开启HUD", "HUD", KeyCode.F);
            Height = AddToggle("高度指示", "HeightIndi", true);
            Pitch = AddToggle("俯仰指示", "PitchIndi", true);
            Center = AddToggle("屏幕中心指示", "CenterIndi", true);
            Direction = AddToggle("罗盘", "DirectIndi", true);
            MapCenter = AddToggle("地图中心指示", "MapCenterIndi", true);
            HeightIndicator = AddToggle("相机高度指示", "MyHeightIndi", true);
            IceFreezeIndicator = AddToggle("结冰层指示", "IceIndi", true);
            GroundIndicator = AddToggle("地面指示", "0mIndi", true);
            OneThousandIndicator = AddToggle("1000m指示", "1000mIndi", true);
            MiniMap = AddToggle("小地图", "Minimap", false);
            MiniMapLerpValue = AddSlider("小地图平滑度", "MinimapLerp", 0.015f, 0, 1);
            MiniMapHeight = AddSlider("小地图高度", "MinimapHeight", 400, 5, 1300);
            UseLockWindow = AddToggle("使用预锁定窗口", "UseLockWindow", false);
            LockerHeight = AddSlider("预锁定窗口高度", "LockerH", 800, 100, 2000);
            LockerWidth = AddSlider("预锁定窗口宽度", "LockerW", 800, 100, 2000);

            ReduceCameraShake = AddToggle("减轻相机抖动", "Noshake", false);
        }

        private void EnglishInit()
        {
            ActivateTargetIndicator = AddKey("Bombard Indicator", //按键信息
                                 "TI",           //名字
                                 KeyCode.C);       //默认按键

            HideThisPanel = AddToggle("Hide this\n block's panel",   //toggle信息
                                       "Hide",       //名字
                                       false);             //默认状态

            //HidePanel = AddKey("Hide Panel", "Panel", KeyCode.Backspace);

            AdvTI = AddToggle("ADVANCED \n Bombard Indicator",   //toggle信息
                                       "AdvTI",       //名字
                                       false);             //默认状态
            AdvTIS = AddSlider("Smoothness of Indicator", "LineAmt", 20f, 2f, 45f);

            StartColour = AddColourSlider("Start Color of the line", "SColor", Color.red, true);

            EndColour = AddColourSlider("End Color of the line", "EColor", Color.yellow, true);


            ConfigMenu = AddMenu("ConfigMenu", 0, new List<string> { "Pilot Panel Func", "HUD Func" });
            HUDConfigMenu = AddMenu("HUDConfigMenu", 0, new List<string> { "Center", "Height", "Pitch", "Direction", "Map Center", "Minimap", "Locking" });
            ActiveHUD = AddKey("Toggle HUD", "HUD", KeyCode.F);
            Height = AddToggle("Height Indication", "HeightIndi", true);
            Pitch = AddToggle("Pitch Indication", "PitchIndi", true);
            Center = AddToggle("Center Indication", "CenterIndi", true);
            Direction = AddToggle("Compass Indication", "DirectIndi", true);
            MapCenter = AddToggle("Map Center Indication", "MapCenterIndi", true);
            HeightIndicator = AddToggle("Height Indication", "MyHeightIndi", true);
            IceFreezeIndicator = AddToggle("Ice Freeze Indication", "IceIndi", true);
            GroundIndicator = AddToggle("Ground(0m) Indication", "0mIndi", true);
            OneThousandIndicator = AddToggle("1000m Indication", "1000mIndi", true);
            MiniMap = AddToggle("Minimap", "Minimap", false);
            MiniMapLerpValue = AddSlider("Minimap smoothess", "MinimapLerp", 0.015f, 0, 1);
            MiniMapHeight = AddSlider("Minimap height", "MinimapHeight", 400, 5, 1300);
            UseLockWindow = AddToggle("Enable Locking Window", "UseLockWindow", false);
            LockerHeight = AddSlider("Locking Window Height", "LockerH", 800, 100, 2000);
            LockerWidth = AddSlider("Locking Window Width", "LockerW", 800, 100, 2000);

            ReduceCameraShake = AddToggle("Reduce Camera Shake When Focused", "Noshake", false);
        }
        public override void BuildingUpdate()
        {
            ActivateTargetIndicator.DisplayInMapper = ConfigMenu.Value == 0;
            HideThisPanel.DisplayInMapper = ConfigMenu.Value == 0;
            //HidePanel.DisplayInMapper = ConfigMenu.Value == 0; Tracking computer aiming assistor thingy
            AdvTI.DisplayInMapper = ConfigMenu.Value == 0;
            AdvTIS.DisplayInMapper = AdvTI.IsActive && ConfigMenu.Value == 0;
            StartColour.DisplayInMapper = AdvTI.IsActive && ConfigMenu.Value == 0;
            EndColour.DisplayInMapper = AdvTI.IsActive && ConfigMenu.Value == 0;

            ActiveHUD.DisplayInMapper = ConfigMenu.Value == 1;
            HUDConfigMenu.DisplayInMapper = ConfigMenu.Value == 1;

            Height.DisplayInMapper = ConfigMenu.Value == 1 && HUDConfigMenu.Value == 1;
            Pitch.DisplayInMapper = ConfigMenu.Value == 1 && HUDConfigMenu.Value == 2;
            Center.DisplayInMapper = ConfigMenu.Value == 1 && HUDConfigMenu.Value == 0;
            MapCenter.DisplayInMapper = ConfigMenu.Value == 1 && HUDConfigMenu.Value == 4;
            MiniMap.DisplayInMapper = ConfigMenu.Value == 1 && HUDConfigMenu.Value == 5;
            MiniMapLerpValue.DisplayInMapper = ConfigMenu.Value == 1 && HUDConfigMenu.Value == 5;
            MiniMapHeight.DisplayInMapper = ConfigMenu.Value == 1 && HUDConfigMenu.Value == 5;
            Direction.DisplayInMapper = ConfigMenu.Value == 1 && HUDConfigMenu.Value == 3;
            UseLockWindow.DisplayInMapper = ConfigMenu.Value == 1 && HUDConfigMenu.Value == 6;
            LockerHeight.DisplayInMapper = ConfigMenu.Value == 1 && HUDConfigMenu.Value == 6;
            LockerWidth.DisplayInMapper = ConfigMenu.Value == 1 && HUDConfigMenu.Value == 6;

            HeightIndicator.DisplayInMapper = ConfigMenu.Value == 1 && Height.IsActive && HUDConfigMenu.Value == 1;
            IceFreezeIndicator.DisplayInMapper = ConfigMenu.Value == 1 && Height.IsActive && HUDConfigMenu.Value == 1;
            GroundIndicator.DisplayInMapper = ConfigMenu.Value == 1 && Height.IsActive && HUDConfigMenu.Value == 1;
            OneThousandIndicator.DisplayInMapper = ConfigMenu.Value == 1 && Height.IsActive && HUDConfigMenu.Value == 1;
        }

        public override void OnSimulateStart()
        {
            LockerRect = new Rect(new Vector2(Screen.width / 2 - LockerHeight.Value / 2, Screen.height / 2 - LockerWidth.Value / 2), new Vector2(LockerHeight.Value, LockerWidth.Value));
            ticc = Time.time;
            disp = !HideThisPanel.IsActive;

            BombDrop = new GameObject();
            BombDrop.transform.SetParent(transform);
            BombDrop.name = "Bomb Indicator";

            Light BombDropLight = BombDrop.AddComponent<Light>();

            BombDropLight.type = LightType.Spot;
            BombDropLight.intensity = 8;
            BombDropLight.range = Camera.main.farClipPlane;
            BombDropLight.color = Color.red;
            BombDropLight.cookieSize = 100;
            BombDropLight.range = 5;
            BombDrop.transform.LookAt(new Vector3(BombDrop.transform.position.x, BombDrop.transform.position.y - 10, BombDrop.transform.position.z));
            BombDropLight.enabled = false;
            BombDrop.AddComponent<DestroyIfEditMode>();

            if (AdvTI.IsActive)
            {
                float width = 1f;
                AdvBombDrop = this.gameObject.AddComponent<LineRenderer>();
                AdvBombDrop.material = new Material(Shader.Find("Particles/Additive"));
                AdvBombDrop.SetWidth(width, width);
                AdvBombDrop.SetColors(StartColour.Value, EndColour.Value);
                AdvBombDrop.SetPosition(0, transform.position);
                AdvBombDrop.SetVertexCount((int)AdvTIS.Value);
                AdvBombDrop.enabled = false;

            }

            机体准星 = Center.IsActive ? ModResource.GetTexture("HUD/Center.png") : null;
            俯仰刻度 = Pitch.IsActive ? ModResource.GetTexture("HUD/Gradienter.png") : null;
            正00纹理 = MapCenter.IsActive ? ModResource.GetTexture("HUD/Zero Zero Front.png") : null;
            负00纹理 = MapCenter.IsActive ? ModResource.GetTexture("HUD/Zero Zero Back.png") : null;
            现时高度指示纹理 = HeightIndicator.IsActive ? ModResource.GetTexture("HUD/Height Line.png") : null;
            冰层纹理 = IceFreezeIndicator.IsActive && HeightIndicator.IsActive ? ModResource.GetTexture("HUD/Ice Floor.png") : null;
            地面那一条杠杠滴纹理 = GroundIndicator.IsActive && HeightIndicator.IsActive ? ModResource.GetTexture("HUD/Floor Line.png") : null;
            一千杠杠 = OneThousandIndicator.IsActive && HeightIndicator.IsActive ? ModResource.GetTexture("HUD/OverICE Line.png") : null;
            北罗盘纹理 = Direction.IsActive ? ModResource.GetTexture("HUD/Direction Indicator North.png") : null;
            南罗盘纹理 = Direction.IsActive ? ModResource.GetTexture("HUD/Direction Indicator South.png") : null;
            西罗盘纹理 = Direction.IsActive ? ModResource.GetTexture("HUD/Direction Indicator East.png") : null;
            东罗盘纹理 = Direction.IsActive ? ModResource.GetTexture("HUD/Direction Indicator West.png") : null;
            小罗盘纹理 = Direction.IsActive ? ModResource.GetTexture("HUD/Direction Indicator Small.png") : null;
            小地图纹理 = MiniMap.IsActive ? ModResource.GetTexture("HUD/Minimap Addition.png") : null;
            锁定窗口 = UseLockWindow.IsActive? ModResource.GetTexture("HUD/Locker.png") : null;


            if (MiniMap.IsActive)
            {
                MiniMapGO = new GameObject("MiniMapCam");
                MiniMapGO.transform.SetParent(Machine.SimulationMachine);
                MiniMapCamera = MiniMapGO.AddComponent<Camera>();
                MiniMapCamera.CopyFrom(Camera.main);
                MiniMapCamera.aspect = 1;
                MiniMapCamera.orthographic = true;
                MiniMapCamera.orthographicSize = 150;
                MiniMapCamera.nearClipPlane = 45;
                MiniMapCamera.targetTexture = RTCamera;
                MiniMapCamera.clearFlags = CameraClearFlags.SolidColor;
                //MiniMapCamera.rect = new Rect(
                //            new Vector2(
                //                Screen.width * 10 / 9,
                //                Screen.height * 10 / 9),
                //            new Vector2(Screen.width / 10, Screen.height / 10));

                RTCamera = new RenderTexture(Screen.width / 5, Screen.height / 5, 24);
            }

        }

        public void SetParams(Vector3 dis, Vector3 velo, Vector3 rotat)
        {
            displacement = dis;
            velocity = velo;
            rotation = rotat;
        }

        public override void SimulateFixedUpdateAlways()
        {
            if (!StatMaster.isClient)
            {
                SetParams(Vector3.Lerp(displacement, Rigidbody.position, 0.2f), Vector3.Lerp(velocity, Rigidbody.velocity, 0.2f), Vector3.Lerp(rotation, Rigidbody.rotation.eulerAngles, 0.2f));
            }
            if (StatMaster.isMP && StatMaster.isHosting)
            {
                Message syncc = Messages.PilotPanelSync.CreateMessage(Block.From(this), displacement, velocity, rotation);
                ModNetworking.SendTo(Machine.Player, syncc);
            }
            direction = transform.forward;
            horizontal = new Vector3(-direction.z / direction.x, 0f, 1f).normalized;

            T1 = Time.time;
            dt = Time.fixedDeltaTime;

            if (disp)
            {
                vel0 = vel1;
                vel1 = velocity;
                acce = (vel1.magnitude - vel0.magnitude) / dt;
                overload = (Vector3.Dot((vel1 - vel0), this.transform.up) / dt + (float)38.5 * Vector3.Dot(Vector3.up, this.transform.up)) / (float)38.5;

                alt = displacement.y;
                climbrate = velocity.y;

                pitch = 90 - Mathf.Acos((2 - (direction - Vector3.up).magnitude * (direction - Vector3.up).magnitude) / 2) / Mathf.PI * 180;
                yaw = rotation.y;
                roll = Mathf.Sign(direction.x) * (Mathf.Acos((2 - (horizontal - this.transform.up).magnitude * (horizontal - this.transform.up).magnitude) / 2) / Mathf.PI * 180 - 90);
            }




        }
        public override void SimulateUpdateAlways()
        {
            if (!MainMo)
            {
                MainMo = Camera.main.GetComponent<MouseOrbit>();
            }
            MainMo.focusLerpSmooth = ReduceCameraShake.IsActive ? 999999999999999 : 8;

            if (MiniMap.IsActive)
                MiniMapGO.transform.SetParent(Machine.SimulationMachine);

            if (ActivateTargetIndicator.IsReleased)
            {
                disp2 = !disp2;
            }
            if (ActiveHUD.IsPressed)
            {
                HUD_Activated = !HUD_Activated;
            }
            //if (HidePanel.IsPressed)
            //{
            //    HidePanelBool = !HidePanelBool;
            //} Maybe for Tracking

            if (HUD_Activated)
            {
                if (ShowHUD == null)
                {
                    ShowHUD = new GameObject("ShowHUD");
                }

            }
            else
            {
                if (ShowHUD != null)
                {
                    Destroy(ShowHUD);
                }
            }

            if (disp2)
            {
                float grav = Physics.gravity.y;
                T_hitground = (velocity.y + Mathf.Sqrt(velocity.y * velocity.y + 2 * 38.5f * displacement.y)) / 38.5f;
                bombposition = new Vector3(
                    displacement.x + T_hitground * velocity.x,
                    4,
                    displacement.z + T_hitground * velocity.z
                    );
                Light BombLight = BombDrop.GetComponent<Light>();
                if (BombLight)
                {
                    BombLight.enabled = true;
                    BombLight.transform.position = bombposition;
                    BombLight.intensity = 8;
                    BombLight.spotAngle = Math.Abs(displacement.y * 3) + 60;
                    BombDrop.transform.LookAt(new Vector3(bombposition.x, bombposition.y - 10, bombposition.z));
                }

                if (AdvTI.IsActive)
                {
                    for (int i = (int)AdvTIS.Value; i >= 1; --i)
                    {
                        /*for (int i = (int)0; i != AdvTIS.Value; ++i)
                             {
                             T_hitground = ((velocity.y + Mathf.Sqrt(velocity.y * velocity.y + 2 * -Physics.gravity.y * displacement.y)) / -Physics.gravity.y);
                             float gotit = (T_hitground * ((AdvTIS.Value - i) / AdvTIS.Value));
                             bombposition = new Vector3(
                                     displacement.x + gotit * velocity.x,
                                     displacement.y + velocity.y * gotit + (gotit * Physics.gravity.y),
                                     displacement.z + gotit * velocity.z
                                     );*/
                        T_hitground = (velocity.y + Mathf.Sqrt(velocity.y * velocity.y + 2 * 38.5f * displacement.y / (int)AdvTIS.Value * i)) / 38.5f;
                        bombposition = new Vector3(
                            displacement.x + T_hitground * velocity.x,
                            displacement.y / (int)AdvTIS.Value * ((int)AdvTIS.Value - i),
                            displacement.z + T_hitground * velocity.z
                            );
                        //AdvBombDrop.SetPosition((int)Mathf.Clamp(i,0,AdvTIS.Value - 1), bombposition);
                        AdvBombDrop.SetPosition((int)Mathf.Clamp(AdvTIS.Value - i - 1, 0, AdvTIS.Value - 1), bombposition);
                        AdvBombDrop.SetPosition((int)AdvTIS.Value - 1, transform.position);
                        AdvBombDrop.enabled = true;
                        //Debug.Log(bombposition + " abd " + i + " tm " + T_hitground * (i / AdvTIS.Value) + " und " + (T_hitground * (i / AdvTIS.Value) * Physics.gravity.y));
                    }
                    //Debug.Log("Total Time:" + T_hitground);
                }

            }
            else
            {
                if (BombDrop)
                {
                    BombDrop.GetComponent<Light>().enabled = false;
                }
                if (AdvBombDrop)
                {
                    AdvBombDrop.enabled = false;
                }
            }




        }

        public void OnGUI()
        {
            if (StatMaster.isMP && (Machine.Player != Player.GetLocalPlayer())) return;
            if (disp && Game.IsSimulating)
            {

                GUILayout.BeginArea(new Rect(0f, 58f, 200f, 400f));

                if (num_vel == 1)
                {
                    row1 = vel1.magnitude;
                }
                else
                {
                    row1 = acce;
                }


                if (GUILayout.Button(string.Concat(label_row1, ((int)(row1)).ToString())))
                {
                    if (num_vel == 1)
                    {
                        label_row1 = "Accelaration/(m/s^2): ";
                        num_vel = 2;
                    }
                    else
                    {
                        label_row1 = "Speed/m/s: ";
                        num_vel = 1;
                    }
                }


                if (num_alt == 1)
                {
                    row2 = (int)alt;
                }
                else
                {
                    row2 = (int)climbrate;
                }

                if (GUILayout.Button(string.Concat(label_row2, row2.ToString())))
                {
                    if (num_alt == 1)
                    {
                        label_row2 = "ClimbRate/(m/s): ";
                        num_alt = 2;
                    }
                    else
                    {
                        label_row2 = "Altitude/m: ";
                        num_alt = 1;
                    }


                }


                GUILayout.Button(string.Concat("Overload/G: ", ((int)overload).ToString(), ".", (Mathf.Sign(overload) * ((int)((overload - ((int)overload)) * 10))).ToString()));

                if (num_time == 1)
                {
                    row3 = (int)(T1 - ticc);
                }
                else if (num_time == 2)
                {
                    row3 = T1 - tic;
                }
                else
                {
                    row3 = toc;
                }

                if (GUILayout.Button(string.Concat(label_row3, row3.ToString())))
                {
                    if (num_time == 1)
                    {
                        tic = T1;
                        label_row3 = "tic... ";
                        num_time = 2;

                    }
                    else if (num_time == 2)
                    {
                        label_row3 = "toc: ";
                        num_time = 3;
                        toc = T1 - tic;
                    }
                    else
                    {
                        label_row3 = "Time/s: ";
                        num_time = 1;
                    }
                }

                GUILayout.BeginHorizontal();

                GUILayout.Button(string.Concat("roll ", ((int)roll).ToString()));
                GUILayout.Button(string.Concat("pitch ", ((int)pitch).ToString()));
                GUILayout.Button(string.Concat("yaw ", ((int)yaw).ToString()));

                GUILayout.EndHorizontal();


                GUILayout.EndArea();

                if (MiniMap.IsActive)
                {
                    MiniMapCamera.enabled = HUD_Activated;
                }
                if (HUD_Activated)
                { OnHUDGUI(); }




            }

            //if (/*按下了reset*/Input.GetKey(s.GetComponent<MyBlockInfo>().key2/*这里是打算使用零件专用的按键*/))
            //{
            //    
            //}

        }

        private void OnHUDGUI()
        {
            float 全局屏幕比值W = Screen.width / 1920;
            float 全局屏幕比值H = Screen.height / 1080;
            Matrix4x4 UnRotatedTempMatrix = GUI.matrix;
            Transform MainCameraTransform = GameObject.Find("Main Camera").transform;
            CurrentCameraSpeed = Vector3.Dot(MainCameraTransform.GetComponent<Camera>().velocity, MainCameraTransform.forward);
            Vector3 zerooncamera = MainCameraTransform.GetComponent<Camera>().WorldToScreenPoint(Vector3.zero);

            if (MapCenter.IsActive)
            {
                GUIUtility.RotateAroundPivot(MainCameraTransform.eulerAngles.z, new Vector2(zerooncamera.x - 20, (Screen.height - zerooncamera.y) - 20));
                if (zerooncamera.z > 0)
                {
                    GUI.DrawTexture(
                        new Rect(
                            new Vector2(
                                zerooncamera.x - 20,
                                (Screen.height - zerooncamera.y) - 20),
                            new Vector2(40, 40)),
                        正00纹理);
                }
                else if (zerooncamera.z < 0)
                {
                    GUI.DrawTexture(
                        new Rect(
                            new Vector2(
                                zerooncamera.x - 20,
                                (Screen.height - zerooncamera.y) - 20),
                            new Vector2(40, 40)),
                        负00纹理);
                }
                GUI.matrix = UnRotatedTempMatrix;
            }
            //Camera.main.gameObject.AddComponent<HUDthingy>();
            //水平球.transform.position = Camera.main.gameObject.transform.position;
            //罗盘球.transform.position = Camera.main.gameObject.transform.position;
            //指示球.transform.position = Camera.main.gameObject.transform.position;
            //GUI.DrawTexture(new Rect(new Vector2(40, 40), new Vector2(40, 40)), 校准纹理, ScaleMode.ScaleAndCrop);
            //GUIUtility.ScaleAroundPivot(new Vector2(Screen.width / 1920, Screen.height / 1080), new Vector2(Screen.width, Screen.height) / 2);
            if (Center.IsActive)
            {
                GUI.DrawTexture(new Rect(new Vector2(Screen.width / 2 - 400, Screen.height / 2 - 400), new Vector2(800, 800)), 机体准星);
            }
            if (Center.IsActive)
            {
                GUI.DrawTexture(LockerRect, 锁定窗口);
            }
            GUIUtility.RotateAroundPivot(MainCameraTransform.eulerAngles.z, new Vector2(Screen.width / 2, Screen.height / 2));
            float FOVHeight = 180 / MainCameraTransform.GetComponent<Camera>().fieldOfView;
            float Height = MainCameraTransform.position.y;
            float 视角 = MainCameraTransform.eulerAngles.x;
            float 方向 = MainCameraTransform.eulerAngles.y;

            if (视角 >= 270) 视角 -= 360;
            Matrix4x4 RotatedTempMatrix = GUI.matrix;
            //G俯仰刻度.pixelInset = (new Rect(new Vector2(0 - 453.5f / 2, 0 - (1215 * 1f) * ((视角 + 180 - 10f) / 180)),
            //new Vector2(453.5f, 1215 * 1)));
            if (Pitch.IsActive)
            {
                GUI.DrawTexture(
                    new Rect(new Vector2(Screen.width / 2 - 453.5f / 2, Screen.height - (1215 * 1f) * ((视角 + 180 - (89.175f - 0.1484868421f * Screen.height / 2)) / 180)),
                    new Vector2(453.5f, 1215 * 1))
                    , 俯仰刻度
                    , ScaleMode.ScaleAndCrop);
            }

            if (Direction.IsActive)
            {
                //绘制罗盘(方向, 罗盘纹理, RotatedTempMatrix);
                float fov = MainCameraTransform.GetComponent<Camera>().fieldOfView;
                float 比例 = fov / 180;
                不旋转限制绘制罗盘(方向, 北罗盘纹理, 比例);
                不旋转限制绘制罗盘(方向 + 90, 东罗盘纹理, 比例);
                不旋转限制绘制罗盘(方向 - 90, 西罗盘纹理, 比例);
                不旋转限制绘制罗盘(方向 + 180, 南罗盘纹理, 比例);

                for (int i = -180; i < 180; i += 10)
                {
                    if (i != 0 || i != 90 || i != -90 || i != 180)
                    {
                        不旋转限制绘制罗盘(方向 + i, 小罗盘纹理, 比例);
                    }
                }

            }
            if (HeightIndicator.IsActive)
            {
                高度计状态 = 判断高度计状态(MainCameraTransform.position.y);

                if (高度计状态 != 比较用高度计状态 && !高度计_渐变中)
                {
                    高度计_渐变中 = true;
                }
                if (高度计_渐变中 && (高度计状态 == 1 && 比较用高度计状态 == 0))
                {
                    绘制0到1渐变高度计(MainCameraTransform.position.y, 高度计状态);
                }
                else if (高度计_渐变中 && (高度计状态 == 0 && 比较用高度计状态 == 1))
                {
                    绘制1到0渐变高度计(MainCameraTransform.position.y, 高度计状态);
                }
                else
                {
                    if (高度计状态 == -1) { 绘制天花板高度计(MainCameraTransform.position.y); }
                    if (高度计状态 == 0) { 绘制下冰层高度计(MainCameraTransform.position.y); }
                    if (高度计状态 == 1) { 绘制下千高度计(MainCameraTransform.position.y); }
                    if (高度计状态 == 2) { 绘制随意高度计(MainCameraTransform.position.y); }
                }
            }
            GUI.matrix = UnRotatedTempMatrix;

            if (MiniMap.IsActive)
            {
                MiniMapDrawing();
            }

            float angel = Mathf.Atan2((Screen.height / 6) * 2, Screen.width / 2) * Mathf.Rad2Deg;
            GUIUtility.RotateAroundPivot(angel, new Vector2(Screen.width / 2, Screen.height / 2));
        }

        private void MiniMapDrawing()
        {
            MiniMapCamera.transform.position = this.transform.position + Vector3.up * MiniMapHeight.Value;
            MiniMapCamera.transform.LookAt(
                Vector3.Lerp(
                    PreviousMiniMapLookAtPosition,
                    this.transform.position - Vector3.up * 400 + (velocity.sqrMagnitude > 1 ? velocity.normalized * 10 : transform.forward),
                    MiniMapLerpValue.Value * 0.1f)); ;
            PreviousMiniMapLookAtPosition = this.transform.position - Vector3.up * 400 + (velocity.sqrMagnitude > 11 ? velocity.normalized * 10 : this.transform.forward);
            MiniMapCamera.targetTexture = RTCamera;
            MiniMapCamera.clearFlags = CameraClearFlags.Color;
            MiniMapCamera.orthographicSize = Mathf.Max(150, 150 + this.transform.position.y);
            int 长 = Mathf.Min(Screen.width, Screen.height);
            GUI.DrawTexture(
                    new Rect(
                        new Vector2(
                            Screen.width * 7 / 10,
                            Screen.height * 7 / 10),
                        new Vector2(长 / 5, 长 / 5)),
                    RTCamera);
            GUI.DrawTexture(
                    new Rect(
                        new Vector2(
                            Screen.width * 7 / 10 - 25,
                            Screen.height * 7 / 10 - 25),
                        new Vector2(长 / 5 + 50, 长 / 5 + 50)),
                    小地图纹理);
        }

        private void 不旋转限制绘制罗盘(float 输入方向, Texture 纹理, float 比例)
        {
            float TheValue = Screen.width * (Mathf.Sin(-输入方向 * Mathf.Deg2Rad));
            if (Math.Abs(TheValue) >= 比例 * Screen.width)
            {
                return;
            }

            GUI.DrawTexture(
            new Rect(
            new Vector2(
                Screen.width / 2 + TheValue - 100f,
                Screen.height / 2 + Screen.height / 4 + (Screen.height / 2) * Math.Abs(Mathf.Sin(输入方向 / 2 * Mathf.Deg2Rad))),
            new Vector2(200, 200)),
            纹理);
        }

        private void 绘制罗盘(float 输入方向, Texture 纹理, Matrix4x4 正常矩阵)
        {
            GUIUtility.RotateAroundPivot(
            -输入方向,
            new Vector2(
            Screen.width / 2 + Screen.width * (Mathf.Sin(-输入方向 * Mathf.Deg2Rad)) - 2.5f,
            Screen.height / 2 + Screen.height / 4 + (Screen.height / 2) * Math.Abs(Mathf.Sin(输入方向 / 2 * Mathf.Deg2Rad))
            ));
            GUI.DrawTexture(
            new Rect(
            new Vector2(
                Screen.width / 2 + Screen.width * (Mathf.Sin(-输入方向 * Mathf.Deg2Rad)) - 2.5f,
                Screen.height / 2 + Screen.height / 4 + (Screen.height / 2) * Math.Abs(Mathf.Sin(输入方向 / 2 * Mathf.Deg2Rad))),
            new Vector2(5, 40)),
            纹理);
            GUI.matrix = 正常矩阵;
        }

        private void 绘制下冰层高度计(float CurrentHeight)
        {
            Transform ICEtrans = GameObject.Find("ICE FREEZE").transform;
            float IceFreezehickness = ICEtrans.localScale.y;
            float IceCenterHeight = ICEtrans.position.y;
            float IF比 = (49.5f / IceFreezehickness);
            if (IceFreezeIndicator.IsActive)
            {
                GUI.DrawTexture(new Rect(new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比, Screen.height / 1080 * 40 * IF比), new Vector2(283.5f * IF比, 100 * IF比)), 冰层纹理);
            }
            if (GroundIndicator.IsActive)
            {
                GUI.DrawTexture(new Rect(new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比, Screen.height / 1080 * 40 * IF比 + (100 * IF比 / IceFreezehickness * IceCenterHeight)), new Vector2(283.5f * IF比, 10)), 地面那一条杠杠滴纹理);
            }
            GUI.DrawTexture(new Rect(new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比, Screen.height / 1080 * 40 * IF比 + (100 * IF比 / IceFreezehickness * (IceCenterHeight - CurrentHeight))), new Vector2(283.5f * IF比, 10)), 现时高度指示纹理);
        }

        private void 绘制下千高度计(float CurrentHeight)
        {
            Transform ICEtrans = GameObject.Find("ICE FREEZE").transform;
            float IceFreezehickness = ICEtrans.localScale.y;
            float IceCenterHeight = ICEtrans.position.y;
            float IF比 = (49.5f / IceFreezehickness);
            float 千比 = (800 * IF比 - 20 * IF比) / 1000;
            if (GroundIndicator.IsActive)
            {
                GUI.DrawTexture(new Rect(new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 千比, Screen.height / 1080 * 800 * IF比), new Vector2(283.5f * IF比 * 千比, 5 * IF比)), 地面那一条杠杠滴纹理);//地面
            }
            if (OneThousandIndicator.IsActive)
            {
                GUI.DrawTexture(new Rect(new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 千比, Screen.height / 1080 * 20 * IF比), new Vector2(283.5f * IF比 * 千比, 5 * IF比)), 一千杠杠);//一千
            }
            if (IceFreezeIndicator.IsActive)
            {
                GUI.DrawTexture(new Rect(new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 千比, Screen.height / 1080 * 800 * IF比 - 千比 * IceCenterHeight), new Vector2(283.5f * IF比 * 千比, 100 * IF比 * 千比)), 冰层纹理);
            }
            GUI.DrawTexture(new Rect(new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 千比, Screen.height / 1080 * 800 * IF比 - 千比 * CurrentHeight), new Vector2(283.5f * IF比 * 千比, 10)), 现时高度指示纹理);
        }

        private void 绘制随意高度计(float CurrentHeight)
        {
            Transform ICEtrans = GameObject.Find("ICE FREEZE").transform;
            float IceFreezehickness = ICEtrans.localScale.y;
            float IceCenterHeight = ICEtrans.position.y;
            float IF比 = (49.5f / IceFreezehickness);
            float 自比 = (800 * IF比 - 20 * IF比) / CurrentHeight;
            GUI.DrawTexture(new Rect(new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 自比, Screen.height / 1080 * 20 * IF比), new Vector2(283.5f * IF比 * 自比, 10)), 现时高度指示纹理);
            if (GroundIndicator.IsActive)
            {
                GUI.DrawTexture(new Rect(new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 自比, Screen.height / 1080 * 800 * IF比), new Vector2(283.5f * IF比 * 自比, 5 * IF比)), 地面那一条杠杠滴纹理);
            }
            if (OneThousandIndicator.IsActive)
            {
                GUI.DrawTexture(new Rect(new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 自比, Screen.height / 1080 * 800 * IF比 - 自比 * 1000), new Vector2(283.5f * IF比 * 自比, 5 * IF比)), 一千杠杠);
            }
            if (IceFreezeIndicator.IsActive)
            {
                GUI.DrawTexture(new Rect(new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 自比, Screen.height / 1080 * 800 * IF比 - 自比 * IceCenterHeight), new Vector2(283.5f * IF比 * 自比, 100 * IF比 * 自比)), 冰层纹理);
            }
        }

        private void 绘制天花板高度计(float CurrentHeight)
        {
            Transform ICEtrans = GameObject.Find("ICE FREEZE").transform;
            float IceFreezehickness = ICEtrans.localScale.y;
            float IceCenterHeight = ICEtrans.position.y;

            float IF比 = (49.5f / IceFreezehickness);
            float IF2比 = IF比 * (IceCenterHeight / (IceCenterHeight - CurrentHeight));
            float WidthScale = IF比 * IceCenterHeight / (IceCenterHeight - CurrentHeight);
            if (IceFreezeIndicator.IsActive)
            {
                GUI.DrawTexture(
                new Rect(
                    new Vector2(
                        Screen.width / 2 - 100 * IF比 - 283.5f * WidthScale,
                        Screen.height / 1080 * 40 * IF比),
                    new Vector2(
                        283.5f * WidthScale,
                        100 * IF2比)),
                冰层纹理);
            }
            GUI.DrawTexture(
                new Rect(
                    new Vector2(
                        Screen.width / 2 - 100 * IF比 - 283.5f * WidthScale,
                        Screen.height / 1080 * 40 * IF比 + (100 * IF比 / IceFreezehickness * IceCenterHeight)),
                    new Vector2(
                        283.5f * WidthScale,
                        10)),
                现时高度指示纹理);



            if (GroundIndicator.IsActive)
            {
                GUI.DrawTexture(
                new Rect(
                    new Vector2(
                        Screen.width / 2 - 100 * IF比 - 283.5f * WidthScale,
                        Screen.height / 1080 * 40 * IF比 + (IceCenterHeight / (IceCenterHeight - CurrentHeight)) * (100 * IF比 / IceFreezehickness * IceCenterHeight)),
                    new Vector2(
                        283.5f * WidthScale,
                        10)),
                地面那一条杠杠滴纹理);
            }
        }

        private void 绘制0到1渐变高度计(float CurrentHeight, int ToSituation)
        {
            Transform ICEtrans = GameObject.Find("ICE FREEZE").transform;
            if (渐变高度计使用的临时函数 == 0) 渐变高度计使用的临时函数 = Time.time;
            float IceFreezehickness = ICEtrans.localScale.y;
            float IceCenterHeight = ICEtrans.position.y;
            float IF比 = (49.5f / IceFreezehickness);
            float 千比 = (800 * IF比 - 20 * IF比) / 1000;
            float zhe = Time.time - 渐变高度计使用的临时函数;
            if (GroundIndicator.IsActive)
            {
                GUI.DrawTexture(
                new Rect(
                    Vector2.Lerp(
                        new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比, Screen.height / 1080 * 800 * IF比),
                        new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 千比, Screen.height / 1080 * 800 * IF比), zhe
                        ),
                    Vector2.Lerp(
                        new Vector2(283.5f * IF比, 10),
                        new Vector2(283.5f * IF比 * 千比, 5 * IF比),
                        zhe)
                        ),
                    地面那一条杠杠滴纹理);//地面
            }
            if (OneThousandIndicator.IsActive)
            {
                GUI.DrawTexture(
                new Rect(
                    Vector2.Lerp(
                        new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比, Screen.height / 1080 * 40 * IF比),
                        new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 千比, Screen.height / 1080 * 20 * IF比), zhe
                        ),
                    Vector2.Lerp(
                        new Vector2(283.5f * IF比, 10),
                        new Vector2(283.5f * IF比 * 千比, 5 * IF比),
                        zhe)
                        ),
                    一千杠杠);//一千
            }
            if (IceFreezeIndicator.IsActive)
            {
                GUI.DrawTexture(
                    new Rect(
                        Vector2.Lerp(
                            new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比, Screen.height / 1080 * 40 * IF比),
                        new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 千比, Screen.height / 1080 * 800 * IF比 - 千比 * IceCenterHeight),
                        zhe),
                        Vector2.Lerp(
                            new Vector2(283.5f * IF比, 100 * IF比),
                            new Vector2(283.5f * IF比 * 千比, 100 * IF比 * 千比)
                        , zhe)), 冰层纹理);
            }

            GUI.DrawTexture(
                new Rect(
                    Vector2.Lerp(
                        new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比, Screen.height / 1080 * 40 * IF比 + (100 * IF比 / IceFreezehickness * (IceCenterHeight - CurrentHeight))),
                    new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 千比, Screen.height / 1080 * 800 * IF比 - 千比 * CurrentHeight), zhe),
                    Vector2.Lerp(
                        new Vector2(283.5f * IF比, 10),
                    new Vector2(283.5f * IF比 * 千比, 10), zhe)),
                现时高度指示纹理);
            if (Time.time - 渐变高度计使用的临时函数 >= 1 || (ToSituation != 1 && 比较用高度计状态 == 0))
            {
                高度计_渐变中 = false;
                比较用高度计状态 = 高度计状态;
                渐变高度计使用的临时函数 = 0;
            }
        }

        private void 绘制1到0渐变高度计(float CurrentHeight, int ToSituation)
        {
            Transform ICEtrans = GameObject.Find("ICE FREEZE").transform;
            if (渐变高度计使用的临时函数 == 0) 渐变高度计使用的临时函数 = Time.time;
            float IceFreezehickness = ICEtrans.localScale.y;
            float IceCenterHeight = ICEtrans.position.y;
            float IF比 = (49.5f / IceFreezehickness);
            float 千比 = (800 * IF比 - 20 * IF比) / 1000;
            float zhe = 1 - (Time.time - 渐变高度计使用的临时函数);

            if (GroundIndicator.IsActive)
                GUI.DrawTexture(
                    new Rect(
                        Vector2.Lerp(
                            new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比, Screen.height / 1080 * 800 * IF比),
                            new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 千比, Screen.height / 1080 * 800 * IF比), zhe
                            ),
                        Vector2.Lerp(
                            new Vector2(283.5f * IF比, 10),
                            new Vector2(283.5f * IF比 * 千比, 5 * IF比),
                            zhe)
                            ),
                        地面那一条杠杠滴纹理);//地面

            if (OneThousandIndicator.IsActive)
                GUI.DrawTexture(
                            new Rect(
                                Vector2.Lerp(
                                    new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比, Screen.height / 1080 * 40 * IF比),
                                    new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 千比, Screen.height / 1080 * 20 * IF比), zhe
                                    ),
                                Vector2.Lerp(
                                    new Vector2(283.5f * IF比, 10),
                                    new Vector2(283.5f * IF比 * 千比, 5 * IF比),
                                    zhe)
                                    ),
                                一千杠杠);//一千

            if (IceFreezeIndicator.IsActive)
                GUI.DrawTexture(
                    new Rect(
                        Vector2.Lerp(
                            new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比, Screen.height / 1080 * 40 * IF比),
                        new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 千比, Screen.height / 1080 * 800 * IF比 - 千比 * IceCenterHeight),
                        zhe),
                        Vector2.Lerp(
                            new Vector2(283.5f * IF比, 100 * IF比),
                            new Vector2(283.5f * IF比 * 千比, 100 * IF比 * 千比)
                        , zhe)), 冰层纹理);

            GUI.DrawTexture(
                new Rect(
                    Vector2.Lerp(
                        new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比, Screen.height / 1080 * 40 * IF比 + (100 * IF比 / IceFreezehickness * (IceCenterHeight - CurrentHeight))),
                    new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 千比, Screen.height / 1080 * 800 * IF比 - 千比 * CurrentHeight), zhe),
                    Vector2.Lerp(
                        new Vector2(283.5f * IF比, 10),
                    new Vector2(283.5f * IF比 * 千比, 10), zhe)),
                现时高度指示纹理);
            if (Time.time - 渐变高度计使用的临时函数 >= 1 || (ToSituation != 0 && 比较用高度计状态 == 1))
            {
                高度计_渐变中 = false;
                比较用高度计状态 = 高度计状态;
                渐变高度计使用的临时函数 = 0;
            }
        }

        private int 判断高度计状态(float Height)
        {
            int 最终状态;
            if (GameObject.Find("ICE FREEZE"))
            {
                Transform ICEtrans = GameObject.Find("ICE FREEZE").transform;
                float IceFreezehickness = ICEtrans.localScale.y;
                float IceCenterHeight = ICEtrans.position.y;
                if (Height < 0)
                {
                    最终状态 = -1;
                }
                else if (Height < (IceCenterHeight + IceFreezehickness / 2))
                {
                    最终状态 = 0;
                }
                else if (Height < 1000)
                {
                    最终状态 = 1;
                }
                else
                {
                    最终状态 = 2;
                }
                return 最终状态;
            }
            else { return -2; }
            /*else
            {
                if (Height <= 0)
                {
                    最终状态 = -1;
                }
                else if (Height < 1000)
                {
                    最终状态 = 1;
                }
                else
                {
                    最终状态 = 2;
                }
            }*/

        }


        public override void OnSimulateStop()
        {
            Destroy(ShowHUD);
        }
    }




}