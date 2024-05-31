using HarmonyLib;
using RimWorld;
using Verse;

namespace SensibleBedOwnership
{
    // Notify room assigned pawns changed for all assigned beds on all maps
    [HarmonyPatch(typeof(CompAssignableToPawn_DeathrestCasket))]
    [HarmonyPatch(nameof(CompAssignableToPawn_DeathrestCasket.TryUnassignPawn))]
    public static class Patch_CompAssignableToPawn_DeathrestCasket
    {
        public static void Postfix(Pawn pawn)
        {
            foreach (Building_Bed bed in Utility.AllAssignedBeds(pawn))
            {
                bed.NotifyRoomAssignedPawnsChanged();
            }
        }
    }
}
