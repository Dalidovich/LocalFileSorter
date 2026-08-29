using LocalFileSorter.Common.Localization;
using LocalFileSorter.Common.Model;
using LocalFileSorter.Common.Services;
using LocalFileSorter.Ui.Input;
using LocalFileSorter.Ui.Rendering;
using LocalFileSorter.Ui.Theme;
using LocalFileSorter.Ui.Widgets;

using SFML.Graphics;
using SFML.System;

namespace LocalFileSorter.Ui.Panels;

public sealed class PaletteStrip
{
    private static float CellHeight => UiTheme.SwatchHeight + UiTheme.SwatchLabelHeight + UiTheme.SwatchGap;

    private readonly Strings strings;
    private readonly SortSession session;
    private readonly SortPlanService plan;
    private readonly TooltipHost tooltips;
    private readonly ScrollState scroll = new();

    public PaletteStrip(Strings strings, SortSession session, SortPlanService plan, TooltipHost tooltips)
    {
        this.strings = strings;
        this.session = session;
        this.plan = plan;
        this.tooltips = tooltips;
    }

    public void Draw(Painter painter, UiContext input, FloatRect area)
    {
        painter.DrawPart(UiPart.PaletteStrip, PartState.Normal, area);

        if (session.Buckets.Count == 0)
        {
            painter.DrawTextCentered(strings.BucketsEmpty, area, UiTheme.BodyTextSize, UiTheme.TextMuted);
            return;
        }

        FloatRect content = new(
            new Vector2f(area.Position.X + UiTheme.PanelPadding, area.Position.Y + UiTheme.SwatchGap),
            new Vector2f(
                MathF.Max(0f, area.Size.X - (UiTheme.PanelPadding * 2f)),
                MathF.Max(0f, area.Size.Y - (UiTheme.SwatchGap * 2f))));

        int columns = Columns(content.Size.X);
        int rows = ((session.Buckets.Count - 1) / columns) + 1;
        float contentHeight = (rows * CellHeight) - UiTheme.SwatchGap;

        scroll.Apply(input, area, contentHeight);

        FileEntry? active = session.ActiveFile;
        bool insideStrip = input.IsHovering(content);
        bool enabled = insideStrip && active is not null && active.State != FileState.Moved;

        painter.PushClip(content);

        for (int index = 0; index < session.Buckets.Count; index++)
        {
            Bucket bucket = session.Buckets[index];
            FloatRect chip = new(
                new Vector2f(
                    content.Position.X + ((index % columns) * (UiTheme.SwatchWidth + UiTheme.SwatchGap)),
                    content.Position.Y - scroll.Offset + ((index / columns) * CellHeight)),
                new Vector2f(UiTheme.SwatchWidth, UiTheme.SwatchHeight));

            bool selected = active?.AssignedBucket == bucket.Id;

            if (ColorSwatch.Draw(painter, input, chip, bucket.Color.ToSfml(), selected, enabled))
            {
                plan.ToggleActive(bucket);
            }

            DrawLabel(painter, input, chip, bucket.Name, selected, insideStrip);
        }

        painter.PopClip();
        scroll.DrawBar(painter, area, contentHeight);
    }

    private static int Columns(float width) =>
        Math.Max(1, (int)((width + UiTheme.SwatchGap) / (UiTheme.SwatchWidth + UiTheme.SwatchGap)));

    private void DrawLabel(Painter painter, UiContext input, FloatRect chip, string name, bool selected, bool insideStrip)
    {
        TextMetrics metrics = painter.Metrics(mono: false, UiTheme.SmallTextSize);
        string shown = metrics.Truncate(name, chip.Size.X);

        FloatRect label = new(
            new Vector2f(chip.Position.X, chip.Position.Y + UiTheme.SwatchHeight),
            new Vector2f(chip.Size.X, UiTheme.SwatchLabelHeight));

        painter.DrawTextCentered(
            shown,
            label,
            UiTheme.SmallTextSize,
            selected ? UiTheme.TextPrimary : UiTheme.TextMuted);

        if (insideStrip && shown != name && (input.IsHovering(chip) || input.IsHovering(label)))
        {
            tooltips.Show(name);
        }
    }
}
