using System;
using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;
using Verse.AI;
using Harmony;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace Gloomylynx
{
    [StaticConstructorOnStartup]
    public static class TantrumPatch
    {
        private static readonly Type patchType = typeof(TantrumPatch);
        static TantrumPatch()
        {
            HarmonyInstance harmonyInstance = HarmonyInstance.Create("com.Gloomylynx.rimworld.mod");
            harmonyInstance.Patch(AccessTools.Method(typeof(TantrumMentalStateUtility), "CanSmash"), null, new HarmonyMethod(patchType, "CanSmashPostfix", null));
            harmonyInstance.Patch(AccessTools.Method(typeof(Pawn), "CheckForDisturbedSleep"), new HarmonyMethod(patchType, "CheckForDisturbedSleepPrefix", null));
            harmonyInstance.Patch(AccessTools.Method(typeof(GenConstruct), "CanPlaceBlueprintOver"), new HarmonyMethod(patchType, "CanPlaceBlueprintOverPrefix", null));
        }

        public static void CanSmashPostfix(ref bool __result, Thing thing)
        {
            if (!(thing is Building_Locker) && thing.Position.GetEdifice(thing.Map) is Building_Locker)
            {
                __result = false;
            }

        }
        public static bool CheckForDisturbedSleepPrefix(Pawn __instance, Pawn source,ref int ___lastSleepDisturbedTick)
        {
            if (__instance.needs.mood == null)
            {
                return false;
            }
            if (__instance.Awake())
            {
                return false;
            }
            if (__instance.Faction != Faction.OfPlayer)
            {
                return false;
            }
            if (Find.TickManager.TicksGame < ___lastSleepDisturbedTick + 300)
            {
                return false;
            }
            if (source != null)
            {
                if (LovePartnerRelationUtility.LovePartnerRelationExists(__instance, source))
                {
                    return false;
                }
                if (source.RaceProps.petness > 0f)
                {
                    return false;
                }
                if (source.relations != null)
                {
                    if (source.relations.DirectRelations.Any((DirectPawnRelation dr) => dr.def == PawnRelationDefOf.Bond))
                    {
                        return false;
                    }
                }
            }
            if(__instance.CurrentBed().def.defName == "GL_ClassyDoubleBed")
            {
                return false;
            }
            ___lastSleepDisturbedTick = Find.TickManager.TicksGame;
            __instance.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleepDisturbed, null);
            return false;
        }
        public static bool CanPlaceBlueprintOverPrefix(ref bool __result, BuildableDef newDef, ThingDef oldDef)
        {
            if (oldDef.EverHaulable)
            {
                __result = true;
                return false;
            }
            TerrainDef terrainDef = newDef as TerrainDef;
            if (terrainDef != null)
            {
                if (oldDef.IsBlueprint || oldDef.IsFrame)
                {
                    if (!terrainDef.affordances.Contains(oldDef.entityDefToBuild.terrainAffordanceNeeded))
                    {
                        __result = false;
                        return false;
                    }
                }
                else if (oldDef.category == ThingCategory.Building && !terrainDef.affordances.Contains(oldDef.terrainAffordanceNeeded))
                {
                    __result = false;
                    return false;
                }
            }
            ThingDef thingDef = newDef as ThingDef;
            BuildableDef buildableDef = GenConstruct.BuiltDefOf(oldDef);
            ThingDef thingDef2 = buildableDef as ThingDef;
            if (oldDef == ThingDefOf.SteamGeyser && !newDef.ForceAllowPlaceOver(oldDef))
            {
                __result = false;
                return false;
            }
            if (oldDef.category == ThingCategory.Plant && oldDef.passability == Traversability.Impassable && thingDef != null && thingDef.category == ThingCategory.Building && !thingDef.building.canPlaceOverImpassablePlant)
            {
                __result = false;
                return false;
            }
            if (oldDef.category == ThingCategory.Building || oldDef.IsBlueprint || oldDef.IsFrame)
            {
                if (thingDef != null)
                {
                    if (!thingDef.IsEdifice())
                    {
                        __result = (oldDef.building == null || oldDef.building.canBuildNonEdificesUnder) && (!thingDef.EverTransmitsPower || !oldDef.EverTransmitsPower);
                        return false;
                    }
                    if (thingDef.IsEdifice() && oldDef != null && oldDef.category == ThingCategory.Building && !oldDef.IsEdifice())
                    {
                        __result = thingDef.building == null || thingDef.building.canBuildNonEdificesUnder;
                        return false;
                    }
                    if (thingDef2 != null && (thingDef2 == ThingDefOf.Wall || thingDef2.IsSmoothed) && thingDef.building != null && thingDef.building.canPlaceOverWall)
                    {
                        __result = true;
                        return false;
                    }
                    if (newDef != ThingDefOf.PowerConduit && buildableDef == ThingDefOf.PowerConduit)
                    {
                        __result = true;
                        return false;
                    }
                }
                __result = (newDef is TerrainDef && buildableDef is ThingDef && ((ThingDef)buildableDef).CoexistsWithFloors) || (buildableDef is TerrainDef && !(newDef is TerrainDef));
                return false;
            }
            __result = true;
            return false;
        }
    }
}