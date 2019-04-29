using Modding;
using Modding.Blocks;
using Modding.Common;
using UnityEngine;
using System.Collections.Generic;

namespace FiaoCombinedMod
{
    public class p
    {
        public static void l(string msg)
        {
            BesiegeConsoleController.ShowMessage(msg);
        }
        public static GameObject DebugBall(Vector3 pos, Vector3 scale, bool fade)
        {
            GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.transform.localScale = scale;
            GameObject.DestroyImmediate(ball.GetComponent<Rigidbody>());
            GameObject.DestroyImmediate(ball.GetComponent<Collider>());
            ball.transform.position = pos;
            if (fade)
                ball.AddComponent<autoFade>();
            return ball;
        }

        private class autoFade : MonoBehaviour
        {
            private Renderer rd;
            private void Start()
            {
                rd = this.GetComponent<Renderer>();
            }
            private void FixedUpdate()
            {
                rd.material.color = Color.Lerp(rd.material.color, Color.red, 0.01f);
                this.transform.localScale = Vector3.Lerp(this.transform.localScale, Vector3.zero, 0.01f);
                if (this.transform.localScale.z < 0.03f)
                {
                    DestroyImmediate(this.gameObject);
                }
            }
        }
    }
    public static class Messages
    {
        public static MessageType LaserSimuStart;
        public static MessageType LaserProgress;

        public static MessageType TrackingComputerLock;
        public static MessageType TrackingComputerSync;
        public static MessageType ModTrackingComputerShot;
        public static MessageType MissileGuidanceModeSwitch;
        public static MessageType OverLoaded;

        public static MessageType PilotPanelSync;


        public static Dictionary<Player, PilotPanelScript> PPdic = new Dictionary<Player, PilotPanelScript>();
    }

    public class FiaoCombinedMod : ModEntryPoint
    {
        // This is called when the mod is first loaded.
        public override void OnLoad()
        {
            LaserMessages();
            TrackingComputerMessages();

            Messages.PilotPanelSync = ModNetworking.CreateMessageType(DataType.Block, DataType.Vector3, DataType.Vector3, DataType.Vector3);
            ModNetworking.Callbacks[Messages.PilotPanelSync] += message31 =>
            {
                Block block = (Block)message31.GetData(0);
                // The script on cloak block in client
                PilotPanelScript clk = block.SimBlock.GameObject.GetComponent<PilotPanelScript>();
                // Use the initialization
                clk.SetParams((Vector3)message31.GetData(1), (Vector3)message31.GetData(2), (Vector3)message31.GetData(3) );
            };
        }

        private static void TrackingComputerMessages()
        {
            Messages.TrackingComputerLock = ModNetworking.CreateMessageType(DataType.Block, DataType.Vector3, DataType.Vector3);
            ModNetworking.Callbacks[Messages.TrackingComputerLock] += message6 =>
            {
                Block block = (Block)message6.GetData(0);
                // The script on cloak block in client
                BasicTrackingComputerBehavior clk = block.SimBlock.GameObject.GetComponent<BasicTrackingComputerBehavior>();
                // Use the initialization
                clk.AcquireTarget(new Ray((Vector3)message6.GetData(1), (Vector3)message6.GetData(2)));
            };

            Messages.TrackingComputerSync = ModNetworking.CreateMessageType(DataType.Block, DataType.Vector3, DataType.Boolean);
            ModNetworking.Callbacks[Messages.TrackingComputerSync] += message7 =>
            {
                Block block = (Block)message7.GetData(0);
                // The script on cloak block in client
                BasicTrackingComputerBehavior clk = block.SimBlock.GameObject.GetComponent<BasicTrackingComputerBehavior>();
                // Use the initialization
                clk.setSign((bool)message7.GetData(2), (Vector3)message7.GetData(1));
            };

            Messages.ModTrackingComputerShot = ModNetworking.CreateMessageType(DataType.Block);
            ModNetworking.Callbacks[Messages.ModTrackingComputerShot] += message11 =>
            {
                Block block = (Block)message11.GetData(0);
                // The script on cloak block in client
                ModifiedTurret clk = block.SimBlock.GameObject.GetComponent<ModifiedTurret>();
                clk.shot();
            };

            Messages.MissileGuidanceModeSwitch = ModNetworking.CreateMessageType(DataType.Block, DataType.Integer);
            ModNetworking.Callbacks[Messages.MissileGuidanceModeSwitch] += message =>
            {
                Block block = (Block)message.GetData(0);
                // The script on cloak block in client
                TrackingComputer clk = block.SimBlock.GameObject.GetComponent<TrackingComputer>();
                // Use the initialization
                clk.MissileGuidanceModeInt = (int)message.GetData(1);
                clk.MissileVisReplacement((int)message.GetData(1));
            };

            Messages.OverLoaded = ModNetworking.CreateMessageType(DataType.Block);
            ModNetworking.Callbacks[Messages.OverLoaded] += message8 =>
            {
                Block block = (Block)message8.GetData(0);
                // The script on cloak block in client
                BasicTrackingComputerBehavior clk = block.SimBlock.GameObject.GetComponent<BasicTrackingComputerBehavior>();
            };
        }

        private static void LaserMessages()
        {
            // Initialize messages
            Messages.LaserProgress = ModNetworking.CreateMessageType(DataType.Block, DataType.Boolean, DataType.Boolean, DataType.Single, DataType.Boolean, DataType.Vector3);
            // Script after message has been received
            ModNetworking.Callbacks[Messages.LaserProgress] += message2 =>
            {
                // Save and convert data that just received from message
                Block block = (Block)message2.GetData(0);
                bool ActivationY = (bool)message2.GetData(1);
                bool ActivationK = (bool)message2.GetData(2);
                float length = (float)message2.GetData(3);
                bool hit = (bool)message2.GetData(4);
                Vector3 velo = (Vector3)message2.GetData(5);
                NewLaserBlock clk = block.SimBlock.GameObject.GetComponent<NewLaserBlock>();
                // Use the initialization
                clk.SetMPState(ActivationY, ActivationK, length, hit, velo);
            };
            Messages.LaserSimuStart = ModNetworking.CreateMessageType(DataType.Block);
            ModNetworking.Callbacks[Messages.LaserSimuStart] += message4 =>
            {
                Block block = (Block)message4.GetData(0);
                // The script on cloak block in client
                NewLaserBlock clk = block.SimBlock.GameObject.GetComponent<NewLaserBlock>();
                // Use the initialization
                clk.OnSimulateStart();
            };
        }
    }

    public class KeepHiding : MonoBehaviour
    {
        private Block parenttt;
        public void setParentt(Block parrent)
        {
            this.parenttt = parrent;
        }
        private void Update()
        {
            foreach (Collider ccu in GetComponentsInChildren<Collider>())
            {
                ccu.isTrigger = true;
            }
            this.GetComponent<Rigidbody>().isKinematic = true;
            this.transform.localScale = Vector3.one * 0.001f;

            if (parenttt?.SimBlock != null)
            {
                this.transform.position = parenttt.SimBlock.GameObject.transform.position;
            }
            else if (parenttt?.BuildingBlock != null)
            {
                this.transform.position = parenttt.BuildingBlock.GameObject.transform.position;
            }
            else
            {
                DestroyImmediate(this.gameObject);
            }
        }
    }
}
