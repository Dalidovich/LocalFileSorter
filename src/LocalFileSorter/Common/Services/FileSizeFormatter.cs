using System.Globalization;

using LocalFileSorter.Common.Localization;

namespace LocalFileSorter.Common.Services;

public static class FileSizeFormatter
{
    private const double Kilobyte = 1024d;
    private const double Megabyte = Kilobyte * 1024d;
    private const double Gigabyte = Megabyte * 1024d;

    public static string Format(long bytes, Strings strings)
    {
        if (bytes < Kilobyte)
        {
            return string.Format(CultureInfo.InvariantCulture, strings.SizeBytes, bytes);
        }

        if (bytes < Megabyte)
        {
            return Scaled(strings.SizeKilobytes, bytes / Kilobyte);
        }

        return bytes < Gigabyte
            ? Scaled(strings.SizeMegabytes, bytes / Megabyte)
            : Scaled(strings.SizeGigabytes, bytes / Gigabyte);
    }

    private static string Scaled(string format, double value) =>
        string.Format(CultureInfo.InvariantCulture, format, value.ToString("0.#", CultureInfo.InvariantCulture));
}
