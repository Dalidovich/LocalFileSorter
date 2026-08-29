using System.Globalization;
using System.IO.Compression;
using System.Xml;

using LocalFileSorter.Common.Abstractions;
using LocalFileSorter.Common.Localization;
using LocalFileSorter.Common.Model;

namespace LocalFileSorter.Previews.Vector;

public sealed class SvgPreviewProvider : IPreviewProvider
{
    private const long MaxDocumentBytes = 32L * 1024L * 1024L;

    private const int ChunkBytes = 64 * 1024;

    private const string CompressedExtension = ".svgz";

    private static readonly HashSet<string> KnownExtensions = new(StringComparer.Ordinal)
    {
        ".svg", CompressedExtension,
    };

    private readonly Strings strings;

    public SvgPreviewProvider(Strings strings)
    {
        this.strings = strings;
    }

    public string Id => "svg";

    public int Priority => 0;

    public IReadOnlySet<string> Extensions => KnownExtensions;

    public bool CanHandle(FileEntry entry) => KnownExtensions.Contains(entry.Extension);

    public PreviewResult Load(FileEntry entry, PreviewBudget budget)
    {
        RasterizedSvg? raster;

        try
        {
            byte[]? document = Read(entry);
            if (document is null)
            {
                return PreviewResult.Failed(strings.SvgTooLarge);
            }

            budget.Ct.ThrowIfCancellationRequested();

            if (!SvgRasterizer.IsSvgDocument(document))
            {
                return PreviewResult.Failed(strings.SvgDecodeFailed);
            }

            raster = SvgRasterizer.Rasterize(document, budget.MaxImagePixels, budget.Ct);
        }
        catch (Exception exception) when (exception is XmlException or InvalidDataException
            or EndOfStreamException or FormatException)
        {
            return PreviewResult.Failed(strings.SvgDecodeFailed);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return PreviewResult.Failed(string.Format(strings.PreviewUnreadable, exception.Message));
        }

        if (raster is null)
        {
            return PreviewResult.Failed(strings.SvgEmpty);
        }

        MetadataItem[] metadata =
        [
            new MetadataItem(
                strings.SvgViewport,
                string.Format(strings.SvgSizeValue, Measure(raster.SourceWidth), Measure(raster.SourceHeight))),
            new MetadataItem(
                strings.SvgRendered,
                string.Format(strings.SvgSizeValue, raster.Width, raster.Height)),
        ];

        ImageBlock block = new(raster.Width, raster.Height, raster.Rgba);
        return new PreviewResult(new PreviewDocument([block]), metadata, null);
    }

    private static byte[]? Read(FileEntry entry)
    {
        using FileStream file = new(
            entry.CurrentPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);

        if (file.Length > MaxDocumentBytes)
        {
            return null;
        }

        using Stream content = entry.Extension == CompressedExtension
            ? new GZipStream(file, CompressionMode.Decompress)
            : file;

        using MemoryStream buffer = new();
        byte[] chunk = new byte[ChunkBytes];
        int read;

        while ((read = content.Read(chunk, 0, chunk.Length)) > 0)
        {
            if (buffer.Length + read > MaxDocumentBytes)
            {
                return null;
            }

            buffer.Write(chunk, 0, read);
        }

        return buffer.ToArray();
    }

    private static string Measure(float value) => value.ToString("0.##", CultureInfo.InvariantCulture);
}
