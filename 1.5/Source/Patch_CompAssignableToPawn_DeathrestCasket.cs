using HarmonyLib;
using RimWorld;
using Verse;

namespace SensibleBedOwnership
{
    // Unassign the pawn from the bed attached to the comp, notify room assigned pawns changed for all assigned beds on all maps
    [HarmonyPatch(typeof(CompAssignableToPawn_DeathrestCasket))]
    [HarmonyPatch(nameof(CompAssignableToPawn_DeathrestCasket.TryUnassignPawn))]
    public static class Patch_CompAssignableToPawn_DeathrestCasket_TryUnassignPawn
    {
        public static bool Prefix(CompAssignableToPawn_DeathrestCasket __instance, Pawn pawn)
        {
            __instance.ForceRemovePawn(pawn);
            foreach (Building_Bed bed in Utility.AllAssignedBeds(pawn))
            {
                bed.NotifyRoomAssignedPawnsChanged();
            }

            return false;
        }
    }

    // Don't cross-reference assigned pawns with their ownership since we don't track it separately
    [HarmonyPatch(typeof(CompAssignableToPawn_DeathrestCasket))]
    [HarmonyPatch("PostPostExposeData")]
    public static class Patch_CompAssignableToPawn_DeathrestCasket_PostPostExposeData
    {
        public static bool Prefix()
        {
            return false;
        }
    }
}
