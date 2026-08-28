using LocalFileSorter.Common.Model;
using LocalFileSorter.Common.Services;

namespace LocalFileSorter.Tests;

public static class TestSession
{
    public static SortSession With(int fileCount, params string[] bucketNames) =>
        new("src", "dst", new ScanResult(Files(fileCount), 0, []), Buckets(bucketNames));

    public static IReadOnlyList<FileEntry> Files(int count) =>
    [
        .. Enumerable.Range(0, count).Select(index => new FileEntry
        {
            Id = new FileId(index),
            CurrentPath = $"src/f{index}.txt",
            Name = $"f{index}.txt",
            Extension = ".txt",
            SizeBytes = 1,
            CreatedUtc = DateTime.UnixEpoch,
            ModifiedUtc = DateTime.UnixEpoch,
        }),
    ];

    public static IReadOnlyList<Bucket> Buckets(params string[] names) =>
    [
        .. names.Select((name, index) => new Bucket
        {
            Id = BucketId.FromName(name),
            Name = name,
            DirectoryPath = Path.Combine("dst", name),
            Color = PaletteAllocator.Palette[index % PaletteAllocator.Palette.Count],
            Order = index,
            ExistingFileCount = 0,
        }),
    ];
}
