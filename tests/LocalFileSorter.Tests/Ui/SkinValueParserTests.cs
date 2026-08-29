using LocalFileSorter.Ui.Theme;

using SFML.Graphics;

using Xunit;

namespace LocalFileSorter.Tests.Ui;

public sealed class SkinValueParserTests
{
    [Theory]
    [InlineData("#FFF", 255, 255, 255, 255)]
    [InlineData("#7AA2F7", 122, 162, 247, 255)]
    [InlineData("#0A0B0EBE", 10, 11, 14, 190)]
    [InlineData("#abc", 170, 187, 204, 255)]
    public void ParsesColors(string value, byte r, byte g, byte b, byte a)
    {
        Assert.True(SkinValueParser.TryColor(value, out Color color));
        Assert.Equal(new Color(r, g, b, a), color);
    }

    [Theory]
    [InlineData("7AA2F7")]
    [InlineData("#7AA2F")]
    [InlineData("#GGGGGG")]
    [InlineData("#")]
    [InlineData("")]
    [InlineData(null)]
    public void RejectsMalformedColors(string? value)
    {
        Assert.False(SkinValueParser.TryColor(value, out Color color));
        Assert.Equal(Color.Transparent, color);
    }

    [Theory]
    [InlineData("320", 320f)]
    [InlineData("0.16", 0.16f)]
    [InlineData("-2", -2f)]
    public void ParsesNumbers(string value, float expected)
    {
        Assert.True(SkinValueParser.TryNumber(value, out float number));
        Assert.Equal(expected, number);
    }

    [Theory]
    [InlineData("320px")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData(null)]
    public void RejectsMalformedNumbers(string? value) =>
        Assert.False(SkinValueParser.TryNumber(value, out _));

    [Fact]
    public void ParsesInsets()
    {
        Assert.True(SkinValueParser.TryInsets("6,4,6, 4", out Insets insets));
        Assert.Equal(new Insets(6f, 4f, 6f, 4f), insets);
    }

    [Theory]
    [InlineData("6,4,6")]
    [InlineData("6,4,6,4,4")]
    [InlineData("a,b,c,d")]
    public void RejectsMalformedInsets(string value) =>
        Assert.False(SkinValueParser.TryInsets(value, out _));

    [Theory]
    [InlineData("all", Edges.All)]
    [InlineData("none", Edges.None)]
    [InlineData("top", Edges.Top)]
    [InlineData("top left", Edges.Top | Edges.Left)]
    public void ParsesEdges(string value, Edges expected)
    {
        Assert.True(SkinValueParser.TryEdges(value, out Edges edges));
        Assert.Equal(expected, edges);
    }

    [Fact]
    public void RejectsUnknownEdge() => Assert.False(SkinValueParser.TryEdges("top middle", out _));

    [Theory]
    [InlineData("vertical", GradientDirection.Vertical)]
    [InlineData("horizontal", GradientDirection.Horizontal)]
    public void ParsesEnums(string value, GradientDirection expected)
    {
        Assert.True(SkinValueParser.TryEnum(value, out GradientDirection direction));
        Assert.Equal(expected, direction);
    }

    [Theory]
    [InlineData("diagonal")]
    [InlineData("1")]
    [InlineData("")]
    public void RejectsUnknownEnums(string value) =>
        Assert.False(SkinValueParser.TryEnum(value, out GradientDirection _));
}
