using LocalFileSorter.Ui.Input;
using LocalFileSorter.Ui.Rendering;
using LocalFileSorter.Ui.Theme;

using SFML.Graphics;

namespace LocalFileSorter.Ui.Widgets;

public static class Button
{
    public static bool Draw(Painter painter, UiContext input, FloatRect area, string label, bool enabled)
    {
        bool hovering = enabled && input.IsHovering(area);
        Color background = enabled
            ? hovering ? UiTheme.ButtonHover : UiTheme.ButtonBackground
            : UiTheme.ButtonDisabled;

        painter.FillRect(area, background);
        painter.StrokeRect(area, UiTheme.PanelBorder);
        painter.DrawTextCentered(
            label,
            area,
            UiTheme.BodyTextSize,
            enabled ? UiTheme.TextPrimary : UiTheme.TextDisabled);

        return enabled && input.ClickedIn(area);
    }
}
