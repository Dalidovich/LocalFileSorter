using LocalFileSorter.Common.Model;

namespace LocalFileSorter.Common.Services;

public sealed class SortPlanService
{
    private readonly SortSession session;
    private readonly Dictionary<BucketId, int> assignedPerBucket = [];
    private readonly Stack<PlanChange> undo = new();

    private int pendingCount;

    public SortPlanService(SortSession session)
    {
        this.session = session;
        pendingCount = session.Files.Count(file => file.State == FileState.Pending);
    }

    public int TotalAssigned { get; private set; }

    public bool CanUndo => undo.Count > 0;

    public bool QueueComplete => session.Files.Count > 0 && pendingCount == 0;

    public int AssignedCount(BucketId bucket) => assignedPerBucket.GetValueOrDefault(bucket);

    public bool Assign(FileEntry file, BucketId bucket)
    {
        if (file.State == FileState.Moved || file.AssignedBucket == bucket)
        {
            return false;
        }

        if (file.AssignedBucket is BucketId previous)
        {
            assignedPerBucket[previous]--;
        }
        else
        {
            pendingCount--;
            TotalAssigned++;
        }

        file.AssignedBucket = bucket;
        file.State = FileState.Assigned;
        file.FailureReason = null;
        assignedPerBucket[bucket] = AssignedCount(bucket) + 1;
        return true;
    }

    public bool Unassign(FileEntry file)
    {
        if (file.State is not (FileState.Assigned or FileState.Failed) || file.AssignedBucket is not BucketId bucket)
        {
            return false;
        }

        assignedPerBucket[bucket]--;
        file.AssignedBucket = null;
        file.State = FileState.Pending;
        file.FailureReason = null;
        pendingCount++;
        TotalAssigned--;
        return true;
    }

    public int MovedCount(BucketId bucket) =>
        session.Files.Count(file => file.State == FileState.Moved && file.AssignedBucket == bucket);

    public int ReleaseAssignmentsFor(BucketId bucket)
    {
        int released = 0;
        foreach (FileEntry file in session.Files)
        {
            if (file.AssignedBucket == bucket && Unassign(file))
            {
                released++;
            }
        }

        return released;
    }

    public int NextUnassigned(int after)
    {
        int count = session.Files.Count;
        for (int step = 1; step <= count; step++)
        {
            int index = (after + step + count) % count;
            if (session.Files[index].State == FileState.Pending)
            {
                return index;
            }
        }

        return -1;
    }

    public void ToggleActive(Bucket bucket)
    {
        FileEntry? active = session.ActiveFile;
        if (active is null)
        {
            return;
        }

        PlanChange change = new(active, active.AssignedBucket, session.ActiveIndex);

        if (active.AssignedBucket == bucket.Id)
        {
            if (Unassign(active))
            {
                undo.Push(change);
            }

            return;
        }

        if (!Assign(active, bucket.Id))
        {
            return;
        }

        undo.Push(change);

        int next = NextUnassigned(session.ActiveIndex);
        if (next >= 0)
        {
            session.Activate(next);
        }
    }

    public bool Undo()
    {
        if (!undo.TryPop(out PlanChange change))
        {
            return false;
        }

        if (change.Bucket is BucketId bucket)
        {
            Assign(change.File, bucket);
        }
        else
        {
            Unassign(change.File);
        }

        session.Activate(change.ActiveIndex);
        return true;
    }

    public void ClearUndo() => undo.Clear();

    public IReadOnlyList<MoveTask> BuildCommitTasks()
    {
        List<MoveTask> tasks = [];

        foreach (FileEntry file in session.Files)
        {
            if (file.State == FileState.Moved || file.AssignedBucket is not BucketId id)
            {
                continue;
            }

            Bucket? bucket = session.FindBucket(id);
            if (bucket is not null)
            {
                tasks.Add(new MoveTask(file, bucket));
            }
        }

        return tasks;
    }

    public void MarkMoved(FileEntry file, string destinationPath)
    {
        if (file.State == FileState.Moved || file.AssignedBucket is not BucketId id)
        {
            return;
        }

        undo.Clear();
        assignedPerBucket[id]--;
        TotalAssigned--;

        file.CurrentPath = destinationPath;
        file.State = FileState.Moved;
        file.FailureReason = null;

        Bucket? bucket = session.FindBucket(id);
        if (bucket is not null)
        {
            bucket.ExistingFileCount++;
        }
    }

    public void MarkFailed(FileEntry file, string reason)
    {
        if (file.State == FileState.Moved)
        {
            return;
        }

        file.State = FileState.Failed;
        file.FailureReason = reason;
    }
}
