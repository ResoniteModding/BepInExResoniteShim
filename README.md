<p align="center">
    <img src="https://raw.githubusercontent.com/ResoniteModding/BepInExResoniteShim/master/icon.png">
</p>

# BepInExResoniteShim
[![Thunderstore Version](https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fthunderstore.io%2Fapi%2Fexperimental%2Fpackage%2FResoniteModding%2FBepInExResoniteShim%2F&query=%24.latest.version_number&label=Thunderstore&style=flat&logo=thunderstore&logoColor=white)](https://thunderstore.io/c/resonite/p/ResoniteModding/BepInExResoniteShim/)
[![Thunderstore Downloads](https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fthunderstore.io%2Fapi%2Fv1%2Fpackage-metrics%2FResoniteModding%2FBepInExResoniteShim%2F&query=%24.downloads&label=downloads&style=flat&logo=thunderstore&logoColor=white)](https://thunderstore.io/c/resonite/p/ResoniteModding/BepInExResoniteShim/)
[![NuGet Version](https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fnuget-modding.resonite.net%2Fv3%2Fregistration%2Fresonitemodding.bepinexresoniteshim%2Findex.json&query=%24.items%5B0%5D.upper&label=NuGet&style=flat&logo=nuget&logoColor=white&color=004880)](https://nuget-modding.resonite.net/packages/resonitemodding.bepinexresoniteshim)
[![Build](https://img.shields.io/github/actions/workflow/status/ResoniteModding/BepInExResoniteShim/build.yml?style=flat&logo=github)](https://github.com/ResoniteModding/BepInExResoniteShim/actions/workflows/build.yml)
[![License](https://img.shields.io/badge/license-MIT-blue?style=flat)](#license)

Plugin for [BepisLoader](https://github.com/ResoniteModding/BepisLoader) which applies some patches to allow Resonite to run with BepInEx loaded.

## Installation (Manual)
1. Install [BepisLoader](https://github.com/ResoniteModding/BepisLoader) for Resonite.
2. Download the latest release ZIP file (for example `ResoniteModding-BepInExResoniteShim-0.10.0.zip`) from the [Releases](https://github.com/ResoniteModding/BepInExResoniteShim/releases) page.
3. Extract the ZIP and copy the `plugins` folder to your BepInEx folder in your Resonite installation directory:
   - **Default location:** `C:\Program Files (x86)\Steam\steamapps\common\Resonite\BepInEx\`
4. Start the game. If you want to verify that the mod is working you can check your BepInEx logs.

## Development

Requires the .NET 11 SDK (see `global.json`).

```sh
# build + Thunderstore package into ./build
dotnet build -c Release -t:PackTS -v d

# build + copy the plugin into your BepInEx/plugins folder
dotnet build -p:CopyToPlugins=true
```

The game and BepInEx folders are detected from Steam, r2modman and Gale. If detection gets it wrong, copy `Config.Build.user.props.template` to `Config.Build.user.props` and set `GamePath` / `BepInExPath` there.

If you are writing a plugin, start from the [plugin template](https://github.com/ResoniteModding/BepInEx.Templates) and reference `ResoniteModding.BepInExResoniteShim` from [NuGet](https://nuget-modding.resonite.net/packages/resonitemodding.bepinexresoniteshim).

## License

This project is licensed under MIT License. See [LICENSE](LICENSE) for details.
