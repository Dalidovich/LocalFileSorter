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

public sealed class BucketsPanel : Panel
{
    private readonly Strings strings;
    private readonly SortSession session;
    private readonly SortPlanService plan;
    private readonly TooltipHost tooltips;
    private readonly BucketActions actions;
    private readonly ScrollState scroll = new();

    public BucketsPanel(
        Strings strings,
        SortSession session,
        SortPlanService plan,
        TooltipHost tooltips,
        BucketActions actions)
        : base(strings.PanelBuckets)
    {
        this.strings = strings;
        this.session = session;
        this.plan = plan;
        this.tooltips = tooltips;
        this.actions = actions;
    }

    protected override void DrawBody(Painter painter, UiContext input, FloatRect body)
    {
        float listHeight = MathF.Max(0f, body.Size.Y - UiTheme.BucketsFooterHeight);
        FloatRect list = new(body.Position, new Vector2f(body.Size.X, listHeight));

        DrawList(painter, input, list);

        DrawFooter(
            painter,
            input,
            new FloatRect(
                new Vector2f(body.Position.X, body.Position.Y + listHeight),
                new Vector2f(body.Size.X, UiTheme.BucketsFooterHeight)));
    }

    private void DrawList(Painter painter, UiContext input, FloatRect list)
    {
        if (session.Buckets.Count == 0)
        {
            DrawEmptyState(painter, list);
            return;
        }

        float contentHeight = session.Buckets.Count * UiTheme.BucketRowHeight;
        scroll.Apply(input, list, contentHeight);

        bool insideList = input.IsHovering(list);
        int first = Math.Max(0, (int)(scroll.Offset / UiTheme.BucketRowHeight));
        int last = Math.Min(session.Buckets.Count - 1, (int)((scroll.Offset + list.Size.Y) / UiTheme.BucketRowHeight));

        painter.PushClip(list);

        for (int index = first; index <= last; index++)
        {
            FloatRect row = new(
                new Vector2f(list.Position.X, list.Position.Y - scroll.Offset + (index * UiTheme.BucketRowHeight)),
                new Vector2f(list.Size.X, UiTheme.BucketRowHeight));

            DrawRow(painter, input, row, session.Buckets[index], insideList);
        }

        painter.PopClip();
        scroll.DrawBar(painter, list, contentHeight);
    }

    private void DrawEmptyState(Painter painter, FloatRect list)
    {
        FloatRect area = new(
            new Vector2f(list.Position.X + UiTheme.PanelPadding, list.Position.Y),
            new Vector2f(MathF.Max(0f, list.Size.X - (UiTheme.PanelPadding * 2f)), list.Size.Y));

        float headlineHeight = painter.Metrics(mono: false, UiTheme.BodyTextSize).LineHeight;
        float instructionHeight = painter.MeasureWrappedHeight(
            strings.BucketsEmptyInstruction,
            area.Size.X,
            UiTheme.SmallTextSize);

        float total = headlineHeight + UiTheme.PanelPadding + instructionHeight;
        float top = area.Position.Y + MathF.Max(0f, (area.Size.Y - total) / 2f);

        top = painter.DrawWrappedCentered(strings.BucketsEmpty, area, top, UiTheme.BodyTextSize, UiTheme.TextPrimary);
        painter.DrawWrappedCentered(
            strings.BucketsEmptyInstruction,
            area,
            top + UiTheme.PanelPadding,
            UiTheme.SmallTextSize,
            UiTheme.TextMuted);
    }

    private void DrawFooter(Painter painter, UiContext input, FloatRect footer)
    {
        painter.FillRect(footer, UiTheme.PanelHeaderBackground);
        painter.FillRect(new FloatRect(footer.Position, new Vector2f(footer.Size.X, 1f)), UiTheme.Separator);

        float stackHeight = (UiTheme.FooterButtonHeight * 3f) + (UiTheme.FooterButtonGap * 2f);
        float top = footer.Position.Y + ((footer.Size.Y - stackHeight) / 2f);

        if (Button.Draw(painter, input, FooterRow(footer, top, 0), strings.BucketsReloadMapping, true))
        {
            actions.Reload();
        }

        bool canSort = plan.TotalAssigned > 0;
        string sortLabel = canSort
            ? string.Format(strings.BucketsSortPending, plan.TotalAssigned)
            : strings.BucketsSort;

        if (Button.Draw(painter, input, FooterRow(footer, top, 1), sortLabel, canSort))
        {
            actions.Sort();
        }

        if (Button.Draw(painter, input, FooterRow(footer, top, 2), strings.BucketsUndo, plan.CanUndo))
        {
            actions.Undo();
        }
    }

    private static FloatRect FooterRow(FloatRect footer, float top, int index) => new(
        new Vector2f(
            footer.Position.X + UiTheme.PanelPadding,
            top + (index * (UiTheme.FooterButtonHeight + UiTheme.FooterButtonGap))),
        new Vector2f(
            MathF.Max(0f, footer.Size.X - (UiTheme.PanelPadding * 2f)),
            UiTheme.FooterButtonHeight));

    private void DrawRow(Painter painter, UiContext input, FloatRect row, Bucket bucket, bool insideList)
    {
        bool hovering = insideList && input.IsHovering(row);
        if (hovering)
        {
            painter.FillRect(row, UiTheme.RowHover);
        }

        FloatRect chip = new(
            new Vector2f(
                row.Position.X + UiTheme.PanelPadding,
                row.Position.Y + ((row.Size.Y - UiTheme.BucketChipSize) / 2f)),
            new Vector2f(UiTheme.BucketChipSize, UiTheme.BucketChipSize));

        bool recolor = ColorSwatch.Draw(painter, input, chip, bucket.Color.ToSfml(), selected: false, enabled: true);
        if (insideList && recolor)
        {
            actions.Recolor(bucket);
        }

        string count = string.Format(strings.BucketsCount, plan.AssignedCount(bucket.Id), bucket.ExistingFileCount);

        TextMetrics nameMetrics = painter.Metrics(mono: false, UiTheme.BodyTextSize);
        TextMetrics countMetrics = painter.Metrics(mono: false, UiTheme.SmallTextSize);

        float countWidth = countMetrics.Measure(count);
        float nameLeft = chip.Position.X + UiTheme.BucketChipSize + 10f;
        float nameWidth = MathF.Max(0f, row.Position.X + row.Size.X - UiTheme.PanelPadding - countWidth - 10f - nameLeft);
        string name = nameMetrics.Truncate(bucket.Name, nameWidth);

        painter.DrawText(
            name,
            new Vector2f(nameLeft, row.Position.Y + ((row.Size.Y - nameMetrics.LineHeight) / 2f)),
            UiTheme.BodyTextSize,
            UiTheme.TextPrimary);

        painter.DrawText(
            count,
            new Vector2f(
                row.Position.X + row.Size.X - UiTheme.PanelPadding - countWidth,
                row.Position.Y + ((row.Size.Y - countMetrics.LineHeight) / 2f)),
            UiTheme.SmallTextSize,
            UiTheme.TextMuted);

        if (hovering && name != bucket.Name)
        {
            tooltips.Show(bucket.Name);
        }
    }
}
