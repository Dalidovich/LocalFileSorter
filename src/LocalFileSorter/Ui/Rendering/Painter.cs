using LocalFileSorter.Ui.Theme;

using SFML.Graphics;
using SFML.System;

namespace LocalFileSorter.Ui.Rendering;

public sealed class Painter : IDisposable
{
    private readonly RenderWindow window;
    private readonly FontLibrary fonts;
    private readonly RectangleShape rectangle = new();
    private readonly CircleShape circle = new();
    private readonly VertexArray quad = new(PrimitiveType.TriangleStrip, 4);
    private readonly VertexArray fan = new(PrimitiveType.TriangleFan);
    private readonly VertexArray outline = new(PrimitiveType.LineStrip);
    private readonly SkinRenderer skin;
    private readonly Text uiText;
    private readonly Text monoText;
    private readonly View baseView = new();
    private readonly View clipView = new();
    private readonly Stack<FloatRect> clips = new();

    private Sprite? sprite;
    private Vector2f surfaceSize;

    public Painter(RenderWindow window, FontLibrary fonts)
    {
        this.window = window;
        this.fonts = fonts;
        skin = new SkinRenderer(this);
        uiText = new Text(fonts.Ui, string.Empty, UiTheme.BodyTextSize);
        monoText = new Text(fonts.Mono, string.Empty, UiTheme.BodyTextSize);
        Resize(window.Size);
    }

    public void Resize(Vector2u size)
    {
        surfaceSize = new Vector2f(MathF.Max(1f, size.X), MathF.Max(1f, size.Y));
        baseView.Size = surfaceSize;
        baseView.Center = surfaceSize / 2f;
        baseView.Viewport = new FloatRect(new Vector2f(0f, 0f), new Vector2f(1f, 1f));
        window.SetView(baseView);
    }

    public TextMetrics Metrics(bool mono, uint size) => fonts.Metrics(mono, size);

    public void PushClip(FloatRect area)
    {
        FloatRect clipped = clips.Count == 0 ? area : Intersect(clips.Peek(), area);
        clips.Push(clipped);
        ApplyClip(clipped);
    }

    public void PopClip()
    {
        clips.Pop();
        if (clips.Count == 0)
        {
            window.SetView(baseView);
        }
        else
        {
            ApplyClip(clips.Peek());
        }
    }

    public void FillRect(FloatRect area, Color color)
    {
        rectangle.Position = area.Position;
        rectangle.Size = area.Size;
        rectangle.FillColor = color;
        rectangle.OutlineThickness = 0f;
        window.Draw(rectangle);
    }

    public void FillCircle(Vector2f center, float radius, Color color, Color? border = null, float borderThickness = 0f)
    {
        circle.Radius = radius;
        circle.Origin = new Vector2f(radius, radius);
        circle.Position = center;
        circle.FillColor = color;
        circle.OutlineColor = border ?? Color.Transparent;
        circle.OutlineThickness = border is null ? 0f : -borderThickness;
        window.Draw(circle);
        circle.OutlineThickness = 0f;
    }

    public void DrawPart(UiPart part, PartState state, FloatRect area, Color? dataFill = null) =>
        skin.DrawPart(part, state, area, dataFill);

    public void DrawPartFrame(UiPart part, PartState state, FloatRect area) => skin.DrawFrame(part, state, area);

    public void FillGradient(FloatRect area, Color from, Color to, GradientDirection direction, float radius = 0f)
    {
        if (radius > 0f)
        {
            FillFan(area, radius, from, to, direction);
            return;
        }

        bool vertical = direction == GradientDirection.Vertical;
        Vector2f position = area.Position;
        Vector2f size = area.Size;

        quad[0] = new Vertex(position, from);
        quad[1] = new Vertex(new Vector2f(position.X + size.X, position.Y), vertical ? from : to);
        quad[2] = new Vertex(new Vector2f(position.X, position.Y + size.Y), vertical ? to : from);
        quad[3] = new Vertex(position + size, to);
        window.Draw(quad);
    }

    public void FillRoundedRect(FloatRect area, Color color, float radius)
    {
        if (radius <= 0f)
        {
            FillRect(area, color);
            return;
        }

        FillFan(area, radius, color, color, GradientDirection.Vertical);
    }

