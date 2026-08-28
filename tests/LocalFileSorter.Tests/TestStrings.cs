using LocalFileSorter.Common.Localization;

namespace LocalFileSorter.Tests;

public static class TestStrings
{
    public static Strings Shipped()
    {
        string i18n = Path.Combine(AppContext.BaseDirectory, "assets", "i18n");
        LocalizationCatalogLoader.TryLoad(i18n, LocalizationCatalogLoader.DefaultLanguage, out LocalizationCatalog catalog, out _);
        return new Strings(catalog);
    }
}
