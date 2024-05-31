using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace SensibleBedOwnership
{
    [HarmonyPatch(typeof(LovePartnerRelationUtility))]
    [HarmonyPatch(nameof(LovePartnerRelationUtility.GetMostDislikedNonPartnerBedOwner))]
    public static class Patch_LovePartnerRelationUtility_GetMostDislikedNonPartnerBedOwner
    {
        public static bool Prefix(Pawn p, ref Pawn __result)
        {
            Building_Bed bed = Utility.AssignedBedOnCurrentMap(p);
            if (bed != null)
            {
                bed.OwnersForReading.Where(o => o != p && o.Map == p.Map && !LovePartnerRelationUtility.LovePartnerRelationExists(p, o)).TryMinBy(o => p.relations.OpinionOf(o), out __result);
            }
            else
            {
                __result = null;
            }
            
            return false;
        }
    }

    [HarmonyPatch(typeof(LovePartnerRelationUtility))]
    [HarmonyPatch("TryToShareBed_Int")]
    public static class Patch_LovePartnerRelationUtility_TryToShareBed_Int
    {
        public static void Postfix(Pawn bedOwner, Pawn otherPawn, ref bool __result)
        {
            if (!BedUtility.WillingToShareBed(bedOwner, otherPawn))
            {
                __result = false;
            }
            else
            {
                foreach (Building_Bed bed in Utility.AllAssignedBeds(bedOwner).Where(b => b.AnyUnownedSleepingSlot))
                {
                    otherPawn.ownership.ClaimBedIfNonMedical(bed);
                }
                foreach (Building_Bed bed in Utility.AllAssignedBeds(otherPawn).Where(b => b.AnyUnownedSleepingSlot))
                {
                    bedOwner.ownership.ClaimBedIfNonMedical(bed);
                }
                __result = true;
            }
        }
    }
}
