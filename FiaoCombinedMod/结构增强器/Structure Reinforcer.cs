using System;
using System.Collections.Generic;
using UnityEngine;
using Modding;
using Modding.Blocks;


namespace FiaoCombinedMod
{
    public class StructureReinforcer : Modding.BlockScript
    {
        public MKey Activate;
        public MSlider projectionDistance;
        public MSlider projectionAngle;

        public MSlider passiveReinforcement;
        public MSlider activeReinforcement;

        public MToggle ActivateOnStart;

        public MToggle AdaptBrace;
        public MToggle enableProjectionSetting;
        public MToggle DisableVisualEffect;

        ReinforcerScript rfs;

        public override void SafeAwake()
        {
            base.SafeAwake();
            EnglishInitialize();
        }

        void EnglishInitialize()
        {
            Activate = AddKey("Activate", "act", KeyCode.C);
            projectionDistance = AddSlider("Projection Distance", "ProjDist", 0.1f, -1, 2);
            projectionAngle = AddSlider("Projection Angle", "ProjAng", 0.01f, -360, 360);
            passiveReinforcement = AddSlider("Passive Strength Multiplier", "PSM", 15f, 0f, Mathf.Infinity);
            activeReinforcement = AddSlider("Active Strength Multiplier", "ASM", 175f, 0f, Mathf.Infinity);
            ActivateOnStart = AddToggle("Activate On Start (Not Suggested)", "AOS", false);
            AdaptBrace = AddToggle("Adapt on Brace", "adapt", true);
            enableProjectionSetting = AddToggle("Joint Hardening", "Hard", false);
            DisableVisualEffect = AddToggle("Disable Visual Effect", "NoVis", false);
        }

        // cycle through settings etc.
        public override void BuildingUpdate()
        {
            projectionDistance.DisplayInMapper = enableProjectionSetting.IsActive;
            projectionAngle.DisplayInMapper = enableProjectionSetting.IsActive;
        }


        public override void OnSimulateStart()
        {

            if (StatMaster.isMP && !StatMaster.isClient)
            {

            }
        }
        public override void SimulateUpdateAlways()
        {
            if (!StatMaster.isClient)
            {
                if (!this.GetComponent<ReinforcerScript>())
                {
                    rfs = this.gameObject.AddComponent<ReinforcerScript>();
                    rfs.SetMe(this.activeReinforcement.Value, this.passiveReinforcement.Value, this.projectionDistance.Value, this.projectionAngle.Value, this.enableProjectionSetting.IsActive, this.DisableVisualEffect.IsActive, this.AdaptBrace.IsActive);
                    rfs.Passive(passiveReinforcement.Value, AdaptBrace.IsActive);
                    if (this.ActivateOnStart.IsActive)
                    {
                        rfs.activate = true;
                    }

                }

                if (Activate.IsHeld)
                {
                    rfs.activate = true;
                }
            }
            if (StatMaster.isMP && !StatMaster.isClient)
            {
            }
        }



        public void ReduceBreakForce(ConfigurableJoint Jointo)
        {
            if (Jointo && Jointo.breakForce == Mathf.Infinity)
            {
                Jointo.breakForce = 50000;
            }
        }








