using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace SensibleBedOwnership
{
    public static class Utility
    {
        private static HashSet<Building_Bed> bedCache = new HashSet<Building_Bed>();
        private static bool bedCacheDirty = true;
        private static Dictionary<Pawn, Building_Bed> preferredBeds = new Dictionary<Pawn, Building_Bed>();

        public static QuickSearchWidget AssignOwnerSearchWidget = new QuickSearchWidget();

        private static bool OnSubstructure(Thing thing)
        {
            TerrainGrid grid = thing.Map.terrainGrid;
            if (GenAdj.CellsOccupiedBy(thing).All(c => grid.FoundationAt(c) == TerrainDefOf.Substructure))
            {
                return true;
            }
            return false;
        }

        private static float AssignableSelectionWeight(CompAssignableToPawn comp, Pawn pawn)
        {
            if (pawn.GetPreferredBed() == comp.parent)
            {
                return float.MaxValue;
            }
            return OnSubstructure(comp.parent) ? -1f : 1f;
        }

        public static List<Building_Bed> AllAssignedBedsAndDeathrestCaskets(Pawn pawn)
        {
            if (bedCacheDirty)
            {
                bedCache = new HashSet<Building_Bed>();
                foreach (Map map in Find.Maps)
                {
                    bedCache.AddRange(map.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>());
                }
                bedCacheDirty = false;
            }
            return bedCache.Where(b => b.OwnersForReading.Contains(pawn)).ToList();
        }

        public static void SetBedCacheDirty()
        {
            bedCacheDirty = true;
        }

        public static void ClearPreferredBeds()
        {
            preferredBeds.Clear();
        }

        public static Building_Bed GetPreferredBed(this Pawn pawn) => preferredBeds.TryGetValue(pawn);

        public static void SetPreferredBed(this Pawn pawn, Building_Bed bed)
        {
            preferredBeds[pawn] = bed;
        }

        public static List<Building_Bed> AllAssignedBeds(Pawn pawn)
        {
            return AllAssignedBedsAndDeathrestCaskets(pawn).Where(b => b.def != ThingDefOf.DeathrestCasket).ToList();
        }

        public static List<Building_Bed> AllAssignedDeathrestCaskets(Pawn pawn)
        {
            return AllAssignedBedsAndDeathrestCaskets(pawn).Where(b => b.def == ThingDefOf.DeathrestCasket).ToList();
        }

        public static Building_Bed AssignedBed(Pawn pawn, Map map, bool allowDeathrestCaskets = false)
        {
            try
            {
                return (allowDeathrestCaskets ? AllAssignedBedsAndDeathrestCaskets(pawn) : AllAssignedBeds(pawn)).Where(b => b.Map != null && b.Map == map).MaxBy(b => AssignableSelectionWeight(b.CompAssignableToPawn, pawn));
            }
            catch (Exception e)
            {
                Log.ErrorOnce("Exception checking assigned bed for " + pawn + " on map " + map + ": " + e, "SensibleBedOwnership_ErrorAssignedBed".GetHashCode());
                return null;
            }
        }

        public static Building_Bed AssignedDeathrestCasket(Pawn pawn, Map map)
        {
            try
            {
                return AllAssignedDeathrestCaskets(pawn).Where(b => b.Map != null && b.Map == map).FirstOrDefault();
            }
            catch (Exception e)
            {
                Log.ErrorOnce("Exception checking assigned deathrest casket for " + pawn + " on map " + map + ": " + e, "SensibleBedOwnership_ErrorAssignedDeathrestCasket".GetHashCode());
                return null;
            }
        }

        public static Building_Bed GetMainBed(Pawn_Ownership ownership) => GetMainBed(typeof(Pawn_Ownership).Field("pawn").GetValue(ownership) as Pawn);

        public static Building_Bed GetMainBed(Pawn pawn)
        {
            Building_Bed bed;
            List<Building_Bed> allBeds = AllAssignedBedsAndDeathrestCaskets(pawn);
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
                bed = AssignedBed(pawn, map, true);
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

        public static Building_Bed AssignedBedOnCurrentMap(Pawn pawn)
        {
            return AssignedBed(pawn, pawn.SpawnedParentOrMe?.Map);
        }

        public static Building_Bed AssignedDeathrestCasketOnCurrentMap(Pawn pawn)
        {
            return AssignedDeathrestCasket(pawn, pawn.SpawnedParentOrMe?.Map);
        }

        public static void UnassignBed(Pawn pawn, Map map, Building_Bed newBed)
        {
            if (newBed != null)
            {
                bool onSubstructure = OnSubstructure(newBed);
                foreach (Building_Bed bed in AllAssignedBeds(pawn).Where(b => b.Map != null && b.Map == map && OnSubstructure(b) == onSubstructure))
                {
                    bed.CompAssignableToPawn.TryUnassignPawn(pawn);
                }
            }
        }

        public static void UnassignDeathrestCasket(Pawn pawn, Map map, Building_Bed newDeathrestCasket)
        {
            if (newDeathrestCasket != null)
            {
                bool onSubstructure = OnSubstructure(newDeathrestCasket);
                foreach (Building_Bed deathrestCasket in AllAssignedDeathrestCaskets(pawn).Where(b => b.Map != null && b.Map == map && OnSubstructure(b) == onSubstructure))
                {
                    deathrestCasket.CompAssignableToPawn.TryUnassignPawn(pawn);
                }
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
            return comp.AssigningCandidates.Where(c => comp.CanAssignTo(c) && !comp.AssignedPawnsForReading.Contains(c) && AssignOwnerSearchWidget.filter.Matches(c.LabelShort));
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
