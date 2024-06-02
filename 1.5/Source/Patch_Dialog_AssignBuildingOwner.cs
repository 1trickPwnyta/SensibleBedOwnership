using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace SensibleBedOwnership
{
    // Make room for search box, remove extra space at bottom from counting assigned pawns twice and filtering, and add search box
    [HarmonyPatch(typeof(Dialog_AssignBuildingOwner))]
    [HarmonyPatch(nameof(Dialog_AssignBuildingOwner.DoWindowContents))]
    public static class Patch_Dialog_AssignBuildingOwner_DoWindowContents
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool madeRoomForSearch = false;
            bool removedExtraSpace = false;

            foreach (CodeInstruction instruction in instructions)
            {
                if (!madeRoomForSearch && instruction.operand is float && (float)instruction.operand == 20f)
                {
                    instruction.operand = 20f + Window.QuickSearchSize.y;
                    madeRoomForSearch = true;
                }

                if (!removedExtraSpace && instruction.opcode == OpCodes.Callvirt && (MethodInfo)instruction.operand == SensibleBedOwnershipRefs.m_CompAssignableToPawn_get_AssigningCandidates)
                {
                    yield return new CodeInstruction(OpCodes.Call, SensibleBedOwnershipRefs.m_Utility_AssigningCandidatesMatchingFilterNotAlreadyAssigned);
                    removedExtraSpace = true;
                    continue;
                }

                yield return instruction;
            }
        }

        public static void Postfix(Rect inRect)
        {
            Utility.AssignOwnerSearchWidget.OnGUI(new Rect(inRect.x, inRect.y + 20f, Window.QuickSearchSize.x, Window.QuickSearchSize.y));
            Utility.AssignOwnerSearchWidget.Focus();
        }
    }

    // Don't draw a row for anyone not matching the search filter
    [HarmonyPatch(typeof(Dialog_AssignBuildingOwner))]
    [HarmonyPatch("DrawUnassignedRow")]
    public static class Patch_Dialog_AssignBuildingOwner_DrawUnassignedRow
    {
        public static bool Prefix(CompAssignableToPawn ___assignable, Pawn pawn)
        {
            return Utility.AssignOwnerSearchWidget.filter.Matches(pawn.Name.ToStringShort);
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
}
