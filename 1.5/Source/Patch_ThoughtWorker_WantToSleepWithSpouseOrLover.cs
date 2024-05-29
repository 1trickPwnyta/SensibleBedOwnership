using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace SensibleBedOwnership
{
    [HarmonyPatch(typeof(ThoughtWorker_WantToSleepWithSpouseOrLover))]
    [HarmonyPatch("CurrentStateInternal")]
    public static class Patch_ThoughtWorker_WantToSleepWithSpouseOrLover
    {
        public static void Postfix(Pawn p, ref ThoughtState __result)
        {
            if (__result.Active)
            {
                if (Utility.AllAssignedBeds(p).Any(b => b.CompAssignableToPawn.AssignedPawnsForReading.Intersect(LovePartnerRelationUtility.ExistingLovePartners(p, false).Select(r => r.otherPawn)).Any()))
                {
                    __result = false;
                }
            }
        }
    }
}