    public void StrokeRoundedRect(FloatRect area, Color color, float radius, float thickness = 1f)
    {
        if (radius <= 0f)
        {
            StrokeRect(area, color, thickness);
            return;
        }

        for (int step = 0; step < (int)MathF.Max(1f, MathF.Round(thickness)); step++)
        {
            FloatRect inner = Deflate(area, step);
            if (inner.Size.X <= 0f || inner.Size.Y <= 0f)
            {
                return;
            }

            outline.Clear();
            foreach (Vector2f point in Perimeter(inner, MathF.Max(0f, radius - step)))
            {
                outline.Append(new Vertex(point, color));
            }

            window.Draw(outline);
        }
    }

    public void DrawBevel(FloatRect area, Color light, Color dark, BevelKind kind, float thickness)
    {
        if (kind == BevelKind.Flat || thickness <= 0f)
        {
            return;
        }

        Color topLeft = kind == BevelKind.Sunken ? dark : light;
        Color bottomRight = kind == BevelKind.Sunken ? light : dark;
        float band = MathF.Min(thickness, MathF.Min(area.Size.X, area.Size.Y) / 2f);

        FillRect(new FloatRect(area.Position, new Vector2f(area.Size.X, band)), topLeft);
        FillRect(new FloatRect(area.Position, new Vector2f(band, area.Size.Y)), topLeft);
        FillRect(
            new FloatRect(
                new Vector2f(area.Position.X, area.Position.Y + area.Size.Y - band),
                new Vector2f(area.Size.X, band)),
            bottomRight);
        FillRect(
            new FloatRect(
                new Vector2f(area.Position.X + area.Size.X - band, area.Position.Y),
                new Vector2f(band, area.Size.Y)),
            bottomRight);
    }

    public void StrokeRect(FloatRect area, Color color, float thickness = 1f)
    {
        rectangle.Position = new Vector2f(area.Position.X + thickness, area.Position.Y + thickness);
        rectangle.Size = new Vector2f(area.Size.X - (thickness * 2f), area.Size.Y - (thickness * 2f));
        rectangle.FillColor = Color.Transparent;
        rectangle.OutlineColor = color;
        rectangle.OutlineThickness = thickness;
        window.Draw(rectangle);
    }

    public void DrawTexture(Texture texture, FloatRect area)
    {
        if (sprite is null)
        {
            sprite = new Sprite(texture);
        }
        else
        {
            sprite.Texture = texture;
        }

        Vector2u size = texture.Size;
        sprite.TextureRect = new IntRect(new Vector2i(0, 0), new Vector2i((int)size.X, (int)size.Y));
        sprite.Position = new Vector2f(MathF.Round(area.Position.X), MathF.Round(area.Position.Y));
        sprite.Scale = new Vector2f(area.Size.X / size.X, area.Size.Y / size.Y);
        window.Draw(sprite);
    }

    public void DrawText(string value, Vector2f position, uint size, Color color, bool mono = false)
    {
        Text text = Prepare(value, size, color, mono);
        text.Position = new Vector2f(MathF.Round(position.X), MathF.Round(position.Y));
        window.Draw(text);
    }

    public void DrawTextCentered(string value, FloatRect area, uint size, Color color, bool mono = false)
    {
        Text text = Prepare(value, size, color, mono);
        FloatRect bounds = text.GetLocalBounds();
        float x = area.Position.X + ((area.Size.X - bounds.Size.X) / 2f) - bounds.Position.X;
        float y = area.Position.Y + ((area.Size.Y - bounds.Size.Y) / 2f) - bounds.Position.Y;
        text.Position = new Vector2f(MathF.Round(x), MathF.Round(y));
        window.Draw(text);
    }

    public float DrawWrappedCentered(string value, FloatRect area, float top, uint size, Color color)
    {
        TextMetrics metrics = Metrics(mono: false, size);

        foreach (string line in metrics.Wrap(value, area.Size.X))
        {
            DrawText(
                line,
                new Vector2f(area.Position.X + ((area.Size.X - metrics.Measure(line)) / 2f), top),
                size,
                color);

            top += metrics.LineHeight;
        }

        return top;
    }

    public float MeasureWrappedHeight(string value, float maxWidth, uint size)
    {
        TextMetrics metrics = Metrics(mono: false, size);
        return metrics.Wrap(value, maxWidth).Count * metrics.LineHeight;
    }

    public void Dispose()
    {
        rectangle.Dispose();
        circle.Dispose();
        quad.Dispose();
        fan.Dispose();
        outline.Dispose();
        sprite?.Dispose();
        uiText.Dispose();
        monoText.Dispose();
        baseView.Dispose();
        clipView.Dispose();
    }

