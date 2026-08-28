using LocalFileSorter.Common.Model;
using LocalFileSorter.Common.Services;

using Xunit;

namespace LocalFileSorter.Tests.Common;

public sealed class SortPlanServiceTests
{
    private static readonly BucketId Docs = BucketId.FromName("Docs");
    private static readonly BucketId Photos = BucketId.FromName("Photos");

    [Fact]
    public void AssignsAFileToABucket()
    {
        SortSession session = Session(3);
        SortPlanService plan = new(session);

        Assert.True(plan.Assign(session.Files[0], Docs));

        Assert.Equal(FileState.Assigned, session.Files[0].State);
        Assert.Equal(Docs, session.Files[0].AssignedBucket);
        Assert.Equal(1, plan.AssignedCount(Docs));
        Assert.Equal(1, plan.TotalAssigned);
    }

    [Fact]
    public void ReassigningMovesTheCountBetweenBuckets()
    {
        SortSession session = Session(3);
        SortPlanService plan = new(session);

        plan.Assign(session.Files[0], Docs);
        plan.Assign(session.Files[0], Photos);

        Assert.Equal(0, plan.AssignedCount(Docs));
        Assert.Equal(1, plan.AssignedCount(Photos));
        Assert.Equal(1, plan.TotalAssigned);
    }

    [Fact]
    public void AssigningTheSameBucketTwiceChangesNothing()
    {
        SortSession session = Session(3);
        SortPlanService plan = new(session);

        plan.Assign(session.Files[0], Docs);

        Assert.False(plan.Assign(session.Files[0], Docs));
        Assert.Equal(1, plan.AssignedCount(Docs));
    }

    [Fact]
    public void RefusesToAssignAMovedFile()
    {
        SortSession session = Session(1);
        SortPlanService plan = new(session);
        session.Files[0].State = FileState.Moved;

        Assert.False(plan.Assign(session.Files[0], Docs));
        Assert.Equal(0, plan.AssignedCount(Docs));
    }

    [Fact]
    public void UnassignReturnsTheFileToPending()
    {
        SortSession session = Session(3);
        SortPlanService plan = new(session);
        plan.Assign(session.Files[0], Docs);

        Assert.True(plan.Unassign(session.Files[0]));

        Assert.Equal(FileState.Pending, session.Files[0].State);
        Assert.Null(session.Files[0].AssignedBucket);
        Assert.Equal(0, plan.AssignedCount(Docs));
        Assert.Equal(0, plan.TotalAssigned);
    }

    [Fact]
    public void UnassignReleasesAFailedFile()
    {
        SortSession session = TestSession.With(1, "Docs", "Photos");
        SortPlanService plan = new(session);
        plan.Assign(session.Files[0], Docs);
        plan.MarkFailed(session.Files[0], "held");

        Assert.True(plan.Unassign(session.Files[0]));

        Assert.Equal(FileState.Pending, session.Files[0].State);
        Assert.Null(session.Files[0].FailureReason);
        Assert.Equal(0, plan.TotalAssigned);
    }

    [Fact]
    public void MarkMovedRetiresTheAssignmentIntoTheBucket()
    {
        SortSession session = TestSession.With(1, "Docs", "Photos");
        SortPlanService plan = new(session);
        plan.Assign(session.Files[0], Docs);

        plan.MarkMoved(session.Files[0], "dst/Docs/f0.txt");

        Assert.Equal(FileState.Moved, session.Files[0].State);
        Assert.Equal("dst/Docs/f0.txt", session.Files[0].CurrentPath);
        Assert.Equal(0, plan.TotalAssigned);
        Assert.Equal(0, plan.AssignedCount(Docs));
        Assert.Equal(1, session.Buckets[0].ExistingFileCount);
    }

    [Fact]
    public void BuildsCommitTasksOnlyForLivingAssignments()
    {
        SortSession session = TestSession.With(3, "Docs", "Photos");
        SortPlanService plan = new(session);
        plan.Assign(session.Files[0], Docs);
        plan.Assign(session.Files[1], Photos);
        plan.MarkMoved(session.Files[1], "dst/Photos/f1.txt");

        IReadOnlyList<MoveTask> tasks = plan.BuildCommitTasks();

        MoveTask task = Assert.Single(tasks);
        Assert.Equal(session.Files[0], task.File);
        Assert.Equal(Docs, task.Bucket.Id);
    }

    [Fact]
    public void UnassigningAPendingFileChangesNothing()
    {
        SortSession session = Session(1);
        SortPlanService plan = new(session);

        Assert.False(plan.Unassign(session.Files[0]));
        Assert.Equal(0, plan.TotalAssigned);
    }

    [Fact]
    public void ReleasesEveryAssignmentPointingAtABucket()
    {
        SortSession session = Session(3);
        SortPlanService plan = new(session);
        plan.Assign(session.Files[0], Docs);
        plan.Assign(session.Files[1], Photos);
        plan.Assign(session.Files[2], Docs);

        Assert.Equal(2, plan.ReleaseAssignmentsFor(Docs));

        Assert.Equal(0, plan.AssignedCount(Docs));
        Assert.Equal(1, plan.AssignedCount(Photos));
    }

    [Fact]
    public void FindsTheNextPendingFileAndWraps()
    {
        SortSession session = Session(4);
        SortPlanService plan = new(session);
        plan.Assign(session.Files[2], Docs);

        Assert.Equal(3, plan.NextUnassigned(1));
        Assert.Equal(0, plan.NextUnassigned(3));
    }

