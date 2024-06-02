using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace SensibleBedOwnership
{
    // Add assign to assignable order for able, controllable pawns
    [HarmonyPatch(typeof(FloatMenuMakerMap))]
    [HarmonyPatch(nameof(FloatMenuMakerMap.ChoicesAtFor))]
    public static class Patch_FloatMenuMakerMap_ChoicesAtFor
    {
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
            if ((bool)typeof(FloatMenuMakerMap).Method("CanTakeOrder").Invoke(null, new object[] { pawn }))
            {
                if (pawn.Downed || (ModsConfig.BiotechActive && pawn.Deathresting))
                {
                    showOption = true;
                }
            }
            else if (pawn.Map.mapPawns.PawnsInFaction(Faction.OfPlayer).Contains(pawn) && pawn.IsNonMutantAnimal)
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
