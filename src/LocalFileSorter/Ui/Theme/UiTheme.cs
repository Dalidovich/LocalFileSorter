using SFML.Graphics;

namespace LocalFileSorter.Ui.Theme;

public static class UiTheme
{
    private static readonly int StateCount = Enum.GetValues<PartState>().Length;

    private static SurfaceStyle[] styles = BuildStyles(Skin.BuiltIn);

    static UiTheme() => Apply(Skin.BuiltIn);

    public static Skin Current { get; private set; } = Skin.BuiltIn;

    public static float QueueWidth { get; private set; }

    public static float BucketsWidth { get; private set; }

    public static float PanelGap { get; private set; }

    public static float PanelHeaderHeight { get; private set; }

    public static float PanelPadding { get; private set; }

    public static float QueueRowHeight { get; private set; }

    public static float QueueFooterHeight { get; private set; }

    public static float NavigationRowHeight { get; private set; }

    public static float PaletteStripHeight { get; private set; }

    public static float SwatchWidth { get; private set; }

    public static float SwatchHeight { get; private set; }

    public static float SwatchLabelHeight { get; private set; }

    public static float SwatchGap { get; private set; }

    public static float BucketRowHeight { get; private set; }

    public static float BucketChipSize { get; private set; }

    public static float MetadataRowHeight { get; private set; }

    public static float MetadataStripPadding { get; private set; }

    public static float ButtonWidth { get; private set; }

    public static float ButtonHeight { get; private set; }

    public static float BucketsFooterHeight { get; private set; }

    public static float FooterButtonHeight { get; private set; }

    public static float FooterButtonGap { get; private set; }

    public static float ModalWidth { get; private set; }

    public static float ModalPadding { get; private set; }

    public static float ModalHeaderHeight { get; private set; }

    public static float ModalRowHeight { get; private set; }

    public static float ModalButtonWidth { get; private set; }

    public static float ModalButtonGap { get; private set; }

    public static float ProgressBarHeight { get; private set; }

    public static float ScrollBarWidth { get; private set; }

    public static float ScrollStep { get; private set; }

    public static float TooltipPadding { get; private set; }

    public static float RowTintAmount { get; private set; }

    public static float RowTintAmountHover { get; private set; }

    public static float RowTintAmountActive { get; private set; }

    public static float MovedMarkerTintAmount { get; private set; }

    public static float SwatchHoverTintAmount { get; private set; }

    public static float SwatchDisabledTintAmount { get; private set; }

    public static uint HeaderTextSize { get; private set; }

    public static uint BodyTextSize { get; private set; }

    public static uint SmallTextSize { get; private set; }

    public static uint MonoTextSize { get; private set; }

    public static Color TextPrimary { get; private set; }

    public static Color TextMuted { get; private set; }

    public static Color TextDisabled { get; private set; }

    public static Color MessageInfo { get; private set; }

    public static Color MessageError { get; private set; }

    public static Color SwatchHoverTint { get; private set; }

    public static void Apply(Skin skin)
    {
        Current = skin;
        styles = BuildStyles(skin);

        QueueWidth = skin.Metrics["queueWidth"];
        BucketsWidth = skin.Metrics["bucketsWidth"];
        PanelGap = skin.Metrics["panelGap"];
        PanelHeaderHeight = skin.Metrics["panelHeaderHeight"];
        PanelPadding = skin.Metrics["panelPadding"];
        QueueRowHeight = skin.Metrics["queueRowHeight"];
        QueueFooterHeight = skin.Metrics["queueFooterHeight"];
        NavigationRowHeight = skin.Metrics["navigationRowHeight"];
        PaletteStripHeight = skin.Metrics["paletteStripHeight"];
        SwatchWidth = skin.Metrics["swatchWidth"];
        SwatchHeight = skin.Metrics["swatchHeight"];
        SwatchLabelHeight = skin.Metrics["swatchLabelHeight"];
        SwatchGap = skin.Metrics["swatchGap"];
        BucketRowHeight = skin.Metrics["bucketRowHeight"];
        BucketChipSize = skin.Metrics["bucketChipSize"];
        MetadataRowHeight = skin.Metrics["metadataRowHeight"];
        MetadataStripPadding = skin.Metrics["metadataStripPadding"];
        ButtonWidth = skin.Metrics["buttonWidth"];
        ButtonHeight = skin.Metrics["buttonHeight"];
        BucketsFooterHeight = skin.Metrics["bucketsFooterHeight"];
        FooterButtonHeight = skin.Metrics["footerButtonHeight"];
        FooterButtonGap = skin.Metrics["footerButtonGap"];
        ModalWidth = skin.Metrics["modalWidth"];
        ModalPadding = skin.Metrics["modalPadding"];
        ModalHeaderHeight = skin.Metrics["modalHeaderHeight"];
        ModalRowHeight = skin.Metrics["modalRowHeight"];
        ModalButtonWidth = skin.Metrics["modalButtonWidth"];
        ModalButtonGap = skin.Metrics["modalButtonGap"];
        ProgressBarHeight = skin.Metrics["progressBarHeight"];
        ScrollBarWidth = skin.Metrics["scrollBarWidth"];
        ScrollStep = skin.Metrics["scrollStep"];
        TooltipPadding = skin.Metrics["tooltipPadding"];
        RowTintAmount = skin.Metrics["rowTintAmount"];
        RowTintAmountHover = skin.Metrics["rowTintAmountHover"];
        RowTintAmountActive = skin.Metrics["rowTintAmountActive"];
        MovedMarkerTintAmount = skin.Metrics["movedMarkerTintAmount"];
        SwatchHoverTintAmount = skin.Metrics["swatchHoverTintAmount"];
        SwatchDisabledTintAmount = skin.Metrics["swatchDisabledTintAmount"];

        HeaderTextSize = skin.TextSizes["headerSize"];
        BodyTextSize = skin.TextSizes["bodySize"];
        SmallTextSize = skin.TextSizes["smallSize"];
        MonoTextSize = skin.TextSizes["monoSize"];

        TextPrimary = skin.Colors["textPrimary"];
        TextMuted = skin.Colors["textMuted"];
        TextDisabled = skin.Colors["textDisabled"];
        MessageInfo = skin.Colors["messageInfo"];
        MessageError = skin.Colors["messageError"];
        SwatchHoverTint = skin.Colors["swatchHoverTint"];
    }

    public static SurfaceStyle Style(UiPart part, PartState state = PartState.Normal) =>
        styles[((int)part * StateCount) + (int)state];

    public static Color Foreground(UiPart part, PartState state = PartState.Normal) =>
        Style(part, state).Foreground ?? TextPrimary;

    private static SurfaceStyle[] BuildStyles(Skin skin)
    {
        int stateCount = Enum.GetValues<PartState>().Length;
        SurfaceStyle[] table = new SurfaceStyle[Enum.GetValues<UiPart>().Length * stateCount];

        foreach (UiPart part in Enum.GetValues<UiPart>())
        {
            foreach (PartState state in Enum.GetValues<PartState>())
            {
                table[((int)part * stateCount) + (int)state] = skin.Style(part, state);
            }
        }

        return table;
    }
}
