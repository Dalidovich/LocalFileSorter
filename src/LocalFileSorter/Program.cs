using LocalFileSorter.App.Startup;
using LocalFileSorter.Common.Abstractions;
using LocalFileSorter.Common.Localization;
using LocalFileSorter.Common.Model;
using LocalFileSorter.Common.Persistence;
using LocalFileSorter.Common.Services;
using LocalFileSorter.Previews;
using LocalFileSorter.Previews.Image;
using LocalFileSorter.Previews.Text;
using LocalFileSorter.Ui.Rendering;
using LocalFileSorter.Ui.Shell;

using SFML;

namespace LocalFileSorter;

public static class Program
{
    public static int Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        AppSettingsStore settingsStore = AppSettingsStore.ForCurrentUser();
        AppSettings settings = settingsStore.Load();

        if (!LocalizationCatalogLoader.TryLoad(AssetPaths.I18n, settings.Language, out LocalizationCatalog catalog, out string catalogPath))
        {
            Console.Error.WriteLine("Localization catalog not found: " + catalogPath);
        }

        Strings strings = new(catalog);
        ReportMissingKeys(catalog, strings);

        StartupOptions? options = new ConsoleStartup(strings).Prompt();
        if (options is null)
        {
            return 0;
        }

        PreviewRegistry registry = new(strings, new IPreviewProvider[]
        {
            new TextPreviewProvider(strings),
            new ImagePreviewProvider(strings),
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
            using FontLibrary fonts = new(AssetPaths.UiFont, AssetPaths.MonoFont);
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

        settingsStore.Save(settings);
        return 0;
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
