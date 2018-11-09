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
using Harmony;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Gloomylynx
{
    public class CompJukeBox : ThingComp
    {
        //string assemblyFile = (new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath;
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
            Scribe_Collections.Look<SongDef>(ref songList, "songList", LookMode.Def, new object[0]);
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
            if (compPower != null && currentSong != null)
            {
                if (compPower.connectParent != null)
                {
                    string str;
                    str = "State".Translate() + ": ";
                    if (currentPlayState)
                    {
                        str += "Play".Translate()+ "\n";
                    }
                    else
                    {
                        str += "Pause".Translate()+"\n";
                    }
                    str = str + "CurrentSong".Translate() + ": " + currentSong.defName;
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
            if (signal == "FlickedOn")
            {
                if (currentPlayState)
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
            if (compPower.connectParent != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "previousSong".Translate(),
                    defaultDesc = "previousSongDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/Previous", true),
                    action = delegate ()
                    {
                        /*Log.Message("this.GetType().Assembly.Location: "+this.GetType().Assembly.Location);
                        Log.Message("Directory.GetCurrentDirectory(): "+ Directory.GetCurrentDirectory());
                        Log.Message("AppDomain.CurrentDomain.BaseDirectory: "+ AppDomain.CurrentDomain.BaseDirectory);
                        Log.Message("Application.StartupPath: " + Application.absoluteURL);*/
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
                        defaultLabel = "play".Translate(),
                        defaultDesc = "playCurrentMusic".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Commands/Play", true),
                        action = delegate ()
                        {
                            if (compPower.PowerOn)
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
                        defaultLabel = "stop".Translate(),
                        defaultDesc = "stopCurrentMusic".Translate(),
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
                    defaultLabel = "nextSong".Translate(),
                    defaultDesc = "nextSongDesc".Translate(),
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
                /*yield return new Command_Action
                {
                    defaultLabel = "loadFromShip",
                    defaultDesc = "loadFromShipDesc",
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/Next", true),
                    action = delegate ()
                    {
                        //DoSome
                    }
                };*/
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
    
    
    /*
    public class CustomMusicCore
    {
        public const string musicPath = "Songs/";
        public CustomMusicCore()
        {
            Current.Root_Play.musicManagerPlay = new MusicManagerPlay();
        }

        public static void scanMusicPath()
        {

            if (!Directory.Exists(CustomMusicCore.settings.musicPath))
            {
                Find.WindowStack.Add(new Dialog_MessageBox("CustomMusic_dirNotExist".Translate(), null, null, null, null, null, false, null, null));
                return;
            }

            string[] files = Directory.GetFiles(CustomMusicCore.settings.musicPath, "*.ogg", SearchOption.AllDirectories);
            IEnumerable<string> enumerable = from file in files
                                             where CustomMusicCore.settings.songs.FindIndex((SongEntry song) => song.clipPath == file) == -1
                                             select file;
            IEnumerable<string> removedFiles = from song in (from song in CustomMusicCore.settings.songs
                                                             select song.clipPath).ToList<string>()
                                               where !files.Contains(song)
                                               select song;
            foreach (string clipPath in enumerable)
            {
                CustomMusicCore.settings.songs.Add(new SongEntry());
                SongEntry songEntry = CustomMusicCore.settings.songs.Last<SongEntry>();
                songEntry.clipPath = clipPath;
                songEntry.PostLoad();
                songEntry.ResolveReferences();
                DefDatabase<SongDef>.Add(songEntry);
            }
            using (IEnumerator<string> enumerator = removedFiles.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    string file = enumerator.Current;
                    typeof(DefDatabase<SongDef>).GetMethod("Remove", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[]
                    {
                        CustomMusicCore.settings.songs.Find((SongEntry song) => song.clipPath == file)
                    });
                }
            }
            CustomMusicCore.settings.songs.RemoveAll((SongEntry song) => removedFiles.Contains(song.clipPath));
        }
    }*/

    /*
    public class MusicSettings : ModSettings
    {
        public string musicPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public List<SongEntry> songs = new List<SongEntry>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<string>(ref this.musicPath, "musicPath", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), false);
            Scribe_Collections.Look<SongEntry>(ref this.songs, "songList", LookMode.Deep, new object[0]);
        }
    }

    public class SongEntry : SongDef, IExposable
    {
        public WWW source;

        public SongEntry()
        {
            this.allowedSeasons = new List<Season>(3);
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

        public override void PostLoad()
        {
            if (this.defName == "UnnamedDef")
            {
                this.defName = Regex.Replace(Path.GetFileNameWithoutExtension(this.clipPath), "[^\\w-]", "_", RegexOptions.IgnoreCase);
            }
        }

        public override void ResolveReferences()
        {
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                this.source = new WWW("file://" + this.clipPath);
            });
        }

        public void setAllowedSeasons(bool spring, bool summer, bool fall, bool winter)
        {
            if (spring && summer && fall && winter)
            {
                if (!this.allowedSeasons.Contains(Season.Undefined))
                {
                    this.allowedSeasons.Clear();
                    this.allowedSeasons.Add(Season.Undefined);
                    return;
                }
            }
            else
            {
                this.allowedSeasons.Remove(Season.Undefined);
                if (spring)
                {
                    if (!this.allowedSeasons.Contains(Season.Spring))
                    {
                        this.allowedSeasons.Add(Season.Spring);
                    }
                }
                else
                {
                    this.allowedSeasons.Remove(Season.Spring);
                }
                if (summer)
                {
                    if (!this.allowedSeasons.Contains(Season.Summer))
                    {
                        this.allowedSeasons.Add(Season.Summer);
                    }
                }
                else
                {
                    this.allowedSeasons.Remove(Season.Summer);
                }
                if (fall)
                {
                    if (!this.allowedSeasons.Contains(Season.Fall))
                    {
                        this.allowedSeasons.Add(Season.Fall);
                    }
                }
                else
                {
                    this.allowedSeasons.Remove(Season.Fall);
                }
                if (winter)
                {
                    if (!this.allowedSeasons.Contains(Season.Winter))
                    {
                        this.allowedSeasons.Add(Season.Winter);
                        return;
                    }
                }
                else
                {
                    this.allowedSeasons.Remove(Season.Winter);
                }
            }
        }

        public bool playsDuringSpring
        {
            get
            {
                return this.allowedSeasons.Contains(Season.Spring);
            }
        }

        public bool playsDuringSummer
        {
            get
            {
                return this.allowedSeasons.Contains(Season.Summer);
            }
        }

        public bool playsDuringFall
        {
            get
            {
                return this.allowedSeasons.Contains(Season.Fall);
            }
        }

        public bool playsDuringWinter
        {
            get
            {
                return this.allowedSeasons.Contains(Season.Winter);
            }
        }
    }*/
}
