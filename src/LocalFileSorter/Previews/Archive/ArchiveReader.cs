using System.Formats.Tar;
using System.IO.Compression;

using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;

namespace LocalFileSorter.Previews.Archive;

public static class ArchiveReader
{
    public static ArchiveListing Read(string path, ArchiveFormat format, int maxEntries, CancellationToken ct)
    {
        using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

        if (format == ArchiveFormat.Zip)
        {
            return ReadZip(stream, maxEntries, ct);
        }

        if (format == ArchiveFormat.Tar)
        {
            return ReadTar(stream, ArchiveFormat.Tar, maxEntries, ct);
        }

        if (format is ArchiveFormat.SevenZip or ArchiveFormat.Rar)
        {
            return ReadWithSharpCompress(stream, format, maxEntries, ct);
        }

        using GZipStream decompressed = new(stream, CompressionMode.Decompress);
        return ReadTar(decompressed, ArchiveFormat.GzippedTar, maxEntries, ct);
    }

    private static ArchiveListing ReadWithSharpCompress(Stream stream, ArchiveFormat format, int maxEntries, CancellationToken ct)
    {
        using IArchive archive = format == ArchiveFormat.SevenZip
            ? SevenZipArchive.OpenArchive(stream)
            : RarArchive.OpenArchive(stream);

        List<ArchiveEntry> entries = [];
        int total = 0;
        long uncompressed = 0L;

        foreach (IArchiveEntry entry in archive.Entries)
        {
            ct.ThrowIfCancellationRequested();

            long size = entry.IsDirectory ? 0L : entry.Size;
            total++;
            uncompressed += size;

            if (entries.Count < maxEntries)
            {
                entries.Add(new ArchiveEntry(NormalizeSeparators(entry.Key), size, entry.IsDirectory));
            }
        }

        return new ArchiveListing(format, entries, total, uncompressed, total > entries.Count);
    }

    private static string NormalizeSeparators(string? name) =>
        name is null ? string.Empty : name.Replace('\\', '/');

    private static ArchiveListing ReadZip(Stream stream, int maxEntries, CancellationToken ct)
    {
        using ZipArchive archive = new(stream, ZipArchiveMode.Read);

        List<ArchiveEntry> entries = [];
        int total = 0;
        long uncompressed = 0L;

        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            ct.ThrowIfCancellationRequested();

            bool isDirectory = entry.Name.Length == 0;
            total++;
            uncompressed += entry.Length;

            if (entries.Count < maxEntries)
            {
                entries.Add(new ArchiveEntry(entry.FullName, entry.Length, isDirectory));
            }
        }

        return new ArchiveListing(ArchiveFormat.Zip, entries, total, uncompressed, total > entries.Count);
    }

    private static ArchiveListing ReadTar(Stream stream, ArchiveFormat format, int maxEntries, CancellationToken ct)
    {
        using TarReader reader = new(stream, leaveOpen: true);

        List<ArchiveEntry> entries = [];
        int total = 0;
        long uncompressed = 0L;

        while (reader.GetNextEntry(copyData: false) is TarEntry entry)
        {
            ct.ThrowIfCancellationRequested();

            bool isDirectory = entry.EntryType is TarEntryType.Directory or TarEntryType.DirectoryList;
            long size = isDirectory ? 0L : entry.Length;
            total++;
            uncompressed += size;

            if (entries.Count < maxEntries)
            {
                entries.Add(new ArchiveEntry(entry.Name, size, isDirectory));
            }
        }

        return new ArchiveListing(format, entries, total, uncompressed, total > entries.Count);
    }
}
