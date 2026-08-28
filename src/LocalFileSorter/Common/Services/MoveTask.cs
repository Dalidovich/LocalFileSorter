using LocalFileSorter.Common.Model;

namespace LocalFileSorter.Common.Services;

public sealed record MoveTask(FileEntry File, Bucket Bucket);
