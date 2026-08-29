using SFML.Graphics;

namespace LocalFileSorter.Ui.Theme;

public static class SkinDefaults
{
    public const string Name = "flat-dark";

    private static readonly Dictionary<string, Color> colors = new(StringComparer.Ordinal)
    {
        ["textPrimary"] = Rgb("#E2E5EB"),
        ["textMuted"] = Rgb("#848B98"),
        ["textDisabled"] = Rgb("#585E69"),
        ["messageInfo"] = Rgb("#96BE8C"),
        ["messageError"] = Rgb("#E87A7A"),
        ["rowActiveBorder"] = Rgb("#7AA2F7"),
        ["swatchSelectedBorder"] = Rgb("#F0F3F8"),
        ["swatchHoverTint"] = Rgb("#FFFFFF"),
        ["separator"] = Rgb("#323740"),
    };

    private static readonly Dictionary<string, float> metrics = new(StringComparer.Ordinal)
    {
        ["queueWidth"] = 320f,
        ["bucketsWidth"] = 320f,
        ["panelGap"] = 1f,
        ["panelHeaderHeight"] = 34f,
        ["panelPadding"] = 12f,
        ["queueRowHeight"] = 40f,
        ["queueFooterHeight"] = 28f,
        ["navigationRowHeight"] = 44f,
        ["paletteStripHeight"] = 140f,
        ["swatchWidth"] = 92f,
        ["swatchHeight"] = 34f,
        ["swatchLabelHeight"] = 18f,
        ["swatchGap"] = 8f,
        ["bucketRowHeight"] = 40f,
        ["bucketChipSize"] = 16f,
        ["metadataRowHeight"] = 20f,
        ["metadataStripPadding"] = 8f,
        ["buttonWidth"] = 96f,
        ["buttonHeight"] = 28f,
        ["bucketsFooterHeight"] = 138f,
        ["footerButtonHeight"] = 34f,
        ["footerButtonGap"] = 8f,
        ["modalWidth"] = 480f,
        ["modalPadding"] = 18f,
        ["modalHeaderHeight"] = 34f,
        ["modalRowHeight"] = 22f,
        ["modalButtonWidth"] = 110f,
        ["modalButtonGap"] = 10f,
        ["progressBarHeight"] = 10f,
        ["scrollBarWidth"] = 6f,
        ["scrollStep"] = 48f,
        ["tooltipPadding"] = 6f,
        ["rowTintAmount"] = 0.16f,
        ["rowTintAmountHover"] = 0.24f,
        ["rowTintAmountActive"] = 0.32f,
        ["movedMarkerTintAmount"] = 0.35f,
        ["swatchHoverTintAmount"] = 0.18f,
        ["swatchDisabledTintAmount"] = 0.35f,
    };

    private static readonly Dictionary<string, uint> textSizes = new(StringComparer.Ordinal)
    {
        ["headerSize"] = 15u,
        ["bodySize"] = 14u,
        ["smallSize"] = 12u,
        ["monoSize"] = 13u,
    };

