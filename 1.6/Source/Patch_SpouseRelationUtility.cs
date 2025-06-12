using HarmonyLib;
using RimWorld;
using Verse;

namespace SensibleBedOwnership
{
    // Unassign one pawn from every bed they share on every map
    [HarmonyPatch(typeof(SpouseRelationUtility))]
    [HarmonyPatch(nameof(SpouseRelationUtility.DoDivorce))]
    public static class Patch_SpouseRelationUtility_DoDivorce
    {
        public static void Postfix(Pawn initiator, Pawn recipient)
        {
            Utility.UnassignBedsAfterBreakup(initiator, recipient);
        }
    }
}
