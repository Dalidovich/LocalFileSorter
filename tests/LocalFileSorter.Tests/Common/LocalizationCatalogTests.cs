using LocalFileSorter.Common.Localization;

using Xunit;

namespace LocalFileSorter.Tests.Common;

public sealed class LocalizationCatalogTests
{
    [Fact]
    public void ResolvesKnownKey()
    {
        LocalizationCatalog catalog = Catalog(("panel.queue", "Queue"));

        Assert.Equal("Queue", catalog.Resolve("panel.queue"));
        Assert.Empty(catalog.MissingKeys);
    }

    [Fact]
    public void FallsBackToKeyAndRecordsIt()
    {
        LocalizationCatalog catalog = Catalog();

        Assert.Equal("panel.queue", catalog.Resolve("panel.queue"));
        Assert.Equal(new[] { "panel.queue" }, catalog.MissingKeys);
    }

    [Fact]
    public void RecordsEachMissingKeyOnce()
    {
        LocalizationCatalog catalog = Catalog();

        catalog.Resolve("a");
        catalog.Resolve("a");
        catalog.Resolve("b");

        Assert.Equal(new[] { "a", "b" }, catalog.MissingKeys);
    }

    [Fact]
    public void TreatsEmptyValueAsMissing()
    {
        LocalizationCatalog catalog = Catalog(("panel.queue", ""));

        Assert.Equal("panel.queue", catalog.Resolve("panel.queue"));
    }

    [Fact]
    public void ShippedEnglishCatalogCoversEveryStringsMember()
    {
        string i18n = Path.Combine(AppContext.BaseDirectory, "assets", "i18n");
        Assert.True(LocalizationCatalogLoader.TryLoad(i18n, "en", out LocalizationCatalog catalog, out _));

        _ = new Strings(catalog);

        Assert.Empty(catalog.MissingKeys);
    }

    private static LocalizationCatalog Catalog(params (string Key, string Value)[] entries) =>
        new("en", entries.ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal));
}
