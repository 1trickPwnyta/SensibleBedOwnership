using RimWorld;
using System.Collections.Generic;
using System.Linq;
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

        public static Building_Bed AssignedBedOnCurrentMap(Pawn pawn)
        {
            return AssignedBed(pawn, pawn.Map);
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
            return comp.AssigningCandidates.Where(c => !comp.AssignedPawnsForReading.Contains(c) && AssignOwnerSearchWidget.filter.Matches(c.Name.ToStringShort));
        }
    }
}
