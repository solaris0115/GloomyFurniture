using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;
using Verse.AI;
using Harmony;
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

        public AltitudeLayer altitudeLayer = AltitudeLayer.MoteOverhead;

        public float Altitude
        {
            get
            {
                return this.altitudeLayer.AltitudeFor();
            }
        }

        public CompProperties_SecondLayer()
        {
            this.compClass = typeof(CompSecondLayer);
        }
    }
    internal class CompSecondLayer : ThingComp
    {
        private Graphic graphicInt;

        public CompProperties_SecondLayer Props
        {
            get
            {
                return (CompProperties_SecondLayer)this.props;
            }
        }

        public virtual Graphic Graphic
        {
            get
            {
                if (this.graphicInt == null)
                {
                    if (this.Props.graphicData == null)
                    {
                        Log.ErrorOnce(this.parent.def + "GloomylynxFurniture - has no SecondLayer graphicData but we are trying to access it.", 764532, false);
                        return BaseContent.BadGraphic;
                    }
                    this.graphicInt = this.Props.graphicData.GraphicColoredFor(this.parent);
                }
                return this.graphicInt;
            }
        }

        public override void PostDraw()
        {
            if (parent.Rotation == Rot4.South)
            {
                this.Graphic.Draw(GenThing.TrueCenter(this.parent.Position, this.parent.Rotation, this.parent.def.size, Props.Altitude), this.parent.Rotation, this.parent, 0f);
                return;
            }
            
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
            IntVec3 c;
            Building t2;
            if (!WatchBuildingUtility.TryFindBestWatchCell(t, pawn, this.def.desireSit, out c, out t2))
            {
                return null;
            }
            return new Job(this.def.jobDef, t, c, t2);
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
