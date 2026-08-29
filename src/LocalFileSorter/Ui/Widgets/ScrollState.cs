using LocalFileSorter.Ui.Input;
using LocalFileSorter.Ui.Rendering;
using LocalFileSorter.Ui.Theme;

using SFML.Graphics;
using SFML.System;

namespace LocalFileSorter.Ui.Widgets;

public sealed class ScrollState
{
    public float Offset { get; private set; }

    public void Apply(UiContext input, FloatRect viewport, float contentHeight)
    {
        float wheel = input.WheelOver(viewport);
        if (wheel != 0f)
        {
            Offset -= wheel * UiTheme.ScrollStep;
        }

        Clamp(viewport.Size.Y, contentHeight);
    }

    public void Reset() => Offset = 0f;

    public void EnsureVisible(float itemTop, float itemHeight, float viewportHeight, float contentHeight)
    {
        if (itemTop < Offset)
        {
            Offset = itemTop;
        }
        else if (itemTop + itemHeight > Offset + viewportHeight)
        {
            Offset = itemTop + itemHeight - viewportHeight;
        }

        Clamp(viewportHeight, contentHeight);
    }

    public void DrawBar(Painter painter, FloatRect viewport, float contentHeight)
    {
        if (contentHeight <= viewport.Size.Y || viewport.Size.Y <= 0f)
        {
            return;
        }

        float thumbHeight = MathF.Max(24f, viewport.Size.Y * viewport.Size.Y / contentHeight);
        float travel = viewport.Size.Y - thumbHeight;
        float progress = Offset / (contentHeight - viewport.Size.Y);

        painter.DrawPart(
            UiPart.ScrollTrack,
            PartState.Normal,
            new FloatRect(
                new Vector2f(viewport.Position.X + viewport.Size.X - UiTheme.ScrollBarWidth, viewport.Position.Y),
                new Vector2f(UiTheme.ScrollBarWidth, viewport.Size.Y)));

        painter.DrawPart(
            UiPart.ScrollThumb,
            PartState.Normal,
            new FloatRect(
                new Vector2f(viewport.Position.X + viewport.Size.X - UiTheme.ScrollBarWidth, viewport.Position.Y + (travel * progress)),
                new Vector2f(UiTheme.ScrollBarWidth, thumbHeight)));
    }

    private void Clamp(float viewportHeight, float contentHeight)
    {
        float max = MathF.Max(0f, contentHeight - viewportHeight);
        Offset = Math.Clamp(Offset, 0f, max);
    }
}
