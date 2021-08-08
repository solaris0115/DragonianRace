using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using UnityEngine;
using HarmonyLib;
using System.Reflection.Emit;
using RimWorld.Planet;

namespace Dragonian
{
    [StaticConstructorOnStartup]
    public static class PatchSet
    {
        static PatchSet()
        {
            Harmony harmonyInstance = new Harmony("com.ColorPatch.rimworld.mod");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
    [HarmonyPatch(typeof(GenRecipe))]
    [HarmonyPatch("PostProcessProduct")]
    public static class ProductFinishGenColorHook
    {

        [HarmonyPrefix]
        static void Prefix(ref Thing product)
        {
            ThingWithComps twc = product as ThingWithComps;
            if (twc != null)
            {
                CustomThingDef def = twc.def as CustomThingDef;
                if (def != null && !def.followStuffColor)
                {
                    twc.SetColor(Color.white);
                }
            }
        }
    }
    [HarmonyPatch(typeof(PawnApparelGenerator))]
    [HarmonyPatch("GenerateStartingApparelFor")]
    public static class PawnGenColorHook
    {
        [HarmonyPostfix]
        static void Postfix(ref Pawn pawn)
        {
            if (pawn.apparel != null)
            {
                List<Apparel> wornApparel = pawn.apparel.WornApparel;
                for (int i = 0; i < wornApparel.Count; i++)
                {
                    CustomThingDef def = wornApparel[i].def as CustomThingDef;
                    if (def != null && !def.followStuffColor)
                    {
                        wornApparel[i].SetColor(Color.white);
                        wornApparel[i].SetColor(Color.black);
                        wornApparel[i].SetColor(Color.white);
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(ThingMaker))]
    [HarmonyPatch("MakeThing")]
    public static class ThingMakeColorHook
    {
        [HarmonyPostfix]
        static void Postfix(ref Thing __result)
        {
            ThingWithComps twc = __result as ThingWithComps;
            if (twc != null)
            {
                CustomThingDef def = twc.def as CustomThingDef;
                if (def != null && !def.followStuffColor)
                {
                    twc.SetColor(Color.white);
                    twc.SetColor(Color.black);
                    twc.SetColor(Color.white);
                }
            }
        }
    }

    public class CustomThingDef : ThingDef
    {
        public bool followStuffColor = true;
    }

    
    [DefOf]
    public class DragonianBodyDefOf
    {
        public static BodyDef Dragonian;
    }
    [DefOf]
    public class DragonianPawnKindDefOf
    {
        public static PawnKindDef Dragonian_Female;
        public static PawnKindDef Dragonian_Male;
    }
    
    [HarmonyPatch(typeof(ForbidUtility))]
    [HarmonyPatch("SetForbidden")]
    public static class FobidPatch
    {

        [HarmonyPrefix]
        static bool Prefix(this Thing t, bool value, bool warnOnFail = true)
        {
            if (t == null)
            {
                if (warnOnFail)
                {
                    Log.Error("Tried to SetForbidden on null Thing.", false);
                }
                return false;
            }
            ThingWithComps thingWithComps = t as ThingWithComps;
            if (thingWithComps == null)
            {
                if (warnOnFail)
                {
                    Log.Error("Tried to SetForbidden on non-ThingWithComps Thing " + t, false);
                }
                return false;
            }
            CompForbiddable comp = thingWithComps.GetComp<CompForbiddable>();
            if (comp == null)
            {
                return false;
            }
            comp.Forbidden = value;
            return false;
        }

    }
}