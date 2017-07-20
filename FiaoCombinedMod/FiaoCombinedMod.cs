using spaar;
using System;
using spaar.ModLoader;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;
using TheGuysYouDespise;
using Blocks;

namespace FiaoCombinedMod
{
    public class FiaoCombinedMod : BlockMod
    {
        public override Version Version { get { return new Version("1.0"); } }
        public override string Name { get { return "FiaoCombinesMod"; } }
        public override string DisplayName { get { return "Fiao's Combined Mods"; } }
        public override string BesiegeVersion { get { return "v0.45a"; } }
        public override string Author { get { return "覅是(but actually a lot are just maintaining and improving)"; } }

        protected Block Drone;
        protected Block TurretBlock;
        protected Block CormacksModifiedTrackingComputer​;

        void OnTrackingLoad()
        {
            if (!spaar.ModLoader.Configuration.DoesKeyExist("MovieMode"))
            {
                Configuration.SetBool("MovieMode", false);
            }

            spaar.ModLoader.SettingsButton MovieMode = new spaar.ModLoader.SettingsButton();
            MovieMode.Text = "Tracking C.\r\nMovie Mode";
            MovieMode.Value = spaar.ModLoader.Configuration.GetBool("MovieMode", false);
            MovieMode.FontSize = 12;
            MovieMode.Create();
            //spaar.ModLoader.SettingsMenu.RegisterSettingsButton("Tracking C.\r\nMovie Mode", GetMovieModeOn, spaar.ModLoader.Configuration.GetBool("MovieMode", false), 12);

            if (!spaar.ModLoader.Configuration.DoesKeyExist("LockingTimer"))
            {
                spaar.ModLoader.Configuration.SetFloat("LockingTimer", 4);
            }

            spaar.Commands.RegisterCommand("SetTrackingComputerMovieMode", (args, notUses) =>
            {
                if (!spaar.ModLoader.Configuration.DoesKeyExist("MovieMode"))
                {
                    spaar.ModLoader.Configuration.SetBool("MovieMode", true);
                }
                else
                {
                    spaar.ModLoader.Configuration.SetBool("MovieMode", spaar.ModLoader.Configuration.GetBool("MovieMode", false));
                }
                spaar.ModLoader.Configuration.Save();
                return "Complete! Now Movie Mode is " + spaar.ModLoader.Configuration.GetBool("MovieMode", false);

            }, "Set Tracking Computer for taking movies, thus ignore overload, target lost, cloak, and allow customized locking timer.");



            spaar.Commands.RegisterCommand("SetTrackingComputerLockingTimer", (args, notUses) =>
            {
                if (!spaar.ModLoader.Configuration.DoesKeyExist("LockingTimer"))
                {
                    spaar.ModLoader.Configuration.SetFloat("LockingTimer", 4);
                }
                float i = 0;
                if (float.TryParse(args[0], out i) && i >= 0)
                {
                    spaar.ModLoader.Configuration.SetFloat("LockingTimer", i);
                    spaar.ModLoader.Configuration.Save();
                    return "Complete! Now Locking need " + spaar.ModLoader.Configuration.GetFloat("LockingTimer", 4) + " seconds to lock.";
                }
                else
                {
                    return "Movie Mode is not ON or your input is incorrect! Your current locking timer is " + spaar.ModLoader.Configuration.GetFloat("LockingTimer", 4);
                }

            }, "Set Tracking Computer locking timer.");
            LoadBlock(TurretBlock);//加载该模块
            LoadBlock(CormacksModifiedTrackingComputer);//加载该模块
        }
        void GetMovieModeOn(bool active)
        {
            spaar.ModLoader.Configuration.SetBool("MovieMode", active);
            spaar.ModLoader.Configuration.Save();
        }

