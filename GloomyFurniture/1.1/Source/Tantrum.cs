using System;
using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;
using Verse.AI;
using HarmonyLib;
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
            Harmony harmonyInstance = new Harmony("com.Gloomylynx.rimworld.mod");
            harmonyInstance.Patch(AccessTools.Method(typeof(TantrumMentalStateUtility), "CanSmash"), null, new HarmonyMethod(patchType, "CanSmashPostfix", null));
            harmonyInstance.Patch(AccessTools.Method(typeof(Pawn), "CheckForDisturbedSleep"), new HarmonyMethod(patchType, "CheckForDisturbedSleepPrefix", null));
            harmonyInstance.Patch(AccessTools.Method(typeof(GenConstruct), "CanPlaceBlueprintOver"), new HarmonyMethod(patchType, "CanPlaceBlueprintOverPrefix", null));
            harmonyInstance.Patch(AccessTools.Method(typeof(GenSpawn), "SpawningWipes"), new HarmonyMethod(patchType, "SpawningWipesPrefix", null));

        }

        public static void CanSmashPostfix(ref bool __result, Thing thing)
        {
            if (!(thing is Building_Locker) && thing.Position.GetEdifice(thing.Map) is Building_Locker)
            {
                __result = false;
            }

        }
        public static bool CheckForDisturbedSleepPrefix(Pawn __instance)
        {
            if (__instance!=null && __instance.CurrentBed() !=null && __instance.CurrentBed().def !=null && __instance.CurrentBed().def.defName == "GL_ClassyDoubleBed")
            {
                return false;
            }
            return true;
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
                    if (thingDef2 != null && (thingDef2 == ThingDefOf.Wall || thingDef2.IsSmoothed || thingDef2.thingClass == typeof(GL_Building)) && thingDef.building != null && thingDef.building.canPlaceOverWall)
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
        public static bool SpawningWipesPrefix(ref bool __result, BuildableDef newEntDef, BuildableDef oldEntDef)
        {
            ThingDef thingDef = newEntDef as ThingDef;
            ThingDef thingDef2 = oldEntDef as ThingDef;
            if (thingDef == null || thingDef2 == null)
            {
                __result = false;
                return false;
            }
            if (thingDef.category == ThingCategory.Attachment || thingDef.category == ThingCategory.Mote || thingDef.category == ThingCategory.Filth || thingDef.category == ThingCategory.Projectile)
            {
                __result = false;
                return false;
            }
            if (!thingDef2.destroyable)
            {
                __result = false;
                return false;
            }
            if (thingDef.category == ThingCategory.Plant)
            {
                __result = false;
                return false;
            }
            if (thingDef2.category == ThingCategory.Filth && thingDef.passability != Traversability.Standable)
            {
                __result = true;
                return false;
            }
            if (thingDef2.category == ThingCategory.Item && thingDef.passability == Traversability.Impassable && thingDef.surfaceType == SurfaceType.None)
            {
                __result = true;
                return false;
            }
            if (thingDef.EverTransmitsPower && thingDef2 == ThingDefOf.PowerConduit)
            {
                __result = true;
                return false;
            }
            if (thingDef.IsFrame && GenSpawn.SpawningWipes(thingDef.entityDefToBuild, oldEntDef))
            {
                __result = true;
                return false;
            }
            BuildableDef buildableDef = GenConstruct.BuiltDefOf(thingDef);
            BuildableDef buildableDef2 = GenConstruct.BuiltDefOf(thingDef2);
            if (buildableDef == null || buildableDef2 == null)
            {
                __result = false;
                return false;
            }
            ThingDef thingDef3 = thingDef.entityDefToBuild as ThingDef;
            if (thingDef2.IsBlueprint)
            {
                if (thingDef.IsBlueprint)
                {
                    if (thingDef3 != null && thingDef3.building != null && thingDef3.building.canPlaceOverWall && thingDef2.entityDefToBuild is ThingDef && ((ThingDef)thingDef2.entityDefToBuild == ThingDefOf.Wall || ((ThingDef)thingDef2.entityDefToBuild).thingClass == typeof(GL_Building) ))
                    {
                        __result = true;
                        return false;
                    }
                    if (thingDef2.entityDefToBuild is TerrainDef)
                    {
                        if (thingDef.entityDefToBuild is ThingDef && ((ThingDef)thingDef.entityDefToBuild).coversFloor)
                        {
                            __result = true;
                            return false;
                        }
                        if (thingDef.entityDefToBuild is TerrainDef)
                        {
                            __result = true;
                            return false;
                        }
                    }
                }
                __result = thingDef2.entityDefToBuild == ThingDefOf.PowerConduit && thingDef.entityDefToBuild is ThingDef && (thingDef.entityDefToBuild as ThingDef).EverTransmitsPower;
                return false;
            }
            if ((thingDef2.IsFrame || thingDef2.IsBlueprint) && thingDef2.entityDefToBuild is TerrainDef)
            {
                ThingDef thingDef4 = buildableDef as ThingDef;
                if (thingDef4 != null && !thingDef4.CoexistsWithFloors)
                {
                    __result = true;
                    return false;
                }
            }
            if (thingDef2 == ThingDefOf.ActiveDropPod)
            {
                __result = false;
                return false;
            }
            if (thingDef == ThingDefOf.ActiveDropPod)
            {
                __result = thingDef2 != ThingDefOf.ActiveDropPod && (thingDef2.category == ThingCategory.Building && thingDef2.passability == Traversability.Impassable);
                return false;
            }
            if (thingDef.IsEdifice())
            {
                if (thingDef.BlockPlanting && thingDef2.category == ThingCategory.Plant)
                {
                    __result = true;
                    return false;
                }
                if (!(buildableDef is TerrainDef) && buildableDef2.IsEdifice())
                {
                    __result = true;
                    return false;
                }
            }
            __result = false;
            return false;
        }
    }
    [DefOf]
    public static class WallDefOf
    {
        public static ThingDef RGK_Wall;
        public static ThingDef GL_Wall;
    }
    public class GL_Building : Building
    {

    }
}