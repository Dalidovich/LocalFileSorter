using LocalFileSorter.Ui.Theme;

using SFML.Graphics;

using Xunit;

namespace LocalFileSorter.Tests.Ui;

public sealed class SkinTests : IDisposable
{
    private readonly string themes = Directory.CreateTempSubdirectory("lfs-skin-").FullName;

    public void Dispose() => Directory.Delete(themes, recursive: true);

    [Fact]
    public void StateInheritsThePart()
    {
        Skin skin = Load("""
        {
          "parts": {
            "button": { "fill": "#101010", "border": "#202020", "foreground": "#303030" },
            "button.hover": { "fill": "#404040" }
          }
        }
        """);

        SurfaceStyle hover = skin.Style(UiPart.Button, PartState.Hover);

        Assert.Equal(new Color(0x40, 0x40, 0x40), hover.Fill);
        Assert.Equal(new Color(0x20, 0x20, 0x20), hover.Border);
        Assert.Equal(new Color(0x30, 0x30, 0x30), hover.Foreground);
    }

    [Fact]
    public void UnnamedStateOfATouchedPartFallsBackToItsNormal()
    {
        Skin skin = Load("""
        { "parts": { "button": { "fill": "#101010" } } }
        """);

        Assert.Equal(
            skin.Style(UiPart.Button, PartState.Normal),
            skin.Style(UiPart.Button, PartState.Disabled));
    }

    [Fact]
    public void UntouchedPartKeepsItsBuiltInStates()
    {
        Skin skin = Load("{ }");

        Assert.Equal(
            SkinDefaults.Style(UiPart.Button, PartState.Disabled),
            skin.Style(UiPart.Button, PartState.Disabled));
    }

    [Fact]
    public void StateKeyAloneLeavesTheOtherStatesOnTheDefaultNormal()
    {
        Skin skin = Load("""
        { "parts": { "bucketRow.hover": { "fill": "#101010" } } }
        """);

        Assert.Equal(new Color(0x10, 0x10, 0x10), skin.Style(UiPart.BucketRow, PartState.Hover).Fill);
        Assert.Equal(
            SkinDefaults.Style(UiPart.BucketRow, PartState.Normal),
            skin.Style(UiPart.BucketRow, PartState.Active));
    }

    [Fact]
    public void RecordsEachUnusableTokenOnce()
    {
        Skin skin = Load("""
        { "metrics": { "queueWidth": "wide", "queueWidth": "wider" } }
        """);

        Assert.Equal(new[] { "metrics.queueWidth" }, skin.MissingTokens);
    }

    [Fact]
    public void ForegroundFallsBackToPrimaryText()
    {
        Skin skin = Load("""
        {
          "colors": { "textPrimary": "#123456" },
          "parts": { "queueRow": { "fill": "#101010" } }
        }
        """);

        Assert.Null(skin.Style(UiPart.QueueRow, PartState.Normal).Foreground);
        Assert.Equal(new Color(0x12, 0x34, 0x56), skin.Foreground(UiPart.QueueRow, PartState.Normal));
    }

    private Skin Load(string content)
    {
        string directory = Path.Combine(themes, "under-test");
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "theme.json"), content);

        Assert.True(SkinLoader.TryLoad(themes, "under-test", out Skin skin, out _));
        return skin;
    }
}