    private void FillFan(FloatRect area, float radius, Color from, Color to, GradientDirection direction)
    {
        Vector2f center = new(area.Position.X + (area.Size.X / 2f), area.Position.Y + (area.Size.Y / 2f));

        fan.Clear();
        fan.Append(new Vertex(center, Lerp(from, to, 0.5f)));

        foreach (Vector2f point in Perimeter(area, radius))
        {
            fan.Append(new Vertex(point, Lerp(from, to, Progress(point, area, direction))));
        }

        window.Draw(fan);
    }

    private static IEnumerable<Vector2f> Perimeter(FloatRect area, float radius)
    {
        const int Segments = 4;

        float left = area.Position.X;
        float top = area.Position.Y;
        float right = left + area.Size.X;
        float bottom = top + area.Size.Y;
        float corner = MathF.Min(radius, MathF.Min(area.Size.X, area.Size.Y) / 2f);

        (float X, float Y, float Start)[] corners =
        [
            (left + corner, top + corner, MathF.PI),
            (right - corner, top + corner, MathF.PI * 1.5f),
            (right - corner, bottom - corner, 0f),
            (left + corner, bottom - corner, MathF.PI * 0.5f),
        ];

        Vector2f first = default;

        for (int index = 0; index < corners.Length; index++)
        {
            (float x, float y, float start) = corners[index];

            for (int segment = 0; segment <= Segments; segment++)
            {
                float angle = start + (MathF.PI * 0.5f * segment / Segments);
                Vector2f point = new(x + (MathF.Cos(angle) * corner), y + (MathF.Sin(angle) * corner));

                if (index == 0 && segment == 0)
                {
                    first = point;
                }

                yield return point;
            }
        }

        yield return first;
    }

    private static float Progress(Vector2f point, FloatRect area, GradientDirection direction)
    {
        float span = direction == GradientDirection.Vertical ? area.Size.Y : area.Size.X;
        if (span <= 0f)
        {
            return 0f;
        }

        float offset = direction == GradientDirection.Vertical
            ? point.Y - area.Position.Y
            : point.X - area.Position.X;

        return Math.Clamp(offset / span, 0f, 1f);
    }

    private static Color Lerp(Color from, Color to, float amount) => new(
        Channel(from.R, to.R, amount),
        Channel(from.G, to.G, amount),
        Channel(from.B, to.B, amount),
        Channel(from.A, to.A, amount));

    private static byte Channel(byte from, byte to, float amount) =>
        (byte)Math.Clamp(MathF.Round(from + ((to - from) * amount)), 0f, 255f);

    private static FloatRect Deflate(FloatRect area, float amount)
    {
        float inset = amount + 0.5f;
        return new FloatRect(
            new Vector2f(area.Position.X + inset, area.Position.Y + inset),
            new Vector2f(area.Size.X - (inset * 2f), area.Size.Y - (inset * 2f)));
    }

    private static FloatRect Intersect(FloatRect outer, FloatRect inner)
    {
        float left = MathF.Max(outer.Position.X, inner.Position.X);
        float top = MathF.Max(outer.Position.Y, inner.Position.Y);
        float right = MathF.Min(outer.Position.X + outer.Size.X, inner.Position.X + inner.Size.X);
        float bottom = MathF.Min(outer.Position.Y + outer.Size.Y, inner.Position.Y + inner.Size.Y);
        return new FloatRect(new Vector2f(left, top), new Vector2f(MathF.Max(0f, right - left), MathF.Max(0f, bottom - top)));
    }

    private void ApplyClip(FloatRect area)
    {
        clipView.Size = new Vector2f(MathF.Max(1f, area.Size.X), MathF.Max(1f, area.Size.Y));
        clipView.Center = new Vector2f(area.Position.X + (area.Size.X / 2f), area.Position.Y + (area.Size.Y / 2f));
        clipView.Viewport = new FloatRect(
            new Vector2f(area.Position.X / surfaceSize.X, area.Position.Y / surfaceSize.Y),
            new Vector2f(area.Size.X / surfaceSize.X, area.Size.Y / surfaceSize.Y));
        window.SetView(clipView);
    }

    private Text Prepare(string value, uint size, Color color, bool mono)
    {
        Text text = mono ? monoText : uiText;
        text.DisplayedString = value;
        text.CharacterSize = size;
        text.FillColor = color;
        return text;
    }
}
