using System.Globalization;

using LocalFileSorter.Common.Abstractions;
using LocalFileSorter.Common.Localization;
using LocalFileSorter.Common.Model;
using LocalFileSorter.Common.Services;

namespace LocalFileSorter.Previews.Text;

public sealed class TextPreviewProvider : IPreviewProvider
{
    private static readonly HashSet<string> KnownExtensions = new(StringComparer.Ordinal)
    {
        ".txt", ".md", ".log", ".json", ".xml", ".csv", ".yml", ".yaml", ".ini", ".cfg",
        ".bat", ".c", ".cpp", ".cs", ".css", ".go", ".h", ".hpp", ".html", ".java",
        ".js", ".jsx", ".php", ".ps1", ".py", ".rb", ".rs", ".sh", ".sql", ".toml",
        ".ts", ".tsx",
    };

    private readonly Strings strings;

    public TextPreviewProvider(Strings strings)
    {
        this.strings = strings;
    }

    public string Id => "text";

    public int Priority => 0;

    public IReadOnlySet<string> Extensions => KnownExtensions;

    public bool CanHandle(FileEntry entry) => KnownExtensions.Contains(entry.Extension);

    public PreviewResult Load(FileEntry entry, PreviewBudget budget)
    {
        byte[] bytes;
        bool truncated;

        try
        {
            using FileStream stream = new(
                entry.CurrentPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete);

            truncated = stream.Length > budget.MaxBytes;
            bytes = new byte[(int)Math.Min(stream.Length, budget.MaxBytes)];
            stream.ReadExactly(bytes);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return PreviewResult.Failed(string.Format(strings.PreviewUnreadable, exception.Message));
        }

        budget.Ct.ThrowIfCancellationRequested();

        DecodedText decoded = TextDecoder.Decode(bytes, truncated);
        string text = decoded.Text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');

        List<PreviewBlock> blocks = [new TextBlock(text, TextStyle.Mono)];
        if (truncated)
        {
            blocks.Add(new MessageBlock(
                string.Format(strings.TextTruncationNotice, FileSizeFormatter.Format(budget.MaxBytes, strings)),
                MessageKind.Info));
        }

        MetadataItem[] metadata =
        [
            new MetadataItem(strings.TextLines, CountLines(text).ToString(CultureInfo.InvariantCulture)),
            new MetadataItem(strings.TextEncoding, decoded.EncodingName),
            new MetadataItem(strings.TextTruncated, truncated ? strings.CommonYes : strings.CommonNo),
        ];

        return new PreviewResult(new PreviewDocument(blocks), metadata, null);
    }

    private static int CountLines(string text)
    {
        if (text.Length == 0)
        {
            return 0;
        }

        int lines = 1;
        foreach (char character in text)
        {
            if (character == '\n')
            {
                lines++;
            }
        }

        return text.EndsWith('\n') ? lines - 1 : lines;
    }
}
