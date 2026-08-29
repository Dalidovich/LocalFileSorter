using System.Xml;

using SkiaSharp;

using Svg;
using Svg.Model;
using Svg.Skia;

using ShimPicture = ShimSkiaSharp.SKPicture;

namespace LocalFileSorter.Previews.Vector;

public static class SvgRasterizer
{
    public const int TargetLongEdge = 1024;

    private const string RootElement = "svg";

    private static readonly SvgParameters Parameters = new(
        null,
        null,
        null,
        new SvgDocumentLoadOptions { ExternalResources = SvgExternalResourcePolicy.SameDocumentAndDataOnly });

    private static readonly XmlReaderSettings SniffSettings = new()
    {
        DtdProcessing = DtdProcessing.Ignore,
        XmlResolver = null,
        IgnoreComments = true,
        IgnoreProcessingInstructions = true,
        IgnoreWhitespace = true,
    };

    public static bool IsSvgDocument(byte[] content)
    {
        using MemoryStream stream = new(content, false);
        using XmlReader reader = XmlReader.Create(stream, SniffSettings);

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                return string.Equals(reader.LocalName, RootElement, StringComparison.Ordinal);
            }
        }

        return false;
    }

    public static RasterizedSvg? Rasterize(byte[] content, int maxPixels, CancellationToken ct)
    {
        using MemoryStream stream = new(content, false);
        using SKSvg svg = new();

        SKPicture? picture = svg.Load(stream, Parameters);
        ShimPicture? model = svg.Model;
        if (picture is null || model?.Commands is null || model.Commands.Count == 0)
        {
            return null;
        }

        ct.ThrowIfCancellationRequested();

        SKRect cull = picture.CullRect;
        if (!IsDrawable(cull))
        {
            return null;
        }

        float scale = ResolveScale(cull.Width, cull.Height, maxPixels);
        if (!float.IsFinite(scale) || scale <= 0f)
        {
            return null;
        }

        SKImageInfo info = new(
            Math.Max(1, (int)(cull.Width * scale)),
            Math.Max(1, (int)(cull.Height * scale)),
            SKColorType.Rgba8888,
            SKAlphaType.Premul);

        using SKBitmap bitmap = new(info);
        using (SKCanvas canvas = new(bitmap))
        {
            canvas.Clear(SKColors.Transparent);
            canvas.Scale(scale);
            canvas.Translate(-cull.Left, -cull.Top);
            canvas.DrawPicture(picture);
        }

        ct.ThrowIfCancellationRequested();

        return new RasterizedSvg(info.Width, info.Height, ToStraightRgba(bitmap, info), cull.Width, cull.Height);
    }

    private static bool IsDrawable(SKRect cull) =>
        float.IsFinite(cull.Width) && float.IsFinite(cull.Height) && cull.Width > 0f && cull.Height > 0f;

    private static float ResolveScale(float width, float height, int maxPixels)
    {
        float fit = TargetLongEdge / MathF.Max(width, height);
        float affordable = MathF.Sqrt(maxPixels / (width * height));
        return MathF.Min(fit, affordable);
    }

    private static byte[] ToStraightRgba(SKBitmap bitmap, SKImageInfo info)
    {
        ReadOnlySpan<byte> source = bitmap.GetPixelSpan();
        int rowLength = info.Width * 4;
        byte[] rgba = new byte[rowLength * info.Height];

        for (int row = 0; row < info.Height; row++)
        {
            ReadOnlySpan<byte> line = source.Slice(row * bitmap.RowBytes, rowLength);
            Span<byte> target = rgba.AsSpan(row * rowLength, rowLength);

            for (int offset = 0; offset < rowLength; offset += 4)
            {
                byte alpha = line[offset + 3];
                target[offset + 3] = alpha;

                if (alpha == byte.MaxValue)
                {
                    line.Slice(offset, 3).CopyTo(target[offset..]);
                }
                else if (alpha != 0)
                {
                    target[offset] = Straighten(line[offset], alpha);
                    target[offset + 1] = Straighten(line[offset + 1], alpha);
                    target[offset + 2] = Straighten(line[offset + 2], alpha);
                }
            }
        }

        return rgba;
    }

    private static byte Straighten(byte channel, byte alpha) => (byte)Math.Min(byte.MaxValue, channel * byte.MaxValue / alpha);
}
