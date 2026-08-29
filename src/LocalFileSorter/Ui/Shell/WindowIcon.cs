using SFML.Graphics;
using SFML.System;

namespace LocalFileSorter.Ui.Shell;

internal static class WindowIcon
{
    private const string Resource = "app.ico";
    private const uint PreferredSide = 64u;
    private const int DirectorySize = 6;
    private const int EntrySize = 16;
    private const int DibHeaderSize = 40;
    private const int BytesPerPixel = 4;

    public static void Apply(RenderWindow window)
    {
        byte[]? file = ReadResource();
        if (file is null || !TrySelectFrame(file, out int offset, out uint side))
        {
            return;
        }

        window.SetIcon(new Vector2u(side, side), DecodeFrame(file, offset, (int)side));
    }

    private static byte[]? ReadResource()
    {
        using Stream? stream = typeof(WindowIcon).Assembly.GetManifestResourceStream(Resource);
        if (stream is null)
        {
            return null;
        }

        using MemoryStream buffer = new();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }

    private static bool TrySelectFrame(byte[] file, out int offset, out uint side)
    {
        offset = 0;
        side = 0;

        if (file.Length < DirectorySize)
        {
            return false;
        }

        int count = BitConverter.ToUInt16(file, 4);
        for (int index = 0; index < count; index++)
        {
            int entry = DirectorySize + (index * EntrySize);
            if (entry + EntrySize > file.Length)
            {
                break;
            }

            uint candidate = file[entry] == 0 ? 256u : file[entry];
            int candidateOffset = BitConverter.ToInt32(file, entry + 12);
            int candidateLength = BitConverter.ToInt32(file, entry + 8);
            if (!IsSupportedDib(file, candidateOffset, candidateLength, candidate))
            {
                continue;
            }

            if (side == 0 || IsCloser(candidate, side))
            {
                offset = candidateOffset;
                side = candidate;
            }
        }

        return side != 0;
    }

    private static bool IsCloser(uint candidate, uint current) =>
        current < PreferredSide ? candidate > current : candidate >= PreferredSide && candidate < current;

    private static bool IsSupportedDib(byte[] file, int offset, int length, uint side)
    {
        if (offset < 0 || length < DibHeaderSize || offset > file.Length - length)
        {
            return false;
        }

        if (BitConverter.ToInt32(file, offset) != DibHeaderSize
            || BitConverter.ToInt32(file, offset + 4) != side
            || BitConverter.ToInt32(file, offset + 8) != side * 2
            || BitConverter.ToUInt16(file, offset + 14) != 32
            || BitConverter.ToInt32(file, offset + 16) != 0)
        {
            return false;
        }

        return length >= DibHeaderSize + (side * side * BytesPerPixel);
    }

    private static byte[] DecodeFrame(byte[] file, int offset, int side)
    {
        int stride = side * BytesPerPixel;
        byte[] rgba = new byte[side * stride];
        int start = offset + DibHeaderSize;

        for (int y = 0; y < side; y++)
        {
            int source = start + ((side - 1 - y) * stride);
            int target = y * stride;
            for (int x = 0; x < side; x++)
            {
                rgba[target] = file[source + 2];
                rgba[target + 1] = file[source + 1];
                rgba[target + 2] = file[source];
                rgba[target + 3] = file[source + 3];
                source += BytesPerPixel;
                target += BytesPerPixel;
            }
        }

        return rgba;
    }
}
