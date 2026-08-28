using LocalFileSorter.Common.Services;

using Xunit;

namespace LocalFileSorter.Tests.Common;

public sealed class RootValidatorTests : IDisposable
{
    private readonly string root = Directory.CreateTempSubdirectory("lfs-tests-").FullName;

    [Fact]
    public void RejectsEmptyPath()
    {
        Assert.Equal(RootProblem.PathRequired, RootValidator.ValidateSource("   ").Problem);
    }

    [Fact]
    public void RejectsMissingFolder()
    {
        Assert.Equal(RootProblem.NotFound, RootValidator.ValidateSource(Path.Combine(root, "nope")).Problem);
    }

    [Fact]
    public void AcceptsExistingFolderAndReturnsFullPath()
    {
        string source = Directory.CreateDirectory(Path.Combine(root, "source")).FullName;

        RootValidation validation = RootValidator.ValidateSource(source + Path.DirectorySeparatorChar);

        Assert.True(validation.IsValid);
        Assert.Equal(source, validation.FullPath);
    }

    [Fact]
    public void AcceptsQuotedPath()
    {
        string source = Directory.CreateDirectory(Path.Combine(root, "quoted")).FullName;

        Assert.True(RootValidator.ValidateSource("\"" + source + "\"").IsValid);
    }

    [Fact]
    public void RejectsEqualRoots()
    {
        Assert.Equal(RootProblem.RootsEqual, RootValidator.ValidatePair(root, root).Problem);
    }

    [Fact]
    public void RejectsDestinationInsideSource()
    {
        string destination = Directory.CreateDirectory(Path.Combine(root, "inner")).FullName;

        Assert.Equal(RootProblem.DestinationInsideSource, RootValidator.ValidatePair(root, destination).Problem);
    }

    [Fact]
    public void AcceptsSiblingRoots()
    {
        string source = Directory.CreateDirectory(Path.Combine(root, "in")).FullName;
        string destination = Directory.CreateDirectory(Path.Combine(root, "out")).FullName;

        Assert.True(RootValidator.ValidatePair(source, destination).IsValid);
    }

    [Fact]
    public void DoesNotTreatNamePrefixAsContainment()
    {
        string source = Directory.CreateDirectory(Path.Combine(root, "data")).FullName;
        string destination = Directory.CreateDirectory(Path.Combine(root, "data-out")).FullName;

        Assert.True(RootValidator.ValidatePair(source, destination).IsValid);
    }

    [Fact]
    public void AcceptsWritableDestination()
    {
        Assert.True(RootValidator.ValidateDestination(root).IsValid);
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
}
