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
    public static class LockerPatch
    {
        private static readonly Type patchType = typeof(LockerPatch);
        static LockerPatch()
        {
            HarmonyInstance harmonyInstance = HarmonyInstance.Create("com.Gloomylynx.rimworld.mod");
            harmonyInstance.Patch(AccessTools.Method(typeof(Fire), "DoFireDamage"), new HarmonyMethod(patchType, "DoFireDamagePrefix", null));
        }
        public static bool DoFireDamagePrefix(Thing targ)
        {
            if(targ is Pawn || targ is Building_Locker)
            {
                return true;
            }
            if(targ.Position.GetEdifice(targ.Map) is Building_Locker)
            {
                return false;
            }
            return true;
        }
    }
    public class Building_Locker : Building_Storage
    {

    }
}
