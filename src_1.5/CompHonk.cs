using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vehicles;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace SmallVehicleAddons
{
    public class CompHonk : VehicleComp
    {
        public CompProperties_CompHonk Props => props as CompProperties_CompHonk;
        public bool HasOperator => SmallVehicleAddons_Utility.HasOperator(Vehicle);
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo item in base.CompGetGizmosExtra())
            {
                yield return item;
            }
            if (Vehicle.Faction == Faction.OfPlayer && HasOperator && Props.soundDef != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "SmallVehicleAddonsHonk".Translate(),
                    defaultDesc = "SmallVehicleAddonsHonkDesc".Translate(),
                    icon = SmallVehicleAddons_Content.HonkTex,
                    hotKey = KeyBindingDefOf.Command_TogglePower,
                    onHover = delegate
                    {
                        GenDraw.DrawRadiusRing(Vehicle.Position, Props.pawnsScanRadius);
                    },
                    action = delegate
                    {
                        DoHonkHonk();
                    }
                };
            }
        }
        public void DoHonkHonk()
        {
            if (Vehicle != null && Vehicle.Map != null && Vehicle.Spawned)
            {
                Props?.soundDef?.PlayOneShot(SoundInfo.InMap(parent));
                List<Pawn> list = SmallVehicleAddons_Utility.GetNearbyThings<Pawn>(Vehicle.Map, Vehicle.Position, Props.pawnsScanRadius, (Pawn x) => SmallVehicleAddons_Utility.CanPawnBeScaredByHonk(x, Vehicle.Map, Vehicle.Position));
                if (list.NullOrEmpty())
                {
                    return;
                }
                foreach (Pawn item in list)
                {
                    Notify_DangerousPosition(item, Vehicle.Position);
                }
            }
        }
        public void Notify_DangerousPosition(Pawn pawn, IntVec3 cell)
        {
            if (!PawnUtility.PlayerForcedJobNowOrSoon(pawn) && RCellFinder.TryFindDirectFleeDestination(cell, 9f, pawn, out var fleeCell))
            {
                ForcePawnFlee(pawn, fleeCell);
            }
        }
        public void ForcePawnFlee(Pawn pawn, IntVec3 cell)
        {
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            Job job = JobMaker.MakeJob(JobDefOf.Goto, cell);
            job.locomotionUrgency = LocomotionUrgency.Sprint;
            if (pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc))
            {
                MoteMaker.MakeColonistActionOverlay(pawn, ThingDefOf.Mote_ColonistFleeing);
            }
        }
    }
}