using BepInEx;
using FrooxEngine;
using HarmonyLib;
using System.Diagnostics;

namespace BepInExResoniteShim;

class RelativePathFixer
{
    [HarmonyPatchCategory(nameof(RelativePathFixer))]
    [HarmonyPatch(typeof(LaunchOptions))]
    class LaunchOptionsPathPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LaunchOptions.LogsDirectory), MethodType.Getter)]
        public static void LogsDirectory_Postfix(ref string __result)
        {
            if (string.IsNullOrWhiteSpace(__result))
            {
                __result = "Logs";
            }

            if (!string.IsNullOrWhiteSpace(__result) && !Path.IsPathRooted(__result))
            {
                var absolutePath = Path.Combine(Paths.GameRootPath, __result);
                BepInExResoniteShim.Log.LogDebug($"Patched LogsDirectory from '{__result}' to '{absolutePath}'");
                __result = absolutePath;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(LaunchOptions.OverrideRendererIcon), MethodType.Getter)]
        public static void OverrideRendererIcon_Postfix(ref string __result)
        {
            if (!string.IsNullOrWhiteSpace(__result) && !Path.IsPathRooted(__result))
            {
                var absolutePath = Path.Combine(Paths.GameRootPath, __result);
                BepInExResoniteShim.Log.LogDebug($"Patched OverrideRendererIcon from '{__result}' to '{absolutePath}'");
                __result = absolutePath;
            }
        }
    }

    [HarmonyPatchCategory(nameof(RelativePathFixer))]
    [HarmonyPatch(typeof(Process), nameof(Process.Start), typeof(ProcessStartInfo))]
    class StartRendererPatch
    {
        public static void Prefix(ProcessStartInfo startInfo)
        {
            if (startInfo == null) return;

            if (startInfo.FileName != null && startInfo.FileName.Contains("Renderite.Renderer.exe"))
            {
                if (startInfo.WorkingDirectory == "Renderer" || string.IsNullOrEmpty(startInfo.WorkingDirectory))
                {
                    var originalWorkingDir = startInfo.WorkingDirectory;
                    var correctWorkingDir = Path.GetDirectoryName(startInfo.FileName);
                    startInfo.WorkingDirectory = correctWorkingDir;
                    BepInExResoniteShim.Log.LogDebug($"Patched renderer WorkingDirectory from '{originalWorkingDir}' to '{correctWorkingDir}'");
                }
            }
        }
    }

    [HarmonyPatchCategory(nameof(RelativePathFixer))]
    [HarmonyPatch(typeof(File), nameof(File.WriteAllText), typeof(string), typeof(string))]
    class CrashLogPathFixer
    {
        public static void Prefix(ref string path, string contents)
        {
            if (path != null && path.Contains("Renderite.Host.Crash"))
            {
                if (!Path.IsPathRooted(path))
                {
                    var absolutePath = Path.Combine(Paths.GameRootPath, path);
                    BepInExResoniteShim.Log.LogDebug($"Patched crash log path from '{path}' to '{absolutePath}'");
                    path = absolutePath;
                }
            }
        }
    }
}
