using BepInEx;
using FrooxEngine;
using HarmonyLib;

namespace BepInExResoniteShim;

[HarmonyPatch(typeof(RenderSystem), nameof(RenderSystem.Initialize))]
class WindowTitlePatcher
{
    public static void Prefix(Engine engine, ref string? windowTitle)
    {
        if (!BepInExResoniteShim.ShowWatermark.Value)
            return;

        var version = BepInExResoniteShim.GetBepisLoaderVersion();
        windowTitle = $"{engine.PlatformProfile.Name} - BepisLoader {version ?? ""}";
        BepInExResoniteShim.Log.LogInfo($"Successfully patched window title to: {windowTitle}");
    }
}