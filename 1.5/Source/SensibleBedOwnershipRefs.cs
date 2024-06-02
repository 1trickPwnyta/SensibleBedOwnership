using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using Verse.Sound;

namespace SensibleBedOwnership
{
    public static class SensibleBedOwnershipRefs
    {
        public static readonly FieldInfo f_Utility_AssignOwnerSearchWidget = AccessTools.Field(typeof(Utility), nameof(Utility.AssignOwnerSearchWidget));
        public static readonly FieldInfo f_Dialog_AssignBuildingOwner_assignable = AccessTools.Field(typeof(Dialog_AssignBuildingOwner), "assignable");
        public static readonly FieldInfo f_ThingComp_parent = AccessTools.Field(typeof(ThingComp), nameof(ThingComp.parent));
        public static readonly FieldInfo f_Pawn_Ownership_pawn = AccessTools.Field(typeof(Pawn_Ownership), "pawn");
        public static readonly FieldInfo f_DirectPawnRelation_otherPawn = AccessTools.Field(typeof(DirectPawnRelation), nameof(DirectPawnRelation.otherPawn));

        public static readonly MethodInfo m_Utility_UnassignBed = AccessTools.Method(typeof(Utility), nameof(Utility.UnassignBed));
        public static readonly MethodInfo m_Utility_AssignableParentIsBed = AccessTools.Method(typeof(Utility), nameof(Utility.AssignableParentIsBed));
        public static readonly MethodInfo m_Utility_AssignedBed = AccessTools.Method(typeof(Utility), nameof(Utility.AssignedBed));
        public static readonly MethodInfo m_Utility_UnassignDeathrestCasket = AccessTools.Method(typeof(Utility), nameof(Utility.UnassignDeathrestCasket));
        public static readonly MethodInfo m_Utility_AssigningCandidatesMatchingFilterNotAlreadyAssigned = AccessTools.Method(typeof(Utility), nameof(Utility.AssigningCandidatesMatchingFilterNotAlreadyAssigned));
        public static readonly MethodInfo m_Utility_LabelCapWithRelation = AccessTools.Method(typeof(Utility), nameof(Utility.LabelCapWithRelation));
        public static readonly MethodInfo m_Pawn_Ownership_UnclaimBed = AccessTools.Method(typeof(Pawn_Ownership), nameof(Pawn_Ownership.UnclaimBed));
        public static readonly MethodInfo m_Pawn_Ownership_ClaimBedIfNonMedical = AccessTools.Method(typeof(Pawn_Ownership), nameof(Pawn_Ownership.ClaimBedIfNonMedical));
        public static readonly MethodInfo m_IdeoUIUtility_DoIdeoIcon = AccessTools.Method(typeof(IdeoUIUtility), nameof(IdeoUIUtility.DoIdeoIcon));
        public static readonly MethodInfo m_Thing_get_Map = AccessTools.Method(typeof(Thing), "get_Map");
        public static readonly MethodInfo m_LovePartnerRelationUtility_ExistingMostLikedLovePartnerRel = AccessTools.Method(typeof(LovePartnerRelationUtility), nameof(LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel));
        public static readonly MethodInfo m_Pawn_Ownership_get_OwnedBed = AccessTools.Method(typeof(Pawn_Ownership), "get_OwnedBed");
        public static readonly MethodInfo m_Pawn_Ownership_UnclaimDeathrestCasket = AccessTools.Method(typeof(Pawn_Ownership), nameof(Pawn_Ownership.UnclaimDeathrestCasket));
        public static readonly MethodInfo m_CompAssignableToPawn_get_AssigningCandidates = AccessTools.Method(typeof(CompAssignableToPawn), "get_AssigningCandidates");
        public static readonly MethodInfo m_Entity_get_LabelCap = AccessTools.Method(typeof(Entity), "get_LabelCap");
        public static readonly MethodInfo m_SoundStarter_PlayOneShotOnCamera = AccessTools.Method(typeof(SoundStarter), nameof(SoundStarter.PlayOneShotOnCamera));
        public static readonly MethodInfo m_QuickSearchWidget_Reset = AccessTools.Method(typeof(QuickSearchWidget), nameof(QuickSearchWidget.Reset));
    }
}
