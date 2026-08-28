namespace LocalFileSorter.Common.Services;

public static class UniqueName
{
    public const int MaxAttempts = 10000;

    public static bool TryResolve(string directory, string fileName, out string path)
    {
        path = Path.Combine(directory, fileName);
        if (IsFree(path))
        {
            return true;
        }

        string stem = Path.GetFileNameWithoutExtension(fileName);
        string extension = Path.GetExtension(fileName);

        for (int counter = 1; counter <= MaxAttempts; counter++)
        {
            path = Path.Combine(directory, $"{stem} ({counter}){extension}");
            if (IsFree(path))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsFree(string path) => !File.Exists(path) && !Directory.Exists(path);
}
