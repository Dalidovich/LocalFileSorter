namespace LocalFileSorter.Common.Model;

public sealed record Bucket
{
    public required BucketId Id { get; init; }

    public required string Name { get; init; }

    public required string DirectoryPath { get; init; }

    public required RgbColor Color { get; set; }

    public required int Order { get; init; }

    public required int ExistingFileCount { get; set; }
}
