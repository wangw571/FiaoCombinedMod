using spaar.ModLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FiaoCombinedMod
{
    public class NewLaserBlock : BlockScript
    {
        // Besiege-specific stuff
        protected MMenu LaserEditModeMenu;
        protected MMenu LaserAbilityModeMenu;
        //protected MMenu LaserCosmeticModeMenu;

        protected MColourSlider LaserColourSlider;

        protected MSlider LaserFocusSlider;
        protected MSlider LaserLengthSlider;
        protected MSlider LaserSafetyRange;
        //protected MSlider LaserKineticUpDownSlider;
        protected MSlider LaserKineticInOutSlider;
        // protected MSlider LaserKineticSideSlider;
        protected MSlider PenetrativeLengthMultiplier;

        protected MToggle LaserOnOffToggle;
        protected MToggle LightOptionToggle;
        //protected MToggle LaserFastUpdateToggle; // temporary 5-tick update limit bypass
        protected MKey LaserOnOffKey;

        protected MKey EffectActivateKey;
        protected MToggle ShrinkEffectToggle;
        protected MToggle InvertShrinking;
        protected MSlider ChargeHoldGasp;
        protected MMenu EffectParticleChargingPosition;

        protected MToggle HoldingToEmit;

        protected MToggle UseLegacy;

        protected MSlider LaserWidth;

        // Block-specific stuff
        private bool laserAtOff;

        private int CountDown;
        private int AlreadyCountDown;

        public float LaserLength;

        private float Multiplier;
        private float SqrtMultiplier;


        public ParticleSystemRenderer PSR;
        public ParticleSystem PS;
        public GameObject Especially;
        public Light PointLight;

        private string EffectKeyText1;
        private string EffectKeyText2;

        public CustomTubeRenderer CTR;
        public GameObject CTRGO;

        public int TheToggleProvideLight = 0;

        delegate void Init();
        delegate void CheckIfNeeeeeeedsUpdate();

        CheckIfNeeeeeeedsUpdate CINU;

        public override void SafeAwake()
        {
            Init init = Configuration.GetBool("UseChinese", false) ? new Init(ChineseInitialize) : new Init(EnglishInitialize);
            init();
        }

        void ChineseInitialize()
        {

            // Setup config window
            LaserEditModeMenu = AddMenu("laserEditMode", 0, new List<string>() { "功能", "通用设置" });
            LaserAbilityModeMenu = AddMenu("laserAbilityMode", 0, new List<string>() { "点燃", "施力", "冰冻", "爆破", "就好看" });

            LaserColourSlider = AddColourSlider("激光颜色", "laserColour", Color.red);

            LaserFocusSlider = AddSlider("聚焦乘子", "laserFocus", 1f, 0.08f, 0.5f);
            LaserLengthSlider = AddSlider("长度", "laserLength", 200f, 0.1f, 1500);
            LaserSafetyRange = AddSlider("安全长度", "laserSafeLength", 1f, 0.1f, 1500);
            LaserKineticInOutSlider = AddSlider("施力力度", "laserKinInOut", 0f, -2.5f, 2.5f);

            //LaserFastUpdateToggle = AddToggle("Fast Raycasting", "laserFastUpdate", false);
            LaserOnOffToggle = AddToggle("默认开启", "laserOnOffToggle", true);
            LightOptionToggle = AddToggle("保持提供灯光", "LightProbeToggle", false);
            LaserOnOffKey = AddKey("发射激光", "laserOnOffKey", KeyCode.Y);

            LaserWidth = AddSlider("宽度", "laserWidth", 0.5f, 0.001f, 10f);

            PenetrativeLengthMultiplier = AddSlider("穿透损失乘子", "PeneMulty", 0, 0, 1);

            EffectActivateKey = AddKey("聚能特效", "DoBomb", KeyCode.K);
            EffectKeyText1 = "聚能特效";
            EffectKeyText2 = "部署炸弹";

            ShrinkEffectToggle = AddToggle("使用粒子/收缩特效", "UseShrink", false);
            InvertShrinking = AddToggle("反转为扩散特效（粒子不变）", "InverseShrink", false);
            ChargeHoldGasp = AddSlider("间隔", "CountDown", 2.3f, 0, 10f);

            EffectParticleChargingPosition = AddMenu("ChargePosition", 0, new List<string> { "所有点添加粒子", "发射端添加粒子效果", "接触点添加粒子效果", "无粒子效果" });

            HoldingToEmit = AddToggle("仅在按住时激发激光", "HoldOnly", false);

            UseLegacy = AddToggle("使用传统渲染", "Legacy", false);

            // register mode switching functions with menu delegates
            LaserAbilityModeMenu.ValueChanged += CycleAbilityMode;
        }

        void EnglishInitialize()
        {

            // Setup config window
            LaserEditModeMenu = AddMenu("laserEditMode", 0, new List<string>() { "Ability", "Misc." });
            LaserAbilityModeMenu = AddMenu("laserAbilityMode", 0, new List<string>() { "Fire", "Kinetic", "Freeze", "Explosive", "Just Visual Effect" });

            LaserColourSlider = AddColourSlider("Beam Colour", "laserColour", Color.red);

            LaserFocusSlider = AddSlider("Laser Focus", "laserFocus", 1f, 0.08f, 0.5f);
            LaserLengthSlider = AddSlider("Laser Length", "laserLength", 200f, 0.1f, 1500);
            LaserSafetyRange = AddSlider("Safety Length", "laserSafeLength", 1f, 0.1f, 1500);

            //LaserKineticUpDownSlider = AddSlider("Up/Down Force", "laserKinUpDown", 1f, -2.5f, 2.5f);
            LaserKineticInOutSlider = AddSlider("In/Out Force", "laserKinInOut", 0f, -2.5f, 2.5f);
            //LaserKineticSideSlider = AddSlider("Sideways Force", "laserKinSide", 0f, -2.5f, 2.5f);

            //LaserFastUpdateToggle = AddToggle("Fast Raycasting", "laserFastUpdate", false);
            LaserOnOffToggle = AddToggle("Start On", "laserOnOffToggle", true);
            LightOptionToggle = AddToggle("Always Provide Light", "LightProbeToggle", false);
            LaserOnOffKey = AddKey("On/Off", "laserOnOffKey", KeyCode.Y);

            LaserWidth = AddSlider("Laser Width", "laserWidth", 0.5f, 0.001f, 10f);

            PenetrativeLengthMultiplier = AddSlider("Penetrative Multiplier", "PeneMulty", 0, 0, 1);

            EffectActivateKey = AddKey("Charge Laser", "DoBomb", KeyCode.K);

            EffectKeyText1 = "Charge Laser";
            EffectKeyText2 = "Deploy Bomb";


            ShrinkEffectToggle = AddToggle("Use Charging Effect", "UseShrink", false);
            InvertShrinking = AddToggle("Inverse Shrinking to Expanding", "InverseShrink", false);
            ChargeHoldGasp = AddSlider("Charging Countdown", "CountDown", 2.3f, 0, 10f);

            EffectParticleChargingPosition = AddMenu("ChargePosition", 0, new List<string> { "All have particles", "Only Laser Block have particles", "Only Intersection have particles", "None" });

            HoldingToEmit = AddToggle("Only emit laser when holding", "HoldOnly", false);

            UseLegacy = AddToggle("Use Legacy Rending", "Legacy", false);

            // register mode switching functions with menu delegates
            LaserAbilityModeMenu.ValueChanged += CycleAbilityMode;
        }

        // cycle through settings etc.
        protected override void BuildingUpdate()
        {
            PenetrativeLengthMultiplier.Value = Mathf.Clamp01(PenetrativeLengthMultiplier.Value);
            LaserFocusSlider.DisplayInMapper = LaserEditModeMenu.Value == 1 && UseLegacy.IsActive;
            LaserLengthSlider.DisplayInMapper = LaserEditModeMenu.Value == 1;
            LaserSafetyRange.DisplayInMapper = LaserEditModeMenu.Value == 1;
            PenetrativeLengthMultiplier.DisplayInMapper = LaserEditModeMenu.Value == 1;
            LaserWidth.DisplayInMapper = LaserEditModeMenu.Value == 1;
            LaserColourSlider.DisplayInMapper = LaserEditModeMenu.Value == 1;
            LaserOnOffToggle.DisplayInMapper = LaserEditModeMenu.Value == 1;
            HoldingToEmit.DisplayInMapper = LaserEditModeMenu.Value == 1;
            LightOptionToggle.DisplayInMapper = LaserEditModeMenu.Value == 1;


            LaserAbilityModeMenu.DisplayInMapper = LaserEditModeMenu.Value == 0;

            LaserKineticInOutSlider.DisplayInMapper = LaserEditModeMenu.Value == 0 && LaserAbilityModeMenu.Value == 1;

            ShrinkEffectToggle.DisplayInMapper = LaserEditModeMenu.Value == 1;
            EffectActivateKey.DisplayInMapper = /*(LaserAbilityModeMenu.Value == 3 ? LaserEditModeMenu.Value == 0 : */ShrinkEffectToggle.IsActive/*)*/;

            EffectActivateKey.DisplayName = (LaserAbilityModeMenu.Value == 3 ? EffectKeyText2 : EffectKeyText1);

            EffectParticleChargingPosition.DisplayInMapper = LaserEditModeMenu.Value == 1 && LaserAbilityModeMenu.Value == 3 && ShrinkEffectToggle.IsActive;
            InvertShrinking.DisplayInMapper = LaserEditModeMenu.Value == 1 && ShrinkEffectToggle.IsActive;

            ChargeHoldGasp.DisplayInMapper = (LaserAbilityModeMenu.Value == 3 ? LaserEditModeMenu.Value == 0 : LaserEditModeMenu.Value == 1 && ShrinkEffectToggle.IsActive);

        }
        private void HideEverything()
        {
            LaserAbilityModeMenu.DisplayInMapper = false;

            LaserColourSlider.DisplayInMapper = false;

            LaserFocusSlider.DisplayInMapper = false;
            LaserLengthSlider.DisplayInMapper = false;
            //LaserKineticUpDownSlider.DisplayInMapper = false;
            LaserKineticInOutSlider.DisplayInMapper = false;
            //LaserKineticSideSlider.DisplayInMapper = false;

            //LaserFastUpdateToggle.DisplayInMapper = false;
            LaserOnOffToggle.DisplayInMapper = false;
        }

        private void CycleAbilityMode(int value)
        {
            HideEverything();
            LaserAbilityModeMenu.DisplayInMapper = true;
            LaserLengthSlider.DisplayInMapper = true;
            //LaserFastUpdateToggle.DisplayInMapper = true;
            LaserOnOffToggle.DisplayInMapper = true;
            //if (value == 1)
            //{
            //LaserKineticUpDownSlider.DisplayInMapper = true;
            LaserKineticInOutSlider.DisplayInMapper = value == 1;
            //LaserKineticSideSlider.DisplayInMapper = true;
            //}
            /*else if (value == 4)*/
            LaserColourSlider.DisplayInMapper = value == 4;

        }

        protected override void OnSimulateStart()
        {
            rHInfos = new List<RHInfo>();

            Especially = new GameObject("TheThings");

            if (!UseLegacy.IsActive)
            {
                CINU = new CheckIfNeeeeeeedsUpdate(CheckIfNeedsUpdate);
                CTRGO = new GameObject("CubeTube");

                if (CTR == null)
                {
                    CTR = CTRGO.AddComponent<CustomTubeRenderer>();
                    //CTR.material = new Material(Shader.Find("Particles/Alpha Blended"));
                    //CTR.crossSegments = 1;
                    CTR.flatAtDistance = 1;
                    CTR.maxRebuildTime = 0.01f;
                }


                CTRGO.transform.SetParent(transform);
                CTRGO.transform.localPosition = Vector3.forward * 1.1f;
                CTRGO.transform.rotation = transform.rotation;
            }
            else
            {
                CINU = new CheckIfNeeeeeeedsUpdate(LegacyCheckIfNeedsUpdate);
                if (lr == null)
                {
                    lr = this.gameObject.AddComponent<LineRenderer>();
                }
                lr.material = new Material(Shader.Find("Particles/Additive"));
                lr.SetWidth(0.08f, 0.08f);

                Color ArgColour = LaserColourSlider.Value;

                lr.SetColors(Color.Lerp(ArgColour, Color.black, 0.45f),
                    Color.Lerp(ArgColour, Color.clear, 0.2f));
                lr.SetVertexCount(0);
            }

            Especially.transform.SetParent(this.transform);
            Especially.transform.localPosition = Vector3.forward * 1.1f;
            Especially.transform.LookAt(this.transform.position);

            //lr.material.SetColors(Color.Lerp(ArgColour, Color.black, 0.45f),
            //    Color.Lerp(ArgColour, Color.clear, 0.2f));



            laserAtOff = !LaserOnOffToggle.IsActive;

            PointLight = Especially.AddComponent<Light>();
            PointLight.color = LaserColourSlider.Value;
            PointLight.intensity = 3 * Math.Max(0.25f, PenetrativeLengthMultiplier.Value);
            PointLight.range = Mathf.Max(this.transform.localScale.x, this.transform.localScale.y) * LaserWidth.Value * 3f;
            PointLight.shadows = LightShadows.Soft;
            TheToggleProvideLight = LightOptionToggle.IsActive ? 1 : 0;
            if (ShrinkEffectToggle.IsActive && !PSR)
            {
                PS = Especially.AddComponent<ParticleSystem>();
                ParticleSystem.Particle NewParticle = new ParticleSystem.Particle();
                PS.startSize = 0.2f;
                PS.startColor = new Color(LaserColourSlider.Value.r, LaserColourSlider.Value.g, LaserColourSlider.Value.b, 0.5f);
                PS.startLifetime = ChargeHoldGasp.Value * 0.45f;
                PS.startSpeed = -0.5f;
                PS.scalingMode = ParticleSystemScalingMode.Shape;
                PS.SetParticles(new ParticleSystem.Particle[] { NewParticle }, 1);

                ParticleSystem.ColorOverLifetimeModule PSCOLM = PS.colorOverLifetime;
                PSCOLM.color = new Gradient() { alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(0.8f, 0.2f), new GradientAlphaKey(1, 1) }, colorKeys = new GradientColorKey[] { new GradientColorKey(PS.startColor, 0), new GradientColorKey(PS.startColor, 1) } };
                PSCOLM.enabled = true;

                PS.simulationSpace = ParticleSystemSimulationSpace.World;

                //ParticleSystem.Burst BUTS = new ParticleSystem.Burst(10, 10, 1000);

                PSR = Especially.GetComponent<ParticleSystemRenderer>();
                //PSR.material = new Material(Shader.Find("Particles/Alpha Blended"));
                PSR.material = new Material(Shader.Find("Particles/Additive"));
                PSR.material.mainTexture = (resources["LaserParticle.png"].texture);
            }
        }
        protected override void OnSimulateUpdate()
        {
            laserAtOff = HoldingToEmit.IsActive ? !LaserOnOffKey.IsDown : laserAtOff;
            if (LaserOnOffKey.IsReleased)
            {
                laserAtOff = HoldingToEmit.IsActive ? true : !laserAtOff;
            }

            PointLight.enabled = !laserAtOff || LightOptionToggle.IsActive;
            CINU(); // better to optimise more expensive stuff instead (eg. trig functions)

            if (!laserAtOff)
            {
                doPassiveAbility();
            }

        }
        protected override void OnSimulateFixedUpdate()
        {
            if (LaserAbilityModeMenu != null && ShrinkEffectToggle.IsActive /*&& !laserAtOff*/)
            {
                if (EffectActivateKey.IsDown && !laserAtOff)
                {

                    CountDown = (int)Mathf.Min(CountDown + 1, ChargeHoldGasp.Value * 100 + 1);
                    AlreadyCountDown = (int)Mathf.Min(AlreadyCountDown + 1, ChargeHoldGasp.Value * 100 + 1);
                    SetLights();
                    if (ShrinkEffectToggle.IsActive && EffectParticleChargingPosition.Value != 2 && EffectParticleChargingPosition.Value != 3)
                    {
                        Vector3 RandomPoint = EulerToDirection(UnityEngine.Random.Range(-360, 360), UnityEngine.Random.Range(-360, 360));
                        Multiplier = AlreadyCountDown / (ChargeHoldGasp.Value * 100);
                        SqrtMultiplier = Mathf.Sqrt(Multiplier);
                        PS.Emit(
                            Especially.transform.position + (RandomPoint * PS.startLifetime * 2 * Multiplier),
                            -RandomPoint * 2 * Multiplier * Multiplier + this.Rigidbody.velocity,
                            0.2f * Multiplier * Multiplier,
                            PS.startLifetime * SqrtMultiplier,
                            new Color(PS.startColor.r, PS.startColor.g, PS.startColor.b, 1));
                    }
                }
                else
                {
                    CountDown = (int)(CountDown * 0.9f);
                    AlreadyCountDown = (int)(AlreadyCountDown * 0.9f);
                    SetLights();
                }
                PS.maxParticles = 100000;
                PS.time -= Time.fixedDeltaTime * 2;
            }

        }
        protected void Ignite(RHInfo rH)
        {
            FireTag FT = rH.transform.GetComponentInChildren<FireTag>();
            if (FT) // ignite
            {
                FT.Ignite();
            }
            ///Just Ignite
            ///Meow
            ///
            /*
            else if (rH.transform.GetComponent(typeof(IExplosionEffect))) // explode stuff

            else if (rH.transform.GetComponent<BreakOnForceNoSpawn>()) // explode stuff
                rH.transform.GetComponent<BreakOnForceNoSpawn>().BreakExplosion(400f, rH.point, 10f, 0f);
            else if (rH.transform.GetComponent<BreakOnForce>())
                rH.transform.GetComponent<BreakOnForce>().BreakExplosion(400f, rH.point, 10f, 0f);
            else if (rH.transform.GetComponent<BreakOnForceNoScaling>())
                rH.transform.GetComponent<BreakOnForceNoScaling>().BreakExplosion(400f, rH.point, 10f, 0f);
            else if (rH.transform.GetComponent<CastleWallBreak>()) // explode ipsilon stuff
                rH.transform.GetComponent<CastleWallBreak>().BreakExplosion(400f, rH.point, 10f, 0f); */
        }
        private void doPassiveAbility()
        {
            if (!BeamHitAnything) return;
            if (ShrinkEffectToggle.IsActive && (CountDown < ChargeHoldGasp.Value * 100)) return;
            foreach (RHInfo rHinfo in rHInfos)
            {
                RHInfo rH = rHinfo;
                if (!rH.transform)
                {
                    continue;
                }
                switch (LaserAbilityModeMenu.Value)
                {
                    case 0: // fire
                        Ignite(rH);
                        break;
                    case 1: // kinetic
                        if (rH.rigidBody != null)
                        {
                            rH.rigidBody.AddForceAtPosition(LaserKineticInOutSlider.Value * (rH.rigidBody.transform.position - this.transform.position).normalized * 1000, rH.point);
                        }
                        //IExplosionEffect IEE = rH.transform.GetComponent<BreakOnForceNoSpawn>();

                        if (rH.transform.GetComponent<BreakOnForceNoSpawn>()) // explode stuff 
                        {
                            rH.transform.GetComponent<BreakOnForceNoSpawn>().BreakExplosion(400f, rH.point, 10f, 0f);
                        }
                        else if (rH.transform.GetComponent<BreakOnForce>())
                        {
                            rH.transform.GetComponent<BreakOnForce>().BreakExplosion(400f, rH.point, 10f, 0f);
                        }
                        else if (rH.transform.GetComponent<BreakOnForceNoScaling>())
                        {
                            rH.transform.GetComponent<BreakOnForceNoScaling>().BreakExplosion(400f, rH.point, 10f, 0f);
                        }
                        else if (rH.transform.GetComponent<CastleWallBreak>()) // explode ipsilon stuff
                        {
                            rH.transform.GetComponent<CastleWallBreak>().BreakExplosion(400f, rH.point, 10f, 0f);
                        }
                        ReduceBreakForce(rH.Joint);
                        break;
                    case 2: // freeze
                        IceTag IT = rH.transform.GetComponent<IceTag>();
                        if (IT)
                        {
                            IT.Freeze();
                        }
                        break;
                    case 3: // bomb
                        if (ShrinkEffectToggle.IsActive && EffectParticleChargingPosition.Value != 1 && EffectParticleChargingPosition.Value != 3 && EffectActivateKey.IsDown)
                        {
                            Vector3 POS = rH.point;
                            Vector3 RandomPoint = EulerToDirection(UnityEngine.Random.Range(-360, 360), UnityEngine.Random.Range(-360, 360));
                            PS.Emit(
                                POS + (RandomPoint * PS.startLifetime * 2 * Multiplier),
                                (-RandomPoint * 2 * Multiplier * Multiplier),
                                0.2f * Multiplier,
                                PS.startLifetime * SqrtMultiplier,
                                new Color(PS.startColor.r, PS.startColor.g, PS.startColor.b, 1));
                        }
                        if (LaserAbilityModeMenu.Value == 3 && (CountDown < ChargeHoldGasp.Value * 100 || !EffectActivateKey.IsDown)) continue;
                        GameObject BOMB = (GameObject)GameObject.Instantiate(PrefabMaster.BlockPrefabs[23].gameObject, rH.point, new Quaternion());
                        DestroyImmediate(BOMB.GetComponent<Renderer>());
                        BOMB.GetComponent<Rigidbody>().detectCollisions = false;
                        BOMB.GetComponentInChildren<Collider>().isTrigger = true;
                        BOMB.GetComponent<ExplodeOnCollideBlock>().Explodey();
                        ReduceBreakForce(rH.Joint);
                        break;
                    case 4:
                        return;
                }
            }
            if (LaserAbilityModeMenu.Value == 3 && (CountDown < ChargeHoldGasp.Value * 100 || !EffectActivateKey.IsDown)) return;
            CountDown = 0;
        }
        public void ReduceBreakForce(ConfigurableJoint Jointo)
        {
            if (Jointo && Jointo.breakForce == Mathf.Infinity)
            {
                Jointo.breakForce = 50000;
            }
        }
        public override void OnLoad(XDataHolder data)
        {
            LoadMapperValues(data);
        }
        public override void OnSave(XDataHolder data)
        {
            SaveMapperValues(data);
        }





        public struct RHInfo
        {
            public Vector3 point;
            public Transform transform;
            public Collider collider;
            public Rigidbody rigidBody;
            public ConfigurableJoint Joint;
            public RHInfo(Vector3 v, Transform t, Collider c, Rigidbody r, ConfigurableJoint jjint)
            {
                point = v;
                transform = t;
                collider = c;
                rigidBody = r;
                Joint = jjint;
            }
        }

        public Color colour;

        public LineRenderer lr;

        // need to port over everything from the old version
        public List<RHInfo> rHInfos;
        public bool BeamHitAnything;            // also used by laser block for abilities


        public void SetBeam()
        {
            Vector3 point = this.transform.position + this.transform.forward * 1.1f * this.transform.localScale.z;
            CTR.SetPoints(
                new Vector3[] {
                point,
                    //Mathf.Max(this.transform.localScale.x, this.transform.localScale.y) * LaserWidth.Value * Mathf.Pow((BombEffectOnOffToggle.IsActive && LaserAbilityModeMenu.Value == 3 ? 1 - (CountDown / (BombHoldTimer.Value * 100)) : 1), 2),
                point + this.transform.forward *  ( !laserAtOff? LaserLength :0)
                    //Mathf.Max(this.transform.localScale.x, this.transform.localScale.y) * 
                    //LaserFocusSlider.Value * 
                    //LaserWidth.Value * 
                    //(LaserLength / LaserLengthSlider.Value) * 
                    //Mathf.Pow(
                    //    (BombEffectOnOffToggle.IsActive && LaserAbilityModeMenu.Value == 3 ?
                    //        1 - (CountDown / (BombHoldTimer.Value * 100)) : 1)
                    //        , 2)
                }
                , LaserWidth.Value / 4 * Mathf.Pow((
                    ShrinkEffectToggle.IsActive ?
                                    (InvertShrinking.IsActive ?
                                            (CountDown / (ChargeHoldGasp.Value * 100)) :
                                            (1 - (CountDown / (ChargeHoldGasp.Value * 100))))
                                            : 1)
                            , 2)
                , LaserColourSlider.Value);

        }
        public void SetLights()
        {
            if (!PointLight.enabled) return;
            PointLight.intensity = 3 * Math.Max(0.25f, PenetrativeLengthMultiplier.Value + TheToggleProvideLight) + 5 * (Mathf.Pow((ShrinkEffectToggle.IsActive ? (InvertShrinking.IsActive ? (1 - (CountDown / (ChargeHoldGasp.Value * 100))) : (CountDown / (ChargeHoldGasp.Value * 100))) : 0) + TheToggleProvideLight, 2));
            PointLight.range = 3 * Mathf.Max(this.transform.localScale.x, this.transform.localScale.y) * LaserWidth.Value + 5 * Mathf.Pow((ShrinkEffectToggle.IsActive ? (InvertShrinking.IsActive ? 1 - (CountDown / (ChargeHoldGasp.Value * 100)) : (CountDown / (ChargeHoldGasp.Value * 100))) : 0) + TheToggleProvideLight, 2);
        }
        public void CheckIfNeedsUpdate()
        {
            SetBeam();
            if (!laserAtOff)
            {
                UpdateFromPoint(this.transform.TransformPoint(0, 0, 1.3f), this.transform.forward);
                CTR.enabled = true;
                //DrawBeam();
            }

        }

        public void LegacyCheckIfNeedsUpdate()
        {

            if (!laserAtOff)
            {
                UpdateFromPoint(this.transform.TransformPoint(0, 0, 1.3f + LaserSafetyRange.Value), this.transform.forward);
                LegacyDrawBeam();
                LegacySetBeamWidth();
            }
            else
            {
                lr.SetVertexCount(0);
            }

        }
        private void UpdateFromPoint(Vector3 point, Vector3 dir)
        {
            Vector3 lastPoint = point;
            Vector3 lastDir = dir;
            BeamHitAnything = false;
            rHInfos.Clear();

            LaserLength = LaserLengthSlider.Value;

            BeamHitAnything = false;

            foreach (RaycastHit Hito in Physics.RaycastAll(lastPoint, lastDir, LaserLength))
            {
                if (!Hito.collider.isTrigger)
                {
                    rHInfos.Add(new RHInfo(Hito.point, Hito.transform, Hito.collider, Hito.rigidbody, Hito.transform.GetComponent<ConfigurableJoint>()));
                    BeamHitAnything = true;
                    {

                        float SqrDist = (this.transform.position - Hito.point).sqrMagnitude;
                        if (SqrDist >= Mathf.Pow(LaserLength * PenetrativeLengthMultiplier.Value, 2))
                        {
                            LaserLength = Mathf.Sqrt(SqrDist);
                        }
                        else
                        {
                            LaserLength *= PenetrativeLengthMultiplier.Value;
                        }
                    }
                }
            }
        }

        private void LegacyDrawBeam()
        {
            lr.SetVertexCount(2);
            lr.SetPositions(new Vector3[] { this.transform.position + this.transform.forward * 0.8f, this.transform.position + this.transform.forward * LaserLength });
        }
        private void LegacySetBeamWidth()
        {
            lr.SetWidth(
                Mathf.Max(this.transform.localScale.x, this.transform.localScale.y) * LaserWidth.Value * Mathf.Pow((ShrinkEffectToggle.IsActive ? (!InvertShrinking.IsActive ? 1 - (CountDown / (ChargeHoldGasp.Value * 100)) : (CountDown / (ChargeHoldGasp.Value * 100))) : 1), 2),
                Mathf.Max(this.transform.localScale.x, this.transform.localScale.y) * LaserFocusSlider.Value * LaserWidth.Value * (LaserLength / LaserLengthSlider.Value) * Mathf.Pow((ShrinkEffectToggle.IsActive ? (!InvertShrinking.IsActive ? 1 - (CountDown / (ChargeHoldGasp.Value * 100)) : (CountDown / (ChargeHoldGasp.Value * 100))) : 1), 2));
        }


        Vector3 EulerToDirection(float Elevation, float Heading)
        {
            float elevation = Elevation * Mathf.Deg2Rad;
            float heading = Heading * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(elevation) * Mathf.Sin(heading), Mathf.Sin(elevation), Mathf.Cos(elevation) * Mathf.Cos(heading));
        }



        void LogHo()
        {
            Debug.Log("Ho");
        }
    }
}
