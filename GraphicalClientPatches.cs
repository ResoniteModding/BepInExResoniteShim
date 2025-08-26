using BepInEx;
using HarmonyLib;
using Renderite.Host;

namespace BepInExResoniteShim;

public static class GraphicalClientPatches
{
    public static void ApplyPatch(Harmony harmony)
    {
        var constructor = AccessTools
            .GetDeclaredConstructors(typeof(GraphicalClientRunner))
            .FirstOrDefault(c => c.IsStatic);
        harmony.Patch(constructor, postfix: new(AccessTools.Method(typeof(GraphicalClientPatches), nameof(Postfix))));
    }

    public static void Postfix(ref string ___AssemblyDirectory)
    {
        ___AssemblyDirectory = Paths.GameRootPath;
    }
}
