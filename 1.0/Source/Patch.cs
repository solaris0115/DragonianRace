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
using Harmony;
using System.Reflection.Emit;
using RimWorld;

namespace Dragonian
{
    [StaticConstructorOnStartup]
    public static class DragonianPatch
    {
        private static readonly Type patchType = typeof(DragonianPatch);
        static DragonianPatch()
        {
            HarmonyInstance harmonyInstance = HarmonyInstance.Create("com.Dragonian.rimworld.mod");
            harmonyInstance.Patch(AccessTools.Method(typeof(PawnGenerator), "GeneratePawn", new Type[] { typeof(PawnKindDef), typeof(Faction) }, null), new HarmonyMethod(patchType, "GeneratePawnPrefix", null), null, null);
            harmonyInstance.Patch(AccessTools.Method(typeof(Pawn), "ChangeKind", null, null), new HarmonyMethod(patchType, "ChangeKindPrefix", null), null, null);
            harmonyInstance.Patch(AccessTools.Method(typeof(IncidentWorker_WildManWandersIn), "TryExecuteWorker", null, null), new HarmonyMethod(patchType, "TryExecuteWorkerPrefix", null), null, null);
            //harmonyInstance.Patch(AccessTools.Method(typeof(WildManUtility), "IsWildMan", null, null), new HarmonyMethod(patchType, "IsWildManPrefix", null), null, null);

            //harmonyInstance.Patch(AccessTools.Method(typeof(ThinkNode_ConditionalPawnKind), "Satisfied", null, null), new HarmonyMethod(patchType, "SatisfiedPrefix", null), null, null);
        }/*
        public static bool IsWildManPrefix(Pawn p, ref bool __result)
        {
            if ((p.kindDef == DragonianPawnKindDefOf.Dragonian_Female || p.kindDef == DragonianPawnKindDefOf.Dragonian_Male))
            {
                __result = true;
            }
            else
            {
                __result = false;
            }
            return false;
        }

        public static bool SatisfiedPrefix(PawnKindDef ___pawnKind, Pawn pawn, ref bool __result)
        {
            if (___pawnKind == PawnKindDefOf.WildMan && (pawn.kindDef == DragonianPawnKindDefOf.Dragonian_Female || pawn.kindDef == DragonianPawnKindDefOf.Dragonian_Male))
            {
                __result = true;
            }
            else
            {
                __result = pawn.kindDef == ___pawnKind;
            }
            return false;
        }*/


        public static bool TryExecuteWorkerPrefix(IncidentWorker_WildManWandersIn __instance, IncidentParms parms,ref bool __result)
        {
            Map map = (Map)parms.target;
            IntVec3 loc;
            if (!(CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => map.reachability.CanReachColony(c), map, CellFinder.EdgeRoadChance_Ignore, out loc)))
            {
                __result= false;
                return  false;
            }
            Faction faction;
            if (!(Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out faction, false, true, TechLevel.Undefined)))
            {
                __result= false;
                return  false;
            }
            PawnKindDef kindDef;
            if (Rand.Value > 0.5)
            {
                kindDef = DragonianPawnKindDefOf.Dragonian_Male;
            }
            else
            {
                kindDef = DragonianPawnKindDefOf.Dragonian_Female;
            }
            Pawn pawn = PawnGenerator.GeneratePawn(kindDef, faction);
            pawn.SetFaction(null, null);
            GenSpawn.Spawn(pawn, loc, map, WipeMode.Vanish);
            pawn.kindDef = PawnKindDefOf.WildMan;
            string label = __instance.def.letterLabel.Formatted(pawn.LabelShort, pawn.Named("PAWN"));
            string text = __instance.def.letterText.Formatted(pawn.LabelShort, pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN").CapitalizeFirst();
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref label, pawn);
            Find.LetterStack.ReceiveLetter(label, text, __instance.def.letterDef, pawn, null, null);
            __result = true;
            return false;
        }

        public static bool ChangeKindPrefix(Pawn __instance, PawnKindDef newKindDef)
        {
            if (__instance.kindDef == PawnKindDefOf.WildMan)
            {
                if (__instance.gender == Gender.Female)
                {
                    __instance.kindDef = DragonianPawnKindDefOf.Dragonian_Female;
                }
                if (__instance.gender == Gender.Male)
                {
                    __instance.kindDef = DragonianPawnKindDefOf.Dragonian_Male;
                }

                return false;
            }
            return newKindDef == PawnKindDefOf.WildMan;
        }


    }
    public class HediffCompProperties_AutoRecovery : HediffCompProperties
    {
        public int tickMultiflier=6000;
        public float healPoint=1;
        public HediffCompProperties_AutoRecovery()
        {
            compClass = typeof(HediffComp_AutoRecovery);
        }
    }

    public class HediffComp_AutoRecovery : HediffComp
    {
        private int ticksToHeal;
        private float healPoint;
        public static Func<Hediff, bool> func;
		public HediffCompProperties_AutoRecovery Props
        {
            get
            {
                return (HediffCompProperties_AutoRecovery)props;
            }
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            ResetTicksToHeal();
        }

        private void ResetTicksToHeal()
        {
            healPoint = Rand.Range(1,3)*Props.healPoint;
            ticksToHeal = Rand.Range(15, 30) * Props.tickMultiflier;
        }
        public override void CompPostTick(ref float severityAdjustment)
        {
            ticksToHeal--;
            if (ticksToHeal <= 0)
            {
                TryHealRandomPermanentWound();
                ResetTicksToHeal();
            }
        }

        private void TryHealRandomPermanentWound()
        {
            IEnumerable<Hediff> hediffs = Pawn.health.hediffSet.hediffs;
            foreach(Hediff hediff in hediffs)
            {
                if(hediff.def.isBad && hediff.IsPermanent())
                {
                    hediff.Severity -= healPoint;
                }
            }

            /*Hediff hediff;
            if (!hediffs.Where(func).TryRandomElement(out hediff))
            {
                return;
            }
            hediff.Severity = 0f;
            if (PawnUtility.ShouldSendNotificationAbout(Pawn))
            {
                Messages.Message("MessagePermanentWoundHealed".Translate(parent.LabelCap, Pawn.LabelShort, hediff.Label, Pawn.Named("PAWN")), Pawn, MessageTypeDefOf.PositiveEvent, true);
            }*/
        }

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref ticksToHeal, "ticksToHeal", 0, false);
        }

        public override string CompDebugString()
        {
            return "ticksToHeal: " + ticksToHeal;
        }
    }
}
