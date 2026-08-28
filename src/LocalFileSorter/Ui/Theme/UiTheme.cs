using SFML.Graphics;

namespace LocalFileSorter.Ui.Theme;

public static class UiTheme
{
    public const float QueueWidth = 320f;
    public const float BucketsWidth = 320f;
    public const float PanelGap = 1f;
    public const float PanelHeaderHeight = 34f;
    public const float PanelPadding = 12f;

    public const float QueueRowHeight = 40f;
    public const float QueueFooterHeight = 28f;
    public const float NavigationRowHeight = 44f;
    public const float PaletteStripHeight = 140f;
    public const float SwatchWidth = 92f;
    public const float SwatchHeight = 34f;
    public const float SwatchLabelHeight = 18f;
    public const float SwatchGap = 8f;
    public const float BucketRowHeight = 40f;
    public const float BucketChipSize = 16f;
    public const float MetadataRowHeight = 20f;
    public const float MetadataStripPadding = 8f;
    public const float ButtonWidth = 96f;
    public const float ButtonHeight = 28f;
    public const float BucketsFooterHeight = 138f;
    public const float FooterButtonHeight = 34f;
    public const float FooterButtonGap = 8f;
    public const float ModalWidth = 480f;
    public const float ModalPadding = 18f;
    public const float ModalHeaderHeight = 34f;
    public const float ModalRowHeight = 22f;
    public const float ModalButtonWidth = 110f;
    public const float ModalButtonGap = 10f;
    public const float ProgressBarHeight = 10f;
    public const float ScrollBarWidth = 6f;
    public const float ScrollStep = 48f;
    public const float TooltipPadding = 6f;

    public const uint HeaderTextSize = 15u;
    public const uint BodyTextSize = 14u;
    public const uint SmallTextSize = 12u;
    public const uint MonoTextSize = 13u;

    public static readonly Color Background = new(24, 26, 30);
    public static readonly Color PanelBackground = new(32, 35, 41);
    public static readonly Color PanelHeaderBackground = new(41, 45, 53);
    public static readonly Color PanelBorder = new(58, 63, 73);
    public static readonly Color TextPrimary = new(226, 229, 235);
    public static readonly Color TextMuted = new(132, 139, 152);
    public static readonly Color TextDisabled = new(88, 94, 105);

    public static readonly Color ViewportBackground = new(26, 28, 33);
    public static readonly Color RowHover = new(45, 50, 59);
    public static readonly Color RowActive = new(52, 60, 74);
    public static readonly Color RowActiveBorder = new(122, 162, 247);
    public static readonly Color Separator = new(50, 55, 64);
    public static readonly Color SwatchSelectedBorder = new(240, 243, 248);

    public static readonly Color ButtonBackground = new(48, 53, 62);
    public static readonly Color ButtonHover = new(60, 67, 79);
    public static readonly Color ButtonDisabled = new(38, 41, 48);

    public static readonly Color ScrollBar = new(78, 85, 98);
    public static readonly Color Scrim = new(10, 11, 14, 190);
    public static readonly Color ModalBackground = new(38, 42, 50);
    public static readonly Color ProgressTrack = new(26, 28, 33);
    public static readonly Color ProgressFill = new(122, 162, 247);
    public static readonly Color MessageInfo = new(150, 190, 140);
    public static readonly Color MessageError = new(232, 122, 122);
    public static readonly Color TooltipBackground = new(18, 20, 24);
}
