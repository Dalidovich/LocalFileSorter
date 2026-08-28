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

public sealed class QueuePanel : Panel
{
    private const float MarkerColumnWidth = 16f;
    private const float MarkerRadius = 4f;
    private const float AssignedTint = 0.16f;
    private const float AssignedTintHover = 0.24f;
    private const float AssignedTintActive = 0.32f;
    private const float StrikeThickness = 1f;
    private const float MovedMarkerTint = 0.35f;

    private readonly Strings strings;
    private readonly SortSession session;
    private readonly SortPlanService plan;
    private readonly TooltipHost tooltips;
    private readonly ScrollState scroll = new();

    private int lastActiveIndex = -1;

    public QueuePanel(Strings strings, SortSession session, SortPlanService plan, TooltipHost tooltips)
        : base(strings.PanelQueue)
    {
        this.strings = strings;
        this.session = session;
        this.plan = plan;
        this.tooltips = tooltips;
    }

    protected override void DrawBody(Painter painter, UiContext input, FloatRect body)
    {
        string footerText = BuildFooterText();
        bool hasFooter = footerText.Length > 0;
        float listHeight = MathF.Max(0f, body.Size.Y - (hasFooter ? UiTheme.QueueFooterHeight : 0f));
        FloatRect list = new(body.Position, new Vector2f(body.Size.X, listHeight));

        if (session.Files.Count == 0)
        {
            painter.DrawTextCentered(strings.QueueEmpty, list, UiTheme.BodyTextSize, UiTheme.TextMuted);
        }
        else
        {
            DrawRows(painter, input, list);
        }

        if (hasFooter)
        {
            DrawFooter(
                painter,
                input,
                new FloatRect(
                    new Vector2f(body.Position.X, body.Position.Y + listHeight),
                    new Vector2f(body.Size.X, UiTheme.QueueFooterHeight)),
                footerText);
        }
    }

    private void DrawRows(Painter painter, UiContext input, FloatRect list)
    {
        float contentHeight = session.Files.Count * UiTheme.QueueRowHeight;
        scroll.Apply(input, list, contentHeight);

        if (session.ActiveIndex != lastActiveIndex)
        {
            scroll.EnsureVisible(
                session.ActiveIndex * UiTheme.QueueRowHeight,
                UiTheme.QueueRowHeight,
                list.Size.Y,
                contentHeight);
            lastActiveIndex = session.ActiveIndex;
        }

        bool insideList = input.IsHovering(list);
        int first = Math.Max(0, (int)(scroll.Offset / UiTheme.QueueRowHeight));
        int last = Math.Min(session.Files.Count - 1, (int)((scroll.Offset + list.Size.Y) / UiTheme.QueueRowHeight));

        painter.PushClip(list);

        for (int index = first; index <= last; index++)
        {
            FloatRect row = new(
                new Vector2f(list.Position.X, list.Position.Y - scroll.Offset + (index * UiTheme.QueueRowHeight)),
                new Vector2f(list.Size.X, UiTheme.QueueRowHeight));

            DrawRow(painter, input, row, index, insideList);

            if (insideList && input.ClickedIn(row))
            {
                session.Activate(index);
            }
        }

        painter.PopClip();
        scroll.DrawBar(painter, list, contentHeight);
    }

