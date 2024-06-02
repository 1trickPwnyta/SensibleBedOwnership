using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace SensibleBedOwnership
{
    [HarmonyPatch(typeof(CompAssignableToPawn))]
    [HarmonyPatch(nameof(CompAssignableToPawn.CompGetGizmosExtra))]
    public static class Patch_CompAssignableToPawn
    {
        public static void Postfix(CompAssignableToPawn __instance, ref IEnumerable<Gizmo> __result)
        {
            if ((bool)typeof(CompAssignableToPawn).Method("ShouldShowAssignmentGizmo").Invoke(__instance, new object[] { }))
            {
                Command_Action action = new Command_Action();
                action.defaultLabel = "SensibleBedOwnership_UnassignAll".Translate();
                action.defaultDesc = "SensibleBedOwnership_UnassignAllDesc".Translate();
                action.icon = ContentFinder<Texture2D>.Get("UI/UnassignAll", true);
                action.action = delegate ()
                {
                    for (int i = __instance.AssignedPawnsForReading.Count - 1; i >= 0; i--)
                    {
                        Pawn pawn = __instance.AssignedPawnsForReading[i];
                        __instance.TryUnassignPawn(pawn, false);
                    }
                    SoundDefOf.Click.PlayOneShotOnCamera();
                };
                if (__instance.AssignedPawnsForReading.Empty())
                {
                    action.Disable();
                }

                __result = __result.AddItem(action);
            }
        }
    }
}
