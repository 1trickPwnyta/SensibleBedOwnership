using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace SensibleBedOwnership
{
    // Add unassign all gizmo
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(CompAssignableToPawn))]
    [HarmonyPatch(nameof(CompAssignableToPawn.CompGetGizmosExtra))]
    public static class Patch_CompAssignableToPawn
    {
        private static Texture2D unassignAllIcon = ContentFinder<Texture2D>.Get("UI/UnassignAll");
        private static Texture2D preferredBedIcon = ContentFinder<Texture2D>.Get("UI/Preferred");

        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> gizmos, CompAssignableToPawn __instance)
        {
            foreach (Gizmo gizmo in gizmos)
            {
                yield return gizmo;
            }

            if ((bool)typeof(CompAssignableToPawn).Method("ShouldShowAssignmentGizmo").Invoke(__instance, new object[] { }))
            {
                Command_Action action = new Command_Action();
                action.defaultLabel = "SensibleBedOwnership_UnassignAll".Translate();
                action.defaultDesc = "SensibleBedOwnership_UnassignAllDesc".Translate();
                action.icon = unassignAllIcon;
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
                yield return action;

                if (__instance.parent is Building_Bed bed && Find.Selector.SingleSelectedThing == bed)
                {
                    foreach (Pawn pawn in __instance.AssignedPawnsForReading.Where(p => Utility.AllAssignedBedsAndDeathrestCaskets(p).Count > 1))
                    {
                        Command_Toggle toggle = new Command_Toggle();
                        toggle.defaultLabel = "SensibleBedOwnership_PreferredBed".Translate(pawn.LabelShortCap);
                        toggle.defaultDesc = "SensibleBedOwnership_PreferredBedDesc".Translate();
                        toggle.icon = preferredBedIcon;
                        toggle.isActive = () => pawn.GetPreferredBed() == bed;
                        toggle.toggleAction = () =>
                        {
                            if (pawn.GetPreferredBed() == bed)
                            {
                                pawn.SetPreferredBed(null);
                            }
                            else
                            {
                                pawn.SetPreferredBed(bed);
                            } 
                        };
                        yield return toggle;
                    }
                }
            }
        }
    }
}
