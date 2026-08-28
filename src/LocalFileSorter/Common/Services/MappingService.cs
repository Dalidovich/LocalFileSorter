using LocalFileSorter.Common.Model;

namespace LocalFileSorter.Common.Services;

public sealed class MappingService
{
    private readonly SortSession session;
    private readonly SortPlanService plan;

    public MappingService(SortSession session, SortPlanService plan)
    {
        this.session = session;
        this.plan = plan;
    }

    public ReloadNotice Reload()
    {
        MappingResult result = BucketMapper.Map(session.DestinationRoot, session.Buckets);

        int released = 0;
        int committed = 0;

        foreach (Bucket bucket in result.Removed)
        {
            committed += plan.MovedCount(bucket.Id);
            released += plan.ReleaseAssignmentsFor(bucket.Id);
        }

        if (result.Removed.Count > 0)
        {
            plan.ClearUndo();
        }

        session.ReplaceBuckets(result.Buckets);

        return new ReloadNotice(
            result.Added,
            [.. result.Removed.Select(bucket => bucket.Name)],
            released,
            committed);
    }

    public void Recolor(Bucket bucket, RgbColor color)
    {
        bucket.Color = color;
    }
}
