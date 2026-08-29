using LocalFileSorter.Ui.Rendering;
using LocalFileSorter.Ui.Theme;

using SFML.Graphics;
using SFML.System;

namespace LocalFileSorter.Ui.Widgets;

public sealed class TooltipHost
{
    private string? pending;

    public void BeginFrame() => pending = null;

    public void Show(string text) => pending = text;

    public void Draw(Painter painter, Vector2f cursor, Vector2f surfaceSize)
    {
        if (string.IsNullOrEmpty(pending))
        {
            return;
        }

        TextMetrics metrics = painter.Metrics(mono: false, UiTheme.SmallTextSize);
        float width = metrics.Measure(pending) + (UiTheme.TooltipPadding * 2f);
        float height = metrics.LineHeight + (UiTheme.TooltipPadding * 2f);

        float x = Math.Clamp(cursor.X + 14f, 0f, MathF.Max(0f, surfaceSize.X - width));
        float y = Math.Clamp(cursor.Y + 18f, 0f, MathF.Max(0f, surfaceSize.Y - height));

        FloatRect area = new(new Vector2f(x, y), new Vector2f(width, height));
        painter.DrawPart(UiPart.Tooltip, PartState.Normal, area);
        painter.DrawText(
            pending,
            new Vector2f(x + UiTheme.TooltipPadding, y + UiTheme.TooltipPadding),
            UiTheme.SmallTextSize,
            UiTheme.Foreground(UiPart.Tooltip));
    }
}
