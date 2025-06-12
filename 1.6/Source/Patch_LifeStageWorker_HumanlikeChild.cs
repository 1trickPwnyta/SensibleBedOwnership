using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace SensibleBedOwnership
{
    [HarmonyPatch(typeof(LifeStageWorker_HumanlikeChild))]
    [HarmonyPatch(nameof(LifeStageWorker_HumanlikeChild.Notify_LifeStageStarted))]
    public static class Patch_LifeStageWorker_HumanlikeChild
    {
        public static void Postfix(Pawn pawn)
        {
            foreach (Building_Bed bed in Utility.AllAssignedBeds(pawn).Where(b => pawn.ageTracker.CurLifeStage.bodySizeFactor > b.def.building.bed_maxBodySize))
            {
                bed.CompAssignableToPawn.TryUnassignPawn(pawn);
            }
        }
    }
}
