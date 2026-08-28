using LocalFileSorter.Common.Localization;
using LocalFileSorter.Common.Model;

namespace LocalFileSorter.Common.Services;

public sealed class MoveExecutor
{
    private const int SharingViolation = unchecked((int)0x80070020);
    private const int LockViolation = unchecked((int)0x80070021);

    private readonly Strings strings;

    public MoveExecutor(Strings strings)
    {
        this.strings = strings;
    }

    public MoveOutcome Execute(MoveTask task)
    {
        FileEntry file = task.File;

        if (file.State == FileState.Moved)
        {
            return Failed(task, strings.CommitReasonAlreadyMoved);
        }

        if (!File.Exists(file.CurrentPath))
        {
            return Failed(task, strings.CommitReasonSourceMissing);
        }

        try
        {
            Directory.CreateDirectory(task.Bucket.DirectoryPath);
        }
        catch (Exception exception) when (IsFileSystem(exception))
        {
            return Failed(task, string.Format(strings.CommitReasonBucketUnavailable, exception.Message));
        }

        if (!UniqueName.TryResolve(task.Bucket.DirectoryPath, file.Name, out string destination))
        {
            return Failed(task, strings.CommitReasonNoFreeName);
        }

        try
        {
            File.Move(file.CurrentPath, destination);
        }
        catch (Exception exception) when (IsFileSystem(exception))
        {
            return Failed(task, Describe(exception));
        }

        bool renamed = !string.Equals(Path.GetFileName(destination), file.Name, StringComparison.Ordinal);
        return new MoveOutcome(task, renamed ? MoveStatus.Renamed : MoveStatus.Moved, destination, null);
    }

    private static bool IsFileSystem(Exception exception) =>
        exception is IOException or UnauthorizedAccessException or NotSupportedException;

    private static MoveOutcome Failed(MoveTask task, string reason) =>
        new(task, MoveStatus.Failed, null, reason);

    private string Describe(Exception exception) => exception switch
    {
        UnauthorizedAccessException => strings.CommitReasonDenied,
        IOException io when io.HResult is SharingViolation or LockViolation => strings.CommitReasonLocked,
        _ => string.Format(strings.CommitReasonMoveFailed, exception.Message),
    };
}
