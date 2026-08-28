using System.Text.Json;

namespace LocalFileSorter.Common.Persistence;

public sealed class AppSettingsStore
{
    private readonly string filePath;

    public AppSettingsStore(string filePath)
    {
        this.filePath = filePath;
    }

    public static AppSettingsStore ForCurrentUser()
    {
        string directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LocalFileSorter");

        return new AppSettingsStore(Path.Combine(directory, "settings.json"));
    }

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return new AppSettings();
            }

            using FileStream stream = File.OpenRead(filePath);
            return JsonSerializer.Deserialize(stream, AppJsonContext.Default.AppSettings) ?? new AppSettings();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        try
        {
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using FileStream stream = File.Create(filePath);
            JsonSerializer.Serialize(stream, settings, AppJsonContext.Default.AppSettings);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
        }
    }
}
