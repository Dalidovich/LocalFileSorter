namespace LocalFileSorter.Common.Model;

public sealed record FileEntry
{
    public required FileId Id { get; init; }

    public required string CurrentPath { get; set; }

    public required string Name { get; init; }

    public required string Extension { get; init; }

    public required long SizeBytes { get; init; }

    public required DateTime CreatedUtc { get; init; }

    public required DateTime ModifiedUtc { get; init; }

    public FileState State { get; set; } = FileState.Pending;

    public BucketId? AssignedBucket { get; set; }

    public string? FailureReason { get; set; }
}
