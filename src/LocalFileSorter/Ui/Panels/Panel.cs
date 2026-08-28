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
        painter.FillRect(area, UiTheme.PanelBackground);

        FloatRect header = new(area.Position, new Vector2f(area.Size.X, UiTheme.PanelHeaderHeight));
        painter.FillRect(header, UiTheme.PanelHeaderBackground);
        painter.DrawText(
            Title,
            new Vector2f(header.Position.X + UiTheme.PanelPadding, header.Position.Y + 8f),
            UiTheme.HeaderTextSize,
            UiTheme.TextPrimary);

        FloatRect body = new(
            new Vector2f(area.Position.X, area.Position.Y + UiTheme.PanelHeaderHeight),
            new Vector2f(area.Size.X, MathF.Max(0f, area.Size.Y - UiTheme.PanelHeaderHeight)));

        DrawBody(painter, input, body);

        painter.StrokeRect(area, UiTheme.PanelBorder);
    }

    protected abstract void DrawBody(Painter painter, UiContext input, FloatRect body);
}
