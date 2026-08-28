using LocalFileSorter.Common.Model;

using Xunit;

namespace LocalFileSorter.Tests.Common;

public sealed class RgbColorTests
{
    [Fact]
    public void FormatsAsUppercaseHexWithHash()
    {
        Assert.Equal("#0A1BFF", new RgbColor(0x0A, 0x1B, 0xFF).ToHex());
    }

    [Theory]
    [InlineData("#4C9AFF")]
    [InlineData("4c9aff")]
    [InlineData("  #4c9AFF  ")]
    public void ParsesHexWithOrWithoutHashAndCase(string value)
    {
        Assert.True(RgbColor.TryParseHex(value, out RgbColor color));
        Assert.Equal(new RgbColor(0x4C, 0x9A, 0xFF), color);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("#4C9AF")]
    [InlineData("#4C9AFFF")]
    [InlineData("#4C9AGG")]
    public void RejectsAnythingElse(string? value)
    {
        Assert.False(RgbColor.TryParseHex(value, out _));
    }
}
