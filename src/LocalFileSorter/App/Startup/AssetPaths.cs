namespace LocalFileSorter.App.Startup;

public static class AssetPaths
{
    public static string Root { get; } = Path.Combine(AppContext.BaseDirectory, "assets");

    public static string Fonts { get; } = Path.Combine(Root, "fonts");

    public static string I18n { get; } = Path.Combine(Root, "i18n");

    public static string UiFont { get; } = Path.Combine(Fonts, "NotoSans-Regular.ttf");

    public static string MonoFont { get; } = Path.Combine(Fonts, "DejaVuSansMono.ttf");
}
