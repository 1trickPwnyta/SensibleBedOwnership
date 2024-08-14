using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace SensibleBedOwnership
{
    // Unassign one pawn from every bed they share on every map
    [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt))]
    [HarmonyPatch("BreakLoverAndFianceRelations")]
    public static class Patch_InteractionWorker_RomanceAttempt
    {
        public static void Postfix(Pawn pawn, List<Pawn> oldLoversAndFiances)
        {
            foreach (Pawn other in oldLoversAndFiances)
            {
                Utility.UnassignBedsAfterBreakup(pawn, other);
            }
        }
    }
}