    private static readonly Dictionary<PartKey, SurfaceStyle> styles = new()
    {
        [Key(UiPart.Window)] = new SurfaceStyle { Fill = Rgb("#181A1E") },
        [Key(UiPart.Panel)] = new SurfaceStyle { Fill = Rgb("#202329"), Border = Rgb("#3A3F49") },
        [Key(UiPart.PanelHeader)] = new SurfaceStyle { Fill = Rgb("#292D35"), Foreground = Rgb("#E2E5EB") },
        [Key(UiPart.PanelFooter)] = new SurfaceStyle { Fill = Rgb("#292D35"), Border = Rgb("#323740"), BorderEdges = Edges.Top },
        [Key(UiPart.PaletteStrip)] = new SurfaceStyle { Fill = Rgb("#292D35"), Border = Rgb("#323740"), BorderEdges = Edges.Top },
        [Key(UiPart.NavigationBar)] = new SurfaceStyle { Fill = Rgb("#292D35") },
        [Key(UiPart.MetadataStrip)] = new SurfaceStyle { Fill = Rgb("#202329"), Border = Rgb("#323740"), BorderEdges = Edges.Top },
        [Key(UiPart.Viewport)] = new SurfaceStyle { Fill = Rgb("#1A1C21") },
        [Key(UiPart.ReportList)] = new SurfaceStyle { Fill = Rgb("#1A1C21"), Border = Rgb("#3A3F49") },

        [Key(UiPart.QueueRow)] = new SurfaceStyle { Fill = Rgb("#00000000") },
        [Key(UiPart.QueueRow, PartState.Hover)] = new SurfaceStyle { Fill = Rgb("#2D323B") },
        [Key(UiPart.QueueRow, PartState.Active)] = new SurfaceStyle { Fill = Rgb("#343C4A"), Border = Rgb("#7AA2F7") },
        [Key(UiPart.BucketRow, PartState.Hover)] = new SurfaceStyle { Fill = Rgb("#2D323B") },

        [Key(UiPart.Button)] = new SurfaceStyle { Fill = Rgb("#30353E"), Border = Rgb("#3A3F49"), Foreground = Rgb("#E2E5EB") },
        [Key(UiPart.Button, PartState.Hover)] = new SurfaceStyle { Fill = Rgb("#3C434F"), Border = Rgb("#3A3F49"), Foreground = Rgb("#E2E5EB") },
        [Key(UiPart.Button, PartState.Disabled)] = new SurfaceStyle { Fill = Rgb("#262930"), Border = Rgb("#3A3F49"), Foreground = Rgb("#585E69") },

        [Key(UiPart.Swatch)] = new SurfaceStyle { Border = Rgb("#3A3F49"), BorderThickness = 1f },
        [Key(UiPart.Swatch, PartState.Active)] = new SurfaceStyle { Border = Rgb("#F0F3F8"), BorderThickness = 2f },
        [Key(UiPart.BucketChip)] = new SurfaceStyle { Border = Rgb("#3A3F49") },
        [Key(UiPart.BucketMarker)] = new SurfaceStyle { Shape = MarkerShape.Circle },

        [Key(UiPart.Modal)] = new SurfaceStyle { Fill = Rgb("#262A32"), Border = Rgb("#3A3F49") },
        [Key(UiPart.ModalHeader)] = new SurfaceStyle { Fill = Rgb("#292D35"), Foreground = Rgb("#E2E5EB") },
        [Key(UiPart.Scrim)] = new SurfaceStyle { Fill = Rgb("#0A0B0EBE") },
        [Key(UiPart.ScrollThumb)] = new SurfaceStyle { Fill = Rgb("#4E5562") },
        [Key(UiPart.ScrollTrack)] = new SurfaceStyle(),
        [Key(UiPart.ProgressTrack)] = new SurfaceStyle { Fill = Rgb("#1A1C21"), Border = Rgb("#3A3F49") },
        [Key(UiPart.ProgressFill)] = new SurfaceStyle { Fill = Rgb("#7AA2F7") },
        [Key(UiPart.Tooltip)] = new SurfaceStyle { Fill = Rgb("#121418"), Border = Rgb("#3A3F49") },
    };

    public static IReadOnlyDictionary<string, Color> Colors => colors;

    public static IReadOnlyDictionary<string, float> Metrics => metrics;

    public static IReadOnlyDictionary<string, uint> TextSizes => textSizes;

    public static SurfaceStyle Style(UiPart part, PartState state)
    {
        if (styles.TryGetValue(new PartKey(part, state), out SurfaceStyle? style))
        {
            return style;
        }

        return styles.TryGetValue(new PartKey(part, PartState.Normal), out SurfaceStyle? normal)
            ? normal
            : Empty;
    }

    public static bool Defines(UiPart part, PartState state) => styles.ContainsKey(new PartKey(part, state));

    private static SurfaceStyle Empty { get; } = new();

    private static PartKey Key(UiPart part, PartState state = PartState.Normal) => new(part, state);

    private static Color Rgb(string value) =>
        SkinValueParser.TryColor(value, out Color color) ? color : Color.Transparent;
}
