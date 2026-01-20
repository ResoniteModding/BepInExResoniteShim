using System.Reflection;
using Elements.Core;
using HarmonyLib;

namespace BepInExResoniteShim;
using static HarmonyExtensions;


[HarmonyPatchCategory(nameof(LogAlerter))]
[HarmonyPatch(typeof(UniLog), "add_OnLog")]
class LogAlerter
{
    static readonly FieldInfo? _logStreamField = AccessTools.Field(BepInExResoniteShim.HeadlessType, "logStream");

    static void Postfix(Action<string> value)
    {
        Task.Run(async () =>
        {
            if (BepInExResoniteShim.IsHeadless && _logStreamField != null)
            {
                var timeout = DateTime.UtcNow.AddSeconds(10);
                while (_logStreamField.GetValue(null) is null && DateTime.UtcNow < timeout)
                    await Task.Delay(1);
            }

            if(AnyPatchFailed) value($"[BepisLoader] BepInExResoniteShim partially loaded.");
            else value($"[BepisLoader] BepInExResoniteShim loaded successfully.");
        });
    }
}
