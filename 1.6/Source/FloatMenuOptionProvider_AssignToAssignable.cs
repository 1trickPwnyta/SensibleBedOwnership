using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace SensibleBedOwnership
{
    public class FloatMenuOptionProvider_AssignToAssignable : FloatMenuOptionProvider
    {
        protected override bool Drafted => true;

        protected override bool Undrafted => true;

        protected override bool Multiselect => true;

        protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
        {
            CompAssignableToPawn comp = clickedThing.TryGetComp<CompAssignableToPawn>();
            if (comp != null)
            {
                if ((bool)typeof(CompAssignableToPawn).Method("ShouldShowAssignmentGizmo").Invoke(comp, new object[] { }))
                {
                    HashSet<Pawn> pawns = Find.Selector.SelectedPawns.Where(p => p.IsFreeColonist).ToHashSet();
                    if (pawns.Count > 1)
                    {
                        pawns.RemoveWhere(p => !comp.CanAssignTo(p));
                        if (pawns.Count > 1 && comp.TotalSlots >= pawns.Count)
                        {
                            List<string> strings = pawns.Select(p => p.Name.ToStringShort).ToList();
                            strings[strings.Count - 1] = "SensibleBedOwnership_And".Translate() + " " + strings[strings.Count - 1];
                            string pawnsList = strings.Count > 2 ? string.Join(", ", strings) : string.Join(" ", strings);
                            foreach (Pawn pawn1 in pawns)
                            {
                                bool forbid = comp.IdeoligionForbids(pawn1);
                                if (!forbid)
                                {
                                    foreach (Pawn pawn2 in pawns)
                                    {
                                        if (pawn1 != pawn2 && !BedUtility.WillingToShareBed(pawn1, pawn2))
                                        {
                                            forbid = true;
                                            break;
                                        }
                                    }
                                }
                                if (forbid)
                                {
                                    return new FloatMenuOption("SensibleBedOwnership_CannotAssignPawnToAssignable".Translate(pawnsList, clickedThing.LabelCap) + ": " + "IdeoligionForbids".Translate(), null);
                                }
                            }

                            if (pawns.Any(p => !comp.AssignedPawnsForReading.Contains(p)))
                            {
                                return new FloatMenuOption("SensibleBedOwnership_AssignPawnToAssignable".Translate(pawnsList, clickedThing.LabelCap), () =>
                                {
                                    while (comp.TotalSlots - comp.AssignedPawnsForReading.Count < pawns.Count - comp.AssignedPawnsForReading.Where(p => pawns.Contains(p)).Count())
                                    {
                                        comp.TryUnassignPawn(comp.AssignedPawnsForReading[comp.AssignedPawnsForReading.Count - 1]);
                                    }
                                    foreach (Pawn pawn in pawns)
                                    {
                                        comp.TryAssignPawn(pawn);
                                    }
                                });
                            }
                        }
                    }

                    if (pawns.Count == 1)
                    {
                        Pawn pawn = pawns.First();
                        if (!comp.AssignedPawnsForReading.Contains(pawn))
                        {
                            AcceptanceReport report = comp.CanAssignTo(pawn);
                            if (!report.Accepted)
                            {
                                return new FloatMenuOption("SensibleBedOwnership_CannotAssignPawnToAssignable".Translate(pawn.Name.ToStringShort, clickedThing.LabelCap) + ": " + report.Reason.StripTags(), null);
                            }
                            else if (comp.IdeoligionForbids(pawn))
                            {
                                return new FloatMenuOption("SensibleBedOwnership_CannotAssignPawnToAssignable".Translate(pawn.Name.ToStringShort, clickedThing.LabelCap) + ": " + "IdeoligionForbids".Translate(), null);
                            }
                            else
                            {
                                return new FloatMenuOption("SensibleBedOwnership_AssignPawnToAssignable".Translate(pawn.Name.ToStringShort, clickedThing.LabelCap), () =>
                                {
                                    comp.TryAssignPawn(pawn);
                                });
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
