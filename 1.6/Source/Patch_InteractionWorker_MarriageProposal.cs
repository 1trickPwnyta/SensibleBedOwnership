using HarmonyLib;
using RimWorld;
using Verse;

namespace SensibleBedOwnership
{
    // Unassign one pawn from every bed they share on every map if they broke up
    [HarmonyPatch(typeof(InteractionWorker_MarriageProposal))]
    [HarmonyPatch(nameof(InteractionWorker_MarriageProposal.Interacted))]
    public static class Patch_InteractionWorker_MarriageProposal
    {
        public static void Postfix(Pawn initiator, Pawn recipient)
        {
            if (!LovePartnerRelationUtility.LovePartnerRelationExists(initiator, recipient))
            {
                Utility.UnassignBedsAfterBreakup(initiator, recipient);
            }
        }
    }
}
