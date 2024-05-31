using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace SensibleBedOwnership
{
    // Unassign all beds on all maps that the pawn has outgrown
    [HarmonyPatch(typeof(Pawn_AgeTracker))]
    [HarmonyPatch("BirthdayBiological")]
    public static class Patch_Pawn_AgeTracker
    {
        public static void Postfix(Pawn ___pawn)
        {
            foreach (Building_Bed bed in Utility.AllAssignedBeds(___pawn).Where(b => ___pawn.ageTracker.CurLifeStage.bodySizeFactor > b.def.building.bed_maxBodySize))
            {
                bed.CompAssignableToPawn.TryUnassignPawn(___pawn);
            }
        }
    }
}
