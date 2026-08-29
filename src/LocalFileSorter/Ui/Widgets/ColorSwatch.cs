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
            ? hovering ? ColorMap.Mix(UiTheme.SwatchHoverTint, color, UiTheme.SwatchHoverTintAmount) : color
            : ColorMap.Mix(color, UiTheme.Style(UiPart.Panel).Fill, UiTheme.SwatchDisabledTintAmount);

        painter.DrawPart(UiPart.Swatch, selected ? PartState.Active : PartState.Normal, area, fill);

        return enabled && input.ClickedIn(area);
    }
}
