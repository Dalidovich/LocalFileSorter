namespace LocalFileSorter.Common.Abstractions;

public sealed record PreviewDocument(IReadOnlyList<PreviewBlock> Blocks);

public abstract record PreviewBlock;

public sealed record TextBlock(string Text, TextStyle Style) : PreviewBlock;

public sealed record ImageBlock(int Width, int Height, byte[] Rgba) : PreviewBlock;

public sealed record KeyValueBlock(IReadOnlyList<MetadataItem> Items) : PreviewBlock;

public sealed record MessageBlock(string Text, MessageKind Kind) : PreviewBlock;

public readonly record struct TextStyle(TextPitch Pitch, TextSizeClass SizeClass)
{
    public static TextStyle Mono => new(TextPitch.Mono, TextSizeClass.Body);

    public static TextStyle Proportional => new(TextPitch.Proportional, TextSizeClass.Body);
}

public enum TextPitch
{
    Proportional,
    Mono,
}

public enum TextSizeClass
{
    Body,
    Heading,
}

public enum MessageKind
{
    Info,
    Error,
}
