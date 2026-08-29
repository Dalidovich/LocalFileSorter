using System.IO.Compression;
using System.Text;

using LocalFileSorter.Common.Abstractions;
using LocalFileSorter.Common.Localization;
using LocalFileSorter.Common.Model;
using LocalFileSorter.Previews.Vector;

using Xunit;

namespace LocalFileSorter.Tests.Previews;

public sealed class SvgPreviewProviderTests : IDisposable
{
    private const string RedSquare =
        "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"100\" height=\"50\">" +
        "<rect width=\"100\" height=\"50\" fill=\"#ff0000\"/></svg>";

    private readonly Strings strings = TestStrings.Shipped();
    private readonly string root = Directory.CreateTempSubdirectory("lfs-svg").FullName;

    public void Dispose() => Directory.Delete(root, true);

    [Fact]
    public void ClaimsTheVectorExtensions()
    {
        SvgPreviewProvider provider = new(strings);

        Assert.Contains(".svg", provider.Extensions);
        Assert.Contains(".svgz", provider.Extensions);
        Assert.DoesNotContain(".png", provider.Extensions);
    }

    [Fact]
    public void RasterizesTheDocumentIntoAnImageBlock()
    {
        PreviewResult result = Load(Write("square.svg", RedSquare));

        ImageBlock block = Assert.IsType<ImageBlock>(Assert.Single(result.Document!.Blocks));
        Assert.Null(result.Error);
        Assert.Equal(SvgRasterizer.TargetLongEdge, block.Width);
        Assert.Equal(SvgRasterizer.TargetLongEdge / 2, block.Height);
        Assert.Equal(block.Width * block.Height * 4, block.Rgba.Length);
    }

    [Fact]
    public void KeepsFillColoursOpaque()
    {
        ImageBlock block = Assert.IsType<ImageBlock>(Assert.Single(Load(Write("square.svg", RedSquare)).Document!.Blocks));

        int centre = ((block.Height / 2 * block.Width) + (block.Width / 2)) * 4;

        Assert.Equal(255, block.Rgba[centre]);
        Assert.Equal(0, block.Rgba[centre + 1]);
        Assert.Equal(0, block.Rgba[centre + 2]);
        Assert.Equal(255, block.Rgba[centre + 3]);
    }

    [Fact]
    public void LeavesUncoveredAreaTransparent()
    {
        string document =
            "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"100\" height=\"100\">" +
            "<rect x=\"0\" y=\"0\" width=\"10\" height=\"10\" fill=\"#00ff00\"/></svg>";

        ImageBlock block = Assert.IsType<ImageBlock>(Assert.Single(Load(Write("corner.svg", document)).Document!.Blocks));

        int lastPixel = ((block.Width * block.Height) - 1) * 4;

        Assert.Equal(0, block.Rgba[lastPixel + 3]);
    }

    [Fact]
    public void ReportsTheViewportAndTheRenderedSize()
    {
        PreviewResult result = Load(Write("square.svg", RedSquare));

        Assert.Collection(
            result.ExtraMetadata,
            item =>
            {
                Assert.Equal(strings.SvgViewport, item.Label);
                Assert.Equal("100 x 50", item.Value);
            },
            item =>
            {
                Assert.Equal(strings.SvgRendered, item.Label);
                Assert.Equal($"{SvgRasterizer.TargetLongEdge} x {SvgRasterizer.TargetLongEdge / 2}", item.Value);
            });
    }

    [Fact]
    public void ScalesTheViewBoxRatherThanTheAttributes()
    {
        string document =
            "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 40 20\" width=\"400\" height=\"200\">" +
            "<rect width=\"40\" height=\"20\" fill=\"#0000ff\"/></svg>";

        ImageBlock block = Assert.IsType<ImageBlock>(Assert.Single(Load(Write("viewbox.svg", document)).Document!.Blocks));

        Assert.Equal(2, block.Width / block.Height);
    }

    [Fact]
    public void ReadsAGzippedDocument()
    {
        string path = Path.Combine(root, "square.svgz");
        using (FileStream file = File.Create(path))
        using (GZipStream compressed = new(file, CompressionLevel.Optimal))
        {
            compressed.Write(Encoding.UTF8.GetBytes(RedSquare));
        }

        PreviewResult result = Load(Entry(path));

        Assert.Null(result.Error);
        Assert.IsType<ImageBlock>(Assert.Single(result.Document!.Blocks));
    }

    [Fact]
    public void HonoursThePixelBudget()
    {
        FileEntry entry = Write("square.svg", RedSquare);

        PreviewResult result = new SvgPreviewProvider(strings)
            .Load(entry, new PreviewBudget(PreviewBudget.DefaultMaxBytes, 200, CancellationToken.None));

        ImageBlock block = Assert.IsType<ImageBlock>(Assert.Single(result.Document!.Blocks));

        Assert.True(block.Width * block.Height <= 200);
    }

    [Fact]
    public void ReportsAFileThatIsNotSvg()
    {
        PreviewResult result = Load(Write("broken.svg", "not a document"));

        MessageBlock block = Assert.IsType<MessageBlock>(Assert.Single(result.Document!.Blocks));

        Assert.Equal(MessageKind.Error, block.Kind);
        Assert.Equal(strings.SvgDecodeFailed, block.Text);
    }

    [Fact]
    public void ReportsXmlThatIsNotRootedInSvg()
    {
        PreviewResult result = Load(Write("other.svg", "<root><child/></root>"));

        MessageBlock block = Assert.IsType<MessageBlock>(Assert.Single(result.Document!.Blocks));

        Assert.Equal(strings.SvgDecodeFailed, block.Text);
    }

    [Fact]
    public void ReportsADocumentWithoutDrawableContent()
    {
        PreviewResult result = Load(Write(
            "empty.svg",
            "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"100\" height=\"100\"></svg>"));

        MessageBlock block = Assert.IsType<MessageBlock>(Assert.Single(result.Document!.Blocks));

        Assert.Equal(strings.SvgEmpty, block.Text);
    }

    [Fact]
    public void ReportsAMissingFile()
    {
        PreviewResult result = Load(Entry(Path.Combine(root, "absent.svg")));

        Assert.NotNull(result.Error);
        Assert.IsType<MessageBlock>(Assert.Single(result.Document!.Blocks));
    }

    private PreviewResult Load(FileEntry entry) =>
        new SvgPreviewProvider(strings).Load(entry, PreviewBudget.Default(CancellationToken.None));

    private FileEntry Write(string name, string document)
    {
        string path = Path.Combine(root, name);
        File.WriteAllText(path, document);
        return Entry(path);
    }

    private static FileEntry Entry(string path) => new()
    {
        Id = new FileId(0),
        CurrentPath = path,
        Name = Path.GetFileName(path),
        Extension = Path.GetExtension(path).ToLowerInvariant(),
        SizeBytes = 0,
        CreatedUtc = DateTime.UnixEpoch,
        ModifiedUtc = DateTime.UnixEpoch,
    };
}
