namespace LocalFileSorter.Common.Abstractions;

public sealed record PreviewResult(
    PreviewDocument? Document,
    IReadOnlyList<MetadataItem> ExtraMetadata,
    string? Error)
{
    public static PreviewResult Failed(string error) =>
        new(new PreviewDocument([new MessageBlock(error, MessageKind.Error)]), [], error);
}
