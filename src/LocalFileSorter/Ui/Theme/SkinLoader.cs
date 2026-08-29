using System.Globalization;
using System.Text.Json;

using SFML.Graphics;

namespace LocalFileSorter.Ui.Theme;

public static class SkinLoader
{
    private static readonly Dictionary<string, UiPart> PartsByName = BuildPartNames();

    public static string SkinPath(string themesDirectory, string name) =>
        Path.Combine(themesDirectory, name, "theme.json");

    public static bool TryLoad(string themesDirectory, string name, out Skin skin, out string path)
    {
        path = SkinPath(themesDirectory, name);
        skin = Skin.BuiltIn;

        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            using FileStream stream = File.OpenRead(path);
            using JsonDocument document = JsonDocument.Parse(stream);

            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            skin = Read(document.RootElement, name, Path.GetDirectoryName(path) ?? themesDirectory);
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            return false;
        }
    }

    public static string ResolveFont(Skin skin, string? file, string fallbackDirectory, string fallbackFile)
    {
        if (string.IsNullOrEmpty(file))
        {
            return fallbackFile;
        }

        foreach (string directory in FontDirectories(skin, fallbackDirectory))
        {
            string candidate = Path.Combine(directory, file);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return fallbackFile;
    }

    private static IEnumerable<string> FontDirectories(Skin skin, string fallbackDirectory)
    {
        if (skin.Directory.Length > 0)
        {
            yield return skin.Directory;
        }

        yield return fallbackDirectory;

        string system = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
        if (system.Length > 0)
        {
            yield return system;
        }
    }

    private static Skin Read(JsonElement root, string name, string directory)
    {
        SortedSet<string> missing = new(StringComparer.Ordinal);

        Dictionary<string, Color> colors = new(SkinDefaults.Colors, StringComparer.Ordinal);
        Dictionary<string, float> metrics = new(SkinDefaults.Metrics, StringComparer.Ordinal);
        Dictionary<string, uint> textSizes = new(SkinDefaults.TextSizes, StringComparer.Ordinal);

        foreach (JsonProperty property in root.EnumerateObject())
        {
            switch (property.Name)
            {
                case "name" when property.Value.ValueKind == JsonValueKind.String:
                case "font" when property.Value.ValueKind == JsonValueKind.Object:
                case "colors" when property.Value.ValueKind == JsonValueKind.Object:
                case "metrics" when property.Value.ValueKind == JsonValueKind.Object:
                case "text" when property.Value.ValueKind == JsonValueKind.Object:
                case "parts" when property.Value.ValueKind == JsonValueKind.Object:
                    break;
                default:
                    missing.Add(property.Name);
                    break;
            }
        }

        ReadColors(Group(root, "colors"), colors, missing);
        ReadMetrics(Group(root, "metrics"), metrics, missing);
        ReadTextSizes(Group(root, "text"), textSizes, missing);

        JsonElement font = Group(root, "font");

        return new Skin(
            ReadName(root, name),
            directory,
            FontFile(font, "ui", missing),
            FontFile(font, "mono", missing),
            colors,
            metrics,
            textSizes,
            ReadParts(Group(root, "parts"), missing),
            missing);
    }

    private static string ReadName(JsonElement root, string fallback) =>
        root.TryGetProperty("name", out JsonElement element) && element.ValueKind == JsonValueKind.String
            ? element.GetString() ?? fallback
            : fallback;

    private static string? FontFile(JsonElement font, string key, SortedSet<string> missing)
    {
        if (font.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        foreach (JsonProperty property in font.EnumerateObject())
        {
            if (property.Name is not ("ui" or "mono"))
            {
                missing.Add("font." + property.Name);
            }
        }

        if (!font.TryGetProperty(key, out JsonElement element))
        {
            return null;
        }

        string? value = element.ValueKind == JsonValueKind.String ? element.GetString() : null;
        if (string.IsNullOrWhiteSpace(value))
        {
            missing.Add("font." + key);
            return null;
        }

        return value;
    }

    private static void ReadColors(JsonElement group, Dictionary<string, Color> colors, SortedSet<string> missing)
    {
        foreach ((string key, string? raw) in Entries(group, colors.Keys, "colors", missing))
        {
            if (SkinValueParser.TryColor(raw, out Color color))
            {
                colors[key] = color;
            }
            else
            {
                missing.Add("colors." + key);
            }
        }
    }

    private static void ReadMetrics(JsonElement group, Dictionary<string, float> metrics, SortedSet<string> missing)
    {
        foreach ((string key, string? raw) in Entries(group, metrics.Keys, "metrics", missing))
        {
            if (SkinValueParser.TryNumber(raw, out float number))
            {
                metrics[key] = number;
            }
            else
            {
                missing.Add("metrics." + key);
            }
        }
    }

    private static void ReadTextSizes(JsonElement group, Dictionary<string, uint> textSizes, SortedSet<string> missing)
    {
        foreach ((string key, string? raw) in Entries(group, textSizes.Keys, "text", missing))
        {
            if (SkinValueParser.TryNumber(raw, out float number) && number >= 1f && number <= 512f)
            {
                textSizes[key] = (uint)MathF.Round(number);
            }
            else
            {
                missing.Add("text." + key);
            }
        }
    }

    private static List<(string Key, string? Raw)> Entries(
        JsonElement group,
        IEnumerable<string> known,
        string prefix,
        SortedSet<string> missing)
    {
        List<(string Key, string? Raw)> entries = [];
        if (group.ValueKind != JsonValueKind.Object)
        {
            return entries;
        }

        HashSet<string> allowed = new(known, StringComparer.Ordinal);

        foreach (JsonProperty property in group.EnumerateObject())
        {
            if (!allowed.Contains(property.Name))
            {
                missing.Add(prefix + "." + property.Name);
                continue;
            }

            entries.Add((property.Name, Raw(property.Value)));
        }

        return entries;
    }

    private static Dictionary<PartKey, SurfaceStyle> ReadParts(JsonElement group, SortedSet<string> missing)
    {
        Dictionary<UiPart, JsonElement> bare = [];
        Dictionary<PartKey, JsonElement> overrides = [];

        if (group.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty property in group.EnumerateObject())
            {
                if (property.Value.ValueKind != JsonValueKind.Object || !TryPartKey(property.Name, out PartKey key))
                {
                    missing.Add("parts." + property.Name);
                    continue;
                }

                if (key.State == PartState.Normal)
                {
                    bare[key.Part] = property.Value;
                }
                else
                {
                    overrides[key] = property.Value;
                }
            }
        }

        Dictionary<PartKey, SurfaceStyle> styles = [];

        foreach (UiPart part in Enum.GetValues<UiPart>())
        {
            string prefix = "parts." + PartName(part);

            SurfaceStyle normal = SkinDefaults.Style(part, PartState.Normal);
            bool touched = bare.TryGetValue(part, out JsonElement element);
            if (touched)
            {
                normal = ReadStyle(element, normal, prefix, missing);
            }

            foreach (PartState state in States())
            {
                touched |= overrides.ContainsKey(new PartKey(part, state));
            }

            styles[new PartKey(part, PartState.Normal)] = normal;

            foreach (PartState state in States())
            {
                PartKey key = new(part, state);
                styles[key] = overrides.TryGetValue(key, out JsonElement stateElement)
                    ? ReadStyle(stateElement, normal, prefix + "." + StateName(state), missing)
                    : touched ? normal : SkinDefaults.Style(part, state);
            }
        }

        return styles;
    }

    private static IEnumerable<PartState> States() =>
        Enum.GetValues<PartState>().Where(state => state != PartState.Normal);

    private static SurfaceStyle ReadStyle(JsonElement element, SurfaceStyle basis, string prefix, SortedSet<string> missing)
    {
        SurfaceStyle style = basis;

        foreach (JsonProperty property in element.EnumerateObject())
        {
            string? raw = Raw(property.Value);

            switch (property.Name)
            {
                case "kind" when SkinValueParser.TryEnum(raw, out SurfaceKind kind):
                    style = style with { Kind = kind };
                    break;
                case "fill" when SkinValueParser.TryColor(raw, out Color fill):
                    style = style with { Fill = fill };
                    break;
                case "fillTo" when SkinValueParser.TryColor(raw, out Color fillTo):
                    style = style with { FillTo = fillTo };
                    break;
                case "direction" when SkinValueParser.TryEnum(raw, out GradientDirection direction):
                    style = style with { Direction = direction };
                    break;
                case "border" when SkinValueParser.TryColor(raw, out Color border):
                    style = style with { Border = border };
                    break;
                case "borderThickness" when SkinValueParser.TryNumber(raw, out float thickness):
                    style = style with { BorderThickness = MathF.Max(0f, thickness) };
                    break;
                case "borderEdges" when SkinValueParser.TryEdges(raw, out Edges edges):
                    style = style with { BorderEdges = edges };
                    break;
                case "cornerRadius" when SkinValueParser.TryNumber(raw, out float radius):
                    style = style with { CornerRadius = MathF.Max(0f, radius) };
                    break;
                case "bevel" when SkinValueParser.TryEnum(raw, out BevelKind bevel):
                    style = style with { Bevel = bevel };
                    break;
                case "bevelLight" when SkinValueParser.TryColor(raw, out Color light):
                    style = style with { BevelLight = light };
                    break;
                case "bevelDark" when SkinValueParser.TryColor(raw, out Color dark):
                    style = style with { BevelDark = dark };
                    break;
                case "bevelThickness" when SkinValueParser.TryNumber(raw, out float bevelThickness):
                    style = style with { BevelThickness = MathF.Max(0f, bevelThickness) };
                    break;
                case "foreground" when SkinValueParser.TryColor(raw, out Color foreground):
                    style = style with { Foreground = foreground };
                    break;
                case "shape" when SkinValueParser.TryEnum(raw, out MarkerShape shape):
                    style = style with { Shape = shape };
                    break;
                case "texture" when !string.IsNullOrWhiteSpace(raw):
                    style = style with { Texture = raw };
                    break;
                case "textureInsets" when SkinValueParser.TryInsets(raw, out Insets insets):
                    style = style with { TextureInsets = insets };
                    break;
                default:
                    missing.Add(prefix + "." + property.Name);
                    break;
            }
        }

        return style;
    }

    private static JsonElement Group(JsonElement root, string name) =>
        root.TryGetProperty(name, out JsonElement element) ? element : default;

    private static string? Raw(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Number => element.GetRawText(),
        _ => null,
    };

    private static bool TryPartKey(string name, out PartKey key)
    {
        key = default;

        int separator = name.IndexOf('.', StringComparison.Ordinal);
        string partName = separator < 0 ? name : name[..separator];

        if (!PartsByName.TryGetValue(partName, out UiPart part))
        {
            return false;
        }

        PartState state = PartState.Normal;
        if (separator >= 0 && !SkinValueParser.TryEnum(name[(separator + 1)..], out state))
        {
            return false;
        }

        key = new PartKey(part, state);
        return true;
    }

    private static Dictionary<string, UiPart> BuildPartNames()
    {
        Dictionary<string, UiPart> names = new(StringComparer.Ordinal);

        foreach (UiPart part in Enum.GetValues<UiPart>())
        {
            names[PartName(part)] = part;
        }

        return names;
    }

    private static string PartName(UiPart part) => Camel(part.ToString());

    private static string StateName(PartState state) => Camel(state.ToString());

    private static string Camel(string name) =>
        string.Create(CultureInfo.InvariantCulture, $"{char.ToLowerInvariant(name[0])}{name[1..]}");
}
