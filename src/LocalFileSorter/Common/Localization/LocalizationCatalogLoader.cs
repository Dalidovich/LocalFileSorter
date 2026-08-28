using System.Text.Json;

using LocalFileSorter.Common.Persistence;

namespace LocalFileSorter.Common.Localization;

public static class LocalizationCatalogLoader
{
    public const string DefaultLanguage = "en";

    public static string CatalogPath(string i18nDirectory, string language) =>
        Path.Combine(i18nDirectory, language + ".json");

    public static bool TryLoad(string i18nDirectory, string language, out LocalizationCatalog catalog, out string catalogPath)
    {
        catalogPath = CatalogPath(i18nDirectory, language);
        if (!File.Exists(catalogPath))
        {
            catalog = new LocalizationCatalog(language, new Dictionary<string, string>(StringComparer.Ordinal));
            return false;
        }

        using FileStream stream = File.OpenRead(catalogPath);
        Dictionary<string, string> entries =
            JsonSerializer.Deserialize(stream, AppJsonContext.Default.DictionaryStringString)
            ?? new Dictionary<string, string>(StringComparer.Ordinal);

        catalog = new LocalizationCatalog(language, entries);
        return true;
    }
}
