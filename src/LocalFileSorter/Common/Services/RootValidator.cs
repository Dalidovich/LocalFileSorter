namespace LocalFileSorter.Common.Services;

public static class RootValidator
{
    public static RootValidation ValidateSource(string? path) => Validate(path, requireWritable: false);

    public static RootValidation ValidateDestination(string? path) => Validate(path, requireWritable: true);

    public static RootValidation ValidatePair(string sourceFullPath, string destinationFullPath)
    {
        if (AreSame(sourceFullPath, destinationFullPath))
        {
            return RootValidation.Fail(RootProblem.RootsEqual, destinationFullPath);
        }

        if (IsInside(destinationFullPath, sourceFullPath))
        {
            return RootValidation.Fail(RootProblem.DestinationInsideSource, destinationFullPath);
        }

        return RootValidation.Ok(destinationFullPath);
    }

    public static string Normalize(string path) =>
        Path.TrimEndingDirectorySeparator(Path.GetFullPath(path.Trim().Trim('"')));

    private static RootValidation Validate(string? path, bool requireWritable)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return RootValidation.Fail(RootProblem.PathRequired, string.Empty);
        }

        string fullPath;
        try
        {
            fullPath = Normalize(path);
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return RootValidation.Fail(RootProblem.NotFound, path);
        }

        if (!Directory.Exists(fullPath))
        {
            return RootValidation.Fail(RootProblem.NotFound, fullPath);
        }

        if (!IsReadable(fullPath))
        {
            return RootValidation.Fail(RootProblem.NotReadable, fullPath);
        }

        if (requireWritable && !IsWritable(fullPath))
        {
            return RootValidation.Fail(RootProblem.NotWritable, fullPath);
        }

        return RootValidation.Ok(fullPath);
    }

    private static bool IsReadable(string fullPath)
    {
        try
        {
            using IEnumerator<string> enumerator = Directory.EnumerateFileSystemEntries(fullPath).GetEnumerator();
            enumerator.MoveNext();
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static bool IsWritable(string fullPath)
    {
        string probe = Path.Combine(fullPath, ".localfilesorter-write-probe-" + Guid.NewGuid().ToString("N"));
        try
        {
            using (File.Create(probe, 1, FileOptions.DeleteOnClose))
            {
            }

            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static bool AreSame(string left, string right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

    private static bool IsInside(string candidate, string container)
    {
        string prefix = container + Path.DirectorySeparatorChar;
        return candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }
}
