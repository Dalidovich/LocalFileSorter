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
        PartState state = enabled
            ? hovering ? PartState.Hover : PartState.Normal
            : PartState.Disabled;

        painter.DrawPart(UiPart.Button, state, area);
        painter.DrawTextCentered(label, area, UiTheme.BodyTextSize, UiTheme.Foreground(UiPart.Button, state));

        return enabled && input.ClickedIn(area);
    }
}
