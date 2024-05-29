namespace SensibleBedOwnership
{
    public static class Debug
    {
        public static void Log(string message)
        {
#if DEBUG
            Verse.Log.Message($"[{SensibleBedOwnershipMod.PACKAGE_NAME}] {message}");
#endif
        }
    }
}
