using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace SensibleBedOwnership
{
    public static class SensibleBedOwnershipRefs
    {
        public static readonly FieldInfo f_Dialog_AssignBuildingOwner_assignable = AccessTools.Field(typeof(Dialog_AssignBuildingOwner), "assignable");
        public static readonly FieldInfo f_ThingComp_parent = AccessTools.Field(typeof(ThingComp), nameof(ThingComp.parent));
        public static readonly FieldInfo f_Pawn_Ownership_pawn = AccessTools.Field(typeof(Pawn_Ownership), "pawn");

        public static readonly MethodInfo m_Utility_UnassignBed = AccessTools.Method(typeof(Utility), nameof(Utility.UnassignBed));
        public static readonly MethodInfo m_Utility_AssignableParentIsBed = AccessTools.Method(typeof(Utility), nameof(Utility.AssignableParentIsBed));
        public static readonly MethodInfo m_Pawn_Ownership_UnclaimBed = AccessTools.Method(typeof(Pawn_Ownership), nameof(Pawn_Ownership.UnclaimBed));
        public static readonly MethodInfo m_Pawn_Ownership_ClaimBedIfNonMedical = AccessTools.Method(typeof(Pawn_Ownership), nameof(Pawn_Ownership.ClaimBedIfNonMedical));
        public static readonly MethodInfo m_IdeoUIUtility_DoIdeoIcon = AccessTools.Method(typeof(IdeoUIUtility), nameof(IdeoUIUtility.DoIdeoIcon));
        public static readonly MethodInfo m_Thing_get_Map = AccessTools.Method(typeof(Thing), "get_Map");
    }
}
