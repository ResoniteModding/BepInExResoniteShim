using System.Reflection;
using Elements.Core;
using HarmonyLib;

namespace BepInExResoniteShim;
using static HarmonyExtensions;


[HarmonyPatchCategory(nameof(LogAlerter))]
[HarmonyPatch(typeof(UniLog), "add_OnLog")]
class LogAlerter
{
    static Type _headlessType = AccessTools.TypeByName("FrooxEngine.Headless.Program");
    static FieldInfo _logStreamField = AccessTools.Field(_headlessType, "logStream");
    static void Postfix(Action<string> value)
    {
        Task.Run(async () =>
        {
            if (_headlessType is not null)
            {
                while (_logStreamField.GetValue(null) is null)
                    await Task.Delay(1);
            }

            if(AnyPatchFailed) value($"[BepisLoader] BepInExResoniteShim partially loaded.");
            else value($"[BepisLoader] BepInExResoniteShim loaded successfully.");
        });
    }
}
