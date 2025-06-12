using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

namespace SensibleBedOwnership
{
    [HarmonyPatch(typeof(JobGiver_GetRest))]
    [HarmonyPatch("TryFindGroundSleepSpotFor")]
    public static class Patch_JobGiver_GetRest
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool finished = false;

            foreach (CodeInstruction instruction in instructions)
            {
                if (!finished && instruction.opcode == OpCodes.Stloc_2)
                {
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldloca, 2);
                    yield return new CodeInstruction(OpCodes.Call, typeof(PatchUtility_JobGiver_GetRest).Method(nameof(PatchUtility_JobGiver_GetRest.TryFindComfortableGroundSleepSpotFor)));
                    finished = true;
                    continue;
                }

                yield return instruction;
            }
        }
    }

    public static class PatchUtility_JobGiver_GetRest
    {
        public static void TryFindComfortableGroundSleepSpotFor(Pawn pawn, ref IntVec3 cell)
        {
            if (SensibleBedOwnershipSettings.SleepInChairs && pawn.needs.comfort != null)
            {
                Predicate<IntVec3> validator = (IntVec3 c) =>
                {
                    Building building = c.GetEdifice(pawn.Map);
                    if (building != null)
                    {
                        bool isValidCell = (bool)typeof(JobGiver_GetRest).Method("IsValidCell").Invoke(null, new object[] { pawn, c });
                        return isValidCell && building.def.building?.isSittable == true && building.GetStatValue(StatDefOf.Comfort) > 0f;
                    }
                    return false;
                };
                for (int i = 0; i < 2; i++)
                {
                    int radius = (i == 0) ? 4 : 12;
                    IntVec3 randomCell;
                    if (CellFinder.TryRandomClosewalkCellNear(pawn.Position, pawn.Map, radius, out randomCell, validator))
                    {
                        cell = randomCell;
                        return;
                    }
                }
            }
        }
    }
}
