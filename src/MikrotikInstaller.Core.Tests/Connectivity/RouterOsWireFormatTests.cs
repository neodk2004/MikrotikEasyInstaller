using MikrotikInstaller.Core.Connectivity.Binary;

namespace MikrotikInstaller.Core.Tests.Connectivity;

public class RouterOsWireFormatTests
{
    [Theory]
    [InlineData("/login")]
    [InlineData("=name=admin")]
    [InlineData("")]
    [InlineData("x")]
    public async Task WriteAndReadSentence_RoundTripsSingleWord(string word)
    {
        await using var stream = new MemoryStream();
        await RouterOsWireFormat.WriteSentenceAsync(stream, word.Length == 0 ? [] : [word], CancellationToken.None);
        stream.Position = 0;

        var words = await RouterOsWireFormat.ReadSentenceAsync(stream, CancellationToken.None);

        if (word.Length == 0)
        {
            Assert.Empty(words);
        }
        else
        {
            Assert.Single(words);
            Assert.Equal(word, words[0]);
        }
    }

    [Fact]
    public async Task WriteAndReadSentence_RoundTripsMultipleWords()
    {
        string[] originalWords = ["/ip/address/add", "=address=192.168.88.1/24", "=interface=ether1"];

        await using var stream = new MemoryStream();
        await RouterOsWireFormat.WriteSentenceAsync(stream, originalWords, CancellationToken.None);
        stream.Position = 0;

        var words = await RouterOsWireFormat.ReadSentenceAsync(stream, CancellationToken.None);

        Assert.Equal(originalWords, words);
    }

    [Fact]
    public async Task WriteAndReadSentence_RoundTripsLongWord_UsesMultiByteLength()
    {
        // > 0x80 Bytes erzwingt die zweistufige Längenkodierung.
        var longValue = new string('a', 500);

        await using var stream = new MemoryStream();
        await RouterOsWireFormat.WriteSentenceAsync(stream, [longValue], CancellationToken.None);
        stream.Position = 0;

        var words = await RouterOsWireFormat.ReadSentenceAsync(stream, CancellationToken.None);

        Assert.Single(words);
        Assert.Equal(longValue, words[0]);
    }

    [Fact]
    public async Task ReadSentence_StopsAtSentenceTerminator_LeavesFollowingSentenceIntact()
    {
        await using var stream = new MemoryStream();
        await RouterOsWireFormat.WriteSentenceAsync(stream, ["!done"], CancellationToken.None);
        await RouterOsWireFormat.WriteSentenceAsync(stream, ["!re", "=name=ether1"], CancellationToken.None);
        stream.Position = 0;

        var first = await RouterOsWireFormat.ReadSentenceAsync(stream, CancellationToken.None);
        var second = await RouterOsWireFormat.ReadSentenceAsync(stream, CancellationToken.None);

        Assert.Equal(["!done"], first);
        Assert.Equal(["!re", "=name=ether1"], second);
    }
}
