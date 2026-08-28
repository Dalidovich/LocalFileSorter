using LocalFileSorter.Common.Model;

using Xunit;

namespace LocalFileSorter.Tests.Common;

public sealed class SortSessionTests
{
    [Fact]
    public void ActivatesFirstFile()
    {
        SortSession session = Session(3);

        Assert.Equal(0, session.ActiveIndex);
        Assert.Equal("f0.txt", session.ActiveFile!.Name);
    }

    [Fact]
    public void HasNoActiveFileWhenQueueIsEmpty()
    {
        SortSession session = Session(0);

        Assert.Equal(-1, session.ActiveIndex);
        Assert.Null(session.ActiveFile);
        Assert.False(session.CanMoveNext);
        Assert.False(session.CanMovePrevious);
    }

    [Fact]
    public void StepsForwardAndBackWithoutWrapping()
    {
        SortSession session = Session(2);

        Assert.False(session.CanMovePrevious);
        session.MovePrevious();
        Assert.Equal(0, session.ActiveIndex);

        session.MoveNext();
        Assert.Equal(1, session.ActiveIndex);

        Assert.False(session.CanMoveNext);
        session.MoveNext();
        Assert.Equal(1, session.ActiveIndex);
    }

    [Fact]
    public void ActivatesByIndexAndIgnoresOutOfRange()
    {
        SortSession session = Session(3);

        session.Activate(2);
        Assert.Equal(2, session.ActiveIndex);

        session.Activate(7);
        session.Activate(-1);
        Assert.Equal(2, session.ActiveIndex);
    }

    [Fact]
    public void ExposesScanCounters()
    {
        SortSession session = new("src", "dst", new ScanResult([], 4, [".png"]), []);

        Assert.Equal(4, session.SkippedUnsupportedCount);
        Assert.Equal([".png"], session.SkippedExtensions);
    }

    [Fact]
    public void FindsBucketsByIdAndIgnoresUnknownOnes()
    {
        SortSession session = TestSession.With(1, "Docs", "Photos");

        Assert.Equal("Photos", session.FindBucket(BucketId.FromName("photos"))!.Name);
        Assert.Null(session.FindBucket(BucketId.FromName("trash")));
    }

    private static SortSession Session(int count) => TestSession.With(count);
}
