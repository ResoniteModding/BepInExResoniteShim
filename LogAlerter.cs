using Elements.Core;
using HarmonyLib;

namespace BepInExResoniteShim;


[HarmonyPatchCategory(nameof(LogAlerter))]
[HarmonyPatch(typeof(UniLog), "add_OnLog")]
internal class LogAlerter
{
    static void Postfix(Action<string> value)
    {
        value("BepInEx shim loaded");
    }
}
