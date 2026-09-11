using System.Text;

namespace MikrotikInstaller.Core.Connectivity.Binary;

/// <summary>
/// Kodierung/Dekodierung des RouterOS-API-Wortprotokolls: Jedes "Wort" ist längenpräfixiert, ein "Satz"
/// (Sentence) ist eine Folge von Wörtern, terminiert durch ein Wort der Länge 0.
/// Siehe MikroTik-Dokumentation "API" für die genaue Längen-Kodierung.
/// </summary>
public static class RouterOsWireFormat
{
    public static async Task WriteSentenceAsync(Stream stream, IEnumerable<string> words, CancellationToken cancellationToken)
    {
        foreach (var word in words)
        {
            await WriteWordAsync(stream, word, cancellationToken);
        }

        await WriteWordAsync(stream, string.Empty, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }

    public static async Task<List<string>> ReadSentenceAsync(Stream stream, CancellationToken cancellationToken)
    {
        var words = new List<string>();
        while (true)
        {
            var word = await ReadWordAsync(stream, cancellationToken);
            if (word is null)
            {
                return words;
            }

            words.Add(word);
        }
    }

    private static async Task WriteWordAsync(Stream stream, string word, CancellationToken cancellationToken)
    {
        var bytes = Encoding.UTF8.GetBytes(word);
        var lengthBytes = EncodeLength(bytes.Length);
        await stream.WriteAsync(lengthBytes, cancellationToken);
        if (bytes.Length > 0)
        {
            await stream.WriteAsync(bytes, cancellationToken);
        }
    }

    /// <summary>Liest ein Wort. Gibt <c>null</c> zurück, wenn es sich um den Satz-Terminator (Länge 0) handelt.</summary>
    private static async Task<string?> ReadWordAsync(Stream stream, CancellationToken cancellationToken)
    {
        var length = await ReadLengthAsync(stream, cancellationToken);
        if (length == 0)
        {
            return null;
        }

        var buffer = new byte[length];
        await ReadExactAsync(stream, buffer, cancellationToken);
        return Encoding.UTF8.GetString(buffer);
    }

    internal static byte[] EncodeLength(int length)
    {
        if (length < 0x80)
        {
            return [(byte)length];
        }

        if (length < 0x4000)
        {
            return [(byte)((length >> 8) | 0x80), (byte)length];
        }

        if (length < 0x200000)
        {
            return [(byte)((length >> 16) | 0xC0), (byte)(length >> 8), (byte)length];
        }

        if (length < 0x10000000)
        {
            return [(byte)((length >> 24) | 0xE0), (byte)(length >> 16), (byte)(length >> 8), (byte)length];
        }

        return [0xF0, (byte)(length >> 24), (byte)(length >> 16), (byte)(length >> 8), (byte)length];
    }

    private static async Task<int> ReadLengthAsync(Stream stream, CancellationToken cancellationToken)
    {
        var firstByte = await ReadByteAsync(stream, cancellationToken);

        if ((firstByte & 0x80) == 0x00)
        {
            return firstByte;
        }

        if ((firstByte & 0xC0) == 0x80)
        {
            var b2 = await ReadByteAsync(stream, cancellationToken);
            return ((firstByte & 0x3F) << 8) | b2;
        }

        if ((firstByte & 0xE0) == 0xC0)
        {
            var b2 = await ReadByteAsync(stream, cancellationToken);
            var b3 = await ReadByteAsync(stream, cancellationToken);
            return ((firstByte & 0x1F) << 16) | (b2 << 8) | b3;
        }

        if ((firstByte & 0xF0) == 0xE0)
        {
            var b2 = await ReadByteAsync(stream, cancellationToken);
            var b3 = await ReadByteAsync(stream, cancellationToken);
            var b4 = await ReadByteAsync(stream, cancellationToken);
            return ((firstByte & 0x0F) << 24) | (b2 << 16) | (b3 << 8) | b4;
        }

        var lengthBytes = new byte[4];
        await ReadExactAsync(stream, lengthBytes, cancellationToken);
        return (lengthBytes[0] << 24) | (lengthBytes[1] << 16) | (lengthBytes[2] << 8) | lengthBytes[3];
    }

    private static async Task<byte> ReadByteAsync(Stream stream, CancellationToken cancellationToken)
    {
        var buffer = new byte[1];
        await ReadExactAsync(stream, buffer, cancellationToken);
        return buffer[0];
    }

    private static async Task ReadExactAsync(Stream stream, byte[] buffer, CancellationToken cancellationToken)
    {
        var offset = 0;
        while (offset < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(offset), cancellationToken);
            if (read == 0)
            {
                throw new IOException("Die Verbindung wurde vom Gerät geschlossen.");
            }

            offset += read;
        }
    }
}
