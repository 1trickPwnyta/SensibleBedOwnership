using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace SensibleBedOwnership
{
    // Unassign pawn from any bed on any map that they are forbidden to sleep in
    [HarmonyPatch(typeof(Pawn_IdeoTracker))]
    [HarmonyPatch(nameof(Pawn_IdeoTracker.SetIdeo))]
    public static class Patch_Pawn_IdeoTracker
    {
        public static void Postfix(Pawn ___pawn)
        {
            foreach (Building_Bed bed in Utility.AllAssignedBeds(___pawn).Where(b => b.CompAssignableToPawn.IdeoligionForbids(___pawn)))
            {
                bed.CompAssignableToPawn.TryUnassignPawn(___pawn);
            }
        }
    }
}
