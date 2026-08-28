using LocalFileSorter.Common.Model;

namespace LocalFileSorter.Common.Services;

public readonly record struct PlanChange(FileEntry File, BucketId? Bucket, int ActiveIndex);
