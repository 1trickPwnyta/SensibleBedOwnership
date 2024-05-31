using HarmonyLib;
using RimWorld;
using Verse;

namespace SensibleBedOwnership
{
    // Unassign one pawn from every bed they share on every map
    [HarmonyPatch(typeof(InteractionWorker_Breakup))]
    [HarmonyPatch(nameof(InteractionWorker_Breakup.Interacted))]
    public static class Patch_InteractionWorker_Breakup
    {
        public static void Postfix(Pawn initiator, Pawn recipient)
        {
            Utility.UnassignBedsAfterBreakup(initiator, recipient);
        }
    }
}
