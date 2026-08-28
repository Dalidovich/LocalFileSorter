using LocalFileSorter.Common.Model;

namespace LocalFileSorter.Common.Services;

public sealed record MappingResult(
    IReadOnlyList<Bucket> Buckets,
    IReadOnlyList<string> Added,
    IReadOnlyList<Bucket> Removed);
