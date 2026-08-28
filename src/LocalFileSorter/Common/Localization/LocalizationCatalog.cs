namespace LocalFileSorter.Common.Localization;

public sealed class LocalizationCatalog
{
    private readonly IReadOnlyDictionary<string, string> entries;
    private readonly SortedSet<string> missingKeys = new(StringComparer.Ordinal);

    public LocalizationCatalog(string language, IReadOnlyDictionary<string, string> entries)
    {
        Language = language;
        this.entries = entries;
    }

    public string Language { get; }

    public IReadOnlyCollection<string> MissingKeys => missingKeys;

    public string Resolve(string key)
    {
        if (entries.TryGetValue(key, out string? value) && !string.IsNullOrEmpty(value))
        {
            return value;
        }

        missingKeys.Add(key);
        return key;
    }
}
