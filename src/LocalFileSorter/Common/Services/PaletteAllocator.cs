using LocalFileSorter.Common.Model;

namespace LocalFileSorter.Common.Services;

public sealed class PaletteAllocator
{
    public static readonly IReadOnlyList<RgbColor> Palette =
    [
        new RgbColor(0x4C, 0x9A, 0xFF),
        new RgbColor(0x36, 0xB3, 0x7E),
        new RgbColor(0xFF, 0xAB, 0x00),
        new RgbColor(0xFF, 0x56, 0x30),
        new RgbColor(0x65, 0x54, 0xC0),
        new RgbColor(0x00, 0xB8, 0xD9),
        new RgbColor(0xFF, 0x7A, 0xB6),
        new RgbColor(0x8F, 0xBF, 0x3F),
        new RgbColor(0xBF, 0x7A, 0x3F),
        new RgbColor(0x9A, 0xA5, 0xB1),
    ];

    private readonly int[] usage = new int[Palette.Count];

    public void MarkUsed(RgbColor color)
    {
        int index = IndexOf(color);
        if (index >= 0)
        {
            usage[index]++;
        }
    }

    public RgbColor Next()
    {
        int chosen = 0;
        for (int index = 1; index < usage.Length; index++)
        {
            if (usage[index] < usage[chosen])
            {
                chosen = index;
            }
        }

        usage[chosen]++;
        return Palette[chosen];
    }

    private static int IndexOf(RgbColor color)
    {
        for (int index = 0; index < Palette.Count; index++)
        {
            if (Palette[index] == color)
            {
                return index;
            }
        }

        return -1;
    }
}
