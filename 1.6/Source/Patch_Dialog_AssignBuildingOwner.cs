using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.Steam;

namespace SensibleBedOwnership
{
    // Make room for search box, remove extra space at bottom from counting assigned pawns twice and filtering, and add search box
    [HarmonyPatch(typeof(Dialog_AssignBuildingOwner))]
    [HarmonyPatch(nameof(Dialog_AssignBuildingOwner.DoWindowContents))]
    public static class Patch_Dialog_AssignBuildingOwner_DoWindowContents
    {
        private static Dialog_AssignBuildingOwner focused = null;

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool madeRoomForSearch = false;

            foreach (CodeInstruction instruction in instructions)
            {
                if (!madeRoomForSearch && instruction.operand is float && (float)instruction.operand == 20f)
                {
                    instruction.operand = 20f + Window.QuickSearchSize.y + 6f;
                    madeRoomForSearch = true;
                }

                if (instruction.opcode == OpCodes.Callvirt && (MethodInfo)instruction.operand == SensibleBedOwnershipRefs.m_CompAssignableToPawn_get_AssigningCandidates)
                {
                    yield return new CodeInstruction(OpCodes.Call, SensibleBedOwnershipRefs.m_Utility_AssigningCandidatesMatchingFilterNotAlreadyAssigned);
                    continue;
                }

                yield return instruction;
            }
        }

        public static void Postfix(Dialog_AssignBuildingOwner __instance, CompAssignableToPawn ___assignable, Rect inRect)
        {
            Utility.AssignOwnerSearchWidget.OnGUI(new Rect(inRect.x, inRect.y + 20f, Window.QuickSearchSize.x, Window.QuickSearchSize.y));
            if (!SteamDeck.IsSteamDeck && focused != __instance)
            {
                Utility.AssignOwnerSearchWidget.Focus();
                focused = __instance;
            }
        }
    }

    // Show pawn relationships
    [HarmonyPatch(typeof(Dialog_AssignBuildingOwner))]
    [HarmonyPatch("DrawAssignedRow")]
    public static class Patch_Dialog_AssignBuildingOwner_DrawAssignedRow
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Callvirt && (MethodInfo)instruction.operand == SensibleBedOwnershipRefs.m_Entity_get_LabelCap)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, SensibleBedOwnershipRefs.f_Dialog_AssignBuildingOwner_assignable);
                    yield return new CodeInstruction(OpCodes.Call, SensibleBedOwnershipRefs.m_Utility_LabelCapWithRelation);
                    continue;
                }

                yield return instruction;
            }
        }
    }

    // Don't draw a row for anyone not matching the search filter, show pawn relations
    [HarmonyPatch(typeof(Dialog_AssignBuildingOwner))]
    [HarmonyPatch("DrawUnassignedRow")]
    public static class Patch_Dialog_AssignBuildingOwner_DrawUnassignedRow
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Callvirt && (MethodInfo)instruction.operand == SensibleBedOwnershipRefs.m_Entity_get_LabelCap)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, SensibleBedOwnershipRefs.f_Dialog_AssignBuildingOwner_assignable);
                    yield return new CodeInstruction(OpCodes.Call, SensibleBedOwnershipRefs.m_Utility_LabelCapWithRelation);
                    continue;
                }

                if (instruction.opcode == OpCodes.Call && instruction.operand is MethodInfo && (MethodInfo)instruction.operand == SensibleBedOwnershipRefs.m_SoundStarter_PlayOneShotOnCamera)
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld, SensibleBedOwnershipRefs.f_Utility_AssignOwnerSearchWidget);
                    yield return new CodeInstruction(OpCodes.Call, SensibleBedOwnershipRefs.m_QuickSearchWidget_Reset);
                }

                yield return instruction;
            }
        }
    }

    // Reset the search box
    // Patched manually in mod constructor
    public static class Patch_Dialog_AssignBuildingOwner_ctor
    {
        public static void Postfix()
        {
            Utility.AssignOwnerSearchWidget.Reset();
        }
    }

    // Sort lovers to the top of the list
    [HarmonyPatch(typeof(Dialog_AssignBuildingOwner))]
    [HarmonyPatch("SortTmpList")]
    public static class Patch_Dialog_AssignBuildingOwner_SortTmpList
    {
        public static bool Prefix(CompAssignableToPawn ___assignable, List<Pawn> ___tmpPawnSorted, IEnumerable<Pawn> collection)
        {
            ___tmpPawnSorted.Clear();
            ___tmpPawnSorted.AddRange(collection.Where(p => ___assignable.AssignedPawnsForReading.Any(pawn => LovePartnerRelationUtility.ExistingLoveRealtionshipBetween(p, pawn, false) != null)).OrderBy(p => p.LabelShort));
            ___tmpPawnSorted.AddRange(collection.Where(p => !___tmpPawnSorted.Contains(p)).OrderBy(p => p.LabelShort));
            return false;
        }
    }
}
