namespace LocalFileSorter.App.Startup;

public static class PortablePaths
{
    public static string Root { get; } = ResolveRoot();

    public static string Themes { get; } = Path.Combine(Root, "themes");

    public static string Settings { get; } = Path.Combine(Root, "settings.json");

    private static string ResolveRoot()
    {
        string? executable = Environment.ProcessPath;
        string? directory = executable is null ? null : Path.GetDirectoryName(executable);

        return string.IsNullOrEmpty(directory)
            ? AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            : directory;
    }
}
