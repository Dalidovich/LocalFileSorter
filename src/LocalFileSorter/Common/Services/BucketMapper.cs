using LocalFileSorter.Common.Model;

namespace LocalFileSorter.Common.Services;

public static class BucketMapper
{
    private readonly record struct Candidate(BucketId Id, string Name, string Directory, RgbColor? Color, int? Order);

    public static MappingResult Map(string destinationRoot, IReadOnlyList<Bucket> current)
    {
        Dictionary<BucketId, Bucket> known = current.ToDictionary(bucket => bucket.Id);

        List<Candidate> candidates =
        [
            .. EnumerateBucketDirectories(destinationRoot).Select(directory => Describe(directory, known)),
        ];

        PaletteAllocator palette = new();
        foreach (Candidate candidate in candidates)
        {
            if (candidate.Color is RgbColor color)
            {
                palette.MarkUsed(color);
            }
        }

        int nextOrder = NextOrder(candidates);
        List<Bucket> buckets = [];

        foreach (Candidate candidate in candidates)
        {
            buckets.Add(new Bucket
            {
                Id = candidate.Id,
                Name = candidate.Name,
                DirectoryPath = candidate.Directory,
                Color = candidate.Color ?? palette.Next(),
                Order = candidate.Order ?? nextOrder++,
                ExistingFileCount = CountFiles(candidate.Directory),
            });
        }

        IReadOnlyList<Bucket> mapped =
        [
            .. buckets
                .OrderBy(bucket => bucket.Order)
                .ThenBy(bucket => bucket.Name, StringComparer.OrdinalIgnoreCase),
        ];

        HashSet<BucketId> mappedIds = [.. mapped.Select(bucket => bucket.Id)];

        return new MappingResult(
            mapped,
            [.. mapped.Where(bucket => !known.ContainsKey(bucket.Id)).Select(bucket => bucket.Name)],
            [.. current.Where(bucket => !mappedIds.Contains(bucket.Id))]);
    }

    private static Candidate Describe(string directory, Dictionary<BucketId, Bucket> known)
    {
        string name = Path.GetFileName(directory);
        BucketId id = BucketId.FromName(name);

        return known.TryGetValue(id, out Bucket? bucket)
            ? new Candidate(id, name, directory, bucket.Color, bucket.Order)
            : new Candidate(id, name, directory, null, null);
    }

    private static int NextOrder(IReadOnlyList<Candidate> candidates)
    {
        int highest = -1;

        foreach (Candidate candidate in candidates)
        {
            if (candidate.Order is int order && order > highest)
            {
                highest = order;
            }
        }

        return highest + 1;
    }

    private static IEnumerable<string> EnumerateBucketDirectories(string destinationRoot)
    {
        try
        {
            return Directory
                .EnumerateDirectories(destinationRoot, "*", SearchOption.TopDirectoryOnly)
                .Where(directory => !Path.GetFileName(directory).StartsWith('.'))
                .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return [];
        }
    }

    private static int CountFiles(string directory)
    {
        try
        {
            return Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly).Count();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return 0;
        }
    }
}
