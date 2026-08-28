using LocalFileSorter.Common.Model;
using LocalFileSorter.Common.Services;

using Xunit;

namespace LocalFileSorter.Tests.Common;

public sealed class MappingServiceTests : IDisposable
{
    private readonly CommitFixture fixture = new();

    [Fact]
    public void ReportsNoChangeWhenTheSubfoldersAreTheSame()
    {
        SortSession session = fixture.Session(["a.txt"], "Docs");

        ReloadNotice notice = Service(session, new SortPlanService(session)).Reload();

        Assert.True(notice.IsUnchanged);
        Assert.Equal(["Docs"], session.Buckets.Select(bucket => bucket.Name));
    }

    [Fact]
    public void AppendsAFolderCreatedAfterStartup()
    {
        SortSession session = fixture.Session(["a.txt"], "Docs");
        Directory.CreateDirectory(Path.Combine(fixture.DestinationRoot, "Photos"));

        ReloadNotice notice = Service(session, new SortPlanService(session)).Reload();

        Assert.Equal(["Photos"], notice.Added);
        Assert.Equal(["Docs", "Photos"], session.Buckets.Select(bucket => bucket.Name));
    }

    [Fact]
    public void KeepsTheColorAndAssignmentsOfAnUnchangedFolder()
    {
        SortSession session = fixture.Session(["a.txt"], "Docs");
        SortPlanService plan = new(session);
        Bucket docs = session.Buckets[0];
        RgbColor color = docs.Color;
        plan.Assign(session.Files[0], docs.Id);

        Directory.CreateDirectory(Path.Combine(fixture.DestinationRoot, "Photos"));
        Service(session, plan).Reload();

        Assert.Equal(color, session.Buckets.Single(bucket => bucket.Name == "Docs").Color);
        Assert.Equal(FileState.Assigned, session.Files[0].State);
        Assert.Equal(1, plan.AssignedCount(docs.Id));
    }

    [Fact]
    public void ReleasesAssignmentsOfAFolderThatDisappeared()
    {
        SortSession session = fixture.Session(["a.txt", "b.txt"], "Docs", "Photos");
        SortPlanService plan = new(session);
        Bucket photos = session.Buckets.Single(bucket => bucket.Name == "Photos");
        plan.Assign(session.Files[0], photos.Id);
        plan.Assign(session.Files[1], photos.Id);

        Directory.Delete(Path.Combine(fixture.DestinationRoot, "Photos"), recursive: true);
        ReloadNotice notice = Service(session, plan).Reload();

        Assert.Equal(["Photos"], notice.Removed);
        Assert.Equal(2, notice.ReleasedAssignments);
        Assert.Equal(["Docs"], session.Buckets.Select(bucket => bucket.Name));
        Assert.All(session.Files, file => Assert.Equal(FileState.Pending, file.State));
        Assert.Equal(0, plan.TotalAssigned);
    }

    [Fact]
    public void ReportsCommittedFilesLeftInAFolderThatDisappeared()
    {
        SortSession session = fixture.Session(["a.txt"], "Photos");
        SortPlanService plan = new(session);
        Bucket photos = session.Buckets[0];
        plan.Assign(session.Files[0], photos.Id);
        plan.MarkMoved(session.Files[0], fixture.InBucket("Photos", "a.txt"));

        Directory.Delete(Path.Combine(fixture.DestinationRoot, "Photos"), recursive: true);
        ReloadNotice notice = Service(session, plan).Reload();

        Assert.Equal(1, notice.CommittedInRemoved);
        Assert.Equal(0, notice.ReleasedAssignments);
        Assert.Equal(FileState.Moved, session.Files[0].State);
    }

    [Fact]
    public void RecolorSurvivesAReload()
    {
        SortSession session = fixture.Session(["a.txt"], "Docs");
        SortPlanService plan = new(session);
        MappingService mapping = Service(session, plan);
        RgbColor color = new(0x12, 0x34, 0x56);

        mapping.Recolor(session.Buckets[0], color);
        mapping.Reload();

        Assert.Equal(color, session.Buckets[0].Color);
    }

    public void Dispose() => fixture.Dispose();

    private MappingService Service(SortSession session, SortPlanService plan) =>
        new(session, plan);
}
