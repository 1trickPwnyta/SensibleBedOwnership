using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace SensibleBedOwnership
{
    public static class Utility
    {
        public static QuickSearchWidget AssignOwnerSearchWidget = new QuickSearchWidget();

        public static List<Building_Bed> AllAssignedBedsAndDeathrestCaskets(Pawn pawn)
        {
            List<Building_Bed> beds = new List<Building_Bed>();
            foreach (Map map in Find.Maps)
            {
                beds.AddRange(map.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>().Where(b => b.OwnersForReading.Contains(pawn)));
            }
            return beds;
        }

        public static List<Building_Bed> AllAssignedBeds(Pawn pawn)
        {
            return AllAssignedBedsAndDeathrestCaskets(pawn).Where(b => b.def != ThingDefOf.DeathrestCasket).ToList();
        }

        public static List<Building_Bed> AllAssignedDeathrestCaskets(Pawn pawn)
        {
            return AllAssignedBedsAndDeathrestCaskets(pawn).Where(b => b.def == ThingDefOf.DeathrestCasket).ToList();
        }

        public static Building_Bed AssignedBed(Pawn pawn, Map map)
        {
            return AllAssignedBeds(pawn).Where(b => b.Map != null && b.Map == map).FirstOrDefault();
        }

        public static Building_Bed AssignedDeathrestCasket(Pawn pawn, Map map)
        {
            return AllAssignedDeathrestCaskets(pawn).Where(b => b.Map != null && b.Map == map).FirstOrDefault();
        }

        public static Building_Bed GetMainBed(Pawn pawn)
        {
            Building_Bed bed;
            List<Building_Bed> allBeds = AllAssignedBeds(pawn);
            if (allBeds.Count == 1)
            {
                bed = allBeds[0];
            }
            else
            {
                Map map = pawn.Map;
                if (map == null)
                {
                    if (pawn.ParentHolder != null)
                    {
                        if (pawn.ParentHolder is Thing)
                        {
                            map = ((Thing)pawn.ParentHolder).Map;
                        }
                        else if (pawn.ParentHolder is ThingComp)
                        {
                            map = ((ThingComp)pawn.ParentHolder).parent.Map;
                        }
                        else if (pawn.ParentHolder is Pawn_CarryTracker)
                        {
                            map = ((Pawn_CarryTracker)pawn.ParentHolder).pawn.Map;
                        }
                    }
                }
                bed = AssignedBed(pawn, map);
            }
            if (bed == null)
            {
                IEnumerable<Building_Bed> homeBeds = allBeds.Where(b => b.Map.IsPlayerHome);
                if (homeBeds.Any())
                {
                    bed = homeBeds.MinBy(b => b.Map.uniqueID);
                }
                else if (allBeds.Any())
                {
                    bed = allBeds.MinBy(b => b.Map.uniqueID);
                }
            }
            return bed;
        }

        public static Building_Bed AssignedDeathrestCasketOnCurrentMap(Pawn pawn)
        {
            return AssignedDeathrestCasket(pawn, pawn.Map);
        }

        public static void UnassignBed(Pawn pawn, Map map)
        {
            Building_Bed bed = AssignedBed(pawn, map);
            if (bed != null)
            {
                bed.CompAssignableToPawn.TryUnassignPawn(pawn);
            }
        }

        public static void UnassignDeathrestCasket(Pawn pawn, Map map)
        {
            Building_Bed bed = AssignedDeathrestCasket(pawn, map);
            if (bed != null)
            {
                bed.CompAssignableToPawn.TryUnassignPawn(pawn);
            }
        }

        public static void UnassignBedsAfterBreakup(Pawn a, Pawn b)
        {
            foreach (Building_Bed bed in AllAssignedBeds(a).Intersect(AllAssignedBeds(b)))
            {
                bed.CompAssignableToPawn.TryUnassignPawn((Rand.Value < 0.5f) ? a : b);
            }
        }

        public static bool AssignableParentIsBed(CompAssignableToPawn comp)
        {
            return comp.parent is Building_Bed;
        }

        public static IEnumerable<Pawn> AssigningCandidatesMatchingFilterNotAlreadyAssigned(CompAssignableToPawn comp)
        {
            return comp.AssigningCandidates.Where(c => !comp.AssignedPawnsForReading.Contains(c) && AssignOwnerSearchWidget.filter.Matches(c.LabelShort));
        }

        public static FloatMenuOption GetAssignToAssignableOption(Vector3 clickPos, Pawn pawn)
        {
            IntVec3 clickCell = IntVec3.FromVector3(clickPos);
            Map map = pawn.GetMapForFloatMenu();
            if (clickCell.InBounds(map) && !clickCell.Fogged(map) && map == Find.CurrentMap)
            {
                foreach (Thing thing in clickCell.GetThingList(map))
                {
                    if (thing.TryGetComp<CompAssignableToPawn>() != null)
                    {
                        CompAssignableToPawn comp = (CompAssignableToPawn)thing.TryGetComp<CompAssignableToPawn>();
                        if ((bool)typeof(CompAssignableToPawn).Method("ShouldShowAssignmentGizmo").Invoke(comp, new object[] { }))
                        {
                            if (comp.AssigningCandidates.Contains(pawn) && !comp.AssignedPawnsForReading.Contains(pawn))
                            {
                                AcceptanceReport report = comp.CanAssignTo(pawn);
                                if (!report.Accepted)
                                {
                                    return new FloatMenuOption("SensibleBedOwnership_CannotAssignPawnToAssignable".Translate(pawn.Name.ToStringShort, thing.LabelCap) + ": " + report.Reason.StripTags(), null);
                                }
                                else if (comp.IdeoligionForbids(pawn))
                                {
                                    return new FloatMenuOption("SensibleBedOwnership_CannotAssignPawnToAssignable".Translate(pawn.Name.ToStringShort, thing.LabelCap) + ": " + "IdeoligionForbids".Translate(), null);
                                }
                                else
                                {
                                    return new FloatMenuOption("SensibleBedOwnership_AssignPawnToAssignable".Translate(pawn.Name.ToStringShort, thing.LabelCap), delegate ()
                                    {
                                        comp.TryAssignPawn(pawn);
                                    });
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        public static string LabelCapWithRelation(this Pawn pawn, CompAssignableToPawn comp)
        {
            DirectPawnRelation relation = comp.AssignedPawnsForReading.Where(p => LovePartnerRelationUtility.ExistingLoveRealtionshipBetween(pawn, p) != null).Select(p => LovePartnerRelationUtility.ExistingLoveRealtionshipBetween(pawn, p)).FirstOrDefault();
            if (relation != null)
            {
                string relationLabel = relation.def.label;
                if (relation.def.labelFemale != null && pawn.gender == Gender.Female)
                {
                    relationLabel = relation.def.labelFemale;
                }
                return pawn.LabelCap + (" (" + relationLabel + ")").Colorize(ColoredText.FactionColor_Neutral);
            }
            else
            {
                return pawn.LabelCap;
            }
        }

        public static Map GetMapForFloatMenu(this Pawn pawn)
        {
            return pawn.Map ?? pawn.CarriedBy?.Map;
        }
    }
}
