using System;
using UnityEngine;
using System.Collections.Generic;
namespace FiaoCombinedMod
{

    // If you need documentation about any of these values or the mod loader
    // in general, take a look at https://spaar.github.io/besiege-modloader.


    /// <summary>
    /// Note: 
    /// HP: The HP will be limited as an square of an acceleration, as (a+b)^2 < a^2 + b^2
    /// Radar: The radar detective distance will change as the velocity changes. 
    /// 
    /// 
    /// Basic Behavior:
    /// 1.Exist, as decloak
    /// 2.Select Target, 
    ///     Explosives first, bombs, tracking computers first, grenade and rocket will be selected as the ammunation is possible to ignite them. 
    ///     Other aggressive second, include cannon, watercannon and flamethrower
    ///         -Mainly select the joint between aggressive block and vulnerable blocks.
    ///         -If connected by Grabber, find the chain of the end and destory that
    ///             -If multiple connected, ignore this target if possible.
    ///     Machanic blocks after, mainly focus on wheel(CogMotor), piston, suspension, flying block, hinge
    ///     Aeromatic propeller, wings and non-powered wheels, as well as structures as last. 
    ///     
    ///     -Not ignoring incoming only if HP is low enough that a crash can disable it
    ///         -Set it with make flame effect and non-fireable and explode after it crash.
    ///     -When close enough to an incoming at the front (velocity,sqrmagnitude * 3), to 5
    /// 3.Set navigation destination and attack
    ///     Attack by projectiles.
    /// 4.Maybe - Switch target every 10 second.
    /// 5.After target destoryed/close enough
    ///     - Switch if attitude is allowed(higher than velocity,sqrmagnitude * 3) and target is far enough(velocity,sqrmagnitude * 8)
    ///     - Escape 
    /// 6.Climb to sky when is clear, set as direction + 500 Y, else keep curise flight and not ignoring incoming. 
    /// 
    /// Target Velocity: Use average velocity for 0.5 seconds
    /// Orbiting Target should be perpendicular to the target's velocity if it's moving. 
    /// </summary>

    public class DroneControlBlockBehavior : Modding.BlockScript
    {
        private RaycastHit hitt;

        MKey Activation;
        MKey Engage;
        MKey Recall;
        MKey ForceEngage;
        MMenu DroneAIType;
        MMenu DroneSize;
        MMenu Difficulty;
        public MSlider OrbitRadius;
        //MMenu DroneWeapon;
        MSlider DroneAmount;
        MToggle ContinousSpawn;
        public MSlider DroneTag;

        public List<FullyAIDrone> AIDroneList;
        public List<Vector3> RelativeLeavePositions;
        public List<Boolean> AIDroneReachedOriginalPosition;

        public bool Engaging = false;

        GameObject DetectiveSphere;
        GameObject Target;
        public override void SafeAwake()
        {
            Engage = AddKey("Engage", "Engage", KeyCode.T);
            ForceEngage = AddKey("Forced Engege", "FEngage", KeyCode.X);
            Recall = AddKey("Recall", "Rc", KeyCode.R);
            //DroneSize = new MMenu("SizeType", 0, new List<string>() { "Heavt", "Medium", "Light" });
            //Difficulty = new MMenu("Difficulty", 0, new List<string>() { "Aggressive", "Defensive", "For Practice" });
            //Aggressive: To all moving items|Defensive: Only to aggressive blocks|For Practice: Flying around, keeping radar function, 
            OrbitRadius = AddSlider("Orbit Radius", "OrbitRadius", 100, 5, 750);
            //DroneAmount = new MSlider("Drone Amount", "Amount", 3, 1, 15);
            //ContinousSpawn = new MToggle("Spawn Drones\r\n after losing", "CSpawn", false);
            DroneTag = AddSlider("Drone Tag", "Tag", 0, 0, 100);

            AIDroneList = new List<FullyAIDrone>();
        }
        public override void BuildingUpdate()
        {
            DroneTag.Value = (int)DroneTag.Value;
            /*if (DroneAIType.Value == 0)
            {
                Engage.DisplayInMapper = true;
                ForceEngage.DisplayInMapper = true;
                OrbitRadius.DisplayInMapper = true;
            }
            else
            {
                Engage.DisplayInMapper = false;
                ForceEngage.DisplayInMapper = false;
                OrbitRadius.DisplayInMapper = false;
            }*/
        }
        public override void SimulateUpdateAlways()
        {
            //Debug.Log(AIDroneList.Count);
            if (Engage.IsPressed)
            {
                RaycastHit[] rhs = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), float.PositiveInfinity);
                if (rhs.Length != 0)
                {
                    foreach (RaycastHit hit in rhs)
                    {
                        if (hit.transform.position != this.transform.position && hit.collider.attachedRigidbody != null && !hit.collider.isTrigger)
                        {
                            Target = hit.transform.gameObject;
                            if (Target.GetComponentInParent<MyBlockInfo>() || this.name.Contains(("IsCloaked")))
                            {
                                if (Target.GetComponentInParent<MyBlockInfo>().gameObject.name.Contains("IsCloaked") || this.name.Contains(("IsCloaked")))
                                {
                                    Target = null;
                                    continue;
                                }
                            }
                            Engaging = true;
                            foreach (FullyAIDrone FAD in AIDroneList)
                            {
                                FAD.currentTarget = Target;
                                FAD.IgnoreIncoming = false;
                            }
                            break;
                        }
                    }
                }

