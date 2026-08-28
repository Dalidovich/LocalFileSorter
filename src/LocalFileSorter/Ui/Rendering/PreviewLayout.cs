using System.Text;

using LocalFileSorter.Common.Abstractions;
using LocalFileSorter.Ui.Theme;

using SFML.Graphics;

namespace LocalFileSorter.Ui.Rendering;

public sealed record PreviewLine(string Text, bool Mono, uint Size, Color Color, float Top, float Height);

public sealed record PreviewImage(ImageBlock Block, float Left, float Top, float Width, float Height);

public sealed class PreviewLayout
{
    private const int TabWidth = 4;

    private PreviewLayout(IReadOnlyList<PreviewLine> lines, IReadOnlyList<PreviewImage> images, float height)
    {
        Lines = lines;
        Images = images;
        Height = height;
    }

    public IReadOnlyList<PreviewLine> Lines { get; }

    public IReadOnlyList<PreviewImage> Images { get; }

    public float Height { get; }

    public static PreviewLayout Build(Painter painter, PreviewDocument document, float width, float height)
    {
        List<PreviewLine> lines = [];
        List<PreviewImage> images = [];
        float top = 0f;

        foreach (PreviewBlock block in document.Blocks)
        {
            if (top > 0f)
            {
                top += painter.Metrics(mono: false, UiTheme.BodyTextSize).LineHeight / 2f;
            }

            switch (block)
            {
                case TextBlock text:
                    AppendText(painter, text, width, lines, ref top);
                    break;
                case ImageBlock image:
                    AppendImage(image, width, height, images, ref top);
                    break;
                case KeyValueBlock keyValues:
                    AppendKeyValues(painter, keyValues, width, lines, ref top);
                    break;
                case MessageBlock message:
                    AppendMessage(painter, message, width, lines, ref top);
                    break;
            }
        }

        return new PreviewLayout(lines, images, top);
    }

    private static void AppendImage(ImageBlock block, float width, float height, List<PreviewImage> images, ref float top)
    {
        float scale = MathF.Min(1f, MathF.Min(width / block.Width, height / block.Height));
        if (!float.IsFinite(scale) || scale <= 0f)
        {
            scale = 1f;
        }

        float drawnWidth = block.Width * scale;
        float drawnHeight = block.Height * scale;

        images.Add(new PreviewImage(block, MathF.Max(0f, (width - drawnWidth) / 2f), top, drawnWidth, drawnHeight));
        top += drawnHeight;
    }

    private static void AppendText(Painter painter, TextBlock block, float width, List<PreviewLine> lines, ref float top)
    {
        bool mono = block.Style.Pitch == TextPitch.Mono;
        uint size = SizeOf(block.Style, mono);
        TextMetrics metrics = painter.Metrics(mono, size);

        foreach (string source in block.Text.Split('\n'))
        {
            foreach (string wrapped in Wrap(metrics, ExpandTabs(source), width))
            {
                lines.Add(new PreviewLine(wrapped, mono, size, UiTheme.TextPrimary, top, metrics.LineHeight));
                top += metrics.LineHeight;
            }
        }
    }

    private static void AppendKeyValues(Painter painter, KeyValueBlock block, float width, List<PreviewLine> lines, ref float top)
    {
        TextMetrics metrics = painter.Metrics(mono: false, UiTheme.BodyTextSize);

        foreach (MetadataItem item in block.Items)
        {
            foreach (string wrapped in Wrap(metrics, item.Label + ": " + item.Value, width))
            {
                lines.Add(new PreviewLine(wrapped, false, UiTheme.BodyTextSize, UiTheme.TextMuted, top, metrics.LineHeight));
                top += metrics.LineHeight;
            }
        }
    }

    private static void AppendMessage(Painter painter, MessageBlock block, float width, List<PreviewLine> lines, ref float top)
    {
        TextMetrics metrics = painter.Metrics(mono: false, UiTheme.BodyTextSize);
        Color color = block.Kind == MessageKind.Error ? UiTheme.MessageError : UiTheme.MessageInfo;

        foreach (string wrapped in Wrap(metrics, block.Text, width))
        {
            lines.Add(new PreviewLine(wrapped, false, UiTheme.BodyTextSize, color, top, metrics.LineHeight));
            top += metrics.LineHeight;
        }
    }

    private static uint SizeOf(TextStyle style, bool mono) => style.SizeClass switch
    {
        TextSizeClass.Heading => UiTheme.HeaderTextSize,
        _ => mono ? UiTheme.MonoTextSize : UiTheme.BodyTextSize,
    };

    private static string ExpandTabs(string value)
    {
        if (!value.Contains('\t', StringComparison.Ordinal))
        {
            return value;
        }

        StringBuilder builder = new(value.Length + TabWidth);
        foreach (char character in value)
        {
            if (character == '\t')
            {
                builder.Append(' ', TabWidth - (builder.Length % TabWidth));
            }
            else
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }

    private static List<string> Wrap(TextMetrics metrics, string value, float maxWidth)
    {
        List<string> result = [];
        if (value.Length == 0 || maxWidth <= 0f)
        {
            result.Add(value);
            return result;
        }

        int lineStart = 0;
        int breakAt = -1;
        float width = 0f;

        for (int index = 0; index < value.Length; index++)
        {
            char character = value[index];
            float advance = metrics.Advance(character);

            if (width + advance > maxWidth && index > lineStart)
            {
                int end = breakAt > lineStart ? breakAt : index;
                result.Add(value[lineStart..end]);
                lineStart = breakAt > lineStart ? breakAt + 1 : index;
                breakAt = -1;
                width = metrics.Measure(value.AsSpan(lineStart, index - lineStart));
            }

            width += advance;
            if (character == ' ')
            {
                breakAt = index;
            }
        }

        result.Add(value[lineStart..]);
        return result;
    }
}
