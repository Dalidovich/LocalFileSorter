using LocalFileSorter.Ui.Theme;

using SFML.Graphics;

using Xunit;

namespace LocalFileSorter.Tests.Ui;

public sealed class SkinLoaderTests : IDisposable
{
    private readonly string themes = Directory.CreateTempSubdirectory("lfs-themes-").FullName;

    public void Dispose() => Directory.Delete(themes, recursive: true);

    [Fact]
    public void FallsBackToDefaultsAndReportsThePath()
    {
        Assert.False(SkinLoader.TryLoad(themes, "missing", out Skin skin, out string path));

        Assert.Same(Skin.BuiltIn, skin);
        Assert.Equal(Path.Combine(themes, "missing", "theme.json"), path);
    }

    [Fact]
    public void SurvivesMalformedJson()
    {
        Write("broken", "{ not json");

        Assert.False(SkinLoader.TryLoad(themes, "broken", out Skin skin, out _));
        Assert.Same(Skin.BuiltIn, skin);
    }

    [Fact]
    public void AppliesOverridesAndDefaultsTheRest()
    {
        Write("partial", """
        {
          "colors": { "textPrimary": "#112233" },
          "metrics": { "queueWidth": "480" },
          "parts": { "button": { "fill": "#445566" } }
        }
        """);

        Assert.True(SkinLoader.TryLoad(themes, "partial", out Skin skin, out _));

        Assert.Equal(new Color(0x11, 0x22, 0x33), skin.Colors["textPrimary"]);
        Assert.Equal(480f, skin.Metrics["queueWidth"]);
        Assert.Equal(new Color(0x44, 0x55, 0x66), skin.Style(UiPart.Button, PartState.Normal).Fill);

        Assert.Equal(SkinDefaults.Colors["textMuted"], skin.Colors["textMuted"]);
        Assert.Equal(SkinDefaults.Metrics["queueRowHeight"], skin.Metrics["queueRowHeight"]);
        Assert.Equal(SkinDefaults.TextSizes["bodySize"], skin.TextSizes["bodySize"]);
        Assert.Equal(SkinDefaults.Style(UiPart.Panel, PartState.Normal), skin.Style(UiPart.Panel, PartState.Normal));
        Assert.Empty(skin.MissingTokens);
    }

    [Fact]
    public void RecordsValuesItCannotUse()
    {
        Write("broken-values", """
        {
          "colors": { "textPrimary": "blue", "textLoudest": "#FFFFFF" },
          "metrics": { "queueWidth": "wide" },
          "parts": { "button": { "kind": "hologram" }, "buton": { "fill": "#000000" } }
        }
        """);

        Assert.True(SkinLoader.TryLoad(themes, "broken-values", out Skin skin, out _));

        Assert.Equal(
            new[] { "colors.textLoudest", "colors.textPrimary", "metrics.queueWidth", "parts.buton", "parts.button.kind" },
            skin.MissingTokens);

        Assert.Equal(SkinDefaults.Colors["textPrimary"], skin.Colors["textPrimary"]);
        Assert.Equal(SkinDefaults.Metrics["queueWidth"], skin.Metrics["queueWidth"]);
        Assert.Equal(SurfaceKind.Solid, skin.Style(UiPart.Button, PartState.Normal).Kind);
    }

    [Fact]
    public void ReadsNameAndFonts()
    {
        Write("named", """
        {
          "name": "named",
          "font": { "ui": "Tahoma.ttf" }
        }
        """);

        Assert.True(SkinLoader.TryLoad(themes, "named", out Skin skin, out string path));

        Assert.Equal("named", skin.Name);
        Assert.Equal("Tahoma.ttf", skin.UiFont);
        Assert.Null(skin.MonoFont);
        Assert.Equal(Path.GetDirectoryName(path), skin.Directory);
    }

    [Fact]
    public void ResolvesFontsAgainstTheSkinFolderFirst()
    {
        Write("fonted", "{ }");
        string inSkin = Path.Combine(themes, "fonted", "Ui.ttf");
        File.WriteAllText(inSkin, "font");

        Assert.True(SkinLoader.TryLoad(themes, "fonted", out Skin skin, out _));

        Assert.Equal(inSkin, SkinLoader.ResolveFont(skin, "Ui.ttf", themes, "fallback.ttf"));
        Assert.Equal("fallback.ttf", SkinLoader.ResolveFont(skin, "Absent.ttf", themes, "fallback.ttf"));
        Assert.Equal("fallback.ttf", SkinLoader.ResolveFont(skin, null, themes, "fallback.ttf"));
    }

    private void Write(string name, string content)
    {
        string directory = Path.Combine(themes, name);
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "theme.json"), content);
    }
}
