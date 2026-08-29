using SFML.Graphics;

namespace LocalFileSorter.Ui.Theme;

public enum SurfaceKind
{
    Solid,
    Gradient,
    Bevel,
    NineSlice,
}

public enum GradientDirection
{
    Vertical,
    Horizontal,
}

public enum BevelKind
{
    Flat,
    Raised,
    Sunken,
}

public enum MarkerShape
{
    Circle,
    Square,
}

[Flags]
public enum Edges
{
    None = 0,
    Left = 1,
    Top = 2,
    Right = 4,
    Bottom = 8,
    All = Left | Top | Right | Bottom,
}

public readonly record struct Insets(float Left, float Top, float Right, float Bottom);

public sealed record SurfaceStyle
{
    public SurfaceKind Kind { get; init; } = SurfaceKind.Solid;

    public Color Fill { get; init; } = Color.Transparent;

    public Color? FillTo { get; init; }

    public GradientDirection Direction { get; init; } = GradientDirection.Vertical;

    public Color? Border { get; init; }

    public float BorderThickness { get; init; } = 1f;

    public Edges BorderEdges { get; init; } = Edges.All;

    public float CornerRadius { get; init; }

    public BevelKind Bevel { get; init; } = BevelKind.Flat;

    public Color BevelLight { get; init; } = new(255, 255, 255);

    public Color BevelDark { get; init; } = new(128, 128, 128);

    public float BevelThickness { get; init; } = 1f;

    public Color? Foreground { get; init; }

    public MarkerShape Shape { get; init; } = MarkerShape.Square;

    public string? Texture { get; init; }

    public Insets TextureInsets { get; init; }
}
