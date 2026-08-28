using LocalFileSorter.Common.Abstractions;

using SFML.Graphics;
using SFML.System;

using LoadingFailedException = SFML.LoadingFailedException;

namespace LocalFileSorter.Ui.Rendering;

public sealed class PreviewTextureCache : IDisposable
{
    private ImageBlock? block;
    private Texture? texture;

    public Texture? Resolve(ImageBlock source)
    {
        if (ReferenceEquals(block, source))
        {
            return texture;
        }

        texture?.Dispose();
        texture = Upload(source);
        block = source;
        return texture;
    }

    public void Dispose()
    {
        texture?.Dispose();
        texture = null;
        block = null;
    }

    private static Texture? Upload(ImageBlock source)
    {
        try
        {
            using Image image = new(new Vector2u((uint)source.Width, (uint)source.Height), source.Rgba);
            return new Texture(image) { Smooth = true };
        }
        catch (LoadingFailedException)
        {
            return null;
        }
    }
}
