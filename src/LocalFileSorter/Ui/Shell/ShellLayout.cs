using LocalFileSorter.Ui.Theme;

using SFML.Graphics;
using SFML.System;

namespace LocalFileSorter.Ui.Shell;

public readonly record struct ShellLayout(FloatRect Preview, FloatRect Queue, FloatRect Buckets)
{
    public static ShellLayout Compute(Vector2u windowSize)
    {
        float width = windowSize.X;
        float height = windowSize.Y;

        float bucketsX = width - UiTheme.BucketsWidth;
        float queueX = bucketsX - UiTheme.PanelGap - UiTheme.QueueWidth;
        float previewWidth = MathF.Max(0f, queueX - UiTheme.PanelGap);

        return new ShellLayout(
            new FloatRect(new Vector2f(0f, 0f), new Vector2f(previewWidth, height)),
            new FloatRect(new Vector2f(queueX, 0f), new Vector2f(UiTheme.QueueWidth, height)),
            new FloatRect(new Vector2f(bucketsX, 0f), new Vector2f(UiTheme.BucketsWidth, height)));
    }
}
