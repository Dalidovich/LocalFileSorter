using System.Globalization;

using SFML.Graphics;

namespace LocalFileSorter.Ui.Theme;

public static class SkinValueParser
{
    public static bool TryColor(string? value, out Color color)
    {
        color = Color.Transparent;
        if (value is null || value.Length < 4 || value[0] != '#')
        {
            return false;
        }

        ReadOnlySpan<char> digits = value.AsSpan(1);
        if (digits.Length is not (3 or 6 or 8))
        {
            return false;
        }

        Span<byte> channels = stackalloc byte[4];
        channels[3] = 255;

        if (digits.Length == 3)
        {
            for (int index = 0; index < 3; index++)
            {
                if (!TryNibble(digits[index], out byte nibble))
                {
                    return false;
                }

                channels[index] = (byte)((nibble << 4) | nibble);
            }
        }
        else
        {
            for (int index = 0; index < digits.Length / 2; index++)
            {
                if (!TryNibble(digits[index * 2], out byte high) || !TryNibble(digits[(index * 2) + 1], out byte low))
                {
                    return false;
                }

                channels[index] = (byte)((high << 4) | low);
            }
        }

        color = new Color(channels[0], channels[1], channels[2], channels[3]);
        return true;
    }

    public static bool TryNumber(string? value, out float number)
    {
        number = 0f;
        return value is not null
            && float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out number)
            && float.IsFinite(number);
    }

    public static bool TryInsets(string? value, out Insets insets)
    {
        insets = default;
        if (value is null)
        {
            return false;
        }

        string[] parts = value.Split(',');
        if (parts.Length != 4)
        {
            return false;
        }

        Span<float> sides = stackalloc float[4];
        for (int index = 0; index < 4; index++)
        {
            if (!TryNumber(parts[index].Trim(), out sides[index]))
            {
                return false;
            }
        }

        insets = new Insets(sides[0], sides[1], sides[2], sides[3]);
        return true;
    }

    public static bool TryEdges(string? value, out Edges edges)
    {
        edges = Edges.None;
        if (value is null)
        {
            return false;
        }

        foreach (string name in value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            switch (name.ToLowerInvariant())
            {
                case "all":
                    edges |= Edges.All;
                    break;
                case "none":
                    break;
                case "left":
                    edges |= Edges.Left;
                    break;
                case "top":
                    edges |= Edges.Top;
                    break;
                case "right":
                    edges |= Edges.Right;
                    break;
                case "bottom":
                    edges |= Edges.Bottom;
                    break;
                default:
                    return false;
            }
        }

        return true;
    }

    public static bool TryEnum<TEnum>(string? value, out TEnum result)
        where TEnum : struct, Enum
    {
        result = default;
        return value is not null
            && value.Length > 0
            && !char.IsDigit(value[0])
            && Enum.TryParse(value, ignoreCase: true, out result)
            && Enum.IsDefined(result);
    }

    private static bool TryNibble(char character, out byte value)
    {
        value = character switch
        {
            >= '0' and <= '9' => (byte)(character - '0'),
            >= 'a' and <= 'f' => (byte)(character - 'a' + 10),
            >= 'A' and <= 'F' => (byte)(character - 'A' + 10),
            _ => byte.MaxValue,
        };

        return value != byte.MaxValue;
    }
}
