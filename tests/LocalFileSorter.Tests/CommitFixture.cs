using LocalFileSorter.Common.Model;
using LocalFileSorter.Common.Services;

namespace LocalFileSorter.Tests;

public sealed class CommitFixture : IDisposable
{
    private static readonly HashSet<string> Supported = new(StringComparer.Ordinal) { ".txt" };

    private readonly string root = Directory.CreateTempSubdirectory("lfs-commit-").FullName;

    public CommitFixture()
    {
        Directory.CreateDirectory(SourceRoot);
        Directory.CreateDirectory(DestinationRoot);
    }

    public string SourceRoot => Path.Combine(root, "source");

    public string DestinationRoot => Path.Combine(root, "destination");

    public string InSource(string name) => Path.Combine(SourceRoot, name);

    public string InBucket(string bucket, string name) => Path.Combine(DestinationRoot, bucket, name);

    public void WriteInSource(string name, string content = "content") =>
        File.WriteAllText(InSource(name), content);

    public void WriteInBucket(string bucket, string name, string content = "content")
    {
        Directory.CreateDirectory(Path.Combine(DestinationRoot, bucket));
        File.WriteAllText(InBucket(bucket, name), content);
    }

    public SortSession Session(IEnumerable<string> fileNames, params string[] bucketNames)
    {
        foreach (string name in fileNames)
        {
            if (!File.Exists(InSource(name)))
            {
                WriteInSource(name);
            }
        }

        foreach (string bucket in bucketNames)
        {
            Directory.CreateDirectory(Path.Combine(DestinationRoot, bucket));
        }

        return new SortSession(
            SourceRoot,
            DestinationRoot,
            SourceScanner.Scan(SourceRoot, Supported),
            BucketMapper.Map(DestinationRoot, []).Buckets);
    }

    public MoveTask Task(string fileName, string bucketName)
    {
        SortSession session = Session([fileName], bucketName);
        return new MoveTask(
            session.Files.Single(file => file.Name == fileName),
            session.Buckets.Single(bucket => bucket.Name == bucketName));
    }

    public void Dispose() => Directory.Delete(root, recursive: true);
}
