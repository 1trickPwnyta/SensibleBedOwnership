using UnityEngine;
using Verse;

namespace SensibleBedOwnership
{
    public class SensibleBedOwnershipSettings : ModSettings
    {
        public static bool SleepInChairs = true;

        public static void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();

            listingStandard.Begin(inRect);

            listingStandard.CheckboxLabeled("SensibleBedOwnership_SleepInChairs".Translate(), ref SleepInChairs);

            listingStandard.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref SleepInChairs, "SleepInChairs", true);
        }
    }
}
