using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace SensibleBedOwnership
{
    [HarmonyPatch(typeof(Pawn_Ownership))]
    [HarmonyPatch("get_OwnedBed")]
    public static class Patch_Pawn_Ownership_get_OwnedBed
    {
        public static bool Prefix(Pawn ___pawn, ref Building_Bed __result)
        {
            __result = Utility.AllAssignedBeds(___pawn).FirstOrFallback();

            return false;
        }
    }

    [HarmonyPatch(typeof(Pawn_Ownership))]
    [HarmonyPatch(nameof(Pawn_Ownership.UnclaimBed))]
    public static class Patch_Pawn_Ownership_UnclaimBed
    {
        public static bool Prefix(Pawn ___pawn, ref bool __result)
        {
            List<Building_Bed> beds = Utility.AllAssignedBeds(___pawn);
            __result = false;
            foreach (Building_Bed bed in beds)
            {
                bed.CompAssignableToPawn.ForceRemovePawn(___pawn);
                __result = true;
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(Pawn_Ownership))]
    [HarmonyPatch(nameof(Pawn_Ownership.ClaimBedIfNonMedical))]
    public static class Patch_Pawn_Ownership_ClaimBedIfNonMedical
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool finished = false;

            foreach (CodeInstruction instruction in instructions)
            {
                if (!finished && instruction.opcode == OpCodes.Call && (MethodInfo)instruction.operand == SensibleBedOwnershipRefs.m_Pawn_Ownership_UnclaimBed)
                {
                    finished = true;
                    continue;
                }

                yield return instruction;
            }
        }
    }
}
