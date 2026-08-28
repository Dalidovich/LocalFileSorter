using LocalFileSorter.Common.Model;
using LocalFileSorter.Common.Services;
using LocalFileSorter.Ui.Input;
using LocalFileSorter.Ui.Rendering;
using LocalFileSorter.Ui.Theme;

using SFML.Graphics;
using SFML.System;

namespace LocalFileSorter.Ui.Widgets;

public static class ColorPickerGrid
{
    public const int Columns = 5;

    public static float Height =>
        (Rows * UiTheme.SwatchHeight) + ((Rows - 1) * UiTheme.SwatchGap);

    private static int Rows => ((PaletteAllocator.Palette.Count - 1) / Columns) + 1;

    public static RgbColor? Draw(Painter painter, UiContext input, FloatRect area, RgbColor selected)
    {
        float cellWidth = (area.Size.X - ((Columns - 1) * UiTheme.SwatchGap)) / Columns;
        RgbColor? picked = null;

        for (int index = 0; index < PaletteAllocator.Palette.Count; index++)
        {
            RgbColor color = PaletteAllocator.Palette[index];

            FloatRect cell = new(
                new Vector2f(
                    area.Position.X + ((index % Columns) * (cellWidth + UiTheme.SwatchGap)),
                    area.Position.Y + ((index / Columns) * (UiTheme.SwatchHeight + UiTheme.SwatchGap))),
                new Vector2f(cellWidth, UiTheme.SwatchHeight));

            if (ColorSwatch.Draw(painter, input, cell, color.ToSfml(), color == selected, enabled: true))
            {
                picked = color;
            }
        }

        return picked;
    }
}
