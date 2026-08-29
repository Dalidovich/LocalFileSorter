using System.Formats.Tar;
using System.IO.Compression;
using System.Text;

using LocalFileSorter.Common.Abstractions;
using LocalFileSorter.Common.Localization;
using LocalFileSorter.Common.Model;
using LocalFileSorter.Previews.Archive;

using Xunit;

namespace LocalFileSorter.Tests.Previews;

public sealed class ArchivePreviewProviderTests : IDisposable
{
    private readonly string root = Directory.CreateTempSubdirectory("lfs-archive-").FullName;
    private readonly Strings strings = TestStrings.Shipped();
    private readonly ArchivePreviewProvider provider;

    public ArchivePreviewProviderTests()
    {
        provider = new ArchivePreviewProvider(strings);
    }

    [Fact]
    public void ClaimsArchiveExtensionsOnly()
    {
        Assert.Contains(".zip", provider.Extensions);
        Assert.Contains(".tar", provider.Extensions);
        Assert.Contains(".tgz", provider.Extensions);
        Assert.Contains(".gz", provider.Extensions);
        Assert.Contains(".7z", provider.Extensions);
        Assert.Contains(".rar", provider.Extensions);
        Assert.DoesNotContain(".png", provider.Extensions);
    }

    [Fact]
    public void ListsZipEntriesAsMonospacedText()
    {
        string path = Zip("a.zip", ("docs/", null), ("docs/readme.txt", "hello"));

        PreviewResult result = Load(path);

        TextBlock block = Assert.IsType<TextBlock>(result.Document!.Blocks[0]);
        Assert.Equal(TextPitch.Mono, block.Style.Pitch);
        string[] lines = block.Text.TrimEnd('\n').Split('\n');
        Assert.Equal(2, lines.Length);
        Assert.EndsWith("docs/", lines[0], StringComparison.Ordinal);
        Assert.EndsWith("docs/readme.txt", lines[1], StringComparison.Ordinal);
        Assert.Contains("5 B", lines[1], StringComparison.Ordinal);
    }

    [Fact]
    public void ReportsFormatEntryCountAndUncompressedSize()
    {
        string path = Zip("a.zip", ("one.txt", "12345"), ("two.txt", "678"));

        PreviewResult result = Load(path);

        Assert.Equal("ZIP", Metadata(result, strings.ArchiveFormat));
        Assert.Equal("2", Metadata(result, strings.ArchiveEntries));
        Assert.Equal("8 B", Metadata(result, strings.ArchiveUncompressed));
    }

    [Fact]
    public void ListsTarEntries()
    {
        string path = Tar("a.tar", gzip: false, ("one.txt", "12345"));

        PreviewResult result = Load(path);

        Assert.Equal("TAR", Metadata(result, strings.ArchiveFormat));
        Assert.Equal("1", Metadata(result, strings.ArchiveEntries));
        Assert.Contains("one.txt", Assert.IsType<TextBlock>(result.Document!.Blocks[0]).Text, StringComparison.Ordinal);
    }

    [Fact]
    public void ListsGzippedTarEntries()
    {
        string path = Tar("a.tar.gz", gzip: true, ("one.txt", "12345"), ("two.txt", "678"));

        PreviewResult result = Load(path);

        Assert.Equal("TAR.GZ", Metadata(result, strings.ArchiveFormat));
        Assert.Equal("2", Metadata(result, strings.ArchiveEntries));
        Assert.Equal("8 B", Metadata(result, strings.ArchiveUncompressed));
    }

    [Fact]
    public void ListsSevenZipEntries()
    {
        PreviewResult result = Load(Fixture("sample.7z"));

        Assert.Equal("7Z", Metadata(result, strings.ArchiveFormat));
        Assert.Equal("3", Metadata(result, strings.ArchiveEntries));
        Assert.Equal("8 B", Metadata(result, strings.ArchiveUncompressed));

        string[] lines = Assert.IsType<TextBlock>(result.Document!.Blocks[0]).Text.TrimEnd('\n').Split('\n');
        Assert.EndsWith("docs/", lines[0], StringComparison.Ordinal);
        Assert.EndsWith("docs/two.txt", lines[1], StringComparison.Ordinal);
        Assert.EndsWith("one.txt", lines[2], StringComparison.Ordinal);
    }

    [Fact]
    public void ListsRarEntriesWithForwardSlashes()
    {
        PreviewResult result = Load(Fixture("sample.rar"));

        Assert.Equal("RAR", Metadata(result, strings.ArchiveFormat));
        Assert.Equal("2", Metadata(result, strings.ArchiveEntries));
        Assert.Equal("8 B", Metadata(result, strings.ArchiveUncompressed));
        Assert.Contains("docs/two.txt", Assert.IsType<TextBlock>(result.Document!.Blocks[0]).Text, StringComparison.Ordinal);
    }

