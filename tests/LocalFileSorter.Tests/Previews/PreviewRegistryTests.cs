using LocalFileSorter.Common.Abstractions;
using LocalFileSorter.Common.Localization;
using LocalFileSorter.Common.Model;
using LocalFileSorter.Previews;

using Xunit;

namespace LocalFileSorter.Tests.Previews;

public sealed class PreviewRegistryTests
{
    private readonly Strings strings = TestStrings.Shipped();

    [Fact]
    public void UnionsSupportedExtensions()
    {
        PreviewRegistry registry = new(strings, [Provider("a", 0, ".txt"), Provider("b", 0, ".md", ".txt")]);

        Assert.Equal([".md", ".txt"], registry.SupportedExtensions.Order());
    }

    [Fact]
    public void ResolvesHighestPriorityProvider()
    {
        PreviewRegistry registry = new(strings, [Provider("low", 0, ".txt"), Provider("high", 5, ".txt")]);

        Assert.Equal("high", registry.Resolve(Entry(".txt"))!.Id);
    }

    [Fact]
    public void SkipsProvidersThatDeclineTheEntry()
    {
        PreviewRegistry registry = new(strings, [Provider("high", 5, ".txt", canHandle: false), Provider("low", 0, ".txt")]);

        Assert.Equal("low", registry.Resolve(Entry(".txt"))!.Id);
    }

    [Fact]
    public void ReportsMissingModuleAsAnErrorBlock()
    {
        PreviewRegistry registry = new(strings, [Provider("a", 0, ".txt")]);

        PreviewResult result = registry.Load(Entry(".png"), PreviewBudget.Default(CancellationToken.None));

        Assert.NotNull(result.Error);
        Assert.IsType<MessageBlock>(Assert.Single(result.Document!.Blocks));
    }

    [Fact]
    public void TurnsAThrowingModuleIntoAnErrorBlock()
    {
        PreviewRegistry registry = new(strings, [new ThrowingProvider()]);

        PreviewResult result = registry.Load(Entry(".txt"), PreviewBudget.Default(CancellationToken.None));

        MessageBlock block = Assert.IsType<MessageBlock>(Assert.Single(result.Document!.Blocks));
        Assert.Equal(MessageKind.Error, block.Kind);
        Assert.Contains("boom", block.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void LetsCancellationPropagate()
    {
        PreviewRegistry registry = new(strings, [new ThrowingProvider(cancel: true)]);

        Assert.Throws<OperationCanceledException>(() =>
            registry.Load(Entry(".txt"), PreviewBudget.Default(CancellationToken.None)));
    }

    private static FileEntry Entry(string extension) => new()
    {
        Id = new FileId(0),
        CurrentPath = "src/file" + extension,
        Name = "file" + extension,
        Extension = extension,
        SizeBytes = 1,
        CreatedUtc = DateTime.UnixEpoch,
        ModifiedUtc = DateTime.UnixEpoch,
    };

    private static IPreviewProvider Provider(string id, int priority, params string[] extensions) =>
        new StubProvider(id, priority, extensions, canHandle: true);

    private static IPreviewProvider Provider(string id, int priority, string extension, bool canHandle) =>
        new StubProvider(id, priority, [extension], canHandle);

    private sealed class StubProvider : IPreviewProvider
    {
        private readonly bool canHandle;

        public StubProvider(string id, int priority, string[] extensions, bool canHandle)
        {
            Id = id;
            Priority = priority;
            Extensions = new HashSet<string>(extensions, StringComparer.Ordinal);
            this.canHandle = canHandle;
        }

        public string Id { get; }

        public int Priority { get; }

        public IReadOnlySet<string> Extensions { get; }

        public bool CanHandle(FileEntry entry) => canHandle;

        public PreviewResult Load(FileEntry entry, PreviewBudget budget) => new(null, [], null);
    }

    private sealed class ThrowingProvider : IPreviewProvider
    {
        private readonly bool cancel;

        public ThrowingProvider(bool cancel = false)
        {
            this.cancel = cancel;
        }

        public string Id => "throwing";

        public int Priority => 0;

        public IReadOnlySet<string> Extensions => new HashSet<string>(StringComparer.Ordinal) { ".txt" };

        public bool CanHandle(FileEntry entry) => true;

        public PreviewResult Load(FileEntry entry, PreviewBudget budget) =>
            cancel ? throw new OperationCanceledException() : throw new InvalidOperationException("boom");
    }
}