        public Vector3 EulerToDirection(float Elevation, float Heading)
        {
            float elevation = Elevation * Mathf.Deg2Rad;
            float heading = Heading * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(elevation) * Mathf.Sin(heading), Mathf.Sin(elevation), Mathf.Cos(elevation) * Mathf.Cos(heading));
        }
    }

    public class ReinforcerScript : MonoBehaviour
    {
        private float Amultiplier, Pmultiplier, PprojD, PprojA;
        public float Counter = 2.5f;
        private Color myOrigColor;
        private bool MeHaveColor = false, passiveSetSuccessfully = false, adaptOnBrace = false, changeProjection = true, meHaveNoVisual = false;
        BlockVisualController blockVisualController = null;

        public bool activate = false;
        private bool trueActivate = false;
        private bool MeJointSet = false;

        private int PrevListCount = 0;

        public List<ReinforcerScript> ListReinforcedScript = new List<ReinforcerScript>();

        public void SetMe(float Amul, float Pmul, float projD, float projA, bool changeProj, bool ColorStuff, bool adaptOnBraceE)
        {
            Amultiplier = Amul;
            Pmultiplier = Pmul;
            PprojD = projD;
            PprojA = projA;
            changeProjection = changeProj;
            meHaveNoVisual = ColorStuff;
            adaptOnBrace = adaptOnBraceE;
        }

        public void Passive(float multiplier, bool adaptBrace)
        {
            ConfigurableJoint thisJoint = this.GetComponent<ConfigurableJoint>();
            if (thisJoint != null)
            {
                thisJoint.breakForce = thisJoint.breakForce * multiplier;
                thisJoint.breakTorque = thisJoint.breakTorque * multiplier;
                passiveSetSuccessfully = true;
            }
            if (adaptOnBrace)
            {
                ConfigurableJoint[] thisJoints = this.GetComponents<ConfigurableJoint>();
                foreach (Joint jt in thisJoints)
                {
                    if (jt != null)
                    {
                        jt.breakForce = jt.breakForce * multiplier;
                        jt.breakTorque = jt.breakTorque * multiplier;
                        passiveSetSuccessfully = true; 
                    }
                }
            }
        }
        void FixedUpdate()
        {
            PassiveCheck();
            MyBaseBlockAdd();
            bBAdd();
            ActivationCheck();
        }

        private void ActivationCheck()
        {
            if (trueActivate)
            {
                Activee();
            }
            if (activate) trueActivate = true;
            if (MeHaveColor && blockVisualController != null)
            {
                blockVisualController.renderers[0].material.color = 
                    Color.Lerp(
                        myOrigColor,
                        new Color(0.4f, 0.4f, 0.8f, 1f) * 5,
                        //new Color(0.6274f, 0.8431f, 1, 1f) * 2,
                        //myOrigColor * 19,
                        //Color.black,
                        Mathf.Clamp(Mathf.PingPong(Counter, 5)-2.5f, 0, 1));
                Counter += 0.05f;
            }
        }

        private void bBAdd()
        {
            BlockBehaviour bB = this.GetComponent<BlockBehaviour>();
            if (bB && (bB.jointsToMe != null))
            {
                foreach (Joint jt in bB.jointsToMe)
                {
                    if (jt != null)
                        AddMe(jt.gameObject);
                }
            }
            if (bB && (bB.iJointTo != null))
            {
                foreach (Joint jt in bB.iJointTo)
                {
                    if (jt != null)
                        AddMe(jt.gameObject);
                }
            }
        }

        private void MyBaseBlockAdd()
        {
            ConfigurableJoint thisJoint = this.GetComponent<ConfigurableJoint>();
            if (thisJoint != null)
            {
                if (thisJoint.connectedBody != null)
                {
                    if (thisJoint.connectedBody.gameObject != null)
                        AddMe(thisJoint.connectedBody.gameObject);
                }
            }
            if (adaptOnBrace)
            {
                ConfigurableJoint[] thisJoints = this.GetComponents<ConfigurableJoint>();
                foreach (Joint jt in thisJoints)
                {
                    if (jt != null && jt.connectedBody != null)
                    {
                        AddMe(jt.connectedBody.gameObject);
                    }
                }
            }
        }

        private void PassiveCheck()
        {
            if (!passiveSetSuccessfully)
            {
                Passive(Pmultiplier, adaptOnBrace);
            }
        }

        void AddMe(GameObject go)
        {
            if (!go.GetComponent<ReinforcerScript>())
            {
                ReinforcerScript tsss = go.AddComponent<ReinforcerScript>();
                tsss.SetMe(Amultiplier, Pmultiplier, PprojD, PprojA, changeProjection, meHaveNoVisual, adaptOnBrace);
                ListReinforcedScript.Add(tsss);
            }
        }

        void Activee()
        {
            if (!MeJointSet)
            {
                ConfigurableJoint thisJoint = this.GetComponent<ConfigurableJoint>();
                if (thisJoint != null)
                {
                    thisJoint.breakForce = thisJoint.breakForce * Amultiplier;
                    thisJoint.breakTorque = thisJoint.breakTorque * Amultiplier;
                    if (changeProjection)
                    {
                        thisJoint.projectionDistance = PprojD;
                        thisJoint.projectionAngle = PprojA;
                        thisJoint.projectionMode = JointProjectionMode.PositionAndRotation;
                    }
                    MeJointSet = true;
                }
            

                if (adaptOnBrace)
                {
                    ConfigurableJoint[] thisJoints = this.GetComponents<ConfigurableJoint>();
                    foreach (ConfigurableJoint thisJointt in thisJoints)
                    {
                        if (thisJointt != null)
                        {
                            thisJointt.breakForce = thisJoint.breakForce * Amultiplier;
                            thisJointt.breakTorque = thisJoint.breakTorque * Amultiplier;
                            if (changeProjection)
                            {
                                thisJoint.projectionDistance = PprojD;
                                thisJoint.projectionAngle = PprojA;
                                thisJointt.projectionMode = JointProjectionMode.PositionAndRotation;
                            }
                            MeJointSet = true;
                        }
                    }
                }
            }

            if (!MeHaveColor && !meHaveNoVisual)
            {
                blockVisualController = this.GetComponent<BlockVisualController>();
                if (blockVisualController && blockVisualController.renderers.Length > 0)
                {
                    //myOrigColor = blockVisualController.renderers[0].material.color;
                    myOrigColor = new Color(blockVisualController.renderers[0].material.color.r, blockVisualController.renderers[0].material.color.g, blockVisualController.renderers[0].material.color.b, blockVisualController.renderers[0].material.color.a);
                    MeHaveColor = true;
                }
            }

            if (PrevListCount != ListReinforcedScript.Count)
            {
                PrevListCount = ListReinforcedScript.Count;
                foreach (ReinforcerScript rs in ListReinforcedScript)
                {
                    if (rs != null)
                    {
                        rs.activate = true;
                    }
                }
            }
        }
    }
}
