using HarmonyLib;
using RimWorld;
using Verse;

namespace SensibleBedOwnership
{
    // Remove all assigned pawns without unclaiming their bed on their current map necessarily
    [HarmonyPatch(typeof(Building_Bed))]
    [HarmonyPatch("RemoveAllOwners")]
    public static class Patch_Building_Bed
    {
        public static bool Prefix(Building_Bed __instance, bool destroyed)
        {
            for (int i = __instance.OwnersForReading.Count - 1; i >= 0; i--)
            {
                Pawn pawn = __instance.OwnersForReading[i];
                __instance.CompAssignableToPawn.TryUnassignPawn(pawn, false);
                string key = "MessageBedLostAssignment";
                if (destroyed)
                {
                    key = "MessageBedDestroyed";
                }
                Messages.Message(key.Translate(__instance.def, pawn), new LookTargets(new TargetInfo[]
                {
                    __instance,
                    pawn
                }), MessageTypeDefOf.CautionInput, false);
            }

            return false;
        }
    }
}
