namespace LocalFileSorter.Common.Model;

public sealed record MoveFailure(FileId FileId, string Name, string Reason);
