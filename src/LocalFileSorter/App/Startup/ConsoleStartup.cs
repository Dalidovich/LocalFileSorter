using LocalFileSorter.Common.Localization;
using LocalFileSorter.Common.Services;

namespace LocalFileSorter.App.Startup;

public sealed class ConsoleStartup
{
    private const string QuitToken = "q";

    private readonly Strings strings;

    public ConsoleStartup(Strings strings)
    {
        this.strings = strings;
    }

    public StartupOptions? Prompt()
    {
        Console.WriteLine(strings.StartupBanner);
        Console.WriteLine(strings.StartupQuitHint);
        Console.WriteLine();

        string? source = PromptRoot(strings.StartupPromptSource, RootValidator.ValidateSource, null);
        if (source is null)
        {
            return null;
        }

        string? destination = PromptRoot(
            strings.StartupPromptDestination,
            RootValidator.ValidateDestination,
            source);

        return destination is null ? null : new StartupOptions(source, destination);
    }

    private string? PromptRoot(
        string label,
        Func<string?, RootValidation> validate,
        string? sourceRoot)
    {
        while (true)
        {
            Console.Write(label + ": ");
            string? input = Console.ReadLine();

            if (input is null || string.Equals(input.Trim(), QuitToken, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(strings.StartupAborted);
                return null;
            }

            RootValidation validation = validate(input);
            if (validation.IsValid && sourceRoot is not null)
            {
                validation = RootValidator.ValidatePair(sourceRoot, validation.FullPath);
            }

            if (validation.IsValid)
            {
                return validation.FullPath;
            }

            Console.WriteLine(Describe(validation));
        }
    }

    private string Describe(RootValidation validation) => validation.Problem switch
    {
        RootProblem.PathRequired => strings.ValidationPathRequired,
        RootProblem.NotFound => string.Format(strings.ValidationPathNotFound, validation.FullPath),
        RootProblem.NotReadable => string.Format(strings.ValidationPathNotReadable, validation.FullPath),
        RootProblem.NotWritable => string.Format(strings.ValidationPathNotWritable, validation.FullPath),
        RootProblem.RootsEqual => strings.ValidationRootsEqual,
        RootProblem.DestinationInsideSource => strings.ValidationDestinationInsideSource,
        _ => string.Empty,
    };
}
