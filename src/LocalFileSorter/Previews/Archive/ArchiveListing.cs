namespace LocalFileSorter.Previews.Archive;

public sealed record ArchiveListing(
    ArchiveFormat Format,
    IReadOnlyList<ArchiveEntry> Entries,
    int TotalEntryCount,
    long UncompressedBytes,
    bool Truncated);

public readonly record struct ArchiveEntry(string Name, long SizeBytes, bool IsDirectory);

public enum ArchiveFormat
{
    Zip,
    Tar,
    GzippedTar,
    SevenZip,
    Rar,
}
