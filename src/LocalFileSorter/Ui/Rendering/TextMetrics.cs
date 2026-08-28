using SFML.Graphics;

namespace LocalFileSorter.Ui.Rendering;

public sealed class TextMetrics
{
    private const string Ellipsis = "…";

    private readonly Font font;
    private readonly uint characterSize;
    private readonly Dictionary<char, float> advances = [];

    public TextMetrics(Font font, uint characterSize)
    {
        this.font = font;
        this.characterSize = characterSize;
        LineHeight = font.GetLineSpacing(characterSize);
    }

    public float LineHeight { get; }

    public float Advance(char character)
    {
        if (advances.TryGetValue(character, out float cached))
        {
            return cached;
        }

        float advance = font.GetGlyph(character, characterSize, bold: false, outlineThickness: 0f).Advance;
        advances[character] = advance;
        return advance;
    }

    public float Measure(ReadOnlySpan<char> value)
    {
        float width = 0f;
        foreach (char character in value)
        {
            width += Advance(character);
        }

        return width;
    }

    public IReadOnlyList<string> Wrap(string value, float maxWidth)
    {
        List<string> lines = [];
        string current = string.Empty;

        foreach (string word in value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            string candidate = current.Length == 0 ? word : current + " " + word;
            if (current.Length > 0 && Measure(candidate) > maxWidth)
            {
                lines.Add(current);
                current = word;
                continue;
            }

            current = candidate;
        }

        if (current.Length > 0)
        {
            lines.Add(current);
        }

        return lines;
    }

    public string Truncate(string value, float maxWidth)
    {
        if (Measure(value) <= maxWidth)
        {
            return value;
        }

        float budget = maxWidth - Measure(Ellipsis);
        if (budget <= 0f)
        {
            return Ellipsis;
        }

        float width = 0f;
        int taken = 0;
        while (taken < value.Length)
        {
            float advance = Advance(value[taken]);
            if (width + advance > budget)
            {
                break;
            }

            width += advance;
            taken++;
        }

        return value[..taken] + Ellipsis;
    }
}
