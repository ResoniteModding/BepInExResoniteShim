using System.Reflection;
using Elements.Core;
using HarmonyLib;

namespace BepInExResoniteShim;

using static HarmonyExtensions;


[HarmonyPatch(typeof(UniLog), "add_OnLog")]
class LogAlerter
{
    static readonly FieldInfo? _logStreamField = AccessTools.Field(
        Type.GetType("FrooxEngine.Headless.Program, Resonite"), "logStream");

    static void Postfix(Action<string> value)
    {
        Task.Run(async () =>
        {
            if (_logStreamField != null)
            {
                var timeout = DateTime.UtcNow.AddSeconds(10);
                while (_logStreamField.GetValue(null) is null && DateTime.UtcNow < timeout)
                    await Task.Delay(1);
            }

            if (AnyPatchFailed) value($"[BepisLoader] BepInExResoniteShim partially loaded.");
            else value($"[BepisLoader] BepInExResoniteShim loaded successfully.");
        });
    }
}
