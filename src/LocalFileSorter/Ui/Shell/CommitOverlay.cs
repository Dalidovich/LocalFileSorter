using LocalFileSorter.Common.Localization;
using LocalFileSorter.Common.Model;
using LocalFileSorter.Common.Services;
using LocalFileSorter.Ui.Input;
using LocalFileSorter.Ui.Rendering;
using LocalFileSorter.Ui.Theme;
using LocalFileSorter.Ui.Widgets;

using SFML.Graphics;
using SFML.System;

namespace LocalFileSorter.Ui.Shell;

public sealed class CommitOverlay
{
    private const int MaxConfirmRows = 8;
    private const float FailureListHeight = 132f;

    private readonly Strings strings;
    private readonly SortSession session;
    private readonly SortPlanService plan;
    private readonly CommitRunner runner;
    private readonly TooltipHost tooltips;
    private readonly ScrollState failureScroll = new();

    private CommitStage stage = CommitStage.Idle;

    public CommitOverlay(
        Strings strings,
        SortSession session,
        SortPlanService plan,
        CommitRunner runner,
        TooltipHost tooltips)
    {
        this.strings = strings;
        this.session = session;
        this.plan = plan;
        this.runner = runner;
        this.tooltips = tooltips;
    }

    public bool IsBlocking => stage != CommitStage.Idle;

    public void Request()
    {
        if (stage == CommitStage.Idle && plan.TotalAssigned > 0)
        {
            stage = CommitStage.Confirm;
        }
    }

    public void Update()
    {
        runner.Update();

        if (stage == CommitStage.Running && runner.Report is not null)
        {
            stage = CommitStage.Report;
            failureScroll.Reset();
        }
    }

    public void Draw(Painter painter, UiContext input, FloatRect surface)
    {
        switch (stage)
        {
            case CommitStage.Confirm:
                DrawConfirm(painter, input, surface);
                break;
            case CommitStage.Running:
                DrawProgress(painter, input, surface);
                break;
            case CommitStage.Report:
                DrawReport(painter, input, surface);
                break;
        }
    }

    private static float TextRow(Painter painter, string value, FloatRect area, float y, uint size, Color color)
    {
        TextMetrics metrics = painter.Metrics(mono: false, size);
        painter.DrawText(metrics.Truncate(value, area.Size.X), new Vector2f(area.Position.X, y), size, color);
        return y + UiTheme.ModalRowHeight;
    }

    private IReadOnlyList<Bucket> AssignedBuckets() =>
        [.. session.Buckets.Where(bucket => plan.AssignedCount(bucket.Id) > 0)];

    private void DrawConfirm(Painter painter, UiContext input, FloatRect surface)
    {
        IReadOnlyList<Bucket> buckets = AssignedBuckets();
        int shown = Math.Min(buckets.Count, MaxConfirmRows);
        bool truncated = buckets.Count > shown;
        float contentHeight = (shown + (truncated ? 2 : 1)) * UiTheme.ModalRowHeight;

        ModalLayout layout = ModalDialog.Draw(painter, surface, contentHeight, strings.CommitConfirmTitle);

        float y = TextRow(
            painter,
            string.Format(strings.CommitConfirmSummary, plan.TotalAssigned),
            layout.Content,
            layout.Content.Position.Y,
            UiTheme.BodyTextSize,
            UiTheme.TextPrimary);

        for (int index = 0; index < shown; index++)
        {
            DrawBucketRow(painter, layout.Content, y, buckets[index]);
            y += UiTheme.ModalRowHeight;
        }

        if (truncated)
        {
            TextRow(
                painter,
                string.Format(strings.CommitConfirmMore, buckets.Count - shown),
                layout.Content,
                y,
                UiTheme.SmallTextSize,
                UiTheme.TextMuted);
        }

        if (Button.Draw(painter, input, ModalDialog.ButtonFromRight(layout.Buttons, 0), strings.CommitConfirmStart, true))
        {
            runner.Start(plan.BuildCommitTasks());
            stage = runner.IsRunning ? CommitStage.Running : CommitStage.Idle;
        }

        if (Button.Draw(painter, input, ModalDialog.ButtonFromRight(layout.Buttons, 1), strings.CommonCancel, true))
        {
            stage = CommitStage.Idle;
        }
    }

    private void DrawBucketRow(Painter painter, FloatRect content, float y, Bucket bucket)
    {
        FloatRect chip = new(
            new Vector2f(content.Position.X, y + ((UiTheme.ModalRowHeight - UiTheme.BucketChipSize) / 2f)),
            new Vector2f(UiTheme.BucketChipSize, UiTheme.BucketChipSize));

        painter.FillRect(chip, bucket.Color.ToSfml());
        painter.StrokeRect(chip, UiTheme.PanelBorder);

        float left = chip.Position.X + UiTheme.BucketChipSize + 10f;
        TextMetrics metrics = painter.Metrics(mono: false, UiTheme.BodyTextSize);
        string row = string.Format(strings.CommitConfirmRow, bucket.Name, plan.AssignedCount(bucket.Id));

        painter.DrawText(
            metrics.Truncate(row, MathF.Max(0f, content.Position.X + content.Size.X - left)),
            new Vector2f(left, y + ((UiTheme.ModalRowHeight - metrics.LineHeight) / 2f)),
            UiTheme.BodyTextSize,
            UiTheme.TextPrimary);
    }

