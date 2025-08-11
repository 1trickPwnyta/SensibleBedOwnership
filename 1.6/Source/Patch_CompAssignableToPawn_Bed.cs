using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace SensibleBedOwnership
{
    // Unassign the pawn from the bed attached to the comp, notify room assigned pawns changed for all assigned beds on all maps
    [HarmonyPatch(typeof(CompAssignableToPawn_Bed))]
    [HarmonyPatch(nameof(CompAssignableToPawn_Bed.TryUnassignPawn))]
    public static class Patch_CompAssignableToPawn_Bed_TryUnassignPawn
    {
        public static bool Prefix(CompAssignableToPawn_Bed __instance, ref List<Pawn> ___uninstalledAssignedPawns, Pawn pawn, bool uninstall)
        {
            __instance.ForceRemovePawn(pawn);
            ((Building_Bed)__instance.parent).NotifyRoomAssignedPawnsChanged();
            foreach (Building_Bed bed in Utility.AllAssignedBeds(pawn))
            {
                bed.NotifyRoomAssignedPawnsChanged();
            }
            if (uninstall && !___uninstalledAssignedPawns.Contains(pawn))
            {
                ___uninstalledAssignedPawns.Add(pawn);
            }

            return false;
        }
    }

    // Only count a pawn as assigned anything if on the same map
    [HarmonyPatch(typeof(CompAssignableToPawn_Bed))]
    [HarmonyPatch(nameof(CompAssignableToPawn_Bed.AssignedAnything))]
    public static class Patch_CompAssignableToPawn_Bed_AssignedAnything
    {
        public static bool Prefix(Pawn pawn, ref bool __result)
        {
            __result = Utility.AssignedBedOnCurrentMap(pawn) != null;
            return false;
        }
    }

    // Don't cross-reference assigned pawns with their ownership since we don't track it separately
    [HarmonyPatch(typeof(CompAssignableToPawn_Bed))]
    [HarmonyPatch("PostPostExposeData")]
    public static class Patch_CompAssignableToPawn_Bed_PostPostExposeData
    {
        public static bool Prefix()
        {
            return false;
        }
    }
}
