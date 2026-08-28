using LocalFileSorter.Common.Model;
using LocalFileSorter.Common.Services;

using Xunit;

namespace LocalFileSorter.Tests.Common;

public sealed class SourceScannerTests : IDisposable
{
    private static readonly HashSet<string> Supported = new(StringComparer.Ordinal) { ".txt", ".md" };

    private readonly string root = Directory.CreateTempSubdirectory("lfs-scan-").FullName;

    [Fact]
    public void KeepsOnlySupportedExtensions()
    {
        Write("a.txt");
        Write("b.md");
        Write("c.png");

        ScanResult result = SourceScanner.Scan(root, Supported);

        Assert.Equal(["a.txt", "b.md"], result.Files.Select(file => file.Name));
    }

    [Fact]
    public void CountsAndListsSkippedExtensions()
    {
        Write("a.txt");
        Write("b.png");
        Write("c.PNG");
        Write("d.webp");

        ScanResult result = SourceScanner.Scan(root, Supported);

        Assert.Equal(3, result.SkippedCount);
        Assert.Equal([".png", ".webp"], result.SkippedExtensions);
    }

    [Fact]
    public void TreatsExtensionMatchingAsCaseInsensitive()
    {
        Write("a.TXT");

        ScanResult result = SourceScanner.Scan(root, Supported);

        Assert.Single(result.Files);
        Assert.Equal(".txt", result.Files[0].Extension);
    }

    [Fact]
    public void SkipsFilesWithoutExtension()
    {
        Write("README");

        ScanResult result = SourceScanner.Scan(root, Supported);

        Assert.Empty(result.Files);
        Assert.Equal([""], result.SkippedExtensions);
    }

    [Fact]
    public void RecursesIntoSubdirectories()
    {
        Directory.CreateDirectory(Path.Combine(root, "nested", "deeper"));
        Write(Path.Combine("nested", "deep.txt"));
        Write(Path.Combine("nested", "deeper", "deepest.txt"));
        Write("top.txt");

        ScanResult result = SourceScanner.Scan(root, Supported);

        Assert.Equal(
            [
                Path.Combine(root, "nested", "deep.txt"),
                Path.Combine(root, "nested", "deeper", "deepest.txt"),
                Path.Combine(root, "top.txt"),
            ],
            result.Files.Select(file => file.CurrentPath));
    }

    [Fact]
    public void CountsSkippedFilesInSubdirectories()
    {
        Directory.CreateDirectory(Path.Combine(root, "nested"));
        Write(Path.Combine("nested", "deep.png"));

        ScanResult result = SourceScanner.Scan(root, Supported);

        Assert.Equal(1, result.SkippedCount);
        Assert.Equal([".png"], result.SkippedExtensions);
    }

    [Fact]
    public void KeepsSameNamedFilesFromDifferentDirectories()
    {
        Directory.CreateDirectory(Path.Combine(root, "nested"));
        Write("a.txt");
        Write(Path.Combine("nested", "a.txt"));

        ScanResult result = SourceScanner.Scan(root, Supported);

        Assert.Equal(2, result.Files.Count);
        Assert.Equal([new FileId(0), new FileId(1)], result.Files.Select(file => file.Id));
    }

    [Fact]
    public void OrdersByNameAndAssignsSequentialIds()
    {
        Write("c.txt");
        Write("a.txt");
        Write("b.txt");

        ScanResult result = SourceScanner.Scan(root, Supported);

        Assert.Equal(["a.txt", "b.txt", "c.txt"], result.Files.Select(file => file.Name));
        Assert.Equal([new FileId(0), new FileId(1), new FileId(2)], result.Files.Select(file => file.Id));
    }

    [Fact]
    public void CapturesFileFacts()
    {
        Write("a.txt", "hello");

        FileEntry entry = SourceScanner.Scan(root, Supported).Files[0];

        Assert.Equal(Path.Combine(root, "a.txt"), entry.CurrentPath);
        Assert.Equal(5, entry.SizeBytes);
        Assert.Equal(FileState.Pending, entry.State);
    }

    [Fact]
    public void ReturnsEmptyForMissingRoot()
    {
        ScanResult result = SourceScanner.Scan(Path.Combine(root, "nope"), Supported);

        Assert.Empty(result.Files);
        Assert.Equal(0, result.SkippedCount);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(root, recursive: true);
        }
        catch (IOException)
        {
        }
    }

    private void Write(string relativePath, string content = "x") =>
        File.WriteAllText(Path.Combine(root, relativePath), content);
}
