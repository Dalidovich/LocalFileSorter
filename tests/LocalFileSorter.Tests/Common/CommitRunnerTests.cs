using LocalFileSorter.Common.Localization;
using LocalFileSorter.Common.Model;
using LocalFileSorter.Common.Services;

using Xunit;

namespace LocalFileSorter.Tests.Common;

public sealed class CommitRunnerTests : IDisposable
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15);

    private readonly Strings strings = TestStrings.Shipped();
    private readonly CommitFixture fixture = new();

    [Fact]
    public void MovesEveryAssignedFileAndReportsIt()
    {
        SortSession session = fixture.Session(["a.txt", "b.txt"], "Docs");
        SortPlanService plan = new(session);
        BucketId docs = session.Buckets[0].Id;
        plan.Assign(session.Files[0], docs);
        plan.Assign(session.Files[1], docs);

        MoveReport report = Run(plan);

        Assert.Equal(2, report.Moved);
        Assert.Equal(0, report.Renamed);
        Assert.Empty(report.Failures);
        Assert.False(report.Cancelled);
        Assert.True(File.Exists(fixture.InBucket("Docs", "a.txt")));
        Assert.True(File.Exists(fixture.InBucket("Docs", "b.txt")));
    }

    [Fact]
    public void MarksMovedFilesAndClearsThePendingCount()
    {
        SortSession session = fixture.Session(["a.txt"], "Docs");
        SortPlanService plan = new(session);
        plan.Assign(session.Files[0], session.Buckets[0].Id);

        Run(plan);

        Assert.Equal(FileState.Moved, session.Files[0].State);
        Assert.Equal(fixture.InBucket("Docs", "a.txt"), session.Files[0].CurrentPath);
        Assert.Equal(0, plan.TotalAssigned);
        Assert.Equal(0, plan.AssignedCount(session.Buckets[0].Id));
        Assert.Equal(1, session.Buckets[0].ExistingFileCount);
    }

    [Fact]
    public void CountsARenameSeparatelyFromAPlainMove()
    {
        SortSession session = fixture.Session(["a.txt", "b.txt"], "Docs");
        SortPlanService plan = new(session);
        plan.Assign(session.Files[0], session.Buckets[0].Id);
        plan.Assign(session.Files[1], session.Buckets[0].Id);
        fixture.WriteInBucket("Docs", "a.txt", "already there");

        MoveReport report = Run(plan);

        Assert.Equal(2, report.Moved);
        Assert.Equal(1, report.Renamed);
        Assert.True(File.Exists(fixture.InBucket("Docs", "a (1).txt")));
    }

    [Fact]
    public void KeepsAFailedFileAssignedSoItCanBeRetried()
    {
        SortSession session = fixture.Session(["a.txt", "b.txt"], "Docs");
        SortPlanService plan = new(session);
        BucketId docs = session.Buckets[0].Id;
        plan.Assign(session.Files[0], docs);
        plan.Assign(session.Files[1], docs);
        File.Delete(session.Files[0].CurrentPath);

        MoveReport report = Run(plan);

        Assert.Equal(1, report.Moved);
        MoveFailure failure = Assert.Single(report.Failures);
        Assert.Equal("a.txt", failure.Name);
        Assert.Equal(strings.CommitReasonSourceMissing, failure.Reason);

        Assert.Equal(FileState.Failed, session.Files[0].State);
        Assert.Equal(strings.CommitReasonSourceMissing, session.Files[0].FailureReason);
        Assert.Equal(docs, session.Files[0].AssignedBucket);
        Assert.Equal(1, plan.TotalAssigned);
        Assert.Equal(1, plan.AssignedCount(docs));
    }

    [Fact]
    public void ReassigningAFailedFileClearsItsFailure()
    {
        SortSession session = fixture.Session(["a.txt"], "Docs", "Photos");
        SortPlanService plan = new(session);
        plan.Assign(session.Files[0], session.Buckets[0].Id);
        plan.MarkFailed(session.Files[0], "held");

        plan.Assign(session.Files[0], session.Buckets[1].Id);

        Assert.Equal(FileState.Assigned, session.Files[0].State);
        Assert.Null(session.Files[0].FailureReason);
        Assert.Equal(0, plan.AssignedCount(session.Buckets[0].Id));
        Assert.Equal(1, plan.AssignedCount(session.Buckets[1].Id));
        Assert.Equal(1, plan.TotalAssigned);
    }

    [Fact]
    public void LeavesTheQueueAloneWhenNothingIsAssigned()
    {
        SortSession session = fixture.Session(["a.txt"], "Docs");
        SortPlanService plan = new(session);

        using CommitRunner runner = new(plan, new MoveExecutor(strings));
        runner.Start(plan.BuildCommitTasks());
        runner.Update();

        Assert.False(runner.IsRunning);
        Assert.Null(runner.Report);
        Assert.True(File.Exists(fixture.InSource("a.txt")));
    }

    [Fact]
    public void SkipsFilesThatWereAlreadyCommitted()
    {
        SortSession session = fixture.Session(["a.txt"], "Docs");
        SortPlanService plan = new(session);
        plan.Assign(session.Files[0], session.Buckets[0].Id);
        Run(plan);

        Assert.Empty(plan.BuildCommitTasks());
    }

    public void Dispose() => fixture.Dispose();

    private MoveReport Run(SortPlanService plan)
    {
        using CommitRunner runner = new(plan, new MoveExecutor(strings));
        runner.Start(plan.BuildCommitTasks());

        DateTime deadline = DateTime.UtcNow + Timeout;
        while (runner.Report is null && DateTime.UtcNow < deadline)
        {
            runner.Update();
            Thread.Sleep(2);
        }

        return runner.Report ?? throw new InvalidOperationException("the commit did not finish in time");
    }
}
