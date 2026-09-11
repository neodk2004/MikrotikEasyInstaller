using System.Runtime.Versioning;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace MikrotikInstaller.Core.Export;

/// <summary>
/// Druckt die Einrichtung als ein einseitiges PDF: alle wichtigen Daten (Internet-Zugang,
/// Heimnetzwerk, VLANs, WLAN inkl. QR-Code, Firewall) zum Aufbewahren/Ausdrucken.
/// Windows-only: liest Systemschriften aus dem Windows-Fonts-Ordner und nutzt GDI+ zur
/// Bildnormalisierung — passend zur reinen Windows-Desktop-App.
/// </summary>
[SupportedOSPlatform("windows")]
public static class SummaryPdfExporter
{
    private const double MarginLeft = 40;
    private const double MarginTop = 40;
    private const double MarginRight = 40;
    private const double MarginBottom = 40;
    private const double RightColumnWidth = 190;
    private const double ColumnGap = 24;

    static SummaryPdfExporter()
    {
        GlobalFontSettings.FontResolver ??= new WindowsSegoeUiFontResolver();
    }

    public static void Export(SummaryDocumentData data, string filePath)
    {
        using var document = new PdfDocument();
        var page = document.AddPage();
        page.Size = PdfSharp.PageSize.A4;

        using var gfx = XGraphics.FromPdfPage(page);

        var titleFont = new XFont("Segoe UI", 19, XFontStyleEx.Bold);
        var subtitleFont = new XFont("Segoe UI", 9, XFontStyleEx.Regular);
        var sectionFont = new XFont("Segoe UI", 12, XFontStyleEx.Bold);
        var textFont = new XFont("Segoe UI", 10, XFontStyleEx.Regular);
        var wifiValueFont = new XFont("Segoe UI", 15, XFontStyleEx.Bold);
        var wifiLabelFont = new XFont("Segoe UI", 9, XFontStyleEx.Regular);

        var pageWidth = page.Width.Point;
        var pageHeight = page.Height.Point;
        var leftColumnWidth = pageWidth - MarginLeft - MarginRight - RightColumnWidth - ColumnGap;
        var rightX = pageWidth - MarginRight - RightColumnWidth;

        double y = MarginTop;

        gfx.DrawString("MikroTik Einrichtungsprotokoll", titleFont, XBrushes.Black, MarginLeft, y + titleFont.Height);
        y += titleFont.Height + 4;

        var subtitle = $"{data.DeviceIdentityName} ({data.DeviceModel}) — erstellt am {DateTime.Now:dd.MM.yyyy HH:mm}";
        gfx.DrawString(subtitle, subtitleFont, XBrushes.Gray, MarginLeft, y + subtitleFont.Height);
        y += subtitleFont.Height + 14;

        gfx.DrawLine(XPens.LightGray, MarginLeft, y, pageWidth - MarginRight, y);
        y += 20;

        var contentTop = y;
        var leftY = contentTop;

        if (data.Wan is { } wan)
        {
            leftY = DrawSection(gfx, "Internet-Zugang", BuildWanLines(wan), MarginLeft, leftY, leftColumnWidth, sectionFont, textFont);
        }

        if (data.Lan is { } lan)
        {
            leftY = DrawSection(gfx, "Heimnetzwerk", BuildLanLines(lan), MarginLeft, leftY, leftColumnWidth, sectionFont, textFont);
        }

        if (data.Vlans.Count > 0)
        {
            leftY = DrawSection(gfx, "Zusätzliche Netzwerke", BuildVlanLines(data.Vlans), MarginLeft, leftY, leftColumnWidth, sectionFont, textFont);
        }

        if (data.Firewall is { } firewall)
        {
            leftY = DrawSection(gfx, "Firewall", BuildFirewallLines(firewall), MarginLeft, leftY, leftColumnWidth, sectionFont, textFont);
        }

        if (data.Wireless is { } wireless)
        {
            DrawWirelessBox(gfx, wireless, rightX, contentTop, RightColumnWidth, sectionFont, wifiLabelFont, wifiValueFont, subtitleFont);
        }

        if (data.BackupName is not null)
        {
            var footerText = $"Sicherung des vorherigen Zustands auf dem Gerät gespeichert unter: {data.BackupName}";
            gfx.DrawString(footerText, subtitleFont, XBrushes.Gray, MarginLeft, pageHeight - MarginBottom + subtitleFont.Height);
        }

        document.Save(filePath);
    }