    private void DrawRow(Painter painter, UiContext input, FloatRect row, int index, bool insideList)
    {
        FileEntry entry = session.Files[index];
        bool active = index == session.ActiveIndex;
        bool hovering = insideList && input.IsHovering(row);
        Bucket? bucket = entry.AssignedBucket is BucketId id ? session.FindBucket(id) : null;

        bool moved = entry.State == FileState.Moved;
        DrawRowBackground(painter, row, moved ? null : bucket, active, hovering);

        if (bucket is not null)
        {
            painter.FillCircle(
                new Vector2f(
                    row.Position.X + UiTheme.PanelPadding + (MarkerColumnWidth / 2f),
                    row.Position.Y + (row.Size.Y / 2f)),
                MarkerRadius,
                moved
                    ? ColorMap.Mix(bucket.Color.ToSfml(), UiTheme.PanelBackground, MovedMarkerTint)
                    : bucket.Color.ToSfml());
        }

        float textLeft = row.Position.X + UiTheme.PanelPadding + MarkerColumnWidth;
        string size = FileSizeFormatter.Format(entry.SizeBytes, strings);

        TextMetrics nameMetrics = painter.Metrics(mono: false, UiTheme.BodyTextSize);
        TextMetrics sizeMetrics = painter.Metrics(mono: false, UiTheme.SmallTextSize);

        float sizeWidth = sizeMetrics.Measure(size);
        float nameWidth = MathF.Max(0f, row.Position.X + row.Size.X - UiTheme.PanelPadding - sizeWidth - 10f - textLeft);
        string name = nameMetrics.Truncate(entry.Name, nameWidth);
        float nameTop = row.Position.Y + ((row.Size.Y - nameMetrics.LineHeight) / 2f);

        painter.DrawText(
            name,
            new Vector2f(textLeft, nameTop),
            UiTheme.BodyTextSize,
            NameColor(entry.State));

        if (entry.State == FileState.Failed)
        {
            painter.FillRect(
                new FloatRect(
                    new Vector2f(textLeft, nameTop + (nameMetrics.LineHeight / 2f)),
                    new Vector2f(nameMetrics.Measure(name), StrikeThickness)),
                UiTheme.MessageError);
        }

        painter.DrawText(
            size,
            new Vector2f(
                row.Position.X + row.Size.X - UiTheme.PanelPadding - sizeWidth,
                row.Position.Y + ((row.Size.Y - sizeMetrics.LineHeight) / 2f)),
            UiTheme.SmallTextSize,
            UiTheme.TextMuted);

        if (active)
        {
            painter.StrokeRect(row, UiTheme.RowActiveBorder);
        }

        if (hovering)
        {
            ShowRowTooltip(entry, name);
        }
    }

    private static Color NameColor(FileState state) => state switch
    {
        FileState.Moved => UiTheme.TextDisabled,
        FileState.Failed => UiTheme.MessageError,
        _ => UiTheme.TextPrimary,
    };

    private void ShowRowTooltip(FileEntry entry, string shownName)
    {
        if (entry.FailureReason is string reason)
        {
            tooltips.Show(string.Format(strings.CommitFailureRow, entry.Name, reason));
        }
        else if (shownName != entry.Name)
        {
            tooltips.Show(entry.Name);
        }
    }

    private static void DrawRowBackground(Painter painter, FloatRect row, Bucket? bucket, bool active, bool hovering)
    {
        if (bucket is not null)
        {
            painter.FillRect(row, ColorMap.Mix(
                bucket.Color.ToSfml(),
                UiTheme.PanelBackground,
                active ? AssignedTintActive : hovering ? AssignedTintHover : AssignedTint));
        }
        else if (active)
        {
            painter.FillRect(row, UiTheme.RowActive);
        }
        else if (hovering)
        {
            painter.FillRect(row, UiTheme.RowHover);
        }
    }

    private string BuildFooterText()
    {
        List<string> parts = [];

        if (plan.QueueComplete)
        {
            parts.Add(strings.QueueComplete);
        }

        if (session.SkippedUnsupportedCount > 0)
        {
            parts.Add(string.Format(strings.QueueHidden, session.SkippedUnsupportedCount));
        }

        return string.Join("  ·  ", parts);
    }

    private void DrawFooter(Painter painter, UiContext input, FloatRect footer, string text)
    {
        painter.FillRect(footer, UiTheme.PanelHeaderBackground);
        painter.DrawText(
            text,
            new Vector2f(
                footer.Position.X + UiTheme.PanelPadding,
                footer.Position.Y + ((footer.Size.Y - painter.Metrics(mono: false, UiTheme.SmallTextSize).LineHeight) / 2f)),
            UiTheme.SmallTextSize,
            UiTheme.TextMuted);

        if (session.SkippedUnsupportedCount > 0 && input.IsHovering(footer))
        {
            tooltips.Show(string.Format(strings.QueueHiddenExtensions, DescribeSkippedExtensions()));
        }
    }

    private string DescribeSkippedExtensions() =>
        string.Join(", ", session.SkippedExtensions.Select(extension => FileMetadata.DescribeExtension(extension, strings)));
}
