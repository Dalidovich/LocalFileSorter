using LocalFileSorter.Common.Localization;
using LocalFileSorter.Common.Model;
using LocalFileSorter.Common.Services;

using Xunit;

namespace LocalFileSorter.Tests.Common;

public sealed class MoveExecutorTests : IDisposable
{
    private readonly Strings strings = TestStrings.Shipped();
    private readonly CommitFixture fixture = new();

    [Fact]
    public void MovesTheFileIntoTheBucketDirectory()
    {
        MoveTask task = fixture.Task("a.txt", "Docs");

        MoveOutcome outcome = new MoveExecutor(strings).Execute(task);

        Assert.Equal(MoveStatus.Moved, outcome.Status);
        Assert.Equal(fixture.InBucket("Docs", "a.txt"), outcome.DestinationPath);
        Assert.True(File.Exists(fixture.InBucket("Docs", "a.txt")));
        Assert.False(File.Exists(fixture.InSource("a.txt")));
    }

    [Fact]
    public void RenamesInsteadOfOverwritingAnExistingName()
    {
        fixture.WriteInBucket("Docs", "a.txt", "already there");
        MoveTask task = fixture.Task("a.txt", "Docs");

        MoveOutcome outcome = new MoveExecutor(strings).Execute(task);

        Assert.Equal(MoveStatus.Renamed, outcome.Status);
        Assert.Equal(fixture.InBucket("Docs", "a (1).txt"), outcome.DestinationPath);
        Assert.Equal("already there", File.ReadAllText(fixture.InBucket("Docs", "a.txt")));
    }

    [Fact]
    public void RecreatesABucketDirectoryThatVanishedAfterMapping()
    {
        MoveTask task = fixture.Task("a.txt", "Docs");
        Directory.Delete(task.Bucket.DirectoryPath, recursive: true);

        MoveOutcome outcome = new MoveExecutor(strings).Execute(task);

        Assert.Equal(MoveStatus.Moved, outcome.Status);
        Assert.True(File.Exists(fixture.InBucket("Docs", "a.txt")));
    }

    [Fact]
    public void FailsWhenTheSourceFileIsGone()
    {
        MoveTask task = fixture.Task("a.txt", "Docs");
        File.Delete(task.File.CurrentPath);

        MoveOutcome outcome = new MoveExecutor(strings).Execute(task);

        Assert.Equal(MoveStatus.Failed, outcome.Status);
        Assert.Equal(strings.CommitReasonSourceMissing, outcome.Reason);
    }

    [Fact]
    public void FailsWhenTheFileIsHeldByAnotherProcess()
    {
        MoveTask task = fixture.Task("a.txt", "Docs");

        using FileStream held = new(task.File.CurrentPath, FileMode.Open, FileAccess.Read, FileShare.None);
        MoveOutcome outcome = new MoveExecutor(strings).Execute(task);

        Assert.Equal(MoveStatus.Failed, outcome.Status);
        Assert.Equal(strings.CommitReasonLocked, outcome.Reason);
        Assert.True(File.Exists(task.File.CurrentPath));
    }

    [Fact]
    public void RefusesAFileThatWasAlreadyMoved()
    {
        MoveTask task = fixture.Task("a.txt", "Docs");
        task.File.State = FileState.Moved;

        MoveOutcome outcome = new MoveExecutor(strings).Execute(task);

        Assert.Equal(MoveStatus.Failed, outcome.Status);
        Assert.Equal(strings.CommitReasonAlreadyMoved, outcome.Reason);
    }

    public void Dispose() => fixture.Dispose();
}