        public override void OnLoad()
        {

            Drone = new Block()
            ///模块ID
            .ID(575)

            ///模块名称
            .BlockName(Configuration.GetBool("UseChinese", false) ? "无人机" : "Drone I")

            ///模型信息
            .Obj(new List<Obj> { new Obj("zDrone.obj", //Obj
                                         "zDrone.png", //贴图 
                                         new VisualOffset(Vector3.one, //Scale
                                                          Vector3.forward * 3f, //Position
                                                          new Vector3(-90f, 0f, 0f)))//Rotation
            })

            ///在UI下方的选模块时的模样
            .IconOffset(new Icon(new Vector3(1f, 1f, 1f),  //Scale
                                 new Vector3(-0.11f, -0.13f, 0.00f),  //Position
                                 new Vector3(350f, 150f, 250f))) //Rotation

            ///没啥好说的。
            .Components(new Type[] {
                                    typeof(FullyAIDrone),
            })

            ///给搜索用的关键词
            .Properties(new BlockProperties().SearchKeywords(new string[] {
                                                             "Drone",
                                                             "无人机",
                                                             "靶机",
                                                             "Target",
                                                             "War",
                                                             "Weapon"
                                             })
            )
            ///质量
            .Mass(2f)

            ///是否显示碰撞器（在公开你的模块的时候记得写false）a
            .ShowCollider(false)

            ///碰撞器
            .CompoundCollider(new List<ColliderComposite> {
                ColliderComposite.Mesh("zDroneColl.obj",Vector3.one,Vector3.forward * 3f,Vector3.zero)
            })

            ///你的模块是不是可以忽视强搭
            //.IgnoreIntersectionForBase()

            ///载入资源
            .NeededResources(new List<NeededResource>
            {
                //new NeededResource(ResourceType.Mesh,"zDroneColl.obj")
                                new NeededResource(ResourceType.Texture,"zDroneBump.png")


            }
            )

            ///连接点
            .AddingPoints(new List<AddingPoint> {
                               new BasePoint(true, false)         //底部连接点。第一个是指你能不能将其他模块安在该模块底部。第二个是指这个点是否是在开局时粘连其他链接点
                                                .Motionable(true,true,true) //底点在X，Y，Z轴上是否是能够活动的。
                                                .SetStickyRadius(0f),
            new AddingPoint(new Vector3(0f, 0.2f, 1.5f), new Vector3(-180f, 00f, 360f),true).SetStickyRadius(0.3f)
            });

            TurretBlock = new Block()
            ///模块ID
            .ID(525)

            ///模块名称
            .BlockName(Configuration.GetBool("UseChinese", false) ? "索敌计算机I" : "Tracking Computer I")

            ///模型信息
            .Obj(new List<Obj> { new Obj("turret.obj", //Obj
                                         "turret.png", //贴图 
                                         new VisualOffset(new Vector3(1f, 1f, 1f), //Scale
                                                          new Vector3(0f, 0f, 0f), //Position
                                                          new Vector3(-90f, 0f, 0f)))//Rotation
            })

            ///在UI下方的选模块时的模样
            .IconOffset(new Icon(new Vector3(1f, 1f, 1f),  //Scale
                                 new Vector3(-0.11f, -0.13f, 0.00f),  //Position
                                 new Vector3(350f, 150f, 250f))) //Rotation

            ///没啥好说的。
            .Components(new Type[] {
                                    typeof(TrackingComputer),
            })

            ///给搜索用的关键词
            .Properties(new BlockProperties().SearchKeywords(new string[] {
                                                             "Turret",
                                                             "炮台",
                                                             "导弹",
                                                             "War",
                                                             "Weapon"
                                             }).Burnable(3)
            )
            ///质量
            .Mass(2f)

            ///是否显示碰撞器（在公开你的模块的时候记得写false）a
            .ShowCollider(false)

            ///碰撞器
            .CompoundCollider(new List<ColliderComposite> {
                ColliderComposite.Box(new Vector3(1f, 1f, 1.2f), new Vector3(0f, 0f, 0.6f), new Vector3(0f, 0f, 0f)),
                //ColliderComposite.Capsule(0.35f,1.0f,Direction.Z,new Vector3(0,0,0.8f),Vector3.zero),
                /*
                                ColliderComposite.Sphere(0.49f,                                //radius
                                                         new Vector3(-0.10f, -0.05f, 0.27f),   //Position
                                                         new Vector3(0f, 0f, 0f))              //Rotation
                                                         .IgnoreForGhost(),                    //Do not use this collider on the ghost

                                ColliderComposite.Capsule(0.33f,                               //radius
                                                          1.33f,                               //length
                                                          Direction.Y,                         //direction
                                                          new Vector3(-0.52f, 0.38f, 0.30f),   //position
                                                          new Vector3(5f, 0f, -5f)),           //rotation                                
                                
                                ColliderComposite.Box(new Vector3(0.65f, 0.65f, 0.25f),        //scale
                                                      new Vector3(0f, 0f, 0.25f),              //position
                                                      new Vector3(0f, 0f, 0f)),                //rotation
                                
                                ColliderComposite.Sphere(0.5f,                                  //radius
                                                         new Vector3(-0.10f, -0.05f, 0.35f),    //Position
                                                         new Vector3(0f, 0f, 0f))               //Rotation
                                                         .Trigger().Layer(2)
                                                         .IgnoreForGhost(),                     //Do not use this collider on the ghost
                              //ColliderComposite.Box(new Vector3(0.35f, 0.35f, 0.15f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f)).Trigger().Layer(2).IgnoreForGhost(),   <---Example: Box Trigger on specific Layer*/
            })

            ///你的模块是不是可以忽视强搭
            //.IgnoreIntersectionForBase()

            ///载入资源
            .NeededResources(new List<NeededResource> {
                                new NeededResource(ResourceType.Audio,"炮台旋转音效.ogg"),
                                new NeededResource(ResourceType.Mesh,"MissileModule.obj"),
                                new NeededResource(ResourceType.Mesh,"turret.obj"),
                                new NeededResource(ResourceType.Texture,"Target 0.png"),
                                new NeededResource(ResourceType.Texture,"Target 1.png"),
                                new NeededResource(ResourceType.Texture,"Target 2.png"),
                                new NeededResource(ResourceType.Texture,"Target 3.png"),
                                new NeededResource(ResourceType.Texture,"Targeted.png")
            })

            ///连接点
            .AddingPoints(new List<AddingPoint> {
                               (AddingPoint)new BasePoint(true, true)         //底部连接点。第一个是指你能不能将其他模块安在该模块底部。第二个是指这个点是否是在开局时粘连其他链接点
                                                .Motionable(true,true,true) //底点在X，Y，Z轴上是否是能够活动的。
                                                .SetStickyRadius(0.5f),        //粘连距离
                              new AddingPoint(new Vector3(0f, 0f, 0.65f), new Vector3(-180f, 00f, 360f),true).SetStickyRadius(0.3f),
                              new AddingPoint(new Vector3(0f, 0f, 0.65f), new Vector3(-90f, 00f, 90f),true).SetStickyRadius(0.3f),
                              new AddingPoint(new Vector3(0f, 0f, 0.65f), new Vector3(180f, 00f, 180f),true).SetStickyRadius(0.3f),
                              new AddingPoint(new Vector3(0f, 0f, 0.65f), new Vector3(90f, 00f, 270f),true).SetStickyRadius(0.3f),


                              new AddingPoint(new Vector3(0f, 0f, 0.7f), new Vector3(0f, -90f, 90f),true).SetStickyRadius(0.3f),
            });

            CormacksModifiedTrackingComputer​ = new Block()
            ///模块ID
            .ID(526)

            ///模块名称
            .BlockName(Configuration.GetBool("UseChinese", false) ? "科尔马克氏改良型索敌计算机" : "Cormack\'s Modified Tracking Computer​")
            ///模型信息
            .Obj(new List<Obj> { new Obj("turret.obj", //Obj
                                         "Cormack\'s Modified Tracking Computer​.png", //贴图
                                         new VisualOffset(new Vector3(1f, 1f, 1f), //Scale
                                                          new Vector3(-0.6f, 0f, 0.5f), //Position
                                                          new Vector3(90f, -90f, 0f)))//Rotation
            })

            ///在UI下方的选模块时的模样
            .IconOffset(new Icon(Vector3.one,  //Scale
                                 new Vector3(-0.11f, -0.13f, 0.00f),  //Position
                                 new Vector3(350f, 150f, 250f))) //Rotation

            ///没啥好说的。
            .Components(new Type[] {
                                    typeof(ModifiedTurret),
            })

            ///给搜索用的关键词
            .Properties(new BlockProperties().SearchKeywords(new string[] {
                                                             "Turret",
                                                             "炮台",
                                                             "War",
                                                             "Weapon"
                                             }).Burnable(7)
            )
            ///质量
            .Mass(2f)

            ///是否显示碰撞器（在公开你的模块的时候记得写false）
            .ShowCollider(false)

            ///碰撞器
            .CompoundCollider(new List<ColliderComposite> {
                ColliderComposite.Box(new Vector3(1f, 1.3f, 1f), new Vector3(0f, 0f, 0.5f), new Vector3(0f, 0f, 0f)),
                //ColliderComposite.Capsule(0.35f,1.0f,Direction.Z,new Vector3(0,0,0.8f),Vector3.zero),
                /*
                                ColliderComposite.Sphere(0.49f,                                //radius
                                                         new Vector3(-0.10f, -0.05f, 0.27f),   //Position
                                                         new Vector3(0f, 0f, 0f))              //Rotation
                                                         .IgnoreForGhost(),                    //Do not use this collider on the ghost

                                ColliderComposite.Capsule(0.33f,                               //radius
                                                          1.33f,                               //length
                                                          Direction.Y,                         //direction
                                                          new Vector3(-0.52f, 0.38f, 0.30f),   //position
                                                          new Vector3(5f, 0f, -5f)),           //rotation                                
                                
                                ColliderComposite.Box(new Vector3(0.65f, 0.65f, 0.25f),        //scale
                                                      new Vector3(0f, 0f, 0.25f),              //position
                                                      new Vector3(0f, 0f, 0f)),                //rotation
                                
                                ColliderComposite.Sphere(0.5f,                                  //radius
                                                         new Vector3(-0.10f, -0.05f, 0.35f),    //Position
                                                         new Vector3(0f, 0f, 0f))               //Rotation
                                                         .Trigger().Layer(2)
                                                         .IgnoreForGhost(),                     //Do not use this collider on the ghost
                              //ColliderComposite.Box(new Vector3(0.35f, 0.35f, 0.15f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f)).Trigger().Layer(2).IgnoreForGhost(),   <---Example: Box Trigger on specific Layer*/
            })

            ///你的模块是不是可以忽视强搭
            .IgnoreIntersectionForBase()

            ///载入资源
            .NeededResources(new List<NeededResource> {
                                new NeededResource(ResourceType.Audio,"炮台旋转音效.ogg"),
                                new NeededResource(ResourceType.Texture,"Target 0.png"),
                                new NeededResource(ResourceType.Texture,"Target 1.png"),
                                new NeededResource(ResourceType.Texture,"Target 2.png"),
                                new NeededResource(ResourceType.Texture,"Target 3.png"),
                                new NeededResource(ResourceType.Texture,"Targeted.png")
            })

            ///连接点
            .AddingPoints(new List<AddingPoint> {
                               (AddingPoint)new BasePoint(true, true)         //底部连接点。第一个是指你能不能将其他模块安在该模块底部。第二个是指这个点是否是在开局时粘连其他链接点
                                                .Motionable(true,true,true) //底点在X，Y，Z轴上是否是能够活动的。
                                                .SetStickyRadius(0.5f),        //粘连距离
                              new AddingPoint(new Vector3(0f, 0.2f, 0.5f), new Vector3(-180f, 00f, 360f),true).SetStickyRadius(0.3f),
                              new AddingPoint(new Vector3(0.05f, 0f, 0.5f), new Vector3(-90f, 00f, 90f),true).SetStickyRadius(0.3f),
                              new AddingPoint(new Vector3(0f, -0.2f, 0.5f), new Vector3(180f, 00f, 180f),true).SetStickyRadius(0.3f),
                              new AddingPoint(new Vector3(0f, 0f, 0.5f), new Vector3(90f, 00f, 270f),true).SetStickyRadius(0.3f),


                              new AddingPoint(new Vector3(0f, 0f, 0.6f), new Vector3(0f, -90f, 90f),true).SetStickyRadius(0.3f),
            });

            Block panelBlock = new Block()
            .ID(505)
            .BlockName(Configuration.GetBool("UseChinese", false) ? "仪表盘模块" : "Panel Block")
            .Obj(new List<Obj> { new Obj("Pilot Panel Block.obj", //Obj
                                         "Pilot Panel Block.png", //贴图
                                         new VisualOffset(new Vector3(1f, 1f, 1f), //Scale
                                                          new Vector3(0f, 0f, 0f), //Position
                                                          new Vector3(0f, 0f, 0f)))//Rotation
            })
            ///在UI下方的选模块时的模样
            .IconOffset(new Icon(1f, new Vector3(0f, 0f, 0f), new Vector3(-90f, 45f, 0f))) //Rotation
            .Components(new Type[] { typeof(PilotPanelScript), })

            ///给搜索用的关键词
            .Properties(new BlockProperties().SearchKeywords(new string[] {
                                                             "panel",
                                                             "pilot",
                                                             "navigate",
                                                             "data",
                                             }))
            .Mass(0.1f)
            .ShowCollider(false)
            .CompoundCollider(new List<ColliderComposite> { new ColliderComposite(new Vector3(0.1f, 0.1f, 0.2f), new Vector3(0f, 0f, 0.1f), new Vector3(0f, 0f, 0f)) })
            .NeededResources(new List<NeededResource> {
                new NeededResource(ResourceType.Texture, "HUD/Center.png"),
                new NeededResource(ResourceType.Texture, "HUD/Gradienter.png"),
                new NeededResource(ResourceType.Texture, "HUD/Zero Zero Front.png"),
                new NeededResource(ResourceType.Texture, "HUD/Zero Zero Back.png"),
                new NeededResource(ResourceType.Texture, "HUD/Ice Floor.png"),
                new NeededResource(ResourceType.Texture, "HUD/Floor Line.png"),
                new NeededResource(ResourceType.Texture, "HUD/Height Line.png"),
                new NeededResource(ResourceType.Texture, "HUD/OverICE Line.png"),
                new NeededResource(ResourceType.Texture, "HUD/Direction Indicator North.png"),
                new NeededResource(ResourceType.Texture, "HUD/Direction Indicator South.png"),
                new NeededResource(ResourceType.Texture, "HUD/Direction Indicator East.png"),
                new NeededResource(ResourceType.Texture, "HUD/Direction Indicator West.png"),
                new NeededResource(ResourceType.Texture, "HUD/Direction Indicator Small.png"),
                new NeededResource(ResourceType.Texture, "HUD/Minimap Addition.png")
            })
            .AddingPoints(new List<AddingPoint> {
                               (AddingPoint)new BasePoint(true, true)
                                                .Motionable(false,false,false)
                                                .SetStickyRadius(0.5f),
            });

            Block laser = new Block()
            .ID(577)
            .BlockName(!Configuration.GetBool("UseChinese", false) ? "Improved Laser Emitter" : "改进型激光发生器")
            //.BlockName("Improved Laser Emitter" )
            .Obj(new List<Obj> { new Obj("LaserBlock2.obj", "LaserBlock2.png",
                new VisualOffset(new Vector3(1f, 1f, 1f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f)))
            })
            .IconOffset(new Icon(Vector3.one * 3f, new Vector3(0.2f, -0.3f, -2.15f), new Vector3(30f, 230f, 0f)))
            .Components(new Type[] { typeof(NewLaserBlock) })
            .Properties(new BlockProperties().SearchKeywords(new string[] {
                "Laser", "Fire", "Kinetic", "Freeze","Explosive","Weapon", "Beam", "RIPTesseractCat"})
            )
            .Mass(0.3f)
            .ShowCollider(false)
            .CompoundCollider(new List<ColliderComposite> {
                ColliderComposite.Box(new Vector3(0.5f, 0.5f, 1.1f), new Vector3(0f, 0f, 0.55f), new Vector3(0f, 0f, 0f))})
            .IgnoreIntersectionForBase()
            .NeededResources(new List<NeededResource>()
            {
                new NeededResource(ResourceType.Texture,"LaserParticle.png")
            })
            .AddingPoints(new List<AddingPoint> {
                new BasePoint(false, true).Motionable(false,false,false).SetStickyRadius(0.5f)});

            Block ControlBlock = new Block()
            .ID(576)
            .BlockName(Configuration.GetBool("UseChinese", false) ? "无人机指挥模块" : "Drone Controller Block")
            .Obj(new List<Obj> { new Obj("DroneController.obj", //Obj
                                         "DroneController.png", //贴图
                                         new VisualOffset(new Vector3(1f, 1f, 1f), //Scale
                                                          new Vector3(0f, 0f, 0f), //Position
                                                          new Vector3(0f, 0f, 0f)))//Rotation
            })
            ///在UI下方的选模块时的模样
            .IconOffset(new Icon(new Vector3(1.30f, 1.30f, 1.30f),  //Scale
                                 new Vector3(-0.11f, -0.13f, 0.00f),  //Position
                                 new Vector3(45f, 45f, 45f))) //Rotation
            .Components(new Type[] { typeof(DroneControlBlockBehavior), })

            ///给搜索用的关键词
            .Properties(new BlockProperties().SearchKeywords(new string[] {
                                                             "Drone",
                                                             "控制",
                                                             "Spawner",
                                             }))
            .Mass(0.5f)
            .ShowCollider(false)
            .CompoundCollider(new List<ColliderComposite> { new ColliderComposite(new Vector3(1, 1, 1), new Vector3(0f, 0f, 0.5f), new Vector3(0f, 0f, 0f)) })
            .NeededResources(null
            )
            .AddingPoints(new List<AddingPoint> {
                               (AddingPoint)new BasePoint(true, true)
                                                .Motionable(false,false,false)
                                                .SetStickyRadius(0.5f),
            });

            LoadBlock(laser);
            LoadBlock(panelBlock);//加载该模块
            OnTrackingLoad();
            LoadBlock(Drone);
            LoadBlock(ControlBlock);
        }
    }
}
