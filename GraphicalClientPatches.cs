using BepInEx;
using HarmonyLib;
using Renderite.Host;

namespace BepInExResoniteShim;

[HarmonyPatchCategory(nameof(GraphicalClientPatch))]
[HarmonyPatch(typeof(GraphicalClientRunner), MethodType.StaticConstructor)]
class GraphicalClientPatch
{
    public static void Postfix(ref string ___AssemblyDirectory)
    {
        ___AssemblyDirectory = Paths.GameRootPath;
    }
}
