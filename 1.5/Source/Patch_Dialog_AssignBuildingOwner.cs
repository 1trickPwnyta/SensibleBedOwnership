using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace SensibleBedOwnership
{
    [HarmonyPatch(typeof(Dialog_AssignBuildingOwner))]
    [HarmonyPatch("DrawUnassignedRow")]
    public static class Patch_Dialog_AssignBuildingOwner
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            bool foundIdeoIcon = false;
            bool finished = false;

            Label label = il.DefineLabel();

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Call && instruction.operand is MethodInfo && (MethodInfo)instruction.operand == SensibleBedOwnershipRefs.m_IdeoUIUtility_DoIdeoIcon)
                {
                    foundIdeoIcon = true;
                }

                if (foundIdeoIcon && !finished && instruction.opcode == OpCodes.Brfalse_S)
                {
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, SensibleBedOwnershipRefs.f_Dialog_AssignBuildingOwner_assignable);
                    yield return new CodeInstruction(OpCodes.Call, SensibleBedOwnershipRefs.m_Utility_AssignableParentIsBed);
                    yield return new CodeInstruction(OpCodes.Brtrue_S, label);
                    finished = true;
                    continue;
                }

                if (instruction.opcode == OpCodes.Ldstr && instruction.operand.ToString() == "BuildingAssign")
                {
                    instruction.labels.Add(label);
                }

                yield return instruction;
            }
        }
    }
}
