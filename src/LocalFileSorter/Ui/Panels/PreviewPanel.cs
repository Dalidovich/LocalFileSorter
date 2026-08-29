using LocalFileSorter.Common.Abstractions;
using LocalFileSorter.Common.Localization;
using LocalFileSorter.Common.Model;
using LocalFileSorter.Common.Services;
using LocalFileSorter.Previews;
using LocalFileSorter.Ui.Input;
using LocalFileSorter.Ui.Rendering;
using LocalFileSorter.Ui.Theme;
using LocalFileSorter.Ui.Widgets;

using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace LocalFileSorter.Ui.Panels;

public sealed class PreviewPanel : Panel, IDisposable
{
    private const int MetadataRows = 3;

    private readonly Strings strings;
    private readonly SortSession session;
    private readonly TooltipHost tooltips;
    private readonly PaletteStrip palette;
    private readonly ScrollState scroll = new();
    private readonly PreviewTextureCache textures = new();

    private PreviewSnapshot? snapshot;
    private PreviewLayout? layout;
    private PreviewDocument? laidOutDocument;
    private float laidOutWidth = -1f;
    private float laidOutHeight = -1f;
    private FileId? scrolledFile;

    public PreviewPanel(Strings strings, SortSession session, SortPlanService plan, TooltipHost tooltips)
        : base(strings.PanelPreview)
    {
        this.strings = strings;
        this.session = session;
        this.tooltips = tooltips;
        palette = new PaletteStrip(strings, session, plan, tooltips);
    }

    public void SetSnapshot(PreviewSnapshot? value) => snapshot = value;

    public void Dispose() => textures.Dispose();

    protected override void DrawBody(Painter painter, UiContext input, FloatRect body)
    {
        float metadataHeight = (MetadataRows * UiTheme.MetadataRowHeight) + (UiTheme.MetadataStripPadding * 2f);
        float viewportHeight = MathF.Max(
            0f,
            body.Size.Y - metadataHeight - UiTheme.NavigationRowHeight - UiTheme.PaletteStripHeight);

        FloatRect viewport = new(body.Position, new Vector2f(body.Size.X, viewportHeight));
        FloatRect metadata = new(
            new Vector2f(body.Position.X, body.Position.Y + viewportHeight),
            new Vector2f(body.Size.X, metadataHeight));
        FloatRect navigation = new(
            new Vector2f(body.Position.X, body.Position.Y + viewportHeight + metadataHeight),
            new Vector2f(body.Size.X, UiTheme.NavigationRowHeight));
        FloatRect paletteArea = new(
            new Vector2f(body.Position.X, navigation.Position.Y + UiTheme.NavigationRowHeight),
            new Vector2f(body.Size.X, MathF.Max(0f, body.Position.Y + body.Size.Y - navigation.Position.Y - UiTheme.NavigationRowHeight)));

        DrawViewport(painter, input, viewport);
        DrawMetadata(painter, input, metadata);
        DrawNavigation(painter, input, navigation);
        palette.Draw(painter, input, paletteArea);
    }

    private static string Join(IReadOnlyList<MetadataItem> items) =>
        string.Join("  ·  ", items.Select(item => item.Label + ": " + item.Value));

    private static FloatRect Inset(FloatRect area, float amount) =>
        new(
            new Vector2f(area.Position.X + amount, area.Position.Y + amount),
            new Vector2f(MathF.Max(0f, area.Size.X - (amount * 2f)), MathF.Max(0f, area.Size.Y - (amount * 2f))));

    private void DrawViewport(Painter painter, UiContext input, FloatRect viewport)
    {
        painter.DrawPart(UiPart.Viewport, PartState.Normal, viewport);

        FileEntry? active = session.ActiveFile;
        if (active is null)
        {
            painter.DrawTextCentered(strings.PreviewNoFile, viewport, UiTheme.BodyTextSize, UiTheme.TextMuted);
            return;
        }

        if (scrolledFile != active.Id)
        {
            scroll.Reset();
            scrolledFile = active.Id;
        }

        PreviewDocument? document = snapshot?.Result?.Document;
        if (snapshot is null || snapshot.IsLoading || document is null)
        {
            painter.DrawTextCentered(strings.PreviewLoading, viewport, UiTheme.BodyTextSize, UiTheme.TextMuted);
            return;
        }

        DrawDocument(painter, input, viewport, Inset(viewport, UiTheme.PanelPadding), document);
    }

