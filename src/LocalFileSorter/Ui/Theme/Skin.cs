using SFML.Graphics;

namespace LocalFileSorter.Ui.Theme;

public sealed class Skin
{
    private readonly IReadOnlyDictionary<PartKey, SurfaceStyle> styles;

    public Skin(
        string name,
        string directory,
        string? uiFont,
        string? monoFont,
        IReadOnlyDictionary<string, Color> colors,
        IReadOnlyDictionary<string, float> metrics,
        IReadOnlyDictionary<string, uint> textSizes,
        IReadOnlyDictionary<PartKey, SurfaceStyle> styles,
        IReadOnlyCollection<string> missingTokens)
    {
        Name = name;
        Directory = directory;
        UiFont = uiFont;
        MonoFont = monoFont;
        Colors = colors;
        Metrics = metrics;
        TextSizes = textSizes;
        this.styles = styles;
        MissingTokens = missingTokens;
    }

    public static Skin BuiltIn { get; } = new(
        SkinDefaults.Name,
        string.Empty,
        uiFont: null,
        monoFont: null,
        SkinDefaults.Colors,
        SkinDefaults.Metrics,
        SkinDefaults.TextSizes,
        ResolvedDefaults(),
        []);

    public string Name { get; }

    public string Directory { get; }

    public string? UiFont { get; }

    public string? MonoFont { get; }

    public IReadOnlyDictionary<string, Color> Colors { get; }

    public IReadOnlyDictionary<string, float> Metrics { get; }

    public IReadOnlyDictionary<string, uint> TextSizes { get; }

    public IReadOnlyCollection<string> MissingTokens { get; }

    public SurfaceStyle Style(UiPart part, PartState state) => styles[new PartKey(part, state)];

    public Color Foreground(UiPart part, PartState state) => Style(part, state).Foreground ?? Colors["textPrimary"];

    private static Dictionary<PartKey, SurfaceStyle> ResolvedDefaults()
    {
        Dictionary<PartKey, SurfaceStyle> resolved = [];

        foreach (UiPart part in Enum.GetValues<UiPart>())
        {
            foreach (PartState state in Enum.GetValues<PartState>())
            {
                resolved[new PartKey(part, state)] = SkinDefaults.Style(part, state);
            }
        }

        return resolved;
    }
}
