using System.Text;

using LocalFileSorter.Common.Abstractions;
using LocalFileSorter.Common.Localization;
using LocalFileSorter.Common.Model;
using LocalFileSorter.Previews.Text;

using Xunit;

namespace LocalFileSorter.Tests.Previews;

public sealed class TextPreviewProviderTests : IDisposable
{
    private readonly string root = Directory.CreateTempSubdirectory("lfs-text-").FullName;
    private readonly Strings strings = TestStrings.Shipped();
    private readonly TextPreviewProvider provider;

    public TextPreviewProviderTests()
    {
        provider = new TextPreviewProvider(strings);
    }

    [Fact]
    public void ClaimsTextExtensionsOnly()
    {
        Assert.Contains(".json", provider.Extensions);
        Assert.Contains(".cs", provider.Extensions);
        Assert.DoesNotContain(".png", provider.Extensions);
    }

    [Fact]
    public void RendersContentAsMonospacedText()
    {
        PreviewResult result = Load(Write("a.txt", "line one\nline two"));

        TextBlock block = Assert.IsType<TextBlock>(result.Document!.Blocks[0]);
        Assert.Equal("line one\nline two", block.Text);
        Assert.Equal(TextPitch.Mono, block.Style.Pitch);
    }

    [Fact]
    public void NormalizesWindowsLineEndings()
    {
        PreviewResult result = Load(Write("a.txt", "one\r\ntwo\r\n"));

        Assert.Equal("one\ntwo\n", Assert.IsType<TextBlock>(result.Document!.Blocks[0]).Text);
    }

    [Fact]
    public void ReportsLineCountEncodingAndTruncation()
    {
        PreviewResult result = Load(Write("a.txt", "one\ntwo\nthree"));

        Assert.Equal("3", Metadata(result, strings.TextLines));
        Assert.Equal("UTF-8", Metadata(result, strings.TextEncoding));
        Assert.Equal(strings.CommonNo, Metadata(result, strings.TextTruncated));
    }

    [Fact]
    public void DoesNotCountTheTrailingNewlineAsALine()
    {
        PreviewResult result = Load(Write("a.txt", "one\ntwo\n"));

        Assert.Equal("2", Metadata(result, strings.TextLines));
    }

    [Fact]
    public void DetectsUtf8Bom()
    {
        string path = Path.Combine(root, "bom.txt");
        File.WriteAllText(path, "héllo", new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        PreviewResult result = Load(path);

        Assert.Equal("UTF-8 (BOM)", Metadata(result, strings.TextEncoding));
        Assert.Equal("héllo", Assert.IsType<TextBlock>(result.Document!.Blocks[0]).Text);
    }

    [Fact]
    public void DetectsUtf16Bom()
    {
        string path = Path.Combine(root, "utf16.txt");
        File.WriteAllText(path, "héllo", Encoding.Unicode);

        Assert.Equal("UTF-16 LE", Metadata(Load(path), strings.TextEncoding));
    }

    [Fact]
    public void FallsBackToLatin1ForInvalidUtf8()
    {
        string path = Path.Combine(root, "latin.txt");
        File.WriteAllBytes(path, [0x63, 0x61, 0x66, 0xE9]);

        PreviewResult result = Load(path);

        Assert.Equal("Latin-1", Metadata(result, strings.TextEncoding));
        Assert.Equal("café", Assert.IsType<TextBlock>(result.Document!.Blocks[0]).Text);
    }

    [Fact]
    public void TruncatesAtTheBudgetAndSaysSo()
    {
        string path = Write("big.txt", new string('a', 300));

        PreviewResult result = Load(path, new PreviewBudget(100, 0, CancellationToken.None));

        Assert.Equal(100, Assert.IsType<TextBlock>(result.Document!.Blocks[0]).Text.Length);
        Assert.Equal(MessageKind.Info, Assert.IsType<MessageBlock>(result.Document.Blocks[1]).Kind);
        Assert.Equal(strings.CommonYes, Metadata(result, strings.TextTruncated));
    }

    [Fact]
    public void DoesNotMisreadUtf8SplitByTruncation()
    {
        string path = Write("split.txt", new string('é', 60));

        PreviewResult result = Load(path, new PreviewBudget(101, 0, CancellationToken.None));

        Assert.Equal("UTF-8", Metadata(result, strings.TextEncoding));
    }

    [Fact]
    public void ReportsAnUnreadableFileAsAnError()
    {
        PreviewResult result = provider.Load(Entry(Path.Combine(root, "missing.txt")), Budget());

        Assert.NotNull(result.Error);
        Assert.Equal(MessageKind.Error, Assert.IsType<MessageBlock>(result.Document!.Blocks[0]).Kind);
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

    private static PreviewBudget Budget() => PreviewBudget.Default(CancellationToken.None);

    private static string Metadata(PreviewResult result, string label) =>
        result.ExtraMetadata.Single(item => item.Label == label).Value;

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

    private PreviewResult Load(string path, PreviewBudget? budget = null) =>
        provider.Load(Entry(path), budget ?? Budget());

    private string Write(string name, string content)
    {
        string path = Path.Combine(root, name);
        File.WriteAllText(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        return path;
    }
}
