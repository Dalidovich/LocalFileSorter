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

    public void FillCircle(Vector2f center, float radius, Color color)
    {
        circle.Radius = radius;
        circle.Origin = new Vector2f(radius, radius);
        circle.Position = center;
        circle.FillColor = color;
        window.Draw(circle);
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
        sprite?.Dispose();
        uiText.Dispose();
        monoText.Dispose();
        baseView.Dispose();
        clipView.Dispose();
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
