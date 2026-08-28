using LocalFileSorter.Common.Abstractions;
using LocalFileSorter.Common.Localization;
using LocalFileSorter.Common.Model;

using SFML.System;

using LoadingFailedException = SFML.LoadingFailedException;
using SfmlImage = SFML.Graphics.Image;

namespace LocalFileSorter.Previews.Image;

public sealed class ImagePreviewProvider : IPreviewProvider
{
    private const long MaxFileBytes = 128L * 1024L * 1024L;

    private static readonly HashSet<string> KnownExtensions = new(StringComparer.Ordinal)
    {
        ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tga", ".psd", ".hdr", ".pic",
    };

    private readonly Strings strings;

    public ImagePreviewProvider(Strings strings)
    {
        this.strings = strings;
    }

    public string Id => "image";

    public int Priority => 0;

    public IReadOnlySet<string> Extensions => KnownExtensions;

    public bool CanHandle(FileEntry entry) => KnownExtensions.Contains(entry.Extension);

    public PreviewResult Load(FileEntry entry, PreviewBudget budget)
    {
        using SfmlImage? decoded = Decode(entry, out string? error);
        if (decoded is null)
        {
            return PreviewResult.Failed(error!);
        }

        budget.Ct.ThrowIfCancellationRequested();

        Vector2u size = decoded.Size;
        long pixels = (long)size.X * size.Y;
        if (pixels == 0L || pixels > budget.MaxImagePixels)
        {
            return PreviewResult.Failed(strings.ImageTooLarge);
        }

        ImageBlock block = new((int)size.X, (int)size.Y, decoded.Pixels);
        MetadataItem[] metadata =
        [
            new MetadataItem(
                strings.ImageResolution,
                string.Format(strings.ImageResolutionValue, size.X, size.Y)),
        ];

        return new PreviewResult(new PreviewDocument([block]), metadata, null);
    }

    private SfmlImage? Decode(FileEntry entry, out string? error)
    {
        try
        {
            using FileStream stream = new(
                entry.CurrentPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete);

            if (stream.Length > MaxFileBytes)
            {
                error = strings.ImageTooLarge;
                return null;
            }

            error = null;
            return new SfmlImage(stream);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            error = string.Format(strings.PreviewUnreadable, exception.Message);
            return null;
        }
        catch (LoadingFailedException)
        {
            error = strings.ImageDecodeFailed;
            return null;
        }
    }
}
