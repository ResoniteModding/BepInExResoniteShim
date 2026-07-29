using BepInEx;

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