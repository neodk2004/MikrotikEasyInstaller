using MikrotikInstaller.Core.Export;

namespace MikrotikInstaller.Core.Tests.Export;

public class WifiQrCodeGeneratorTests
{
    [Fact]
    public void TryGeneratePng_ValidCredentials_ReturnsPngBytes()
    {
        var png = WifiQrCodeGenerator.TryGeneratePng("Mein-WLAN", "sicheresPasswort123");

        Assert.NotNull(png);
        // PNG-Signatur: 89 50 4E 47 0D 0A 1A 0A
        Assert.Equal(0x89, png![0]);
        Assert.Equal((byte)'P', png[1]);
        Assert.Equal((byte)'N', png[2]);
        Assert.Equal((byte)'G', png[3]);
    }
}
