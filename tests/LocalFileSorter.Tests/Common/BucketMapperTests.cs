using LocalFileSorter.Common.Model;
using LocalFileSorter.Common.Services;

using Xunit;

namespace LocalFileSorter.Tests.Common;

public sealed class BucketMapperTests : IDisposable
{
    private readonly string root = Directory.CreateTempSubdirectory("lfs-buckets-").FullName;

    [Fact]
    public void DiscoversImmediateSubfoldersInAlphabeticalOrder()
    {
        Folder("Trash");
        Folder("Docs");
        Folder("Photos");

        IReadOnlyList<Bucket> buckets = Map();

        Assert.Equal(["Docs", "Photos", "Trash"], buckets.Select(bucket => bucket.Name));
        Assert.Equal([0, 1, 2], buckets.Select(bucket => bucket.Order));
    }

    [Fact]
    public void DerivesIdentityFromTheFolderName()
    {
        Folder("Docs");

        Bucket bucket = Map()[0];

        Assert.Equal(BucketId.FromName("DOCS"), bucket.Id);
        Assert.Equal(Path.Combine(root, "Docs"), bucket.DirectoryPath);
    }

    [Fact]
    public void GivesEachBucketADistinctPaletteColor()
    {
        Folder("a");
        Folder("b");
        Folder("c");

        Assert.Equal(3, Map().Select(bucket => bucket.Color).Distinct().Count());
    }

    [Fact]
    public void CountsPreExistingTopLevelFilesOnly()
    {
        string docs = Folder("Docs");
        File.WriteAllText(Path.Combine(docs, "a.txt"), "x");
        File.WriteAllText(Path.Combine(docs, "b.txt"), "x");
        Directory.CreateDirectory(Path.Combine(docs, "nested"));
        File.WriteAllText(Path.Combine(docs, "nested", "c.txt"), "x");

        Assert.Equal(2, Map()[0].ExistingFileCount);
    }

    [Fact]
    public void IgnoresDotFolders()
    {
        Folder(".localfilesorter");
        Folder("Docs");

        Assert.Equal(["Docs"], Map().Select(bucket => bucket.Name));
    }

    [Fact]
    public void IgnoresLooseFilesAtTheDestinationRoot()
    {
        File.WriteAllText(Path.Combine(root, "loose.txt"), "x");

        Assert.Empty(Map());
    }

    [Fact]
    public void ReturnsEmptyForAMissingRoot()
    {
        Assert.Empty(BucketMapper.Map(Path.Combine(root, "nope"), []).Buckets);
    }

    [Fact]
    public void KeepsColorAndOrderOfBucketsAlreadyInTheSession()
    {
        Folder("Docs");
        IReadOnlyList<Bucket> current = TestSession.Buckets("Docs");
        current[0].Color = new RgbColor(9, 9, 9);

        Bucket bucket = BucketMapper.Map(root, current).Buckets[0];

        Assert.Equal(new RgbColor(9, 9, 9), bucket.Color);
        Assert.Equal(0, bucket.Order);
    }

    [Fact]
    public void ReportsFoldersAddedSinceTheCurrentMapping()
    {
        Folder("Docs");
        Folder("Photos");

        MappingResult result = BucketMapper.Map(root, TestSession.Buckets("Docs"));

        Assert.Equal(["Photos"], result.Added);
        Assert.Empty(result.Removed);
    }

    [Fact]
    public void ReportsFoldersThatDisappeared()
    {
        Folder("Docs");

        MappingResult result = BucketMapper.Map(root, TestSession.Buckets("Docs", "Photos"));

        Assert.Empty(result.Added);
        Assert.Equal(["Photos"], result.Removed.Select(bucket => bucket.Name));
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(root, recursive: true);
        }
        catch (IOException)
        {
        }
    }

    private IReadOnlyList<Bucket> Map() => BucketMapper.Map(root, []).Buckets;

    private string Folder(string name) => Directory.CreateDirectory(Path.Combine(root, name)).FullName;
}
