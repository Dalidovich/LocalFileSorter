namespace LocalFileSorter.Common.Persistence;

public sealed class AppSettings
{
    public int Version { get; set; } = 1;

    public string Language { get; set; } = "en";

    public string Theme { get; set; } = "flat-dark";

    public int WindowWidth { get; set; } = 1600;

    public int WindowHeight { get; set; } = 900;
}