    private void DrawProgress(Painter painter, UiContext input, FloatRect surface)
    {
        float contentHeight = (UiTheme.ModalRowHeight * 2f) + UiTheme.ProgressBarHeight + UiTheme.ModalPadding;
        ModalLayout layout = ModalDialog.Draw(painter, surface, contentHeight, strings.CommitProgressTitle);

        float y = TextRow(
            painter,
            string.Format(strings.CommitProgressCount, runner.Completed, runner.Total),
            layout.Content,
            layout.Content.Position.Y,
            UiTheme.BodyTextSize,
            UiTheme.TextPrimary);

        y = TextRow(painter, runner.CurrentName, layout.Content, y, UiTheme.SmallTextSize, UiTheme.TextMuted);

        FloatRect track = new(
            new Vector2f(layout.Content.Position.X, y + UiTheme.ModalPadding),
            new Vector2f(layout.Content.Size.X, UiTheme.ProgressBarHeight));

        painter.FillRect(track, UiTheme.ProgressTrack);

        float progress = runner.Total == 0 ? 0f : (float)runner.Completed / runner.Total;
        painter.FillRect(
            new FloatRect(track.Position, new Vector2f(track.Size.X * progress, track.Size.Y)),
            UiTheme.ProgressFill);

        painter.StrokeRect(track, UiTheme.PanelBorder);

        if (Button.Draw(
                painter,
                input,
                ModalDialog.ButtonFromRight(layout.Buttons, 0),
                strings.CommonCancel,
                !runner.CancelRequested))
        {
            runner.Cancel();
        }
    }

    private void DrawReport(Painter painter, UiContext input, FloatRect surface)
    {
        MoveReport report = runner.Report!;
        List<string> lines = BuildReportLines(report);

        bool hasFailures = report.Failures.Count > 0;
        float contentHeight = (lines.Count * UiTheme.ModalRowHeight)
            + (hasFailures ? FailureListHeight + UiTheme.ModalPadding : 0f);

        ModalLayout layout = ModalDialog.Draw(painter, surface, contentHeight, strings.CommitReportTitle);

        float y = layout.Content.Position.Y;
        foreach (string line in lines)
        {
            y = TextRow(painter, line, layout.Content, y, UiTheme.BodyTextSize, UiTheme.TextPrimary);
        }

        if (hasFailures)
        {
            FloatRect list = new(
                new Vector2f(layout.Content.Position.X, y + UiTheme.ModalPadding),
                new Vector2f(
                    layout.Content.Size.X,
                    MathF.Max(0f, layout.Content.Position.Y + layout.Content.Size.Y - y - UiTheme.ModalPadding)));

            DrawFailures(painter, input, list, report.Failures);
        }

        if (Button.Draw(painter, input, ModalDialog.ButtonFromRight(layout.Buttons, 0), strings.CommonClose, true))
        {
            runner.Clear();
            stage = CommitStage.Idle;
        }
    }

    private List<string> BuildReportLines(MoveReport report)
    {
        List<string> lines = [string.Format(strings.CommitReportMoved, report.Moved)];

        if (report.Renamed > 0)
        {
            lines.Add(string.Format(strings.CommitReportRenamed, report.Renamed));
        }

        if (report.Failures.Count > 0)
        {
            lines.Add(string.Format(strings.CommitReportFailed, report.Failures.Count));
        }

        if (report.Skipped > 0)
        {
            lines.Add(string.Format(strings.CommitReportSkipped, report.Skipped));
        }

        if (report.Cancelled)
        {
            lines.Add(strings.CommitReportCancelled);
        }

        if (report.Failures.Count == 0 && !report.Cancelled)
        {
            lines.Add(strings.CommitReportClean);
        }

        return lines;
    }

    private void DrawFailures(Painter painter, UiContext input, FloatRect list, IReadOnlyList<MoveFailure> failures)
    {
        painter.FillRect(list, UiTheme.ViewportBackground);

        float contentHeight = failures.Count * UiTheme.ModalRowHeight;
        failureScroll.Apply(input, list, contentHeight);

        TextMetrics metrics = painter.Metrics(mono: false, UiTheme.SmallTextSize);
        float width = MathF.Max(0f, list.Size.X - (UiTheme.TooltipPadding * 2f) - UiTheme.ScrollBarWidth);
        bool insideList = input.IsHovering(list);

        painter.PushClip(list);

        for (int index = 0; index < failures.Count; index++)
        {
            float top = list.Position.Y - failureScroll.Offset + (index * UiTheme.ModalRowHeight);
            if (top + UiTheme.ModalRowHeight < list.Position.Y || top > list.Position.Y + list.Size.Y)
            {
                continue;
            }

            MoveFailure failure = failures[index];
            string value = string.Format(strings.CommitFailureRow, failure.Name, failure.Reason);
            string shown = metrics.Truncate(value, width);

            painter.DrawText(
                shown,
                new Vector2f(
                    list.Position.X + UiTheme.TooltipPadding,
                    top + ((UiTheme.ModalRowHeight - metrics.LineHeight) / 2f)),
                UiTheme.SmallTextSize,
                UiTheme.MessageError);

            FloatRect row = new(new Vector2f(list.Position.X, top), new Vector2f(list.Size.X, UiTheme.ModalRowHeight));

            if (insideList && shown != value && input.IsHovering(row))
            {
                tooltips.Show(value);
            }
        }

        painter.PopClip();
        failureScroll.DrawBar(painter, list, contentHeight);
        painter.StrokeRect(list, UiTheme.PanelBorder);
    }
}
