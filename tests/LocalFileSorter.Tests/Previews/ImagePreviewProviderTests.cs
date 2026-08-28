using LocalFileSorter.Common.Abstractions;
using LocalFileSorter.Common.Localization;
using LocalFileSorter.Common.Model;
using LocalFileSorter.Previews.Image;

using SFML.Graphics;
using SFML.System;

using Xunit;

namespace LocalFileSorter.Tests.Previews;

public sealed class ImagePreviewProviderTests : IDisposable
{
    private readonly Strings strings = TestStrings.Shipped();
    private readonly string root = Directory.CreateTempSubdirectory("lfs-image").FullName;

    public void Dispose() => Directory.Delete(root, true);

    [Fact]
    public void ClaimsTheSfmlDecodeSet()
    {
        ImagePreviewProvider provider = new(strings);

        Assert.Contains(".png", provider.Extensions);
        Assert.Contains(".jpg", provider.Extensions);
        Assert.DoesNotContain(".webp", provider.Extensions);
    }

    [Fact]
    public void DecodesAnImageIntoAnImageBlock()
    {
        FileEntry entry = Write("sample.png", 4, 3);

        PreviewResult result = Load(entry);

        ImageBlock block = Assert.IsType<ImageBlock>(Assert.Single(result.Document!.Blocks));
        Assert.Null(result.Error);
        Assert.Equal(4, block.Width);
        Assert.Equal(3, block.Height);
        Assert.Equal(4 * 3 * 4, block.Rgba.Length);
    }

    [Fact]
    public void ReportsTheResolution()
    {
        FileEntry entry = Write("sample.png", 20, 10);

        MetadataItem item = Assert.Single(Load(entry).ExtraMetadata);

        Assert.Equal(strings.ImageResolution, item.Label);
        Assert.Equal("20 x 10", item.Value);
    }

    [Fact]
    public void ReportsAFileThatIsNotAnImage()
    {
        string path = Path.Combine(root, "broken.png");
        File.WriteAllText(path, "not an image");

        PreviewResult result = Load(Entry(path));

        MessageBlock block = Assert.IsType<MessageBlock>(Assert.Single(result.Document!.Blocks));
        Assert.Equal(MessageKind.Error, block.Kind);
        Assert.Equal(strings.ImageDecodeFailed, block.Text);
    }

    [Fact]
    public void RejectsAnImageBeyondThePixelBudget()
    {
        FileEntry entry = Write("big.png", 4, 3);

        PreviewResult result = new ImagePreviewProvider(strings)
            .Load(entry, new PreviewBudget(PreviewBudget.DefaultMaxBytes, 4, CancellationToken.None));

        MessageBlock block = Assert.IsType<MessageBlock>(Assert.Single(result.Document!.Blocks));
        Assert.Equal(strings.ImageTooLarge, block.Text);
    }

    [Fact]
    public void ReportsAMissingFile()
    {
        PreviewResult result = Load(Entry(Path.Combine(root, "absent.png")));

        Assert.NotNull(result.Error);
        Assert.IsType<MessageBlock>(Assert.Single(result.Document!.Blocks));
    }

    private PreviewResult Load(FileEntry entry) =>
        new ImagePreviewProvider(strings).Load(entry, PreviewBudget.Default(CancellationToken.None));

    private FileEntry Write(string name, uint width, uint height)
    {
        string path = Path.Combine(root, name);
        using Image image = new(new Vector2u(width, height), Color.Blue);
        Assert.True(image.SaveToFile(path));
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
