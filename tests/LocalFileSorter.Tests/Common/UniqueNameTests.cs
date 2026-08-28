using LocalFileSorter.Common.Services;

using Xunit;

namespace LocalFileSorter.Tests.Common;

public sealed class UniqueNameTests : IDisposable
{
    private readonly string root = Directory.CreateTempSubdirectory("lfs-name-").FullName;

    [Fact]
    public void KeepsTheOriginalNameWhenNothingCollides()
    {
        Assert.True(UniqueName.TryResolve(root, "report.txt", out string path));

        Assert.Equal(Path.Combine(root, "report.txt"), path);
    }

    [Fact]
    public void AppendsACounterOnCollision()
    {
        Write("report.txt");

        Assert.True(UniqueName.TryResolve(root, "report.txt", out string path));

        Assert.Equal(Path.Combine(root, "report (1).txt"), path);
    }

    [Fact]
    public void ProbesUntilACounterIsFree()
    {
        Write("report.txt");
        Write("report (1).txt");
        Write("report (2).txt");

        Assert.True(UniqueName.TryResolve(root, "report.txt", out string path));

        Assert.Equal(Path.Combine(root, "report (3).txt"), path);
    }

    [Fact]
    public void KeepsTheExtensionlessNameIntact()
    {
        Write("LICENSE");

        Assert.True(UniqueName.TryResolve(root, "LICENSE", out string path));

        Assert.Equal(Path.Combine(root, "LICENSE (1)"), path);
    }

    [Fact]
    public void TreatsADirectoryAsATakenName()
    {
        Directory.CreateDirectory(Path.Combine(root, "report.txt"));

        Assert.True(UniqueName.TryResolve(root, "report.txt", out string path));

        Assert.Equal(Path.Combine(root, "report (1).txt"), path);
    }

    public void Dispose() => Directory.Delete(root, recursive: true);

    private void Write(string name) => File.WriteAllText(Path.Combine(root, name), name);
}
