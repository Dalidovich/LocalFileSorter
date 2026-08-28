using LocalFileSorter.Common.Model;

namespace LocalFileSorter.Common.Services;

public sealed record MoveOutcome(MoveTask Task, MoveStatus Status, string? DestinationPath, string? Reason);
