using LocalFileSorter.Common.Abstractions;
using LocalFileSorter.Common.Model;

namespace LocalFileSorter.Previews;

public sealed class PreviewLoader : IDisposable
{
    private readonly PreviewRegistry registry;
    private readonly Lock gate = new();

    private CancellationTokenSource? cancellation;
    private PreviewSnapshot? current;
    private PreviewSnapshot? completed;

    public PreviewLoader(PreviewRegistry registry)
    {
        this.registry = registry;
    }

    public PreviewSnapshot? Update(FileEntry? active)
    {
        if (active is null)
        {
            Cancel();
            current = null;
            return null;
        }

        if (current is null || current.FileId != active.Id)
        {
            Start(active);
        }

        lock (gate)
        {
            if (completed is not null && completed.FileId == current!.FileId)
            {
                current = completed;
            }

            completed = null;
        }

        return current;
    }

    public void Dispose() => Cancel();

    private void Start(FileEntry entry)
    {
        Cancel();

        CancellationTokenSource source = new();
        cancellation = source;
        current = PreviewSnapshot.Loading(entry.Id);

        lock (gate)
        {
            completed = null;
        }

        PreviewBudget budget = PreviewBudget.Default(source.Token);
        _ = Task.Run(() => Run(entry, budget, source.Token), source.Token);
    }

    private void Run(FileEntry entry, PreviewBudget budget, CancellationToken token)
    {
        PreviewResult result;
        try
        {
            result = registry.Load(entry, budget);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (token.IsCancellationRequested)
        {
            return;
        }

        lock (gate)
        {
            completed = new PreviewSnapshot(entry.Id, false, result);
        }
    }

    private void Cancel()
    {
        cancellation?.Cancel();
        cancellation?.Dispose();
        cancellation = null;
    }
}
