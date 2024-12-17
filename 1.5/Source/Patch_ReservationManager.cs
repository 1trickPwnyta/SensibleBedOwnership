using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace SensibleBedOwnership
{
    [HarmonyPatch(typeof(ReservationManager))]
    [HarmonyPatch(nameof(ReservationManager.Reserve))]
    public static class Patch_ReservationManager
    {
        public static bool Prefix(Pawn claimant, LocalTargetInfo target, ref bool __result)
        {
            HashSet<Pawn> reservers = new HashSet<Pawn>();
            if (target.HasThing && target.Thing is Building_Bed)
            {
                target.Thing.Map.reservationManager.ReserversOf(target, reservers);
                if (reservers.Contains(claimant))
                {
                    __result = true;
                    return false;
                }
            }
            return true;
        }
    }
}
