using BepInEx;
using FrooxEngine;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

namespace BepInExResoniteShim;

class RelativePathFixer
{
    [HarmonyPatchCategory(nameof(RelativePathFixer))]
    [HarmonyPatch(typeof(Program), "<Main>$", MethodType.Async)]
    class RenderiteHostPathFixes
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
        {
            foreach (var code in codes)
            {
                if (code.Is(OpCodes.Ldstr, "Logs"))
                {
                    yield return new(OpCodes.Ldstr, Path.Combine(Paths.GameRootPath, "Logs"));
                    continue;
                }
                if (code.Is(OpCodes.Ldstr, "Icon.png"))
                {
                    yield return new(OpCodes.Ldstr, Path.Combine(Paths.GameRootPath, "Icon.png"));
                    continue;
                }
                if(code.operand is MethodInfo mf && mf.Name == nameof(File.WriteAllText))
                {
                    yield return new(OpCodes.Call, AccessTools.Method(typeof(RenderiteHostPathFixes), nameof(FileWriteInjected)));
                    continue;
                }
                yield return code;
            }
        }

        public static void FileWriteInjected(string path, string? contents)
        {
            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(Paths.GameRootPath, path);
            }
            File.WriteAllText(path, contents);
        }
    }

    [HarmonyPatchCategory(nameof(RelativePathFixer))]
    [HarmonyPatch(typeof(RenderSystem), "StartRenderer", MethodType.Async)]
    public class RenderiteWorkingDirectoryFix
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
        { 
            foreach (var code in codes)
            {
                if(code.Is(OpCodes.Ldstr, "Renderer"))
                {
                    yield return new(OpCodes.Ldstr, Path.Combine(Paths.GameRootPath, "Renderer"));
                    continue;
                }
                yield return code;
            }
        }
    }
}
