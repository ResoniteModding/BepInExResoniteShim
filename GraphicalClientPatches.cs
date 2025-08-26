using BepInEx;
using HarmonyLib;
using Renderite.Host;

namespace BepInExResoniteShim;

public static class GraphicalClientPatches
{
    public static void ApplyPatch()
    {
        AccessTools.Field(typeof(GraphicalClientRunner), "AssemblyDirectory").SetValue(null, Paths.GameRootPath);
    }
}
