namespace LocalFileSorter.Common.Abstractions;

public sealed record PreviewBudget(long MaxBytes, int MaxImagePixels, CancellationToken Ct)
{
    public const long DefaultMaxBytes = 256L * 1024L;

    public const int DefaultMaxImagePixels = 40_000_000;

    public static PreviewBudget Default(CancellationToken ct) =>
        new(DefaultMaxBytes, DefaultMaxImagePixels, ct);
}
