using LocalFileSorter.Ui.Theme;

using SFML.Graphics;
using SFML.System;

namespace LocalFileSorter.Ui.Rendering;

public sealed class SkinRenderer
{
    private readonly Painter painter;

    public SkinRenderer(Painter painter)
    {
        this.painter = painter;
    }

    public void DrawPart(UiPart part, PartState state, FloatRect area, Color? dataFill)
    {
        SurfaceStyle style = UiTheme.Style(part, state);
        if (area.Size.X <= 0f || area.Size.Y <= 0f)
        {
            return;
        }

        if (style.Shape == MarkerShape.Circle)
        {
            DrawCircle(style, area, dataFill);
            return;
        }

        Fill(style, area, dataFill);
        DrawFrame(style, area);
    }

    public void DrawFrame(UiPart part, PartState state, FloatRect area)
    {
        if (area.Size.X > 0f && area.Size.Y > 0f)
        {
            DrawFrame(UiTheme.Style(part, state), area);
        }
    }

    private void DrawCircle(SurfaceStyle style, FloatRect area, Color? dataFill)
    {
        float radius = MathF.Min(area.Size.X, area.Size.Y) / 2f;
        Vector2f center = new(area.Position.X + (area.Size.X / 2f), area.Position.Y + (area.Size.Y / 2f));

        painter.FillCircle(center, radius, dataFill ?? style.Fill, style.Border, style.BorderThickness);
    }

    private void Fill(SurfaceStyle style, FloatRect area, Color? dataFill)
    {
        Color fill = dataFill ?? style.Fill;

        if (style.Kind == SurfaceKind.Gradient)
        {
            painter.FillGradient(area, fill, style.FillTo ?? fill, style.Direction, style.CornerRadius);
            return;
        }

        if (fill.A > 0)
        {
            painter.FillRoundedRect(area, fill, style.CornerRadius);
        }
    }

    private void DrawFrame(SurfaceStyle style, FloatRect area)
    {
        if (style.Kind == SurfaceKind.Bevel || style.Bevel != BevelKind.Flat)
        {
            painter.DrawBevel(
                area,
                style.BevelLight,
                style.BevelDark,
                style.Bevel == BevelKind.Flat ? BevelKind.Raised : style.Bevel,
                style.BevelThickness);
        }

        if (style.Border is not Color border || style.BorderThickness <= 0f || style.BorderEdges == Edges.None)
        {
            return;
        }

        if (style.BorderEdges == Edges.All)
        {
            painter.StrokeRoundedRect(area, border, style.CornerRadius, style.BorderThickness);
            return;
        }

        float thickness = MathF.Min(style.BorderThickness, MathF.Min(area.Size.X, area.Size.Y));

        if (style.BorderEdges.HasFlag(Edges.Top))
        {
            painter.FillRect(new FloatRect(area.Position, new Vector2f(area.Size.X, thickness)), border);
        }

        if (style.BorderEdges.HasFlag(Edges.Bottom))
        {
            painter.FillRect(
                new FloatRect(
                    new Vector2f(area.Position.X, area.Position.Y + area.Size.Y - thickness),
                    new Vector2f(area.Size.X, thickness)),
                border);
        }

        if (style.BorderEdges.HasFlag(Edges.Left))
        {
            painter.FillRect(new FloatRect(area.Position, new Vector2f(thickness, area.Size.Y)), border);
        }

        if (style.BorderEdges.HasFlag(Edges.Right))
        {
            painter.FillRect(
                new FloatRect(
                    new Vector2f(area.Position.X + area.Size.X - thickness, area.Position.Y),
                    new Vector2f(thickness, area.Size.Y)),
                border);
        }
    }
}
