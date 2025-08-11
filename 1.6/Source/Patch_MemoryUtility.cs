using HarmonyLib;
using Verse.Profile;

namespace SensibleBedOwnership
{
    [HarmonyPatch(typeof(MemoryUtility))]
    [HarmonyPatch(nameof(MemoryUtility.ClearAllMapsAndWorld))]
    public static class Patch_MemoryUtility
    {
        public static void Postfix()
        {
            Utility.SetBedCacheDirty();
        }
    }
}