                //if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitt, float.PositiveInfinity))
                //{
                //    if (hitt.transform.position != this.transform.position && hitt.collider.attachedRigidbody != null)
                //    {
                //        Target = hitt.transform.gameObject;
                //        if (Target.GetComponentInParent<MyBlockInfo>() || this.name.Contains(("IsCloaked")))
                //        {
                //            if (Target.GetComponentInParent<MyBlockInfo>().gameObject.name.Contains("IsCloaked") || this.name.Contains(("IsCloaked")))
                //            {
                //                Target = null;
                //            }
                //        }
                //    }
                //}
            }
            if (ForceEngage.IsPressed)
            {
                RaycastHit[] rhs = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), float.PositiveInfinity);
                if (rhs.Length != 0)
                {
                    foreach (RaycastHit hit in rhs)
                    {
                        if (hit.transform.position != this.transform.position && hit.collider.attachedRigidbody != null && !hit.collider.isTrigger)
                        {
                            Target = hit.transform.gameObject;
                            if (Target.GetComponentInParent<MyBlockInfo>() || this.name.Contains(("IsCloaked")))
                            {
                                if (Target.GetComponentInParent<MyBlockInfo>().gameObject.name.Contains("IsCloaked") || this.name.Contains(("IsCloaked")))
                                {
                                    Target = null;
                                    continue;
                                }
                            }
                            Engaging = true;
                            foreach (FullyAIDrone FAD in AIDroneList)
                            {
                                FAD.currentTarget = Target;
                                FAD.IgnoreIncoming = true;
                            }
                            break;
                        }
                    }
                }
            }
            if (Recall.IsPressed)
            {
                if (Engaging)
                {
                    foreach (FullyAIDrone FAD in AIDroneList)
                    {
                        FAD.currentTarget = null;
                        FAD.IgnoreIncoming = false;
                        FAD.IAmEscapingOrReturning = true;
                        FAD.targetPoint = RelativeLeavePositions[AIDroneList.IndexOf(FAD)];
                    }
                }
            }
        }

        public override void SimulateFixedUpdateAlways()
        {
            //FAD.targetPoint = RelativeLeavePositions[AIDroneList.IndexOf(FAD)];
            if (!Engaging)
            {
                RelativeLeavePositions = new List<Vector3>();
                foreach (FullyAIDrone FAD in AIDroneList)
                {
                    RelativeLeavePositions.Add(this.transform.InverseTransformPoint(FAD.transform.position));
                }
            }
        }

        //public Vector3 PleaseGiveMeNewOrbitPoint(Vector3 NowPoistion, Vector3 MyVeloDirection, bool GiveMeRandom)
        //{
        //    Vector3 Relatived = this.transform.InverseTransformPoint(NowPoistion);
        //    Vector3 Returner = Vector3.zero;
        //    float DroneRelativeAngleX = Vector3.Angle(transform.forward, new Vector3(Relatived.x, 0, Relatived.z));
        //    float DroneRelativeAngleY = Vector3.Angle(transform.forward, new Vector3(0, Relatived.y, Relatived.z));
        //    if (!GiveMeRandom)
        //    {
        //        Vector3 one = EulerToDirection(DroneRelativeAngleX + 15, DroneRelativeAngleY + 15) * OrbitRadius.Value;
        //        Vector3 two = EulerToDirection(DroneRelativeAngleX + 15, DroneRelativeAngleY - 15) * OrbitRadius.Value;
        //        Vector3 three = EulerToDirection(DroneRelativeAngleX - 15, DroneRelativeAngleY + 15) * OrbitRadius.Value;
        //        Vector3 four = EulerToDirection(DroneRelativeAngleX - 15, DroneRelativeAngleY - 15) * OrbitRadius.Value;
        //        //Vector3.Min(Vector3.Min(Returner - one, Returner - two), Vector3.Min(Returner - three, Returner - four));
        //        if(Vector3.SqrMagnitude((Relatived + MyVeloDirection) - four) > Vector3.SqrMagnitude((Relatived + MyVeloDirection) - three))
        //        {
        //            Returner = three;
        //        }
        //        else if(Vector3.SqrMagnitude((Relatived + MyVeloDirection) - three) > Vector3.SqrMagnitude((Relatived + MyVeloDirection) - two))
        //        {
        //            Returner = two;
        //        }
        //        else if (Vector3.SqrMagnitude((Relatived + MyVeloDirection) - two) > Vector3.SqrMagnitude((Relatived + MyVeloDirection) - one))
        //        {
        //            Returner = one;
        //        }
        //        else if (Vector3.SqrMagnitude((Relatived + MyVeloDirection) - one) > Vector3.SqrMagnitude((Relatived + MyVeloDirection) - four))
        //        {
        //            Returner = four;
        //        }
        //        //Returner = EulerToDirection(UnityEngine.Random.value * 360 - 180, 15) * OrbitRadius.Value;
        //    }
        //    else
        //    {
        //        Returner = EulerToDirection(UnityEngine.Random.value * 360 - 180, UnityEngine.Random.value * 720 - 360) * OrbitRadius.Value;
        //    }

        //    return Returner;
        //}

        //Vector3 EulerToDirection(float Elevation, float Heading)
        //{
        //    float elevation = Elevation * Mathf.Deg2Rad;
        //    float heading = Heading * Mathf.Deg2Rad;
        //    return new Vector3(Mathf.Cos(elevation) * Mathf.Sin(heading), Mathf.Sin(elevation), Mathf.Cos(elevation) * Mathf.Cos(heading));
        //}
        protected void LogFo()
        {
            Debug.Log("fofo");
        }
    }


}
