using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace SensibleBedOwnership
{
    // Return the assigned bed on the pawn's current map
    [HarmonyPatch(typeof(Pawn_Ownership))]
    [HarmonyPatch("get_OwnedBed")]
    public static class Patch_Pawn_Ownership_get_OwnedBed
    {
        public static bool Prefix(Pawn ___pawn, ref Building_Bed __result)
        {
            __result = Utility.AssignedBedOnCurrentMap(___pawn);
            return false;
        }
    }

    // Unclaim the assigned bed, if any, on the pawn's current map
    [HarmonyPatch(typeof(Pawn_Ownership))]
    [HarmonyPatch(nameof(Pawn_Ownership.UnclaimBed))]
    public static class Patch_Pawn_Ownership_UnclaimBed
    {
        public static bool Prefix(Pawn ___pawn, ref bool __result)
        {
            Building_Bed bed = Utility.AssignedBedOnCurrentMap(___pawn);
            __result = false;
            if (bed != null)
            {
                bed.CompAssignableToPawn.TryUnassignPawn(___pawn);
                __result = true;
            }

            return false;
        }
    }

    // Unassign the pawn from a bed on the new bed's map, unassign a pawn from this specific bed to make room for the new pawn
    [HarmonyPatch(typeof(Pawn_Ownership))]
    [HarmonyPatch(nameof(Pawn_Ownership.ClaimBedIfNonMedical))]
    public static class Patch_Pawn_Ownership_ClaimBedIfNonMedical
    {
        public static void Prefix(Building_Bed newBed, Pawn ___pawn)
        {
            if (!newBed.IsOwner(___pawn) && !newBed.Medical && newBed.def != ThingDefOf.DeathrestCasket)
            {
                if (newBed.OwnersForReading.Count == newBed.SleepingSlotsCount)
                {
                    newBed.CompAssignableToPawn.TryUnassignPawn(newBed.OwnersForReading[newBed.OwnersForReading.Count - 1]);
                }
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool finished = false;

            foreach (CodeInstruction instruction in instructions)
            {
                if (!finished && instruction.opcode == OpCodes.Call && (MethodInfo)instruction.operand == SensibleBedOwnershipRefs.m_Pawn_Ownership_UnclaimBed)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, SensibleBedOwnershipRefs.f_Pawn_Ownership_pawn);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Callvirt, SensibleBedOwnershipRefs.m_Thing_get_Map);
                    yield return new CodeInstruction(OpCodes.Call, SensibleBedOwnershipRefs.m_Utility_UnassignBed);
                    continue;
                }

                yield return instruction;
            }
        }
    }

    // Do not unclaim or claim any beds on file load
    [HarmonyPatch(typeof(Pawn_Ownership))]
    [HarmonyPatch(nameof(Pawn_Ownership.ExposeData))]
    public static class Patch_Pawn_Ownership_ExposeData
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Call && (MethodInfo)instruction.operand == SensibleBedOwnershipRefs.m_Pawn_Ownership_UnclaimBed)
                {
                    continue;
                }

                if (instruction.opcode == OpCodes.Call && (MethodInfo)instruction.operand == SensibleBedOwnershipRefs.m_Pawn_Ownership_ClaimBedIfNonMedical)
                {
                    yield return new CodeInstruction(OpCodes.Pop);
                    continue;
                }

                yield return instruction;
            }
        }
    }

    // Unassign all beds and deathrest caskets on all maps
    [HarmonyPatch(typeof(Pawn_Ownership))]
    [HarmonyPatch(nameof(Pawn_Ownership.UnclaimAll))]
    public static class Patch_Pawn_Ownership_UnclaimAll
    {
        public static void Postfix(Pawn ___pawn)
        {
            foreach (Building_Bed bed in Utility.AllAssignedBedsAndDeathrestCaskets(___pawn))
            {
                bed.CompAssignableToPawn.TryUnassignPawn(___pawn);
            }
        }
    }
}
