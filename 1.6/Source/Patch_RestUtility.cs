using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace SensibleBedOwnership
{
    // Use lover's bed on your own map instead of lover's map
    // Replace all calls to get_OwnedBed with GetMainBed because get_OwnedBed gets inlined when this method is patched in any manner
    [HarmonyPatch(typeof(RestUtility))]
    [HarmonyPatch(nameof(RestUtility.FindBedFor))]
    [HarmonyPatch(new[] { typeof(Pawn), typeof(Pawn), typeof(bool), typeof(bool), typeof(GuestStatus?) })]
    public static class Patch_RestUtility
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool foundLover = false;
            bool finished = false;

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Call && (MethodInfo)instruction.operand == SensibleBedOwnershipRefs.m_LovePartnerRelationUtility_ExistingMostLikedLovePartnerRel)
                {
                    foundLover = true;
                }

                if (instruction.opcode == OpCodes.Callvirt && (MethodInfo)instruction.operand == SensibleBedOwnershipRefs.m_Pawn_Ownership_get_OwnedBed)
                {
                    if (foundLover && !finished)
                    {
                        yield return new CodeInstruction(OpCodes.Pop);
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
                        yield return new CodeInstruction(OpCodes.Ldfld, SensibleBedOwnershipRefs.f_DirectPawnRelation_otherPawn);
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Callvirt, SensibleBedOwnershipRefs.m_Thing_get_Map);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                        yield return new CodeInstruction(OpCodes.Call, SensibleBedOwnershipRefs.m_Utility_AssignedBed);
                        finished = true;
                    }
                    else
                    {
                        yield return new CodeInstruction(OpCodes.Call, typeof(Utility).Method(nameof(Utility.GetMainBed), new[] { typeof(Pawn_Ownership) }));
                    }
                    continue;
                }

                yield return instruction;
            }
        }
    }
}
