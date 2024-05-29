using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace SensibleBedOwnership
{
    public static class Utility
    {
        public static List<Building_Bed> AllAssignedBeds(Pawn pawn)
        {
            List<Building_Bed> beds = new List<Building_Bed>();
            if (pawn.Map != null)
            {
                beds.AddRange(pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>().Where(b => b.CompAssignableToPawn.AssignedPawnsForReading.Contains(pawn)));
            }
            return beds;
        }

        public static bool AssignableParentIsBed(CompAssignableToPawn comp)
        {
            return comp.parent is Building_Bed;
        }
    }
}
