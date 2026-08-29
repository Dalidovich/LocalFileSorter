using LocalFileSorter.Ui.Input;
using LocalFileSorter.Ui.Rendering;
using LocalFileSorter.Ui.Theme;

using SFML.Graphics;
using SFML.System;

namespace LocalFileSorter.Ui.Panels;

public abstract class Panel
{
    protected Panel(string title)
    {
        Title = title;
    }

    public string Title { get; }

    public void Draw(Painter painter, UiContext input, FloatRect area)
    {
        painter.DrawPart(UiPart.Panel, PartState.Normal, area);

        FloatRect header = new(area.Position, new Vector2f(area.Size.X, UiTheme.PanelHeaderHeight));
        painter.DrawPart(UiPart.PanelHeader, PartState.Normal, header);
        painter.DrawText(
            Title,
            new Vector2f(header.Position.X + UiTheme.PanelPadding, header.Position.Y + 8f),
            UiTheme.HeaderTextSize,
            UiTheme.Foreground(UiPart.PanelHeader));

        FloatRect body = new(
            new Vector2f(area.Position.X, area.Position.Y + UiTheme.PanelHeaderHeight),
            new Vector2f(area.Size.X, MathF.Max(0f, area.Size.Y - UiTheme.PanelHeaderHeight)));

        DrawBody(painter, input, body);

        painter.DrawPartFrame(UiPart.Panel, PartState.Normal, area);
    }

    protected abstract void DrawBody(Painter painter, UiContext input, FloatRect body);
}
