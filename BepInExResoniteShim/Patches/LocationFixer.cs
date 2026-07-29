using BepInEx;
using FrooxEngine;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

namespace BepInExResoniteShim;

[HarmonyPatch]
class LocationFixer
{
    public static MethodBase TargetMethod()
    {
        return AccessTools.FirstConstructor(typeof(AssemblyTypeRegistry), (x) => x.GetParameters().Length > 3);
    }
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
    {
        int killCount = 0;
        var cachePath = AccessTools.PropertyGetter(typeof(GlobalTypeRegistry), nameof(GlobalTypeRegistry.MetadataCachePath));
        var asmLoc = AccessTools.PropertyGetter(typeof(Assembly), nameof(Assembly.Location));

        foreach (var code in codes)
        {
            if (killCount > 0)
            {
                killCount--;
                continue;
            }
            if (code.Calls(asmLoc))
            {
                BepInExResoniteShim.Log.LogDebug("Patched AsmLoc");

                yield return new(OpCodes.Call, AccessTools.Method(typeof(LocationFixer), nameof(ProcessCacheTime)));
                killCount = 2;
            }
            else
            {
                yield return code;
            }
            if (code.Calls(cachePath))
            {
                BepInExResoniteShim.Log.LogDebug("Patched CachePath");

                yield return new(OpCodes.Ldarg_1);
                yield return new(OpCodes.Call, AccessTools.Method(typeof(LocationFixer), nameof(ProcessCachePath)));
            }
        }
    }

    public static string? ProcessCachePath(string cachePath, Assembly asm)
    {
        if (string.IsNullOrWhiteSpace(asm.Location)) return null;
        return cachePath;
    }

    public static DateTime ProcessCacheTime(Assembly asm)
    {
        if (string.IsNullOrWhiteSpace(asm.Location)) return DateTime.UtcNow;
        return new FileInfo(asm.Location).LastWriteTimeUtc;
    }
}