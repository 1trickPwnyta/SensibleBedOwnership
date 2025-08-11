using HarmonyLib;
using RimWorld;
using Verse;

namespace SensibleBedOwnership
{
    // Fix panws with null ownership due to previous bug
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch(nameof(Pawn.ExposeData))]
    public static class Patch_Pawn
    {
        public static void Postfix(Pawn __instance)
        {
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (__instance.ownership == null)
                {
                    __instance.ownership = new Pawn_Ownership(__instance);
                }
            }
        }
    }
}
