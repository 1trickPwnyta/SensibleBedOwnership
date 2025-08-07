using HarmonyLib;
using RimWorld;
using Verse;

namespace SensibleBedOwnership
{
    // Dirty the bed cache when a bed changes ownership
    [HarmonyPatch(typeof(Building))]
    [HarmonyPatch(nameof(Building.SetFaction))]
    public static class Patch_Building
    {
        public static void Postfix(Building __instance)
        {
            if (__instance is Building_Bed)
            {
                Utility.SetBedCacheDirty();
            }
        }
    }
}
