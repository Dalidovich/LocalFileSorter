namespace LocalFileSorter.Common.Services;

public enum RootProblem
{
    None,
    PathRequired,
    NotFound,
    NotReadable,
    NotWritable,
    RootsEqual,
    DestinationInsideSource,
}
