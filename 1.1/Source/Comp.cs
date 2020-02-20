using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;
using Verse.AI;
using HarmonyLib;
using System.Reflection;

namespace Gloomylynx
{
    [StaticConstructorOnStartup]
    public class CompFireOverlayRotatable : CompFireOverlay
    {
        List<Rot4> showList = new List<Rot4>();
        public CompFlickable compFlickable;
        public CompPowerTrader compPower;
        public override void PostDraw()
        {
            foreach (Rot4 rot in showList)
            {
                if(rot == parent.Rotation)
                {
                    if (compPower != null)
                    {
                        if (compPower.PowerOn)
                        {
                            DrawCall();
                        }
                    }
                    else
                    {
                        if(refuelableComp!=null)
                        {
                            if (refuelableComp.HasFuel && compFlickable.SwitchIsOn)
                            {
                                DrawCall();
                            }
                        }
                        else
                        {
                            if(compFlickable.SwitchIsOn)
                            {
                                DrawCall();
                            }
                        }                        
                    }
                    return;
                }
            }
        }

        public void DrawCall()
        {
            Vector3 drawPos = this.parent.DrawPos;
            drawPos.y += 0.046875f;
            FireGraphic.Draw(drawPos, Rot4.North, this.parent, 0f);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            showList = ((CompProperties_FireOverlayRotatable)props).showRotateList;
            compFlickable = parent.GetComp<CompFlickable>();
            compPower = parent.GetComp<CompPowerTrader>();
        }
    }
    public class CompProperties_FireOverlayRotatable: CompProperties_FireOverlay
    {
        public List<Rot4> showRotateList = new List<Rot4>();
        public override void ResolveReferences(ThingDef parentDef)
        {
            base.ResolveReferences(parentDef);
            if(showRotateList.Count>0)
            {
                return;
            }
            showRotateList.Add(Rot4.North);
            showRotateList.Add(Rot4.South);
            showRotateList.Add(Rot4.East);
            showRotateList.Add(Rot4.West);
        }
    }

    internal class CompProperties_SecondLayer : CompProperties
    {
        public GraphicData graphicData=null;
        public bool drawOnlySouth = false;

        public AltitudeLayer altitudeLayer = AltitudeLayer.MoteOverhead;

        public float Altitude
        {
            get
            {
                return altitudeLayer.AltitudeFor();
            }
        }

        public CompProperties_SecondLayer()
        {
            compClass = typeof(CompSecondLayer);
        }
    }
    internal class CompSecondLayer : ThingComp
    {
        bool drawOnlySouth = false;
        private Graphic graphicInt;
        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            drawOnlySouth = Props.drawOnlySouth;
        }

        public CompProperties_SecondLayer Props
        {
            get
            {
                return (CompProperties_SecondLayer)props;
            }
        }

        public virtual Graphic Graphic
        {
            get
            {
                if (graphicInt == null)
                {
                    if (Props.graphicData == null)
                    {
                        Log.ErrorOnce(parent.def + "GloomylynxFurniture - has no SecondLayer graphicData but we are trying to access it.", 764532, false);
                        return BaseContent.BadGraphic;
                    }
                    graphicInt = Props.graphicData.Graphic;
                }
                return graphicInt;
            }
        }

