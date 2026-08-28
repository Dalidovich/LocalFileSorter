using System.Collections.Concurrent;

using LocalFileSorter.Common.Model;

namespace LocalFileSorter.Common.Services;

public sealed class CommitRunner : IDisposable
{
    private static readonly TimeSpan ShutdownGrace = TimeSpan.FromSeconds(5);

    private readonly SortPlanService plan;
    private readonly MoveExecutor executor;
    private readonly ConcurrentQueue<MoveOutcome> outcomes = new();
    private readonly List<MoveFailure> failures = [];

    private CancellationTokenSource? cancellation;
    private Task? worker;
    private IReadOnlyList<MoveTask> tasks = [];
    private bool cancelRequested;
    private int moved;
    private int renamed;

    public CommitRunner(SortPlanService plan, MoveExecutor executor)
    {
        this.plan = plan;
        this.executor = executor;
    }

    public bool IsRunning => worker is not null;

    public int Total => tasks.Count;

    public int Completed { get; private set; }

    public bool CancelRequested => cancelRequested;

    public MoveReport? Report { get; private set; }

    public string CurrentName => tasks.Count == 0
        ? string.Empty
        : tasks[Math.Min(Completed, tasks.Count - 1)].File.Name;

    public void Start(IReadOnlyList<MoveTask> pending)
    {
        if (IsRunning || pending.Count == 0)
        {
            return;
        }

        tasks = pending;
        Completed = 0;
        Report = null;
        cancelRequested = false;
        moved = 0;
        renamed = 0;
        failures.Clear();
        outcomes.Clear();

        CancellationTokenSource source = new();
        cancellation = source;
        worker = Task.Run(() => Run(pending, source.Token), CancellationToken.None);
    }

    public void Cancel()
    {
        if (!IsRunning)
        {
            return;
        }

        cancelRequested = true;
        cancellation?.Cancel();
    }

    public void Update()
    {
        while (outcomes.TryDequeue(out MoveOutcome? outcome))
        {
            Apply(outcome);
            Completed++;
        }

        if (worker is not { IsCompleted: true } || !outcomes.IsEmpty)
        {
            return;
        }

        Report = new MoveReport(moved, renamed, [.. failures], cancelRequested, tasks.Count - Completed);
        worker = null;
        cancellation?.Dispose();
        cancellation = null;
    }

    public void Clear()
    {
        Report = null;
        tasks = [];
        Completed = 0;
    }

    public void Dispose()
    {
        Cancel();

        try
        {
            worker?.Wait(ShutdownGrace);
        }
        catch (AggregateException)
        {
        }

        cancellation?.Dispose();
        cancellation = null;
        worker = null;
    }

    private void Run(IReadOnlyList<MoveTask> pending, CancellationToken token)
    {
        foreach (MoveTask task in pending)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            outcomes.Enqueue(Attempt(task));
        }
    }

    private MoveOutcome Attempt(MoveTask task)
    {
        try
        {
            return executor.Execute(task);
        }
        catch (Exception exception)
        {
            return new MoveOutcome(task, MoveStatus.Failed, null, exception.Message);
        }
    }

    private void Apply(MoveOutcome outcome)
    {
        FileEntry file = outcome.Task.File;

        if (outcome.Status == MoveStatus.Failed)
        {
            string reason = outcome.Reason ?? string.Empty;
            plan.MarkFailed(file, reason);
            failures.Add(new MoveFailure(file.Id, file.Name, reason));
            return;
        }

        plan.MarkMoved(file, outcome.DestinationPath!);
        moved++;

        if (outcome.Status == MoveStatus.Renamed)
        {
            renamed++;
        }
    }
}
