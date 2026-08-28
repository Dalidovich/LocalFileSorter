using LocalFileSorter.Common.Model;

namespace LocalFileSorter.Common.Abstractions;

public interface IPreviewProvider
{
    string Id { get; }

    int Priority { get; }

    IReadOnlySet<string> Extensions { get; }

    bool CanHandle(FileEntry entry);

    PreviewResult Load(FileEntry entry, PreviewBudget budget);
}
