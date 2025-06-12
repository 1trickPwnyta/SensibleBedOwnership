using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace SensibleBedOwnership
{
    // Ensure float menu option providers are queried even when no valid pawns are selected, since we also care about invalid pawns
    [HarmonyPatch(typeof(FloatMenuMakerMap))]
    [HarmonyPatch(nameof(FloatMenuMakerMap.GetOptions))]
    public static class Patch_FloatMenuMakerMap_GetOptions
    {
        public static bool Prefix(List<Pawn> selectedPawns, Vector3 clickPos, ref FloatMenuContext context, ref List<FloatMenuOption> __state)
        {
            __state = new List<FloatMenuOption>();
            if (!selectedPawns.Any(p => FloatMenuMakerMap.ShouldGenerateFloatMenuForPawn(p) && p.CanTakeOrder) && Find.Selector.SelectedPawns.Count > 0)
            {
                context = new FloatMenuContext(selectedPawns, clickPos, Find.CurrentMap);
                if (context.ClickedCell.IsValid && context.ClickedCell.InBounds(Find.CurrentMap))
                {
                    List<FloatMenuOptionProvider> providers = typeof(FloatMenuMakerMap).Field("providers").GetValue(null) as List<FloatMenuOptionProvider>;
                    FloatMenuOptionProvider assignProvider = providers.Where(p => p is FloatMenuOptionProvider_AssignToAssignable).First();
                    foreach (Thing thing in context.ClickedThings)
                    {
                        foreach (FloatMenuOption option in assignProvider.GetOptionsFor(thing, context))
                        {
                            option.iconThing = thing;
                            __state.Add(option);
                        }
                    }
                }
                return false;
            }
            return true;
        }

        public static void Postfix(List<Pawn> selectedPawns, ref List<FloatMenuOption> __state, ref List<FloatMenuOption> __result)
        {
            if (__state.NullOrEmpty() && selectedPawns.Count == 1)
            {
                AcceptanceReport report = FloatMenuMakerMap.ShouldGenerateFloatMenuForPawn(selectedPawns[0]);
                if (!report.Accepted)
                {
                    if (!report.Reason.NullOrEmpty())
                    {
                        Messages.Message(report.Reason, selectedPawns[0], MessageTypeDefOf.RejectInput, false);
                    }
                }
            }
            if (__result == null)
            {
                __result = new List<FloatMenuOption>();
            }
            __result.AddRange(__state);
        }
    }
}
