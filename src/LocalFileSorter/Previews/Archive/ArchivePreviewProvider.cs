using System.Globalization;
using System.Text;

using LocalFileSorter.Common.Abstractions;
using LocalFileSorter.Common.Localization;
using LocalFileSorter.Common.Model;
using LocalFileSorter.Common.Services;

using SharpCompress.Common;

namespace LocalFileSorter.Previews.Archive;

public sealed class ArchivePreviewProvider : IPreviewProvider
{
    public const int MaxListedEntries = 1000;

    private const long MaxCompressedStreamBytes = 256L * 1024L * 1024L;

    private const int SizeColumnWidth = 10;

    private static readonly HashSet<string> KnownExtensions = new(StringComparer.Ordinal)
    {
        ".zip", ".tar", ".tgz", ".gz", ".7z", ".rar",
    };

    private readonly Strings strings;

    public ArchivePreviewProvider(Strings strings)
    {
        this.strings = strings;
    }

    public string Id => "archive";

    public int Priority => 0;

    public IReadOnlySet<string> Extensions => KnownExtensions;

    public bool CanHandle(FileEntry entry) => KnownExtensions.Contains(entry.Extension);

    public PreviewResult Load(FileEntry entry, PreviewBudget budget)
    {
        ArchiveFormat format = ResolveFormat(entry.Extension);
        if (format == ArchiveFormat.GzippedTar && entry.SizeBytes > MaxCompressedStreamBytes)
        {
            return PreviewResult.Failed(strings.ArchiveTooLarge);
        }

        ArchiveListing listing;

        try
        {
            listing = ArchiveReader.Read(entry.CurrentPath, format, MaxListedEntries, budget.Ct);
        }
        catch (CryptographicException)
        {
            return PreviewResult.Failed(strings.ArchiveEncrypted);
        }
        catch (Exception exception) when (exception is InvalidDataException or EndOfStreamException
            or ArchiveException or ExtractionException)
        {
            return PreviewResult.Failed(strings.ArchiveDamaged);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return PreviewResult.Failed(string.Format(strings.PreviewUnreadable, exception.Message));
        }

        budget.Ct.ThrowIfCancellationRequested();

        List<PreviewBlock> blocks = [];
        if (listing.Entries.Count == 0)
        {
            blocks.Add(new MessageBlock(strings.ArchiveEmpty, MessageKind.Info));
        }
        else
        {
            blocks.Add(new TextBlock(Render(listing), TextStyle.Mono));
        }

        if (listing.Truncated)
        {
            blocks.Add(new MessageBlock(
                string.Format(strings.ArchiveTruncationNotice, MaxListedEntries.ToString(CultureInfo.InvariantCulture)),
                MessageKind.Info));
        }

        MetadataItem[] metadata =
        [
            new MetadataItem(strings.ArchiveFormat, FormatName(listing.Format)),
            new MetadataItem(strings.ArchiveEntries, listing.TotalEntryCount.ToString(CultureInfo.InvariantCulture)),
            new MetadataItem(strings.ArchiveUncompressed, FileSizeFormatter.Format(listing.UncompressedBytes, strings)),
        ];

        return new PreviewResult(new PreviewDocument(blocks), metadata, null);
    }

    private static ArchiveFormat ResolveFormat(string extension) => extension switch
    {
        ".zip" => ArchiveFormat.Zip,
        ".tar" => ArchiveFormat.Tar,
        ".7z" => ArchiveFormat.SevenZip,
        ".rar" => ArchiveFormat.Rar,
        _ => ArchiveFormat.GzippedTar,
    };

    private static string FormatName(ArchiveFormat format) => format switch
    {
        ArchiveFormat.Zip => "ZIP",
        ArchiveFormat.Tar => "TAR",
        ArchiveFormat.SevenZip => "7Z",
        ArchiveFormat.Rar => "RAR",
        _ => "TAR.GZ",
    };

    private string Render(ArchiveListing listing)
    {
        StringBuilder builder = new();

        foreach (ArchiveEntry entry in listing.Entries)
        {
            string size = entry.IsDirectory ? string.Empty : FileSizeFormatter.Format(entry.SizeBytes, strings);
            builder.Append(size.PadLeft(SizeColumnWidth));
            builder.Append("  ");
            builder.Append(entry.Name);

            if (entry.IsDirectory && !entry.Name.EndsWith('/'))
            {
                builder.Append('/');
            }

            builder.Append('\n');
        }

        return builder.ToString();
    }
}
