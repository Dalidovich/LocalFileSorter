namespace LocalFileSorter.Common.Services;

public sealed record ReloadNotice(
    IReadOnlyList<string> Added,
    IReadOnlyList<string> Removed,
    int ReleasedAssignments,
    int CommittedInRemoved)
{
    public bool IsUnchanged => Added.Count == 0 && Removed.Count == 0;
}
