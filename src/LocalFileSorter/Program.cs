using LocalFileSorter.App.Startup;
using LocalFileSorter.Common.Abstractions;
using LocalFileSorter.Common.Localization;
using LocalFileSorter.Common.Model;
using LocalFileSorter.Common.Persistence;
using LocalFileSorter.Common.Services;
using LocalFileSorter.Previews;
using LocalFileSorter.Previews.Archive;
using LocalFileSorter.Previews.Image;
using LocalFileSorter.Previews.Text;
using LocalFileSorter.Previews.Vector;
using LocalFileSorter.Ui.Rendering;
using LocalFileSorter.Ui.Shell;
using LocalFileSorter.Ui.Theme;

using SFML;

namespace LocalFileSorter;

public static class Program
{
    public static int Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        AppSettingsStore settingsStore = new(PortablePaths.Settings);
        AppSettings settings = settingsStore.Load();

        if (!LocalizationCatalogLoader.TryLoad(AssetPaths.I18n, settings.Language, out LocalizationCatalog catalog, out string catalogPath))
        {
            Console.Error.WriteLine("Localization catalog not found: " + catalogPath);
        }

        Strings strings = new(catalog);
        ReportMissingKeys(catalog, strings);

        Skin skin = LoadSkin(settings.Theme);
        UiTheme.Apply(skin);
        ReportMissingTokens(skin, strings);

        StartupOptions? options = new ConsoleStartup(strings).Prompt();
        if (options is null)
        {
            return 0;
        }

        PreviewRegistry registry = new(strings, new IPreviewProvider[]
        {
            new TextPreviewProvider(strings),
            new ImagePreviewProvider(strings),
            new ArchivePreviewProvider(strings),
            new SvgPreviewProvider(strings),
        });
        ScanResult scan = SourceScanner.Scan(options.SourceRoot, registry.SupportedExtensions);
        MappingResult mapping = BucketMapper.Map(options.DestinationRoot, []);
        SortSession session = new(options.SourceRoot, options.DestinationRoot, scan, mapping.Buckets);
        SortPlanService plan = new(session);
        MappingService mappingService = new(session, plan);

        Console.WriteLine(string.Format(strings.StartupScanSummary, scan.Files.Count, scan.SkippedCount));
        Console.WriteLine(string.Format(strings.StartupBucketSummary, session.Buckets.Count));
        Console.WriteLine(strings.StartupOpening);

        try
        {
            using FontLibrary fonts = new(
                SkinLoader.ResolveFont(skin, skin.UiFont, AssetPaths.Fonts, AssetPaths.UiFont),
                SkinLoader.ResolveFont(skin, skin.MonoFont, AssetPaths.Fonts, AssetPaths.MonoFont));
            using PreviewLoader loader = new(registry);
            using CommitRunner runner = new(plan, new MoveExecutor(strings));
            using AppShell shell = new(strings, fonts, session, plan, loader, runner, mappingService, settings);
            shell.Run();
        }
        catch (LoadingFailedException exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }

        if (!settingsStore.Save(settings))
        {
            Console.Error.WriteLine("Settings not saved, location is not writable: " + settingsStore.FilePath);
        }

        return 0;
    }

    private static Skin LoadSkin(string name)
    {
        if (SkinLoader.TryLoad(PortablePaths.Themes, name, out Skin skin, out _))
        {
            return skin;
        }

        if (SkinLoader.TryLoad(AssetPaths.Themes, name, out skin, out string path))
        {
            return skin;
        }

        Console.Error.WriteLine("Theme not found: " + path);
        return Skin.BuiltIn;
    }

    private static void ReportMissingTokens(Skin skin, Strings strings)
    {
        if (skin.MissingTokens.Count == 0)
        {
            return;
        }

        Console.Error.WriteLine(string.Format(
            strings.StartupMissingThemeTokens,
            skin.MissingTokens.Count,
            string.Join(", ", skin.MissingTokens)));
    }

    private static void ReportMissingKeys(LocalizationCatalog catalog, Strings strings)
    {
        if (catalog.MissingKeys.Count == 0)
        {
            return;
        }

        Console.Error.WriteLine(string.Format(
            strings.StartupMissingKeys,
            catalog.MissingKeys.Count,
            string.Join(", ", catalog.MissingKeys)));
    }
}
