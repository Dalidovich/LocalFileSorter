using LocalFileSorter.Common.Localization;
using LocalFileSorter.Common.Persistence;
using LocalFileSorter.Common.Model;
using LocalFileSorter.Common.Services;
using LocalFileSorter.Previews;
using LocalFileSorter.Ui.Input;
using LocalFileSorter.Ui.Panels;
using LocalFileSorter.Ui.Rendering;
using LocalFileSorter.Ui.Theme;
using LocalFileSorter.Ui.Widgets;

using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace LocalFileSorter.Ui.Shell;

public sealed class AppShell : IDisposable
{
    private const uint MinimumWidth = 1024u;
    private const uint MinimumHeight = 600u;
    private const uint FrameRateLimit = 60u;

    private readonly RenderWindow window;
    private readonly Painter painter;
    private readonly UiContext input = new();
    private readonly TooltipHost tooltips = new();
    private readonly AppSettings settings;
    private readonly SortSession session;
    private readonly PreviewLoader loader;
    private readonly CommitOverlay commit;
    private readonly MappingOverlay mapping;
    private readonly PreviewPanel previewPanel;
    private readonly QueuePanel queuePanel;
    private readonly BucketsPanel bucketsPanel;

    public AppShell(
        Strings strings,
        FontLibrary fonts,
        SortSession session,
        SortPlanService plan,
        PreviewLoader loader,
        CommitRunner runner,
        MappingService mappingService,
        AppSettings settings)
    {
        this.settings = settings;
        this.session = session;
        this.loader = loader;
        commit = new CommitOverlay(strings, session, plan, runner, tooltips);
        mapping = new MappingOverlay(strings, mappingService);

        window = new RenderWindow(
            new VideoMode(RestoredSize(settings)),
            strings.AppTitle,
            Styles.Default,
            State.Windowed);

        window.SetMinimumSize(new Vector2u(MinimumWidth, MinimumHeight));
        window.SetFramerateLimit(FrameRateLimit);
        WindowIcon.Apply(window);

        painter = new Painter(window, fonts);
        previewPanel = new PreviewPanel(strings, session, plan, tooltips);
        queuePanel = new QueuePanel(strings, session, plan, tooltips);
        bucketsPanel = new BucketsPanel(
            strings,
            session,
            plan,
            tooltips,
            new BucketActions(commit.Request, () => plan.Undo(), mapping.RequestReload, mapping.RequestRecolor));

        window.Closed += (_, _) => window.Close();
        window.Resized += (_, e) => Resize(e.Size);
        input.Attach(window);
    }

    public void Run()
    {
        while (window.IsOpen)
        {
            input.BeginFrame();
            tooltips.BeginFrame();
            window.DispatchEvents();

            commit.Update();
            previewPanel.SetSnapshot(loader.Update(session.ActiveFile));

            window.Clear(UiTheme.Style(UiPart.Window).Fill);
            DrawFrame();
            window.Display();
        }
    }

    public void Dispose()
    {
        previewPanel.Dispose();
        painter.Dispose();
        window.Dispose();
    }

    private static Vector2u RestoredSize(AppSettings settings) => new(
        Math.Max(MinimumWidth, (uint)Math.Max(0, settings.WindowWidth)),
        Math.Max(MinimumHeight, (uint)Math.Max(0, settings.WindowHeight)));

    private void Resize(Vector2u size)
    {
        painter.Resize(size);
        settings.WindowWidth = (int)size.X;
        settings.WindowHeight = (int)size.Y;
    }

    private void DrawFrame()
    {
        ShellLayout layout = ShellLayout.Compute(window.Size);
        Vector2f surfaceSize = new(window.Size.X, window.Size.Y);

        painter.DrawPart(UiPart.Window, PartState.Normal, new FloatRect(new Vector2f(0f, 0f), surfaceSize));

        input.Blocked = commit.IsBlocking || mapping.IsBlocking;
        previewPanel.Draw(painter, input, layout.Preview);
        queuePanel.Draw(painter, input, layout.Queue);
        bucketsPanel.Draw(painter, input, layout.Buckets);

        input.Blocked = false;
        FloatRect surface = new(new Vector2f(0f, 0f), surfaceSize);
        commit.Draw(painter, input, surface);
        mapping.Draw(painter, input, surface);

        tooltips.Draw(painter, input.MousePosition, surfaceSize);
    }
}
