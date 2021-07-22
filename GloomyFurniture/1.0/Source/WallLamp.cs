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
	public class PlaceWorker_WallAttachment : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
		{
			IntVec3 intVec = loc + rot.FacingCell;
			bool flag = false;
			bool flag2 = true;
			if (!GenGrid.Impassable(loc, map))
			{
				flag = true;
			}
			if (!GenGrid.InBounds(intVec, map) || GenGrid.Impassable(intVec, map))
			{
				flag2 = false;
			}
			AcceptanceReport result;
			if (flag || !flag2)
			{
				if (flag && !flag2)
				{
					result = Translator.Translate("WallAttachment_Both");
				}
				else
				{
					if (flag)
					{
						result = Translator.Translate("WallAttachment_WarningNeedWall");
					}
					else
					{
						result = Translator.Translate("WallAttachment_WarningBlocked");
					}
				}
			}
			else
			{
				result = true;
			}
			return result;
		}
	}

    public class WallLamp : Building
    {
        private Thing glower = null;

        private GlowObject glowerObject = null;
        
        public CompPowerTrader compPower;
        public CompFlickable compFlick;
        public CompRefuelable compRefuel;
        

        private IntVec3 glowPos;

        public string defStr = "GlowObject";

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compPower = GetComp<CompPowerTrader>();
            compFlick = GetComp<CompFlickable>();
            compRefuel = GetComp<CompRefuelable>();
            glowPos = Position + Rotation.FacingCell;
            ColorSetup();
            if (glower == null)
            {
                SpawnGlower();
            }
            else
            {
                if (glowerObject == null)
                {
                    glowerObject = (GlowObject)glower;
                }
            }
            if((compPower!=null && !compPower.PowerOn) || (compRefuel != null && !compRefuel.HasFuel))
            {
                glowerObject.ToggleGlower(false);
            }
        }

        public override void DeSpawn(DestroyMode mode)
        {
            DespawnGlower();
            base.DeSpawn(mode);
        }

        public override void Destroy(DestroyMode mode)
        {
            DespawnGlower();
            base.Destroy(mode);
        }

        protected override void ReceiveCompSignal(string signal)
        {
            if (signal == "FlickedOff" || signal == "PowerTurnedOff" || signal == "RanOutOfFuel")
            {
                glowerObject.ToggleGlower(false);
            }
            else
            {
                switch(signal)
                {
                    case "FlickedOn":
                        if ((compPower != null && compPower.PowerOn) || (compRefuel != null && compRefuel.HasFuel))
                        {
                            glowerObject.ToggleGlower(true);
                        }
                        break;
                    case "PowerTurnedOn":
                        if (compFlick == null || !compFlick.SwitchIsOn)
                        {
                            break;
                        }
                        glowerObject.ToggleGlower(true);
                        break;
                    case "Refueled":
                        if (!compRefuel.HasFuel || compFlick == null || !compFlick.SwitchIsOn)
                        {
                            glowerObject.ToggleGlower(false);
                            break;
                        }
                        glowerObject.ToggleGlower(true);
                        break;
                    default:
                        break;
                }
            }
        }
        public override void Tick()
        {
            base.Tick();
            if (Spawned && !Destroyed)
            {
                if (!GenGrid.Impassable(Position, Map))
                {
                    WallDestroyed();
                }
                else
                {
                    if (GenGrid.Impassable(glowPos, Map))
                    {
                        LightSmashed();
                    }
                }
            }
        }

        private void SpawnGlower()
        {
            if (glower == null)
            {
                glower = Map.thingGrid.ThingAt(glowPos, ThingDef.Named(defStr));
                if (glower == null)
                {
                    glower = ThingMaker.MakeThing(ThingDef.Named(defStr), null);
                    GenSpawn.Spawn(glower, glowPos, Map, 0);
                }
                glowerObject = (GlowObject)glower;
            }
        }

        private void DespawnGlower()
        {
            glower = null;
            glowerObject = null;
        }

        private void LightSmashed()
        {
            Messages.Message(Translator.Translate("WallLight_DestroyedBlocked"), MessageTypeDefOf.NegativeEvent, true);
            Destroy(DestroyMode.Vanish);
        }

        private void WallDestroyed()
        {
            Messages.Message(Translator.Translate("WallLight_DestroyedWall"), MessageTypeDefOf.NegativeEvent, true);
            Destroy(DestroyMode.Deconstruct);
        }

        public virtual void ColorSetup()
        {
            defStr = "GlowObject";
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_References.Look(ref glower, "glower", true);
        }
    }
    public class GlowObject : ThingWithComps
    {
        private CompFlickable compFlick;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compFlick = GetComp<CompFlickable>();
        }

        public override void Destroy(DestroyMode mode = 0)
        {
        }

        public void ToggleGlower(bool on)
        {
            if (compFlick != null)
            {
                compFlick.SwitchIsOn = on;
            }
        }
    }

    public class PlaceWorker_PassableObject : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            AcceptanceReport result;
            if (GenGrid.Impassable(loc, map))
            {
                result = Translator.Translate("PassableObject_CantPass");
            }
            else
            {
                result = true;
            }
            return result;
        }
    }
    public class PlaceWorker_StandableObject : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            AcceptanceReport result;
            if (!GenGrid.Standable(loc, map))
            {
                result = Translator.Translate("StandableObject_CantPass");
            }
            else
            {
                result = true;
            }
            return result;
        }
    }

    public class WallLampComp : ThingComp
    {
        private Thing glower = null;

        private GlowObject glowerObject = null;

        public CompPowerTrader compPower;
        public CompFlickable compFlick;
        public CompRefuelable compRefuel;
        
        public WallLampCompProperties Prop
        {
            get
            {
                return (WallLampCompProperties)props;
            }
        }

        private IntVec3 glowPos;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compPower = parent.GetComp<CompPowerTrader>();
            compFlick = parent.GetComp<CompFlickable>();
            compRefuel = parent.GetComp<CompRefuelable>();
            glowPos = parent.Position + parent.Rotation.FacingCell;
            if (glower == null)
            {
                SpawnGlower();
            }
            else
            {
                if (glowerObject == null)
                {
                    glowerObject = (GlowObject)glower;
                }
            }
            if ((compPower != null && !compPower.PowerOn) || (compRefuel != null && !compRefuel.HasFuel))
            {
                glowerObject.ToggleGlower(false);
            }
        }
        public override void PostDeSpawn(Map map)
        {
            DespawnGlower();
            base.PostDeSpawn(map);
        }
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            DespawnGlower();
            base.PostDestroy(mode, previousMap);
        }

        public override void ReceiveCompSignal(string signal)
        {
            if (signal == "FlickedOff" || signal == "PowerTurnedOff" || signal == "RanOutOfFuel")
            {
                glowerObject.ToggleGlower(false);
            }
            else
            {
                switch (signal)
                {
                    case "FlickedOn":
                        if ((compPower != null && compPower.PowerOn) || (compRefuel != null && compRefuel.HasFuel))
                        {
                            glowerObject.ToggleGlower(true);
                        }
                        break;
                    case "PowerTurnedOn":
                        if (compFlick == null || !compFlick.SwitchIsOn)
                        {
                            break;
                        }
                        glowerObject.ToggleGlower(true);
                        break;
                    case "Refueled":
                        if (!compRefuel.HasFuel || compFlick == null || !compFlick.SwitchIsOn)
                        {
                            glowerObject.ToggleGlower(false);
                            break;
                        }
                        glowerObject.ToggleGlower(true);
                        break;
                    default:
                        break;
                }
            }
        }
        public override void CompTick()
        {
            base.CompTick();
            if (parent.Spawned && !parent.Destroyed)
            {
                if (!GenGrid.Impassable(parent.Position, parent.Map))
                {
                    WallDestroyed();
                }
                else
                {
                    if (GenGrid.Impassable(glowPos, parent.Map))
                    {
                        LightSmashed();
                    }
                }
            }
        }

        private void SpawnGlower()
        {
            if (glower == null)
            {
                glower = ThingMaker.MakeThing(Prop.lighingObject, null);
                GenSpawn.Spawn(glower, glowPos, parent.Map, 0);
                glowerObject = (GlowObject)glower;
            }
        }

        private void DespawnGlower()
        {
            if (glower != null)
            {
                glower.DeSpawn(DestroyMode.Vanish);
            }
            glower = null;
            glowerObject = null;
        }

        private void LightSmashed()
        {
            Messages.Message(Translator.Translate("WallLight_DestroyedBlocked"), MessageTypeDefOf.NegativeEvent, true);
            parent.Destroy(DestroyMode.Vanish);
        }

        private void WallDestroyed()
        {
            Messages.Message(Translator.Translate("WallLight_DestroyedWall"), MessageTypeDefOf.NegativeEvent, true);
            parent.Destroy(DestroyMode.Deconstruct);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref glower, "glower", false);
        }
    }
    public class WallLampCompProperties : CompProperties
    {
        public ThingDef lighingObject;
    }
}
