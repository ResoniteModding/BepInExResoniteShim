using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.NET.Common;
using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using System.Reflection;

namespace BepInExResoniteShim;

[ResonitePlugin(PluginMetadata.GUID, PluginMetadata.NAME, PluginMetadata.VERSION, PluginMetadata.AUTHORS, PluginMetadata.REPOSITORY_URL)]
class BepInExResoniteShim : BasePlugin
{
    internal static new ManualLogSource Log = null!;
    internal static ConfigEntry<bool> ShowWatermark = null!;

    internal static string? GetBepisLoaderVersion()
    {
        try
        {
            var bepisLoaderAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "BepisLoader");

            if (bepisLoaderAssembly != null)
            {
                var version = bepisLoaderAssembly.GetName().Version;
                if (version != null)
                {
                    return $"v{version.Major}.{version.Minor}.{version.Build}";
                }
            }
        }
        catch (Exception e)
        {
            Log.LogWarning($"Failed to get BepisLoader version: {e.Message}");
        }

        return null;
    }

    public override void Load()
    {
        Log = base.Log;

        var bepisLoaderVersion = GetBepisLoaderVersion();
        Log.LogInfo(bepisLoaderVersion != null
            ? $"Loader: BepisLoader {bepisLoaderVersion}"
            : "Loader: BepisLoader not found, version unknown");
        ShowWatermark = Config.Bind("General", "ShowWatermark", true, "Shows 'BepisLoader' watermark in the window title");

        Type? lastAttempted = null;
        try
        {
            var types = GenericTypesAttribute.GetTypes(GenericTypesAttribute.Group.EnginePrimitives);
            foreach (var type in types)
            {
                lastAttempted = type;
                if (TomlTypeConverter.CanConvert(type)) continue;
                TomlTypeConverter.AddConverter(type, new TypeConverter
                {
                    ConvertToString = (obj, type) => (string)typeof(Coder<>).MakeGenericType(type).GetMethod("EncodeToString")!.Invoke(null, [obj])!,
                    ConvertToObject = (str, type) => typeof(Coder<>).MakeGenericType(type).GetMethod("DecodeFromString")!.Invoke(null, [str])!,
                });
            }

            lastAttempted = typeof(dummy);
            TomlTypeConverter.AddConverter(typeof(dummy), new TypeConverter
            {
                ConvertToString = (_, _) => "dummy",
                ConvertToObject = (_, _) => default(dummy),
            });
        }
        catch (Exception e)
        {
            Log.LogError($"Failed to register generic type converters (Last attempted = {lastAttempted?.ToString() ?? "NULL"}): " + e);
        }

        RunPatches(HarmonyInstance);
    }

    internal static bool AnyPatchFailed { get; private set; }

    static void RunPatches(Harmony harmony)
    {
        var assembly = Assembly.GetExecutingAssembly();
        foreach (var type in AccessTools.GetTypesFromAssembly(assembly))
        {
            try
            {
                harmony.CreateClassProcessor(type).Patch();
            }
            catch (Exception e)
            {
                Log.LogDebug($"Skipped patching {type.Name}: {e.Message}");
                AnyPatchFailed = true;
            }
        }
    }
}