        public override void PostDraw()
        {
            if(drawOnlySouth && parent.Rotation == Rot4.South)
            {
                Graphic.Draw(GenThing.TrueCenter(parent.Position, parent.Rotation, parent.def.size, Props.Altitude), parent.Rotation, parent, 0f);
            }
            else
            {
                Graphic.Draw(GenThing.TrueCenter(parent.Position, parent.Rotation, parent.def.size, Props.Altitude), parent.Rotation, parent, 0f);
            }            
        }
    }

    internal class CompProperties_SecondLayerFollow : CompProperties
    {
        public GraphicData graphicData = null;
        public bool drawOnlySouth = false;

        public AltitudeLayer altitudeLayer = AltitudeLayer.MoteOverhead;

        public float Altitude
        {
            get
            {
                return altitudeLayer.AltitudeFor();
            }
        }

        public CompProperties_SecondLayerFollow()
        {
            compClass = typeof(CompSecondLayerFollow);
        }
    }
    internal class CompSecondLayerFollow : ThingComp
    {
        bool drawOnlySouth = false;
        private Graphic graphicInt;
        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            drawOnlySouth = Props.drawOnlySouth;
        }

        public CompProperties_SecondLayerFollow Props
        {
            get
            {
                return (CompProperties_SecondLayerFollow)props;
            }
        }

        public virtual Graphic Graphic
        {
            get
            {
                if (graphicInt == null)
                {
                    if (Props.graphicData == null)
                    {
                        Log.ErrorOnce(parent.def + "GloomylynxFurniture - has no SecondLayer graphicData but we are trying to access it.", 764532, false);
                        return BaseContent.BadGraphic;
                    }
                    graphicInt = Props.graphicData.GraphicColoredFor(parent);
                }
                return graphicInt;
            }
        }

        public override void PostDraw()
        {
            if (drawOnlySouth && parent.Rotation == Rot4.South)
            {
                Graphic.Draw(GenThing.TrueCenter(parent.Position, parent.Rotation, parent.def.size, Props.Altitude), parent.Rotation, parent, 0f);
            }
            else
            {
                Graphic.Draw(GenThing.TrueCenter(parent.Position, parent.Rotation, parent.def.size, Props.Altitude), parent.Rotation, parent, 0f);
            }
        }
    }



    internal class CompProperties_SecondLayerOnOffable : CompProperties
    {
        public GraphicData graphicDataOn = null;
        public GraphicData graphicDataOff = null;

        public AltitudeLayer altitudeLayer = AltitudeLayer.MoteOverhead;

        public float Altitude
        {
            get
            {
                return altitudeLayer.AltitudeFor();
            }
        }

        public CompProperties_SecondLayerOnOffable()
        {
            compClass = typeof(CompSecondLayerOnOffable);
        }
    }
    internal class CompSecondLayerOnOffable : ThingComp
    {
        public CompRefuelable refuelableComp;
        public CompFlickable compFlickable;
        public Graphic graphicIntOn;
        public Graphic graphicIntOff;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            refuelableComp = parent.GetComp<CompRefuelable>();
            compFlickable = parent.GetComp<CompFlickable>();
        }
        public CompProperties_SecondLayerOnOffable Props
        {
            get
            {
                return (CompProperties_SecondLayerOnOffable)props;
            }
        }

        public virtual Graphic GraphicOn
        {
            get
            {
                if (graphicIntOn == null)
                {
                    if (Props.graphicDataOn == null)
                    {
                        Log.ErrorOnce(parent.def + "GloomylynxFurniture - has no SecondLayerOnOffable graphicData but we are trying to access it.", 764532, false);
                        return BaseContent.BadGraphic;
                    }
                    graphicIntOn = Props.graphicDataOn.Graphic;
                }
                return graphicIntOn;
            }
        }
        public virtual Graphic GraphicOff
        {
            get
            {
                if (graphicIntOff == null)
                {
                    if (Props.graphicDataOff == null)
                    {
                        Log.ErrorOnce(parent.def + "GloomylynxFurniture - has no SecondLayerOnOffable graphicData but we are trying to access it.", 764532, false);
                        return BaseContent.BadGraphic;
                    }
                    graphicIntOff = Props.graphicDataOff.Graphic;
                }
                return graphicIntOff;
            }
        }

        public override void PostDraw()
        {
            //스위치 온 그리고 연료 유
            if(compFlickable.SwitchIsOn && refuelableComp.HasFuel )
            {
                DrawCallOn();
            }
            else
            {
                //Log.Message("off");
                DrawCallOff();
            }
        }
        public void DrawCallOn()
        {
            GraphicOn.Draw(GenThing.TrueCenter(parent.Position, parent.Rotation, parent.def.size, Props.Altitude), parent.Rotation, parent, 0f);
        }
        public void DrawCallOff()
        {
            GraphicOff.Draw(GenThing.TrueCenter(parent.Position, parent.Rotation, parent.def.size, Props.Altitude), parent.Rotation, parent, 0f);
        }
    }

    public class CompProperties_JukeBox:CompProperties
    {
        public SongDef stopSong;
        public override void ResolveReferences(ThingDef parentDef)
        {
            foreach(SongDef s in from song in DefDatabase<SongDef>.AllDefs where song.defName =="Stop" select song)
            {
                stopSong = s;
                break;
            }
        }
    }

    public class JoyGiver_ListenSong : JoyGiver_InteractBuilding
    {
        protected override bool CanInteractWith(Pawn pawn, Thing t, bool inBed)
        {
            if (!base.CanInteractWith(pawn, t, inBed))
            {
                return false;
            }
            if (inBed)
            {
                Building_Bed bed = pawn.CurrentBed();
                return WatchBuildingUtility.CanWatchFromBed(pawn, bed, t);
            }
            return true;
        }

        protected override Job TryGivePlayJob(Pawn pawn, Thing t)
        {
            if (!WatchBuildingUtility.TryFindBestWatchCell(t, pawn, def.desireSit, out IntVec3 c, out Building t2))
            {
                return null;
            }
            return new Job(def.jobDef, t, c, t2);
        }
    }
    public class JobDriver_ListenSong : JobDriver_WatchBuilding
    {
        protected override void WatchTickAction()
        {
            Building thing = (Building)base.TargetA.Thing;
            if (!thing.TryGetComp<CompPowerTrader>().PowerOn)
            {
                base.EndJobWith(JobCondition.Incompletable);
                return;
            }
            base.WatchTickAction();
        }
    }
    public class CompProperties_FlickableVent : CompProperties_Flickable
    {
        public CompProperties_FlickableVent()
        {
            this.compClass = typeof(CompFlickableVent);
        }
    }
    public class CompFlickableVent : CompFlickable
    {
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield break;
        }
    }
}