    [Fact]
    public void ReportsAnArchiveWithEncryptedHeadersAsEncrypted()
    {
        Assert.Equal(strings.ArchiveEncrypted, Load(Fixture("encrypted-headers.7z")).Error);
    }

    [Fact]
    public void SaysSoWhenTheArchiveHasNoEntries()
    {
        PreviewResult result = Load(Zip("empty.zip"));

        MessageBlock block = Assert.IsType<MessageBlock>(result.Document!.Blocks[0]);
        Assert.Equal(strings.ArchiveEmpty, block.Text);
        Assert.Equal(MessageKind.Info, block.Kind);
        Assert.Null(result.Error);
    }

    [Fact]
    public void TruncatesLongListingsAndCountsEveryEntry()
    {
        (string Name, string? Content)[] entries =
        [
            .. Enumerable
                .Range(0, ArchivePreviewProvider.MaxListedEntries + 5)
                .Select(index => ($"file{index}.txt", (string?)"x")),
        ];

        PreviewResult result = Load(Zip("many.zip", entries));

        string[] lines = Assert.IsType<TextBlock>(result.Document!.Blocks[0]).Text.TrimEnd('\n').Split('\n');
        Assert.Equal(ArchivePreviewProvider.MaxListedEntries, lines.Length);
        Assert.Equal(MessageKind.Info, Assert.IsType<MessageBlock>(result.Document.Blocks[1]).Kind);
        Assert.Equal(
            (ArchivePreviewProvider.MaxListedEntries + 5).ToString(),
            Metadata(result, strings.ArchiveEntries));
    }

    [Fact]
    public void ReportsADamagedArchiveAsAnError()
    {
        string path = Path.Combine(root, "broken.zip");
        File.WriteAllText(path, "not an archive");

        PreviewResult result = Load(path);

        Assert.Equal(strings.ArchiveDamaged, result.Error);
        Assert.Equal(MessageKind.Error, Assert.IsType<MessageBlock>(result.Document!.Blocks[0]).Kind);
    }

    [Fact]
    public void ReportsADamagedSevenZipAsAnError()
    {
        string path = Path.Combine(root, "broken.7z");
        File.WriteAllText(path, "not an archive");

        Assert.Equal(strings.ArchiveDamaged, Load(path).Error);
    }

    [Fact]
    public void ReportsAGzipStreamThatIsNotATarAsAnError()
    {
        string path = Path.Combine(root, "plain.gz");
        using (FileStream file = File.Create(path))
        using (GZipStream compressor = new(file, CompressionMode.Compress))
        {
            compressor.Write(Encoding.UTF8.GetBytes("just text, no tar headers"));
        }

        Assert.Equal(strings.ArchiveDamaged, Load(path).Error);
    }

    [Fact]
    public void ReportsAnUnreadableFileAsAnError()
    {
        PreviewResult result = Load(Path.Combine(root, "missing.zip"));

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

    private static string Fixture(string name) =>
        Path.Combine(AppContext.BaseDirectory, "assets", "archives", name);

    private static string Metadata(PreviewResult result, string label) =>
        result.ExtraMetadata.Single(item => item.Label == label).Value;

    private static FileEntry Entry(string path) => new()
    {
        Id = new FileId(0),
        CurrentPath = path,
        Name = Path.GetFileName(path),
        Extension = Path.GetExtension(path).ToLowerInvariant(),
        SizeBytes = File.Exists(path) ? new FileInfo(path).Length : 0L,
        CreatedUtc = DateTime.UnixEpoch,
        ModifiedUtc = DateTime.UnixEpoch,
    };

    private PreviewResult Load(string path) => provider.Load(Entry(path), Budget());

    private string Zip(string name, params (string Name, string? Content)[] entries)
    {
        string path = Path.Combine(root, name);
        using FileStream file = File.Create(path);
        using ZipArchive archive = new(file, ZipArchiveMode.Create);

        foreach ((string entryName, string? content) in entries)
        {
            ZipArchiveEntry entry = archive.CreateEntry(entryName);
            if (content is null)
            {
                continue;
            }

            using StreamWriter writer = new(entry.Open());
            writer.Write(content);
        }

        return path;
    }

    private string Tar(string name, bool gzip, params (string Name, string Content)[] entries)
    {
        string staging = Directory.CreateDirectory(Path.Combine(root, Path.GetRandomFileName())).FullName;
        foreach ((string entryName, string content) in entries)
        {
            File.WriteAllText(Path.Combine(staging, entryName), content);
        }

        string path = Path.Combine(root, name);
        using (FileStream file = File.Create(path))
        {
            if (gzip)
            {
                using GZipStream compressor = new(file, CompressionMode.Compress);
                TarFile.CreateFromDirectory(staging, compressor, includeBaseDirectory: false);
            }
            else
            {
                TarFile.CreateFromDirectory(staging, file, includeBaseDirectory: false);
            }
        }

        Directory.Delete(staging, recursive: true);
        return path;
    }
}
