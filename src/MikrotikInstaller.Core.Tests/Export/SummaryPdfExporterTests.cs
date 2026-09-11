using MikrotikInstaller.Core.Configuration;
using MikrotikInstaller.Core.Export;

namespace MikrotikInstaller.Core.Tests.Export;

public class SummaryPdfExporterTests
{
    [Fact]
    public void Export_WritesSinglePageValidPdf()
    {
        var data = new SummaryDocumentData(
            DeviceIdentityName: "Test-Router",
            DeviceModel: "CHR",
            Wan: new WanSettings("ether1", WanAddressMode.Dhcp),
            Lan: new LanSettings("bridge-lan", ["ether2", "ether3"], "192.168.88.1/24", "192.168.88.10", "192.168.88.254", "1.1.1.1", "8.8.8.8"),
            Vlans: [new VlanDefinition(20, "Gäste", "192.168.20.1/24", "192.168.20.10", "192.168.20.254")],
            Wireless: new WirelessSettings("wlan1", "Mein-WLAN", "sicheresPasswort123"),
            Firewall: new FirewallSettings("ether1", "bridge-lan", [new PortForward("Spieleserver", "udp", 25565, "192.168.88.50", 25565)]),
            BackupName: "mikrotik-installer-20260101-120000");

        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
        try
        {
            SummaryPdfExporter.Export(data, path);

            Assert.True(File.Exists(path));
            var bytes = File.ReadAllBytes(path);
            Assert.True(bytes.Length > 500);
            Assert.Equal("%PDF"u8.ToArray(), bytes[..4]);

            // Eine einzelne Seite: "/Type/Page" (ohne "s") taucht nur für die eine Seite auf,
            // "/Count 1" bestätigt zusätzlich die Seitenzahl im Pages-Objekt.
            var text = System.Text.Encoding.Latin1.GetString(bytes);
            Assert.Contains("/Count 1", text);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void Export_WithoutWireless_DoesNotThrow()
    {
        var data = new SummaryDocumentData(
            DeviceIdentityName: "Test-Router",
            DeviceModel: "CHR",
            Wan: new WanSettings("ether1", WanAddressMode.Dhcp),
            Lan: null,
            Vlans: [],
            Wireless: null,
            Firewall: null,
            BackupName: null);

        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
        try
        {
            SummaryPdfExporter.Export(data, path);
            Assert.True(File.Exists(path));
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
