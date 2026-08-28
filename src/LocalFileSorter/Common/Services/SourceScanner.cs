using LocalFileSorter.Common.Model;

namespace LocalFileSorter.Common.Services;

public static class SourceScanner
{
    public static ScanResult Scan(string sourceRoot, IReadOnlySet<string> supportedExtensions)
    {
        List<FileEntry> files = [];
        SortedSet<string> skippedExtensions = new(StringComparer.Ordinal);
        int skipped = 0;
        int nextId = 0;

        foreach (string path in Enumerate(sourceRoot).OrderBy(path => Path.GetRelativePath(sourceRoot, path), StringComparer.OrdinalIgnoreCase))
        {
            string extension = NormalizeExtension(path);

            if (!supportedExtensions.Contains(extension))
            {
                skipped++;
                skippedExtensions.Add(extension);
                continue;
            }

            FileEntry? entry = Describe(path, extension, new FileId(nextId));
            if (entry is null)
            {
                skipped++;
                skippedExtensions.Add(extension);
                continue;
            }

            files.Add(entry);
            nextId++;
        }

        return new ScanResult(files, skipped, [.. skippedExtensions]);
    }

    public static string NormalizeExtension(string path) => Path.GetExtension(path).ToLowerInvariant();

    private static IReadOnlyList<string> Enumerate(string sourceRoot)
    {
        EnumerationOptions options = new()
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            AttributesToSkip = FileAttributes.ReparsePoint,
        };

        try
        {
            return [.. Directory.EnumerateFiles(sourceRoot, "*", options)];
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return [];
        }
    }

    private static FileEntry? Describe(string path, string extension, FileId id)
    {
        try
        {
            FileInfo info = new(path);
            return new FileEntry
            {
                Id = id,
                CurrentPath = info.FullName,
                Name = info.Name,
                Extension = extension,
                SizeBytes = info.Length,
                CreatedUtc = info.CreationTimeUtc,
                ModifiedUtc = info.LastWriteTimeUtc,
            };
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }
}
