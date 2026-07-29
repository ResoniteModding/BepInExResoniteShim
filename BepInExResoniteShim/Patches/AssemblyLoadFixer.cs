using BepInEx;
using FrooxEngine;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

namespace BepInExResoniteShim;

[HarmonyPatch(typeof(EngineInitializer), nameof(EngineInitializer.InitializeFrooxEngine), MethodType.Async)]
class AssemblyLoadFixer
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
    {
        var loadFrom = AccessTools.Method(typeof(Assembly), nameof(Assembly.LoadFrom), [typeof(string)]);
        foreach (var code in codes)
        {
            if (code.Calls(loadFrom))
            {
                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AssemblyLoadFixer), nameof(LoadFrom)));
            }
            else
            {
                yield return code;
            }
        }
    }

    public static Assembly? LoadFrom(string path)
    {
        BepInExResoniteShim.Log.LogDebug("Bypassing LoadFrom: " + path);
        return null;
    }
}