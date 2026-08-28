using LocalFileSorter.Common.Model;

using SFML.Graphics;

namespace LocalFileSorter.Ui.Rendering;

public static class ColorMap
{
    public static Color ToSfml(this RgbColor color) => new(color.R, color.G, color.B);

    public static Color Mix(Color color, Color onto, float amount) => new(
        Channel(color.R, onto.R, amount),
        Channel(color.G, onto.G, amount),
        Channel(color.B, onto.B, amount));

    private static byte Channel(byte from, byte to, float amount) =>
        (byte)Math.Clamp(MathF.Round(to + ((from - to) * amount)), 0f, 255f);
}
