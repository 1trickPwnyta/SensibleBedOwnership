using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
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
            __result = Utility.GetMainBed(___pawn);
            return false;
        }
    }

    // Return the assigned deathrest casket on the pawn's current map
    [HarmonyPatch(typeof(Pawn_Ownership))]
    [HarmonyPatch("get_AssignedDeathrestCasket")]
    public static class Patch_Pawn_Ownership_get_AssignedDeathrestCasket
    {
        public static bool Prefix(Pawn ___pawn, ref Building_Bed __result)
        {
            __result = Utility.AssignedDeathrestCasketOnCurrentMap(___pawn);
            return false;
        }
    }

    // Unassign the pawn from a bed/deathrest casket on the new bed's map, unassign a preferably-non-lover pawn from this specific bed to make room for the new pawn
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
                    newBed.CompAssignableToPawn.TryUnassignPawn(newBed.OwnersForReading.Where(o => LovePartnerRelationUtility.ExistingLoveRealtionshipBetween(___pawn, o, false) == null).FirstOrFallback(newBed.OwnersForReading[newBed.OwnersForReading.Count - 1]));
                }
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool unassignedDeathrestCasket = false;
            bool unassignedBed = false;

            foreach (CodeInstruction instruction in instructions)
            {
                if (!unassignedDeathrestCasket && instruction.opcode == OpCodes.Call && (MethodInfo)instruction.operand == SensibleBedOwnershipRefs.m_Pawn_Ownership_UnclaimDeathrestCasket)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, SensibleBedOwnershipRefs.f_Pawn_Ownership_pawn);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Callvirt, SensibleBedOwnershipRefs.m_Thing_get_Map);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, SensibleBedOwnershipRefs.m_Utility_UnassignDeathrestCasket);
                    unassignedDeathrestCasket = true;
                    continue;
                }

                if (!unassignedBed && instruction.opcode == OpCodes.Call && (MethodInfo)instruction.operand == SensibleBedOwnershipRefs.m_Pawn_Ownership_UnclaimBed)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, SensibleBedOwnershipRefs.f_Pawn_Ownership_pawn);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Callvirt, SensibleBedOwnershipRefs.m_Thing_get_Map);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, SensibleBedOwnershipRefs.m_Utility_UnassignBed);
                    unassignedBed = true;
                    continue;
                }

                yield return instruction;
            }
        }
    }

    // Unassign the pawn from a deathrest casket on the new casket's map, unassign a pawn from this specific casket to make room for the new pawn
    [HarmonyPatch(typeof(Pawn_Ownership))]
    [HarmonyPatch(nameof(Pawn_Ownership.ClaimDeathrestCasket))]
    public static class Patch_Pawn_Ownership_ClaimDeathrestCasket
    {
        public static void Prefix(Building_Bed deathrestCasket, Pawn ___pawn)
        {
            if (deathrestCasket.GetAssignedPawn() != null)
            {
                deathrestCasket.CompAssignableToPawn.TryUnassignPawn(deathrestCasket.GetAssignedPawn());
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool unassignedDeathrestCasket = false;

            foreach (CodeInstruction instruction in instructions)
            {
                if (!unassignedDeathrestCasket && instruction.opcode == OpCodes.Call && (MethodInfo)instruction.operand == SensibleBedOwnershipRefs.m_Pawn_Ownership_UnclaimDeathrestCasket)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, SensibleBedOwnershipRefs.f_Pawn_Ownership_pawn);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Callvirt, SensibleBedOwnershipRefs.m_Thing_get_Map);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, SensibleBedOwnershipRefs.m_Utility_UnassignDeathrestCasket);
                    unassignedDeathrestCasket = true;
                    continue;
                }

                yield return instruction;
            }
        }
    }

    // Do not unclaim or claim any beds on file load and save/load preferred bed
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

        public static void Postfix(Pawn ___pawn)
        {
            Building_Bed preferredBed = ___pawn.GetPreferredBed();
            if (Scribe.mode == LoadSaveMode.Saving && preferredBed.DestroyedOrNull())
            {
                preferredBed = null;
            }
            Scribe_References.Look(ref preferredBed, "preferredBed");
            ___pawn.SetPreferredBed(preferredBed);
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

    // Unclaim all beds and deathrest caskets instead of just the one on the current map
    [HarmonyPatch(typeof(Pawn_Ownership))]
    [HarmonyPatch(nameof(Pawn_Ownership.Notify_ChangedGuestStatus))]
    public static class Patch_Pawn_Ownership_Notify_ChangedGuestStatus
    {
        public static void Postfix(Pawn ___pawn)
        {
            foreach (Building_Bed bed in Utility.AllAssignedBedsAndDeathrestCaskets(___pawn))
            {
                if ((bed.ForPrisoners && !___pawn.IsPrisoner && !PawnUtility.IsBeingArrested(___pawn)) || (!bed.ForPrisoners && ___pawn.IsPrisoner) || (bed.ForColonists && ___pawn.HostFaction == null))
                {
                    bed.CompAssignableToPawn.TryUnassignPawn(___pawn);
                }
            }
        }
    }
}
