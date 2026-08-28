using SFML.Graphics;

namespace LocalFileSorter.Ui.Rendering;

public sealed class FontLibrary : IDisposable
{
    private readonly Dictionary<(bool Mono, uint Size), TextMetrics> metrics = [];

    public FontLibrary(string uiFontPath, string monoFontPath)
    {
        Ui = new Font(uiFontPath);
        Mono = new Font(monoFontPath);
    }

    public Font Ui { get; }

    public Font Mono { get; }

    public TextMetrics Metrics(bool mono, uint size)
    {
        if (metrics.TryGetValue((mono, size), out TextMetrics? cached))
        {
            return cached;
        }

        TextMetrics created = new(mono ? Mono : Ui, size);
        metrics[(mono, size)] = created;
        return created;
    }

    public void Dispose()
    {
        Ui.Dispose();
        Mono.Dispose();
    }
}
