using Elements.Core;
using HarmonyLib;

namespace BepInExResoniteShim;
using static HarmonyExtensions;


[HarmonyPatchCategory(nameof(LogAlerter))]
[HarmonyPatch(typeof(UniLog), "add_OnLog")]
class LogAlerter
{
    static void Postfix(Action<string> value)
    {
        if(AnyPatchFailed) value($"[BepisLoader] BepInExResoniteShim partially loaded.");
        else value($"[BepisLoader] BepInExResoniteShim loaded successfully.");
    }
}
