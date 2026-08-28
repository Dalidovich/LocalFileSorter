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

public sealed class MappingOverlay
{
    private readonly Strings strings;
    private readonly MappingService mapping;

    private MappingStage stage = MappingStage.Idle;
    private Bucket? target;
    private ReloadNotice? notice;

    public MappingOverlay(Strings strings, MappingService mapping)
    {
        this.strings = strings;
        this.mapping = mapping;
    }

    public bool IsBlocking => stage != MappingStage.Idle;

    public void RequestRecolor(Bucket bucket)
    {
        if (stage == MappingStage.Idle)
        {
            target = bucket;
            stage = MappingStage.Picker;
        }
    }

    public void RequestReload()
    {
        if (stage != MappingStage.Idle)
        {
            return;
        }

        notice = mapping.Reload();
        stage = MappingStage.Notice;
    }

    public void Draw(Painter painter, UiContext input, FloatRect surface)
    {
        switch (stage)
        {
            case MappingStage.Picker:
                DrawPicker(painter, input, surface);
                break;
            case MappingStage.Notice:
                DrawNotice(painter, input, surface);
                break;
        }
    }

    private void DrawPicker(Painter painter, UiContext input, FloatRect surface)
    {
        Bucket bucket = target!;
        float contentHeight = UiTheme.ModalRowHeight + UiTheme.ModalPadding + ColorPickerGrid.Height;

        ModalLayout layout = ModalDialog.Draw(painter, surface, contentHeight, strings.BucketsRecolorTitle);

        TextMetrics metrics = painter.Metrics(mono: false, UiTheme.BodyTextSize);
        painter.DrawText(
            metrics.Truncate(string.Format(strings.BucketsRecolorSummary, bucket.Name), layout.Content.Size.X),
            layout.Content.Position,
            UiTheme.BodyTextSize,
            UiTheme.TextPrimary);

        FloatRect grid = new(
            new Vector2f(
                layout.Content.Position.X,
                layout.Content.Position.Y + UiTheme.ModalRowHeight + UiTheme.ModalPadding),
            new Vector2f(layout.Content.Size.X, ColorPickerGrid.Height));

        if (ColorPickerGrid.Draw(painter, input, grid, bucket.Color) is RgbColor picked)
        {
            mapping.Recolor(bucket, picked);
            Close();
        }

        if (Button.Draw(painter, input, ModalDialog.ButtonFromRight(layout.Buttons, 0), strings.CommonCancel, true))
        {
            Close();
        }
    }

    private void DrawNotice(Painter painter, UiContext input, FloatRect surface)
    {
        List<string> lines = BuildNoticeLines(notice!);
        ModalLayout layout = ModalDialog.Draw(
            painter,
            surface,
            lines.Count * UiTheme.ModalRowHeight,
            strings.MappingNoticeTitle);

        TextMetrics metrics = painter.Metrics(mono: false, UiTheme.BodyTextSize);
        float y = layout.Content.Position.Y;

        foreach (string line in lines)
        {
            painter.DrawText(
                metrics.Truncate(line, layout.Content.Size.X),
                new Vector2f(layout.Content.Position.X, y),
                UiTheme.BodyTextSize,
                UiTheme.TextPrimary);

            y += UiTheme.ModalRowHeight;
        }

        if (Button.Draw(painter, input, ModalDialog.ButtonFromRight(layout.Buttons, 0), strings.CommonClose, true))
        {
            Close();
        }
    }

    private List<string> BuildNoticeLines(ReloadNotice result)
    {
        if (result.IsUnchanged)
        {
            return [strings.MappingNoticeUnchanged];
        }

        List<string> lines = [];

        if (result.Added.Count > 0)
        {
            lines.Add(string.Format(strings.MappingNoticeAdded, string.Join(", ", result.Added)));
        }

        if (result.Removed.Count > 0)
        {
            lines.Add(string.Format(strings.MappingNoticeRemoved, string.Join(", ", result.Removed)));
        }

        if (result.ReleasedAssignments > 0)
        {
            lines.Add(string.Format(strings.MappingNoticeReleased, result.ReleasedAssignments));
        }

        if (result.CommittedInRemoved > 0)
        {
            lines.Add(string.Format(strings.MappingNoticeCommitted, result.CommittedInRemoved));
        }

        return lines;
    }

    private void Close()
    {
        stage = MappingStage.Idle;
        target = null;
        notice = null;
    }
}