    [Fact]
    public void ReportsNoNextFileWhenEverythingIsAssigned()
    {
        SortSession session = Session(2);
        SortPlanService plan = new(session);
        plan.Assign(session.Files[0], Docs);
        plan.Assign(session.Files[1], Docs);

        Assert.Equal(-1, plan.NextUnassigned(0));
    }

    [Fact]
    public void ReportsNoNextFileForAnEmptyQueue()
    {
        Assert.Equal(-1, new SortPlanService(Session(0)).NextUnassigned(-1));
    }

    [Fact]
    public void TogglingAdvancesToTheNextUnassignedFile()
    {
        SortSession session = Session(3);
        SortPlanService plan = new(session);

        plan.ToggleActive(session.Buckets[0]);

        Assert.Equal(Docs, session.Files[0].AssignedBucket);
        Assert.Equal(1, session.ActiveIndex);
    }

    [Fact]
    public void TogglingWrapsPastAlreadyAssignedFiles()
    {
        SortSession session = Session(3);
        SortPlanService plan = new(session);
        plan.Assign(session.Files[0], Docs);
        session.Activate(2);

        plan.ToggleActive(session.Buckets[1]);

        Assert.Equal(1, session.ActiveIndex);
    }

    [Fact]
    public void TogglingTheCurrentBucketUnassignsAndStaysPut()
    {
        SortSession session = Session(3);
        SortPlanService plan = new(session);
        plan.ToggleActive(session.Buckets[0]);
        session.Activate(0);

        plan.ToggleActive(session.Buckets[0]);

        Assert.Null(session.Files[0].AssignedBucket);
        Assert.Equal(0, session.ActiveIndex);
    }

    [Fact]
    public void TogglingADifferentBucketReassignsAndKeepsAdvancing()
    {
        SortSession session = Session(2);
        SortPlanService plan = new(session);
        plan.Assign(session.Files[0], Docs);

        plan.ToggleActive(session.Buckets[1]);

        Assert.Equal(Photos, session.Files[0].AssignedBucket);
        Assert.Equal(1, session.ActiveIndex);
    }

    [Fact]
    public void KeepsTheActiveFileWhenNothingIsLeftToAssign()
    {
        SortSession session = Session(1);
        SortPlanService plan = new(session);

        plan.ToggleActive(session.Buckets[0]);

        Assert.Equal(0, session.ActiveIndex);
        Assert.True(plan.QueueComplete);
    }

    [Fact]
    public void AnEmptyQueueIsNeverComplete()
    {
        Assert.False(new SortPlanService(Session(0)).QueueComplete);
    }

    [Fact]
    public void HasNothingToUndoBeforeAnyToggle()
    {
        SortSession session = Session(2);
        SortPlanService plan = new(session);

        plan.Assign(session.Files[0], Docs);

        Assert.False(plan.CanUndo);
        Assert.False(plan.Undo());
    }

    [Fact]
    public void UndoRevertsAnAssignmentAndRestoresTheActiveFile()
    {
        SortSession session = Session(3);
        SortPlanService plan = new(session);
        plan.ToggleActive(session.Buckets[0]);

        Assert.True(plan.Undo());

        Assert.Null(session.Files[0].AssignedBucket);
        Assert.Equal(FileState.Pending, session.Files[0].State);
        Assert.Equal(0, plan.TotalAssigned);
        Assert.Equal(0, session.ActiveIndex);
        Assert.False(plan.CanUndo);
    }

    [Fact]
    public void UndoRevertsAnUnassignment()
    {
        SortSession session = Session(2);
        SortPlanService plan = new(session);
        plan.ToggleActive(session.Buckets[0]);
        session.Activate(0);
        plan.ToggleActive(session.Buckets[0]);

        Assert.True(plan.Undo());

        Assert.Equal(Docs, session.Files[0].AssignedBucket);
        Assert.Equal(1, plan.AssignedCount(Docs));
    }

    [Fact]
    public void UndoRestoresThePreviousBucketAfterAReassignment()
    {
        SortSession session = Session(2);
        SortPlanService plan = new(session);
        plan.ToggleActive(session.Buckets[0]);
        session.Activate(0);
        plan.ToggleActive(session.Buckets[1]);

        Assert.True(plan.Undo());

        Assert.Equal(Docs, session.Files[0].AssignedBucket);
        Assert.Equal(1, plan.AssignedCount(Docs));
        Assert.Equal(0, plan.AssignedCount(Photos));
        Assert.Equal(1, plan.TotalAssigned);
    }

    [Fact]
    public void UndoUnwindsOneToggleAtATime()
    {
        SortSession session = Session(3);
        SortPlanService plan = new(session);
        plan.ToggleActive(session.Buckets[0]);
        plan.ToggleActive(session.Buckets[1]);

        plan.Undo();

        Assert.Equal(Docs, session.Files[0].AssignedBucket);
        Assert.Null(session.Files[1].AssignedBucket);
        Assert.Equal(1, session.ActiveIndex);
        Assert.True(plan.CanUndo);
    }

    [Fact]
    public void CommittingAFileDropsTheUndoHistory()
    {
        SortSession session = Session(2);
        SortPlanService plan = new(session);
        plan.ToggleActive(session.Buckets[0]);

        plan.MarkMoved(session.Files[0], "dst/Docs/f0.txt");

        Assert.False(plan.CanUndo);
    }

    [Fact]
    public void ClearUndoDropsTheHistory()
    {
        SortSession session = Session(2);
        SortPlanService plan = new(session);
        plan.ToggleActive(session.Buckets[0]);

        plan.ClearUndo();

        Assert.False(plan.CanUndo);
    }

    private static SortSession Session(int fileCount) => TestSession.With(fileCount, "Docs", "Photos");
}
