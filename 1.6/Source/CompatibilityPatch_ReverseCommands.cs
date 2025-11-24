using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace SensibleBedOwnership
{
    // Patched conditionally in mod initializer
    public static class CompatibilityPatch_ReverseCommands
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionsList = instructions.ToList();
            int index = instructionsList.FindIndex(i => i.Calls(typeof(MapPawns).PropertyGetter(nameof(MapPawns.FreeColonists))));
            instructionsList.Insert(index + 1, new CodeInstruction(OpCodes.Call, typeof(GenList).Method(nameof(GenList.ListFullCopy)).MakeGenericMethod(new[] { typeof(Pawn) })));
            return instructionsList;
        }
    }
}
