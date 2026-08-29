using LocalFileSorter.Ui.Theme;

using SFML.Graphics;

using Xunit;

namespace LocalFileSorter.Tests.Ui;

public sealed class FlatDarkParityTests
{
    private static readonly string Themes = Path.Combine(AppContext.BaseDirectory, "assets", "themes");

    [Fact]
    public void ShippedSkinIsUsable()
    {
        Assert.True(SkinLoader.TryLoad(Themes, "flat-dark", out Skin skin, out _));

        Assert.Equal("flat-dark", skin.Name);
        Assert.Empty(skin.MissingTokens);
    }

    [Fact]
    public void ShippedSkinRepeatsEveryBuiltInToken()
    {
        Skin skin = Load();

        Assert.Equal(SkinDefaults.Colors.Count, skin.Colors.Count);
        Assert.Equal(SkinDefaults.Metrics.Count, skin.Metrics.Count);
        Assert.Equal(SkinDefaults.TextSizes.Count, skin.TextSizes.Count);

        foreach ((string token, Color expected) in SkinDefaults.Colors)
        {
            Assert.Equal(expected, skin.Colors[token]);
        }

        foreach ((string token, float expected) in SkinDefaults.Metrics)
        {
            Assert.Equal(expected, skin.Metrics[token]);
        }

        foreach ((string token, uint expected) in SkinDefaults.TextSizes)
        {
            Assert.Equal(expected, skin.TextSizes[token]);
        }
    }

    [Fact]
    public void ShippedSkinRepeatsEveryBuiltInStyle()
    {
        Skin skin = Load();

        foreach (UiPart part in Enum.GetValues<UiPart>())
        {
            foreach (PartState state in Enum.GetValues<PartState>())
            {
                Assert.Equal(Skin.BuiltIn.Style(part, state), skin.Style(part, state));
            }
        }
    }

    [Fact]
    public void OtherShippedSkinsLoadCleanly()
    {
        foreach (string name in new[] { "classic-9x", "xp" })
        {
            Assert.True(SkinLoader.TryLoad(Themes, name, out Skin skin, out _));
            Assert.Equal(name, skin.Name);
            Assert.Empty(skin.MissingTokens);
        }
    }

    private static Skin Load()
    {
        Assert.True(SkinLoader.TryLoad(Themes, "flat-dark", out Skin skin, out _));
        return skin;
    }
}
