using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace SensibleBedOwnership
{
    [HarmonyPatch(typeof(CompAssignableToPawn_Bed))]
    [HarmonyPatch(nameof(CompAssignableToPawn_Bed.TryUnassignPawn))]
    public static class Patch_CompAssignableToPawn_Bed
    {
        public static bool Prefix(CompAssignableToPawn_Bed __instance, ref List<Pawn> ___uninstalledAssignedPawns, Pawn pawn, bool uninstall)
        {
            __instance.ForceRemovePawn(pawn);
            ((Building_Bed)__instance.parent).NotifyRoomAssignedPawnsChanged();
            if (uninstall && !___uninstalledAssignedPawns.Contains(pawn))
            {
                ___uninstalledAssignedPawns.Add(pawn);
            }

            return false;
        }
    }
}
