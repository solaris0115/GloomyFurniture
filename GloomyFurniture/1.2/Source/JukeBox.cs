using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using System.IO;
using Verse;
using Verse.Sound;
using UnityEngine;
using Verse.AI;
using HarmonyLib;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Gloomylynx
{
    public class CompJukeBox : ThingComp
    {
        public static bool currentState=false;
        public CompPowerTrader compPowerTrader;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            JukeBoxCore.AddJukeBox(this);
            compPowerTrader = parent.GetComp<CompPowerTrader>();
        }
        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            JukeBoxCore.RemoveJukeBox(this);
            //마지막 한대면 복구
        }
        public override void PostExposeData()
        {
            //이미 존재하는 개체
            base.PostExposeData();
            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                if(JukeBoxCore.orignalSongList.Count>0)
                {
                    DefDatabase<SongDef>.Clear();
                    DefDatabase<SongDef>.Add(JukeBoxCore.orignalSongList);
                }
                JukeBoxCore.jukeBoxList.Clear();
            }
            if (Scribe.mode==LoadSaveMode.PostLoadInit)
            {
                compPowerTrader = parent.GetComp<CompPowerTrader>();
                if (compPowerTrader.PowerOn)
                {
                    JukeBoxCore.AddJukeBox(this);
                }
                currentState = false;
            }
        }
        public override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);
            if (signal == "PowerTurnedOff")
            {
                if(JukeBoxCore.orignalSongList.Count<=0)
                {
                    JukeBoxCore.orignalSongList.AddRange(DefDatabase<SongDef>.AllDefs);
                }
                JukeBoxCore.RemoveJukeBox(this);
            }
            if (signal == "PowerTurnedOn")
            {
                JukeBoxCore.AddJukeBox(this);
            }
        }
        public void PlaySong()
        {
            currentState = true;
            if(JukeBoxCore.orignalSongList.Count<=0)
            {
                JukeBoxCore.orignalSongList.AddRange(DefDatabase<SongDef>.AllDefs);
            }
            if(JukeBoxCore.customSongList.Count<=0)
            {
                JukeBoxCore.Scanning();
                if (JukeBoxCore.customSongList.Count <= 0)
                {
                    Log.Error("Songs Folder is Empty");
                    return;
                }
            }
            DefDatabase<SongDef>.Clear();
            DefDatabase<SongDef>.Add(JukeBoxCore.customSongList);
            Find.MusicManagerPlay.ForceStartSong(DefDatabase<SongDef>.GetRandom(),false);
        }
        public void NextSong()
        {
            if(currentState)
            {
                Find.MusicManagerPlay.ForceStartSong(DefDatabase<SongDef>.GetRandom(), false);
            }
        }
        public void StopSong()
        {
            try
            {
                currentState = false;
                if (JukeBoxCore.orignalSongList.Count <= 0)
                {
                    JukeBoxCore.orignalSongList.AddRange(DefDatabase<SongDef>.AllDefs);                    
                    //Log.Message("OriginalSongList is Empty");
                }
                DefDatabase<SongDef>.Clear();
                DefDatabase<SongDef>.Add(JukeBoxCore.orignalSongList);
                Find.MusicManagerPlay.ForceStartSong(((CompProperties_JukeBox)props).stopSong, false);
            }
            catch(Exception ee)
            {
                Log.Error(ee.ToString());
            }
        }
        public void Synchronize()
        {
            JukeBoxCore.Scanning();
        }
        public override string CompInspectStringExtra()
        {
            if (compPowerTrader != null)
            {
                if (compPowerTrader.PowerOn)
                {
                    string str;
                    str = "State".Translate() + ": ";
                    if (currentState)
                    {
                        str += "Play".Translate();
                    }
                    else
                    {
                        str += "Pause".Translate();
                    }
                    return str;
                }
            }
            return string.Empty;
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo c in base.CompGetGizmosExtra())
            {
                yield return c;
            }
            if(compPowerTrader.PowerOn)
            {
                yield return new Command_Action
                {
                    defaultLabel = "nextSong".Translate(),
                    defaultDesc = "nextSongDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/Next", true),
                    action = delegate ()
                    {
                        try
                        {
                            NextSong();
                        }
                        catch (Exception ee)
                        {
                            Log.Error("NextSong " + ee);
                        }
                    }
                };
                if(!currentState)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "playSong".Translate(),
                        defaultDesc = "playSongDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Commands/Play", true),
                        action = delegate ()
                        {
                            try
                            {
                                PlaySong();
                            }
                            catch (Exception ee)
                            {
                                Log.Error("PlaySong " + ee);
                            }
                        }
                    };
                }
                else
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "stopSong".Translate(),
                        defaultDesc = "stopSongDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Commands/Stop", true),
                        action = delegate ()
                        {
                            try
                            {
                                StopSong();
                            }
                            catch(Exception ee)
                            {
                                Log.Error("StopSong " + ee);
                            }                            
                        }
                    };
                }
                /*yield return new Command_Action
                {
                    defaultLabel = "synchronize".Translate(),
                    defaultDesc = "synchronizeDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/Sync", true),
                    action = delegate ()
                    {
                        Synchronize();
                    }
                };*/
            }
            yield break;
        }
    }

    [StaticConstructorOnStartup]
    public class JukeBoxMod : Mod
    {
        public string RootDirectory
        {
            get
            {
                return Content.RootDir;
            }
        }

        public JukeBoxMod(ModContentPack content) : base(content)
        {
            JukeBoxCore.jukeBoxMod = this;
        }
    }
    public class JukeBoxCore
    {
        public static bool isInitialized=false;
        public static JukeBoxMod jukeBoxMod;
        public static List<SongDef> customSongList = new List<SongDef>();
        public static List<SongDef> orignalSongList=new List<SongDef>();
        public static HashSet<CompJukeBox> jukeBoxList = new HashSet<CompJukeBox>();

        public JukeBoxCore()
        {
            Current.Root_Play.musicManagerPlay = new MusicManagerPlay();
        }
        public static void Scanning()
        {
            isInitialized = true;
            if (customSongList==null)
            {
                customSongList = new List<SongDef>();
            }
            try
            {
                string[] files = Directory.GetFiles(jukeBoxMod.RootDirectory + @"\Sounds\Songs", "*.*", SearchOption.AllDirectories);
                for (int index = 0; index < files.Length; index++)
                {
                    files[index] = Path.GetFileName(files[index]).Split('.')[0];
                }
                customSongList.Clear();
                foreach (string s in files)
                {
                    SongEntry songDef = new SongEntry(@"Songs/" + s)
                    {
                        tense = false,
                        playOnMap = true
                    };
                    customSongList.Add(songDef);
                }
                foreach (string s in files)
                {
                    SongEntry songDef = new SongEntry(@"Songs/" + s);
                    songDef.defName = songDef.defName + "_tense";
                    songDef.playOnMap = true;
                    songDef.tense = true;
                    customSongList.Add(songDef);
                }
                if (customSongList.Count<=0)
                {
                    customSongList.AddRange(DefDatabase<SongDef>.AllDefs);
                }
            }
            catch (Exception ee)
            {
                Log.Error("Scanning Error - " + ee);
            }
        }
        public static void AddJukeBox(CompJukeBox comp)
        {
            jukeBoxList.Add(comp);
        }
        public static void RemoveJukeBox(CompJukeBox comp)
        {
            if (jukeBoxList.Count ==1)
            {
                comp.StopSong();
            }
            jukeBoxList.Remove(comp);
        }
    }
    public class SongEntry : SongDef, IExposable
    {
        public SongEntry(string path)
        {
            this.clipPath = path;
            if (this.defName == "UnnamedDef")
            {
                string[] array = this.clipPath.Split(new char[]
                {
                    '/',
                    '\\'
                });
                this.defName = array[array.Length - 1];
            }
            clip = this.clip = ContentFinder<AudioClip>.Get(this.clipPath, true);
        }
        public override void PostLoad()
        {
            base.PostLoad();
            if (this.defName == "UnnamedDef")
            {
                string[] array = this.clipPath.Split(new char[]
                {
                    '/',
                    '\\'
                });
                this.defName = array[array.Length - 1];
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look<string>(ref this.clipPath, "clipPath", null, false);
            Scribe_Values.Look<string>(ref this.defName, "defName", null, false);
            Scribe_Collections.Look<Season>(ref this.allowedSeasons, "allowedSeasons", LookMode.Undefined, new object[0]);
            Scribe_Values.Look<TimeOfDay>(ref this.allowedTimeOfDay, "allowedTimeOfDay", TimeOfDay.Any, false);
            Scribe_Values.Look<bool>(ref this.tense, "tense", false, false);
            Scribe_Values.Look<float>(ref this.commonality, "commonality", 1f, false);
            Scribe_Values.Look<bool>(ref this.playOnMap, "playOnMap", true, false);
            Scribe_Values.Look<float>(ref this.volume, "volume", 1f, false);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.PostLoad();
            }
            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                this.ResolveReferences();
            }
        }
        public override void ResolveReferences()
        {
            base.ResolveReferences();
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                this.clip = ContentFinder<AudioClip>.Get(this.clipPath, true);
            });
        }
    }
}
