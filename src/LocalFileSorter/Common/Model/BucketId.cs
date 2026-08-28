namespace LocalFileSorter.Common.Model;

public readonly record struct BucketId(string NormalizedName)
{
    public static BucketId FromName(string name) => new(name.ToLowerInvariant());
}
