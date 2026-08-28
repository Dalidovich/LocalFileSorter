namespace LocalFileSorter.Common.Model;

public sealed record ScanResult(
    IReadOnlyList<FileEntry> Files,
    int SkippedCount,
    IReadOnlyList<string> SkippedExtensions);
