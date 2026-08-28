using System.Globalization;

namespace LocalFileSorter.Common.Model;

public readonly record struct RgbColor(byte R, byte G, byte B)
{
    public string ToHex() => string.Create(
        CultureInfo.InvariantCulture,
        $"#{R:X2}{G:X2}{B:X2}");

    public static bool TryParseHex(string? value, out RgbColor color)
    {
        color = default;

        if (value is null)
        {
            return false;
        }

        ReadOnlySpan<char> digits = value.AsSpan().Trim();
        if (digits.Length == 7 && digits[0] == '#')
        {
            digits = digits[1..];
        }

        if (digits.Length != 6)
        {
            return false;
        }

        if (!TryParseComponent(digits[..2], out byte red)
            || !TryParseComponent(digits[2..4], out byte green)
            || !TryParseComponent(digits[4..], out byte blue))
        {
            return false;
        }

        color = new RgbColor(red, green, blue);
        return true;
    }

    private static bool TryParseComponent(ReadOnlySpan<char> digits, out byte value) =>
        byte.TryParse(digits, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
}
