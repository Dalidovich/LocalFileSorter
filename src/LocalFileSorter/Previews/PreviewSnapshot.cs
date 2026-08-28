using LocalFileSorter.Common.Abstractions;
using LocalFileSorter.Common.Model;

namespace LocalFileSorter.Previews;

public sealed record PreviewSnapshot(FileId FileId, bool IsLoading, PreviewResult? Result)
{
    public static PreviewSnapshot Loading(FileId fileId) => new(fileId, true, null);
}
