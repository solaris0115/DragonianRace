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
    [DefOf]
    public static class DragonianJobDefOf
    {
        public static JobDef MilkingDragonian;
        public static JobDef ShearingDragonian;
    }

    public abstract class WorkGiver_GatherDragonianBodyResources : WorkGiver_Scanner
    {
        protected abstract JobDef JobDef { get; }

        protected abstract CompHasGatherableBodyResource GetComp(Pawn animal);

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            foreach (Pawn pawn2 in pawn.Map.mapPawns.FreeColonistsAndPrisonersSpawned)
            {
                if(pawn2.RaceProps.body == DragonianBodyDefOf.Dragonian)
                {
                    yield return pawn2;
                }
            }
            yield break;
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            Pawn pawn2 = thing as Pawn;
            if (pawn2 == null || !pawn2.RaceProps.Humanlike)
            {
                return false;
            }
            CompHasGatherableBodyResource comp = GetComp(pawn2);
            if (comp != null && comp.ActiveAndFull && PawnUtility.CanCasuallyInteractNow(pawn2, false) && pawn2 != pawn)
            {
                LocalTargetInfo localTargetInfo = pawn2;
                if (ReservationUtility.CanReserve(pawn, localTargetInfo, 1, -1, null, forced))
                {
                    return true;
                }
            }
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return new Job(JobDef, t);
        }
    }
    public abstract class JobDriver_GatherDragonianBodyResources : JobDriver
    {
        private float gatherProgress;

        protected const TargetIndex AnimalInd = TargetIndex.A;

        protected abstract float WorkTotal { get; }

        protected abstract CompHasGatherableBodyResource GetComp(Pawn animal);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.gatherProgress, "gatherProgress", 0f, false);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo target = this.job.GetTarget(TargetIndex.A);
            Job job = this.job;
            return ReservationUtility.Reserve(pawn, target, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            ToilFailConditions.FailOnDespawnedNullOrForbidden(this, TargetIndex.A);
            ToilFailConditions.FailOnNotCasualInterruptible(this, TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil wait = new Toil();
            wait.initAction = delegate ()
            {
                Pawn actor = wait.actor;
                Pawn pawn = (Pawn)job.GetTarget(TargetIndex.A).Thing;
                actor.pather.StopDead();
                PawnUtility.ForceWait(pawn, 15000, null, true);
            };
            wait.tickAction = delegate ()
            {
                Pawn actor = wait.actor;
                actor.skills.Learn(SkillDefOf.Animals, 0.13f, false);
                gatherProgress += StatExtension.GetStatValue(actor, StatDefOf.AnimalGatherSpeed, true);
                if (gatherProgress >= WorkTotal)
                {
                    GetComp((Pawn)((Thing)job.GetTarget(TargetIndex.A))).Gathered(this.pawn);
                    actor.jobs.EndCurrentJob(JobCondition.Succeeded, true);
                }
            };
            wait.AddFinishAction(delegate ()
            {
                Pawn pawn = (Pawn)job.GetTarget(TargetIndex.A).Thing;
                if (pawn != null && pawn.CurJobDef == JobDefOf.Wait_MaintainPosture)
                {
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
                }
            });
            ToilFailConditions.FailOnDespawnedOrNull<Toil>(wait, TargetIndex.A);
            ToilFailConditions.FailOnCannotTouch<Toil>(wait, TargetIndex.A, PathEndMode.Touch);
            wait.AddEndCondition(delegate ()
            {
                if (!GetComp((Pawn)((Thing)this.job.GetTarget(TargetIndex.A))).ActiveAndFull)
                {
                    return JobCondition.Incompletable;
                }
                return JobCondition.Ongoing;
            });
            wait.defaultCompleteMode = ToilCompleteMode.Never;
            ToilEffects.WithProgressBar(wait, TargetIndex.A, () => this.gatherProgress / this.WorkTotal, false, -0.5f);
            wait.activeSkill = (() => SkillDefOf.Animals);
            yield return wait;
            yield break;
        }
    }
    
    public class JobDriver_MilkDragonian : JobDriver_GatherDragonianBodyResources
    {
        protected override float WorkTotal
        {
            get
            {
                return 400f;
            }
        }

        protected override CompHasGatherableBodyResource GetComp(Pawn animal)
        {
            return animal.GetComp<CompMilkable>();
        }
    }
    public class WorkGiver_MilkDragonian : WorkGiver_GatherDragonianBodyResources
    {
        protected override JobDef JobDef
        {
            get
            {
                return DragonianJobDefOf.MilkingDragonian;
            }
        }

        protected override CompHasGatherableBodyResource GetComp(Pawn animal)
        {
            return ThingCompUtility.TryGetComp<CompMilkable>(animal);
        }
    }
    
    public class JobDriver_ShearDragonian : JobDriver_GatherDragonianBodyResources
    {
        protected override float WorkTotal
        {
            get
            {
                return 400f;
            }
        }

        protected override CompHasGatherableBodyResource GetComp(Pawn animal)
        {
            return animal.GetComp<CompShearable>();
        }
    }
    public class WorkGiver_ShearDragonian : WorkGiver_GatherDragonianBodyResources
    {
        protected override JobDef JobDef
        {
            get
            {
                return DragonianJobDefOf.ShearingDragonian;
            }
        }

        protected override CompHasGatherableBodyResource GetComp(Pawn animal)
        {
            return ThingCompUtility.TryGetComp<CompShearable>(animal);
        }
    }

}
