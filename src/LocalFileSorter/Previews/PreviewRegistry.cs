using LocalFileSorter.Common.Abstractions;
using LocalFileSorter.Common.Localization;
using LocalFileSorter.Common.Model;

namespace LocalFileSorter.Previews;

public sealed class PreviewRegistry
{
    private readonly IReadOnlyList<IPreviewProvider> providers;
    private readonly Strings strings;

    public PreviewRegistry(Strings strings, IEnumerable<IPreviewProvider> providers)
    {
        this.strings = strings;
        this.providers = [.. providers.OrderByDescending(provider => provider.Priority)];
        SupportedExtensions = this.providers
            .SelectMany(provider => provider.Extensions)
            .ToHashSet(StringComparer.Ordinal);
    }

    public IReadOnlySet<string> SupportedExtensions { get; }

    public IPreviewProvider? Resolve(FileEntry entry) =>
        providers.FirstOrDefault(provider => provider.Extensions.Contains(entry.Extension) && provider.CanHandle(entry));

    public PreviewResult Load(FileEntry entry, PreviewBudget budget)
    {
        IPreviewProvider? provider = Resolve(entry);
        if (provider is null)
        {
            return PreviewResult.Failed(string.Format(
                strings.PreviewNoModule,
                entry.Extension.Length == 0 ? entry.Name : entry.Extension));
        }

        try
        {
            return provider.Load(entry, budget);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return PreviewResult.Failed(string.Format(strings.PreviewModuleFailed, provider.Id, exception.Message));
        }
    }
}
