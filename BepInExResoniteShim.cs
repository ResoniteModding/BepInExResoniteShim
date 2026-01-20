using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.NET.Common;
using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using Renderite.Shared;
using System.Reflection;
using System.Reflection.Emit;

namespace BepInExResoniteShim;

public class ResonitePlugin : BepInPlugin
{
    public ResonitePlugin(string GUID, string Name, string Version, string Author, string Link) : base(GUID, Name, Version)
    {
        this.Author = Author;
        this.Link = Link;
    }
    public string Author { get; protected set; }
    public string Link { get; protected set; }
}


[ResonitePlugin(PluginMetadata.GUID, PluginMetadata.NAME, PluginMetadata.VERSION, PluginMetadata.AUTHORS, PluginMetadata.REPOSITORY_URL)]
class BepInExResoniteShim : BasePlugin
{
    internal static new ManualLogSource Log = null!;
    static ConfigEntry<bool> ShowWatermark = null!;

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

    /// <summary>
    /// Apply all patches, incompatible patches are skipped gracefully.
    /// </summary>
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
                    Log.LogDebug("Patched AsmLoc");

                    yield return new(OpCodes.Call, AccessTools.Method(typeof(LocationFixer), nameof(ProcessCacheTime)));
                    killCount = 2; // skip next code
                }
                else
                {
                    yield return code;
                }
                if (code.Calls(cachePath))
                {
                    Log.LogDebug("Patched CachePath");

                    yield return new(OpCodes.Ldarg_1); // assembly
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
            Log.LogDebug("Bypassing LoadFrom: " + path);
            return null;
        }
    }

    [HarmonyPatch(typeof(RendererInitData), "Pack")]
    class WindowTitlePatcher
    {
        public static void Prefix(RendererInitData __instance)
        {
            if (!ShowWatermark.Value)
            {
                return;
            }

            if (__instance.windowTitle == "Resonite")
            {
                var version = GetBepisLoaderVersion();
                var newTitle = $"Resonite - BepisLoader {version ?? ""}";
                __instance.windowTitle = newTitle;
                Log.LogInfo($"Successfully patched window title to: {newTitle}");
            }
        }
    }
}
