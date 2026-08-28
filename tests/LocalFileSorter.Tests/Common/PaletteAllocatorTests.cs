using LocalFileSorter.Common.Model;
using LocalFileSorter.Common.Services;

using Xunit;

namespace LocalFileSorter.Tests.Common;

public sealed class PaletteAllocatorTests
{
    [Fact]
    public void HandsOutEveryPaletteColorBeforeRepeating()
    {
        PaletteAllocator allocator = new();

        RgbColor[] handed = [.. Enumerable.Range(0, PaletteAllocator.Palette.Count).Select(_ => allocator.Next())];

        Assert.Equal(PaletteAllocator.Palette.Count, handed.Distinct().Count());
    }

    [Fact]
    public void CyclesOnceThePaletteIsExhausted()
    {
        PaletteAllocator allocator = new();

        for (int index = 0; index < PaletteAllocator.Palette.Count; index++)
        {
            allocator.Next();
        }

        Assert.Equal(PaletteAllocator.Palette[0], allocator.Next());
    }

    [Fact]
    public void SkipsColorsAlreadyInUse()
    {
        PaletteAllocator allocator = new();
        allocator.MarkUsed(PaletteAllocator.Palette[0]);

        Assert.Equal(PaletteAllocator.Palette[1], allocator.Next());
    }

    [Fact]
    public void IgnoresColorsOutsideThePalette()
    {
        PaletteAllocator allocator = new();
        allocator.MarkUsed(new RgbColor(1, 2, 3));

        Assert.Equal(PaletteAllocator.Palette[0], allocator.Next());
    }
}
