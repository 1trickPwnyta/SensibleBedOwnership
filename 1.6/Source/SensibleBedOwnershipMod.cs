using HarmonyLib;
using RimWorld;
using System.Reflection;
using UnityEngine;
using Verse;

namespace SensibleBedOwnership
{
    [StaticConstructorOnStartup]
    public static class SensibleBedOwnershipInitializer
    {
        static SensibleBedOwnershipInitializer()
        {
            var harmony = new Harmony(SensibleBedOwnershipMod.PACKAGE_ID);
            harmony.PatchAll();
            harmony.Patch(typeof(Dialog_AssignBuildingOwner).Constructor(new[] { typeof(CompAssignableToPawn) }), null, typeof(Patch_Dialog_AssignBuildingOwner_ctor).Method("Postfix"));
            
            MethodBase reverseCommands = AccessTools.TypeByName("ReverseCommands.Tools")?.Method("GetPawnActions");
            if (reverseCommands != null)
            {
                harmony.Patch(reverseCommands, transpiler: typeof(CompatibilityPatch_ReverseCommands).Method(nameof(CompatibilityPatch_ReverseCommands.Transpiler)));
            }
        }
    }

    public class SensibleBedOwnershipMod : Mod
    {
        public const string PACKAGE_ID = "sensiblebedownership.1trickPwnyta";
        public const string PACKAGE_NAME = "Sensible Bed Ownership";

        public static SensibleBedOwnershipSettings Settings;

        public SensibleBedOwnershipMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<SensibleBedOwnershipSettings>();

            Log.Message($"[{PACKAGE_NAME}] Loaded.");
        }

        public override string SettingsCategory() => PACKAGE_NAME;

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            SensibleBedOwnershipSettings.DoSettingsWindowContents(inRect);
        }
    }
}
