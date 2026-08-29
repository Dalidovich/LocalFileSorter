using System.Text.Json;

namespace LocalFileSorter.Common.Persistence;

public sealed class AppSettingsStore
{
    private readonly string filePath;

    public AppSettingsStore(string filePath)
    {
        this.filePath = filePath;
    }

    public string FilePath => filePath;

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

    public bool Save(AppSettings settings)
    {
        try
        {
            using FileStream stream = File.Create(filePath);
            JsonSerializer.Serialize(stream, settings, AppJsonContext.Default.AppSettings);
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }
}
