using HarmonyLib;
using Verse;

namespace SensibleBedOwnership
{
    public class SensibleBedOwnershipMod : Mod
    {
        public const string PACKAGE_ID = "sensiblebedownership.1trickPwnyta";
        public const string PACKAGE_NAME = "Sensible Bed Ownership";

        public SensibleBedOwnershipMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony(PACKAGE_ID);
            harmony.PatchAll();

            Log.Message($"[{PACKAGE_NAME}] Loaded.");
        }
    }
}
