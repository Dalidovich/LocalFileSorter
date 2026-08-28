using LocalFileSorter.Common.Localization;
using LocalFileSorter.Common.Services;

using Xunit;

namespace LocalFileSorter.Tests.Common;

public sealed class FileSizeFormatterTests
{
    private readonly Strings strings = TestStrings.Shipped();

    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(1023, "1023 B")]
    [InlineData(1024, "1 KB")]
    [InlineData(1536, "1.5 KB")]
    [InlineData(1048576, "1 MB")]
    [InlineData(1073741824, "1 GB")]
    public void FormatsWithTheLargestFittingUnit(long bytes, string expected)
    {
        Assert.Equal(expected, FileSizeFormatter.Format(bytes, strings));
    }
}
