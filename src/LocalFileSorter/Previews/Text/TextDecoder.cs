using System.Text;

namespace LocalFileSorter.Previews.Text;

public static class TextDecoder
{
    private static readonly byte[] Utf32LeBom = [0xFF, 0xFE, 0x00, 0x00];
    private static readonly byte[] Utf32BeBom = [0x00, 0x00, 0xFE, 0xFF];
    private static readonly byte[] Utf8Bom = [0xEF, 0xBB, 0xBF];
    private static readonly byte[] Utf16LeBom = [0xFF, 0xFE];
    private static readonly byte[] Utf16BeBom = [0xFE, 0xFF];

    public static DecodedText Decode(ReadOnlySpan<byte> bytes, bool truncated)
    {
        if (TryDecodeWithBom(bytes, out DecodedText? withBom))
        {
            return withBom;
        }

        ReadOnlySpan<byte> candidate = truncated ? TrimIncompleteUtf8Tail(bytes) : bytes;
        if (TryDecodeStrictUtf8(candidate, out string? utf8))
        {
            return new DecodedText(utf8, "UTF-8");
        }

        return new DecodedText(Encoding.Latin1.GetString(bytes), "Latin-1");
    }

    private static bool TryDecodeWithBom(ReadOnlySpan<byte> bytes, out DecodedText decoded)
    {
        if (bytes.StartsWith(Utf32LeBom))
        {
            decoded = new DecodedText(new UTF32Encoding(bigEndian: false, byteOrderMark: true).GetString(bytes[4..]), "UTF-32 LE");
            return true;
        }

        if (bytes.StartsWith(Utf32BeBom))
        {
            decoded = new DecodedText(new UTF32Encoding(bigEndian: true, byteOrderMark: true).GetString(bytes[4..]), "UTF-32 BE");
            return true;
        }

        if (bytes.StartsWith(Utf8Bom))
        {
            decoded = new DecodedText(Encoding.UTF8.GetString(bytes[3..]), "UTF-8 (BOM)");
            return true;
        }

        if (bytes.StartsWith(Utf16LeBom))
        {
            decoded = new DecodedText(Encoding.Unicode.GetString(bytes[2..]), "UTF-16 LE");
            return true;
        }

        if (bytes.StartsWith(Utf16BeBom))
        {
            decoded = new DecodedText(Encoding.BigEndianUnicode.GetString(bytes[2..]), "UTF-16 BE");
            return true;
        }

        decoded = null!;
        return false;
    }

    private static bool TryDecodeStrictUtf8(ReadOnlySpan<byte> bytes, out string text)
    {
        try
        {
            text = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true).GetString(bytes);
            return true;
        }
        catch (DecoderFallbackException)
        {
            text = string.Empty;
            return false;
        }
    }

    private static ReadOnlySpan<byte> TrimIncompleteUtf8Tail(ReadOnlySpan<byte> bytes)
    {
        int start = Math.Max(0, bytes.Length - 4);
        for (int index = bytes.Length - 1; index >= start; index--)
        {
            byte value = bytes[index];
            if ((value & 0b1100_0000) == 0b1000_0000)
            {
                continue;
            }

            int expected = SequenceLength(value);
            return expected == 0 || bytes.Length - index >= expected ? bytes : bytes[..index];
        }

        return bytes;
    }

    private static int SequenceLength(byte lead) => lead switch
    {
        < 0x80 => 1,
        >= 0xC0 and < 0xE0 => 2,
        >= 0xE0 and < 0xF0 => 3,
        >= 0xF0 and < 0xF8 => 4,
        _ => 0,
    };
}
