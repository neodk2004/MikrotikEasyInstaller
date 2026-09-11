using QRCoder;

namespace MikrotikInstaller.Core.Export;

/// <summary>Erzeugt einen QR-Code (PNG), den Handys als "WLAN beitreten" erkennen.</summary>
public static class WifiQrCodeGenerator
{
    /// <summary>Liefert die PNG-Bilddaten des QR-Codes, oder <c>null</c>, wenn die Erzeugung fehlschlägt
    /// (z. B. weil SSID/Passwort für den QR-Code-Standard ungeeignet sind) — der Rest des Dokuments
    /// soll auch dann funktionieren, der QR-Code ist ein Nice-to-have.</summary>
    public static byte[]? TryGeneratePng(string ssid, string password, int pixelsPerModule = 12)
    {
        try
        {
            var payload = new PayloadGenerator.WiFi(ssid, password, PayloadGenerator.WiFi.Authentication.WPA, isHiddenSSID: false);
            using var generator = new QRCodeGenerator();
            using var qrData = generator.CreateQrCode(payload);
            var pngQrCode = new PngByteQRCode(qrData);
            return pngQrCode.GetGraphic(pixelsPerModule);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
