namespace LocalFileSorter.Common.Services;

public readonly record struct RootValidation(RootProblem Problem, string FullPath)
{
    public bool IsValid => Problem == RootProblem.None;

    public static RootValidation Ok(string fullPath) => new(RootProblem.None, fullPath);

    public static RootValidation Fail(RootProblem problem, string fullPath) => new(problem, fullPath);
}
