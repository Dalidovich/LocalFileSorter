using LocalFileSorter.Ui.Input;
using LocalFileSorter.Ui.Rendering;
using LocalFileSorter.Ui.Theme;

using SFML.Graphics;

namespace LocalFileSorter.Ui.Widgets;

public static class ColorSwatch
{
    public static bool Draw(Painter painter, UiContext input, FloatRect area, Color color, bool selected, bool enabled)
    {
        bool hovering = enabled && input.IsHovering(area);
        Color fill = enabled
            ? hovering ? ColorMap.Mix(Color.White, color, 0.18f) : color
            : ColorMap.Mix(color, UiTheme.PanelBackground, 0.35f);

        painter.FillRect(area, fill);
        painter.StrokeRect(area, selected ? UiTheme.SwatchSelectedBorder : UiTheme.PanelBorder, selected ? 2f : 1f);

        return enabled && input.ClickedIn(area);
    }
}
