namespace LocalFileSorter.Common.Model;

public sealed record MoveReport(int Moved, int Renamed, IReadOnlyList<MoveFailure> Failures, bool Cancelled, int Skipped);
