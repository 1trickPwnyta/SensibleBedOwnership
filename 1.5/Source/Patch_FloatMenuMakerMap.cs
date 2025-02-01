using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace SensibleBedOwnership
{
    // Make smarter map check, add assign to assignable order for able, controllable pawns
    [HarmonyPatch(typeof(FloatMenuMakerMap))]
    [HarmonyPatch(nameof(FloatMenuMakerMap.ChoicesAtFor))]
    public static class Patch_FloatMenuMakerMap_ChoicesAtFor
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            LocalBuilder mapLocal = il.DeclareLocal(typeof(Map));

            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Call, SensibleBedOwnershipRefs.m_Utility_GetMapForFloatMenu);
            yield return new CodeInstruction(OpCodes.Stloc_S, mapLocal);

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Callvirt && (MethodInfo)instruction.operand == SensibleBedOwnershipRefs.m_Thing_get_Map)
                {
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, mapLocal);
                    continue;
                }

                yield return instruction;
            }
        }

        public static void Postfix(Vector3 clickPos, Pawn pawn, ref List<FloatMenuOption> __result)
        {
            FloatMenuOption option = Utility.GetAssignToAssignableOption(clickPos, pawn);
            if (option != null)
            {
                __result.Add(option);
            }
        }
    }

    // Add assign to assignable command for disabled pawns and colony animals
    [HarmonyPatch(typeof(FloatMenuMakerMap))]
    [HarmonyPatch(nameof(FloatMenuMakerMap.TryMakeFloatMenu))]
    public static class Patch_FloatMenuMakerMap_TryMakeFloatMenu
    {
        public static void Postfix(Pawn pawn)
        {
            bool showOption = false;
            if (pawn.IsColonist)
            {
                Lord lord;
                if (!pawn.Spawned || pawn.Downed || pawn.InMentalState || (ModsConfig.BiotechActive && pawn.Deathresting) || (pawn.TryGetLord(out lord) && !lord.AllowsFloatMenu(pawn)))
                {
                    showOption = true;
                }
            }
            else if (pawn.IsSlaveOfColony)
            {
                showOption = true;
            }
            else if (pawn.GetMapForFloatMenu().mapPawns.PawnsInFaction(Faction.OfPlayer).Contains(pawn) && pawn.IsNonMutantAnimal)
            {
                showOption = true;
            }

            if (showOption)
            {
                FloatMenuOption option = Utility.GetAssignToAssignableOption(UI.MouseMapPosition(), pawn);
                if (option != null)
                {
                    Find.WindowStack.Add(new FloatMenuMap(new List<FloatMenuOption> { option }, pawn.LabelCap, UI.MouseMapPosition()));
                }
            }
        }
    }
}
