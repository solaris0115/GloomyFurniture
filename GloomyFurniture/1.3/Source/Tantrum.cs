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
        [Obsolete]
        public static bool SpawningWipesPrefix(ref bool __result, BuildableDef newEntDef, BuildableDef oldEntDef)
        {
            ThingDef newThingDef = newEntDef as ThingDef;
            ThingDef oldThingDef = oldEntDef as ThingDef;
            if (newThingDef == null || oldThingDef == null)
            {
                __result = false;
                return false;
            }
            if (newThingDef.category == ThingCategory.Attachment || newThingDef.category == ThingCategory.Mote || newThingDef.category == ThingCategory.Filth || newThingDef.category == ThingCategory.Projectile)
            {
                __result = false;
                return false;
            }
            if (!oldThingDef.destroyable)
            {
                __result = false;
                return false;
            }
            if (newThingDef.category == ThingCategory.Plant)
            {
                __result = false;
                return false;
            }
            if (oldThingDef.category == ThingCategory.Filth && newThingDef.passability != Traversability.Standable)
            {
                __result = true;
                return false;
            }
            if (oldThingDef.category == ThingCategory.Item && newThingDef.passability == Traversability.Impassable && newThingDef.surfaceType == SurfaceType.None)
            {
                __result = true;
                return false;
            }
            if (newThingDef.EverTransmitsPower && oldThingDef == ThingDefOf.PowerConduit)
            {
                __result = true;
                return false;
            }
            if (newThingDef.IsFrame && GenSpawn.SpawningWipes(newThingDef.entityDefToBuild, oldEntDef))
            {
                __result = true;
                return false;
            }
            
            BuildableDef newBuildableDef = GenConstruct.BuiltDefOf(newThingDef);
            BuildableDef oldBuildableDef2 = GenConstruct.BuiltDefOf(oldThingDef);
            if (newBuildableDef == null || oldBuildableDef2 == null)
            {
                __result = false;
                return false;
            }
            
            ThingDef newBuildEntityDef = newThingDef.entityDefToBuild as ThingDef;
            if (oldThingDef.IsBlueprint)
            {
                if (newThingDef.IsBlueprint)
                {
                    if (newBuildEntityDef != null && newBuildEntityDef.building != null && newBuildEntityDef.building.canPlaceOverWall && oldThingDef.entityDefToBuild is ThingDef && ((ThingDef)oldThingDef.entityDefToBuild == ThingDefOf.Wall || ((ThingDef)oldThingDef.entityDefToBuild).thingClass == typeof(GL_Building) ))
                    {
                        __result = true;
                        return false;
                    }
                    if (oldThingDef.entityDefToBuild is TerrainDef)
                    {
                        if (newThingDef.entityDefToBuild is ThingDef && ((ThingDef)newThingDef.entityDefToBuild).coversFloor)
                        {
                            __result = true;
                            return false;
                        }
                        if (newThingDef.entityDefToBuild is TerrainDef)
                        {
                            __result = true;
                            return false;
                        }
                    }
                }
                __result = oldThingDef.entityDefToBuild == ThingDefOf.PowerConduit && newThingDef.entityDefToBuild is ThingDef && (newThingDef.entityDefToBuild as ThingDef).EverTransmitsPower;
                return false;
            }
            if ((oldThingDef.IsFrame || oldThingDef.IsBlueprint) && oldThingDef.entityDefToBuild is TerrainDef)
            {
                ThingDef thingDef4 = newBuildableDef as ThingDef;
                if (thingDef4 != null && !thingDef4.CoexistsWithFloors)
                {
                    __result = true;
                    return false;
                }
            }
            if (oldThingDef == ThingDefOf.ActiveDropPod)
            {
                __result = false;
                return false;
            }
            if (newThingDef == ThingDefOf.ActiveDropPod)
            {
                __result = oldThingDef != ThingDefOf.ActiveDropPod && (oldThingDef.category == ThingCategory.Building && oldThingDef.passability == Traversability.Impassable);
                return false;
            }
            if (newThingDef.IsEdifice())
            {
                if (newThingDef.blockPlants && oldThingDef.category == ThingCategory.Plant)
                {
                    __result = true;
                    return false;
                }
                if (!(newBuildableDef is TerrainDef) && oldBuildableDef2.IsEdifice())
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