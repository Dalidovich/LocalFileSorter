using System.Globalization;

using LocalFileSorter.Common.Abstractions;
using LocalFileSorter.Common.Localization;
using LocalFileSorter.Common.Model;

namespace LocalFileSorter.Common.Services;

public static class FileMetadata
{
    public static IReadOnlyList<MetadataItem> Describe(FileEntry entry, Strings strings) =>
    [
        new MetadataItem(strings.MetaCreated, FormatTimestamp(entry.CreatedUtc, strings)),
        new MetadataItem(strings.MetaModified, FormatTimestamp(entry.ModifiedUtc, strings)),
        new MetadataItem(strings.MetaSize, FileSizeFormatter.Format(entry.SizeBytes, strings)),
        new MetadataItem(strings.MetaType, DescribeExtension(entry.Extension, strings)),
    ];

    public static string DescribeExtension(string extension, Strings strings) =>
        extension.Length == 0 ? strings.QueueNoExtension : extension;

    private static string FormatTimestamp(DateTime utc, Strings strings) =>
        utc.ToLocalTime().ToString(strings.FormatDateTime, CultureInfo.InvariantCulture);
}
