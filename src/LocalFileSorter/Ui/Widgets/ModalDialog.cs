using LocalFileSorter.Ui.Rendering;
using LocalFileSorter.Ui.Theme;

using SFML.Graphics;
using SFML.System;

namespace LocalFileSorter.Ui.Widgets;

public readonly record struct ModalLayout(FloatRect Box, FloatRect Content, FloatRect Buttons);

public static class ModalDialog
{
    public static ModalLayout Draw(Painter painter, FloatRect surface, float contentHeight, string title)
    {
        painter.DrawPart(UiPart.Scrim, PartState.Normal, surface);

        float height = UiTheme.ModalHeaderHeight
            + (UiTheme.ModalPadding * 3f)
            + contentHeight
            + UiTheme.ButtonHeight;

        float width = MathF.Min(UiTheme.ModalWidth, MathF.Max(0f, surface.Size.X - (UiTheme.ModalPadding * 2f)));
        height = MathF.Min(height, MathF.Max(0f, surface.Size.Y - (UiTheme.ModalPadding * 2f)));

        FloatRect box = new(
            new Vector2f(
                surface.Position.X + ((surface.Size.X - width) / 2f),
                surface.Position.Y + ((surface.Size.Y - height) / 2f)),
            new Vector2f(width, height));

        painter.DrawPart(UiPart.Modal, PartState.Normal, box);

        FloatRect header = new(box.Position, new Vector2f(box.Size.X, UiTheme.ModalHeaderHeight));
        painter.DrawPart(UiPart.ModalHeader, PartState.Normal, header);
        painter.DrawText(
            title,
            new Vector2f(header.Position.X + UiTheme.ModalPadding, header.Position.Y + 8f),
            UiTheme.HeaderTextSize,
            UiTheme.Foreground(UiPart.ModalHeader));

        FloatRect buttons = new(
            new Vector2f(
                box.Position.X + UiTheme.ModalPadding,
                box.Position.Y + box.Size.Y - UiTheme.ModalPadding - UiTheme.ButtonHeight),
            new Vector2f(MathF.Max(0f, box.Size.X - (UiTheme.ModalPadding * 2f)), UiTheme.ButtonHeight));

        float contentTop = header.Position.Y + header.Size.Y + UiTheme.ModalPadding;
        FloatRect content = new(
            new Vector2f(box.Position.X + UiTheme.ModalPadding, contentTop),
            new Vector2f(
                MathF.Max(0f, box.Size.X - (UiTheme.ModalPadding * 2f)),
                MathF.Max(0f, buttons.Position.Y - UiTheme.ModalPadding - contentTop)));

        painter.DrawPartFrame(UiPart.Modal, PartState.Normal, box);

        return new ModalLayout(box, content, buttons);
    }

    public static FloatRect ButtonFromRight(FloatRect row, int index) =>
        new(
            new Vector2f(
                row.Position.X + row.Size.X - ((index + 1) * UiTheme.ModalButtonWidth) - (index * UiTheme.ModalButtonGap),
                row.Position.Y),
            new Vector2f(UiTheme.ModalButtonWidth, row.Size.Y));
}
