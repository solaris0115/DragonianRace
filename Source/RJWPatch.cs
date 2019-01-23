using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using rjw;
using UnityEngine;
using Harmony;
using System.Reflection.Emit;
using RimWorld.Planet;

namespace Dragonian
{
    [StaticConstructorOnStartup]
    public static class DragonianPatch
    {
        private static readonly Type patchType = typeof(DragonianPatch);
        static DragonianPatch()
        {
            HarmonyInstance harmonyInstance = HarmonyInstance.Create("com.Dragonian.rimworld.mod");
            harmonyInstance.Patch(AccessTools.Method(typeof(Pawn), "ChangeKind", null, null), new HarmonyMethod(patchType, "ChangeKindPrefix", null), null, null);
        }

        public static bool ChangeKindPrefix(Pawn __instance, PawnKindDef newKindDef)
        {
            if (__instance.kindDef == PawnKindDefOf.WildMan)
            {
                __instance.kindDef = DragonianPawnKindDefOf.Dragonian;
                return false;
            }
            return newKindDef == PawnKindDefOf.WildMan;
        }        
    }
}
