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
        public CompOnOffable compOnOffable;
        public override void PostDraw()
        {
            foreach(Rot4 rot in showList)
            {
                if(rot == parent.Rotation)
                {
                    if ((refuelableComp != null) && (refuelableComp.HasFuel) && (compOnOffable.SwitchIsOn))
                    {
                        Vector3 drawPos = this.parent.DrawPos;
                        drawPos.y += 0.046875f;
                        FireGraphic.Draw(drawPos, Rot4.North, this.parent, 0f);
                    }
                    return;
                }
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            showList = ((CompProperties_FireOverlayRotatable)props).showRotateList;
            compOnOffable = parent.GetComp<CompOnOffable>();
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
    public class CompOnOffable : CompFlickable
    {
        private static FieldInfo FI_CompFlick = AccessTools.Field(typeof(CompFlickable), "wantSwitchOn");
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            //AccessTools.Field(typeof(CompFlickable), "wantSwitchOn").GetValue(this)
            if (this.parent.Faction == Faction.OfPlayer)
            {
                //AccessTools.Field(typeof(CompFlickable), "wantSwitchOn").GetValue(this)
                yield return new Command_Toggle
                {
                    hotKey = KeyBindingDefOf.Command_TogglePower,
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/OnOff", true),
                    defaultLabel = "On/Off",
                    defaultDesc = "",
                    isActive = FI_CompFlick.GetValue(this).ChangeType<bool>,
                    toggleAction = delegate ()
                    {
                        FI_CompFlick.SetValue(this, !(bool)FI_CompFlick.GetValue(this));
                        FlickUtility.UpdateFlickDesignation(this.parent);
                    }
                };
            }
            yield break;
        }
    }

    public class CompJukeBox:ThingComp
    {
        public CompPowerTrader compPower;
        public bool currentPlayState = false;
        public static SongDef currentSong;
        public List<SongDef> songList = new List<SongDef>();
        public static int index = 0;
        
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compPower = parent.GetComp<CompPowerTrader>();
            songList = ((CompProperties_JukeBox)props).songList;
            if (currentSong == null)
            {
                currentSong = songList[0];
            }
        }
        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            StopMusic();
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look<SongDef>(ref currentSong, "currentSong");
            Scribe_Values.Look<bool>(ref currentPlayState, "currentPlayState");
            Scribe_Collections.Look<SongDef>(ref songList, "songList",LookMode.Def, new object[0]);
        }
        public override void CompTick()
        {
            base.CompTick();
            if (!compPower.PowerOn)
            {
                if (currentPlayState)
                {
                    currentPlayState = false;
                    StopMusic();
                }
            }
        }
        public override string CompInspectStringExtra()
        {
            if(compPower != null && currentSong!=null)
            {
                if (compPower.connectParent != null)
                {
                    string str;
                    str = "State" + ": ";
                    if (currentPlayState)
                    {
                        str += "Play \n";
                    }
                    else
                    {
                        str += "Pause \n";
                    }
                    str = str + "CurrentSong" + ": " + currentSong.defName;
                    return str;
                }
            }            
            return string.Empty;

            
        }
        public override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);
            if (signal == "FlickedOff" || signal == "ScheduledOff" || signal == "Breakdown")
            {
                currentPlayState = false;
                StopMusic();
            }
            if(signal== "FlickedOn")
            {
                if(currentPlayState)
                {
                    currentPlayState = true;
                    PlayMusic();
                }
            }
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo c in base.CompGetGizmosExtra())
            {
                yield return c;
            }
            if(compPower.connectParent!=null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "previousSong",
                    defaultDesc = "previousSongDesc",
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/Previous", true),
                    action = delegate ()
                    {
                        index--;
                        if (index < 0)
                        {
                            index = songList.Count - 1;
                        }
                        currentSong = songList[index];
                        if (currentPlayState)
                        {
                            Find.MusicManagerPlay.ForceStartSong(currentSong, false);
                        }
                    }
                };
                if (!currentPlayState)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "play",
                        defaultDesc = "playCurrentMusic",
                        icon = ContentFinder<Texture2D>.Get("UI/Commands/Play", true),
                        action = delegate ()
                        {
                            if(compPower.PowerOn)
                            {
                                currentPlayState = true;
                                PlayMusic();
                            }
                            
                        }
                    };
                }
                else
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "stop",
                        defaultDesc = "stopCurrentMusic",
                        icon = ContentFinder<Texture2D>.Get("UI/Commands/Stop", true),
                        action = delegate ()
                        {
                            if (compPower.PowerOn)
                            {
                                currentPlayState = false;
                                StopMusic();
                            }                                
                        }
                    };
                }
                yield return new Command_Action
                {
                    defaultLabel = "nextSong",
                    defaultDesc = "nextSongDesc",
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/Next", true),
                    action = delegate ()
                    {
                        index++;
                        if (index >= songList.Count)
                        {
                            index = 0;
                        }
                        currentSong = songList[index];
                        if (currentPlayState)
                        {
                            Find.MusicManagerPlay.ForceStartSong(currentSong, false);
                        }
                    }
                };
            }           
            yield break;
        }
        /*public SongDef ChooseNextSong()
        {
            IEnumerable<SongDef> source = from song in DefDatabase<SongDef>.AllDefs
                                          where this.AppropriateNow(song)
                                          select song;
            return source.RandomElementByWeight((SongDef s) => s.commonality);
        }
        public bool AppropriateNow(SongDef song)
        {
            if (!song.playOnMap)
            {
                return false;
            }
            if (this.DangerMusicMode)
            {
                if (!song.tense)
                {
                    return false;
                }
            }
            else if (song.tense)
            {
                return false;
            }
            Map map = Find.AnyPlayerHomeMap ?? Find.CurrentMap;
            if (!song.allowedSeasons.NullOrEmpty<Season>())
            {
                if (map == null)
                {
                    return false;
                }
                if (!song.allowedSeasons.Contains(GenLocalDate.Season(map)))
                {
                    return false;
                }
            }
            if (song.allowedTimeOfDay == TimeOfDay.Any)
            {
                return true;
            }
            if (map == null)
            {
                return true;
            }
            if (song.allowedTimeOfDay == TimeOfDay.Night)
            {
                return GenLocalDate.DayPercent(map) < 0.2f || GenLocalDate.DayPercent(map) > 0.7f;
            }
            return GenLocalDate.DayPercent(map) > 0.2f && GenLocalDate.DayPercent(map) < 0.7f;
        }
        public bool DangerMusicMode
        {
            get
            {
                List<Map> maps = Find.Maps;
                for (int i = 0; i < maps.Count; i++)
                {
                    if (maps[i].dangerWatcher.DangerRating == StoryDanger.High)
                    {
                        return true;
                    }
                }
                return false;
            }
        }*/
        public void StopMusic()
        {
            Find.MusicManagerPlay.ForceStartSong(((CompProperties_JukeBox)props).stopSong, false);
        }
        public void PlayMusic()
        {
            Find.MusicManagerPlay.ForceStartSong(currentSong, false);
        }
    }
    public class CompProperties_JukeBox:CompProperties
    {
        public List<SongDef> songList;
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
