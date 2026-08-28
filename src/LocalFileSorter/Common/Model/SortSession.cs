namespace LocalFileSorter.Common.Model;

public sealed class SortSession
{
    private readonly Dictionary<BucketId, Bucket> bucketsById = [];

    public SortSession(string sourceRoot, string destinationRoot, ScanResult scan, IReadOnlyList<Bucket> buckets)
    {
        SourceRoot = sourceRoot;
        DestinationRoot = destinationRoot;
        Files = scan.Files;
        SkippedUnsupportedCount = scan.SkippedCount;
        SkippedExtensions = scan.SkippedExtensions;
        Buckets = Index(buckets);
        ActiveIndex = Files.Count == 0 ? -1 : 0;
    }

    public string SourceRoot { get; }

    public string DestinationRoot { get; }

    public IReadOnlyList<FileEntry> Files { get; }

    public IReadOnlyList<Bucket> Buckets { get; private set; }

    public int SkippedUnsupportedCount { get; }

    public IReadOnlyList<string> SkippedExtensions { get; }

    public int ActiveIndex { get; private set; }

    public FileEntry? ActiveFile => ActiveIndex < 0 ? null : Files[ActiveIndex];

    public bool CanMovePrevious => ActiveIndex > 0;

    public bool CanMoveNext => ActiveIndex >= 0 && ActiveIndex < Files.Count - 1;

    public Bucket? FindBucket(BucketId id) => bucketsById.GetValueOrDefault(id);

    public void ReplaceBuckets(IReadOnlyList<Bucket> buckets) => Buckets = Index(buckets);

    public void Activate(int index)
    {
        if (index < 0 || index >= Files.Count)
        {
            return;
        }

        ActiveIndex = index;
    }

    public void MovePrevious()
    {
        if (CanMovePrevious)
        {
            ActiveIndex--;
        }
    }

    public void MoveNext()
    {
        if (CanMoveNext)
        {
            ActiveIndex++;
        }
    }

    private IReadOnlyList<Bucket> Index(IReadOnlyList<Bucket> buckets)
    {
        bucketsById.Clear();

        foreach (Bucket bucket in buckets)
        {
            bucketsById[bucket.Id] = bucket;
        }

        return buckets;
    }
}