    private static void DrawWirelessBox(
        XGraphics gfx, Configuration.WirelessSettings wireless, double x, double y, double width,
        XFont sectionFont, XFont labelFont, XFont valueFont, XFont captionFont)
    {
        gfx.DrawString("WLAN", sectionFont, XBrushes.Black, x, y + sectionFont.Height);
        y += sectionFont.Height + 12;

        gfx.DrawString("Name (SSID)", labelFont, XBrushes.Gray, x, y + labelFont.Height);
        y += labelFont.Height + 2;
        gfx.DrawString(wireless.Ssid, valueFont, XBrushes.Black, x, y + valueFont.Height);
        y += valueFont.Height + 12;

        gfx.DrawString("Passwort", labelFont, XBrushes.Gray, x, y + labelFont.Height);
        y += labelFont.Height + 2;
        gfx.DrawString(wireless.Password, valueFont, XBrushes.Black, x, y + valueFont.Height);
        y += valueFont.Height + 16;

        var qrBytes = WifiQrCodeGenerator.TryGeneratePng(wireless.Ssid, wireless.Password);
        if (qrBytes is null)
        {
            return;
        }

        // PDFsharp 6 lehnt manche PNG-Varianten ab (u. a. die von QRCoder erzeugten) — über GDI+
        // (System.Drawing, ohnehin transitive Abhängigkeit von QRCoder) nach BMP normalisieren,
        // das PDFsharp zuverlässig liest.
        using var bmpStream = ConvertToBitmapStream(qrBytes);
        using var qrImage = XImage.FromStream(bmpStream);
        gfx.DrawImage(qrImage, x, y, width, width);
        y += width + 6;

        gfx.DrawString("QR-Code zum direkten WLAN-Beitritt", captionFont, XBrushes.Gray, new XRect(x, y, width, 24), XStringFormats.TopLeft);
    }

    private static MemoryStream ConvertToBitmapStream(byte[] pngBytes)
    {
        using var pngStream = new MemoryStream(pngBytes);
        using var bitmap = System.Drawing.Image.FromStream(pngStream);
        var bmpStream = new MemoryStream();
        bitmap.Save(bmpStream, System.Drawing.Imaging.ImageFormat.Bmp);
        bmpStream.Position = 0;
        return bmpStream;
    }

    private static double DrawSection(
        XGraphics gfx, string title, IReadOnlyList<string> lines, double x, double y, double width,
        XFont sectionFont, XFont textFont)
    {
        y += 10;
        gfx.DrawString(title, sectionFont, XBrushes.Black, x, y + sectionFont.Height);
        y += sectionFont.Height + 8;

        const double lineHeight = 15;
        foreach (var line in lines)
        {
            y = DrawWrappedText(gfx, line, textFont, XBrushes.Black, x, y, width, lineHeight);
        }

        return y;
    }

    private static double DrawWrappedText(XGraphics gfx, string text, XFont font, XBrush brush, double x, double y, double maxWidth, double lineHeight)
    {
        var words = text.Split(' ');
        var line = string.Empty;

        foreach (var word in words)
        {
            var candidate = line.Length == 0 ? word : $"{line} {word}";
            if (gfx.MeasureString(candidate, font).Width > maxWidth && line.Length > 0)
            {
                gfx.DrawString(line, font, brush, x, y + font.Height);
                y += lineHeight;
                line = word;
            }
            else
            {
                line = candidate;
            }
        }

        if (line.Length > 0)
        {
            gfx.DrawString(line, font, brush, x, y + font.Height);
            y += lineHeight;
        }

        return y;
    }

    private static List<string> BuildWanLines(Configuration.WanSettings wan)
    {
        var lines = new List<string> { $"Anschluss: {wan.InterfaceName}" };
        lines.Add(wan.Mode == Configuration.WanAddressMode.Dhcp
            ? "Adresse: automatisch (DHCP)"
            : $"Feste Adresse: {wan.StaticAddressCidr} (Gateway {wan.StaticGateway})");
        return lines;
    }

    private static List<string> BuildLanLines(Configuration.LanSettings lan) =>
    [
        $"Netzwerk: {Configuration.IpNetworkHelper.GetNetworkCidr(lan.RouterAddressCidr)}",
        $"Router-Adresse: {lan.RouterAddressCidr}",
        $"Adressbereich: {lan.DhcpPoolStart} – {lan.DhcpPoolEnd}",
        $"Anschlüsse: {string.Join(", ", lan.MemberInterfaces)}",
        $"DNS: {lan.PrimaryDnsServer}{(lan.SecondaryDnsServer is null ? string.Empty : $", {lan.SecondaryDnsServer}")}",
    ];

    private static List<string> BuildVlanLines(IReadOnlyList<Configuration.VlanDefinition> vlans) =>
        vlans.Select(v => $"{v.Name} (VLAN {v.VlanId}): {Configuration.IpNetworkHelper.GetNetworkCidr(v.RouterAddressCidr)}, Adressbereich {v.DhcpPoolStart}–{v.DhcpPoolEnd}").ToList();

    private static List<string> BuildFirewallLines(Configuration.FirewallSettings firewall)
    {
        var lines = new List<string> { "Router aus dem Internet nicht direkt erreichbar." };
        lines.AddRange(firewall.PortForwards.Select(p =>
            $"Portfreigabe „{p.Name}\": {p.ExternalPort}/{p.Protocol.ToUpperInvariant()} → {p.TargetAddress}:{p.InternalPort}"));
        return lines;
    }
}