    private void DrawDocument(Painter painter, UiContext input, FloatRect viewport, FloatRect content, PreviewDocument document)
    {
        if (!ReferenceEquals(laidOutDocument, document) || laidOutWidth != content.Size.X || laidOutHeight != content.Size.Y)
        {
            layout = PreviewLayout.Build(painter, document, content.Size.X, content.Size.Y);
            laidOutDocument = document;
            laidOutWidth = content.Size.X;
            laidOutHeight = content.Size.Y;
        }

        scroll.Apply(input, viewport, layout!.Height);

        painter.PushClip(content);

        foreach (PreviewLine line in layout.Lines)
        {
            float y = content.Position.Y - scroll.Offset + line.Top;
            if (y + line.Height < content.Position.Y)
            {
                continue;
            }

            if (y > content.Position.Y + content.Size.Y)
            {
                break;
            }

            painter.DrawText(line.Text, new Vector2f(content.Position.X, y), line.Size, line.Color, line.Mono);
        }

        foreach (PreviewImage image in layout.Images)
        {
            float y = content.Position.Y - scroll.Offset + image.Top;
            if (y + image.Height < content.Position.Y || y > content.Position.Y + content.Size.Y)
            {
                continue;
            }

            FloatRect target = new(
                new Vector2f(content.Position.X + image.Left, y),
                new Vector2f(image.Width, image.Height));

            if (textures.Resolve(image.Block) is Texture texture)
            {
                painter.DrawTexture(texture, target);
            }
            else
            {
                painter.DrawTextCentered(strings.ImageTooLarge, target, UiTheme.BodyTextSize, UiTheme.MessageError);
            }
        }

        painter.PopClip();
        scroll.DrawBar(painter, viewport, layout.Height);
    }

    private void DrawMetadata(Painter painter, UiContext input, FloatRect strip)
    {
        painter.DrawPart(UiPart.MetadataStrip, PartState.Normal, strip);

        FileEntry? active = session.ActiveFile;
        if (active is null)
        {
            return;
        }

        float width = MathF.Max(0f, strip.Size.X - (UiTheme.PanelPadding * 2f));
        float x = strip.Position.X + UiTheme.PanelPadding;
        float y = strip.Position.Y + UiTheme.MetadataStripPadding;

        DrawMetadataRow(painter, input, active.Name, x, y, width, UiTheme.BodyTextSize, UiTheme.TextPrimary);
        y += UiTheme.MetadataRowHeight;

        DrawMetadataRow(
            painter,
            input,
            Join(FileMetadata.Describe(active, strings)),
            x,
            y,
            width,
            UiTheme.SmallTextSize,
            UiTheme.TextMuted);
        y += UiTheme.MetadataRowHeight;

        IReadOnlyList<MetadataItem> extra = snapshot?.Result?.ExtraMetadata ?? [];
        if (extra.Count > 0)
        {
            DrawMetadataRow(painter, input, Join(extra), x, y, width, UiTheme.SmallTextSize, UiTheme.TextMuted);
        }
    }

    private void DrawMetadataRow(
        Painter painter,
        UiContext input,
        string value,
        float x,
        float y,
        float width,
        uint size,
        Color color)
    {
        TextMetrics metrics = painter.Metrics(mono: false, size);
        string shown = metrics.Truncate(value, width);
        painter.DrawText(shown, new Vector2f(x, y), size, color);

        FloatRect row = new(new Vector2f(x, y), new Vector2f(width, UiTheme.MetadataRowHeight));
        if (shown != value && input.IsHovering(row))
        {
            tooltips.Show(value);
        }
    }

    private void DrawNavigation(Painter painter, UiContext input, FloatRect row)
    {
        painter.DrawPart(UiPart.NavigationBar, PartState.Normal, row);

        float buttonY = row.Position.Y + ((row.Size.Y - UiTheme.ButtonHeight) / 2f);

        FloatRect previous = new(
            new Vector2f(row.Position.X + UiTheme.PanelPadding, buttonY),
            new Vector2f(UiTheme.ButtonWidth, UiTheme.ButtonHeight));

        FloatRect next = new(
            new Vector2f(row.Position.X + row.Size.X - UiTheme.PanelPadding - UiTheme.ButtonWidth, buttonY),
            new Vector2f(UiTheme.ButtonWidth, UiTheme.ButtonHeight));

        bool goPrevious = Button.Draw(painter, input, previous, strings.PreviewPrevious, session.CanMovePrevious)
            || input.KeyPressed(Keyboard.Key.Left)
            || input.KeyPressed(Keyboard.Key.Up)
            || input.KeyPressed(Keyboard.Key.A)
            || input.KeyPressed(Keyboard.Key.W);

        bool goNext = Button.Draw(painter, input, next, strings.PreviewNext, session.CanMoveNext)
            || input.KeyPressed(Keyboard.Key.Right)
            || input.KeyPressed(Keyboard.Key.Down)
            || input.KeyPressed(Keyboard.Key.D)
            || input.KeyPressed(Keyboard.Key.S);

        if (goPrevious)
        {
            session.MovePrevious();
        }
        else if (goNext)
        {
            session.MoveNext();
        }

        if (session.Files.Count > 0)
        {
            painter.DrawTextCentered(
                string.Format(strings.PreviewPosition, session.ActiveIndex + 1, session.Files.Count),
                row,
                UiTheme.BodyTextSize,
                UiTheme.TextMuted);
        }
    }
}
