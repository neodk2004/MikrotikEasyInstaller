using System.Net;

namespace MikrotikInstaller.Core.Configuration;

internal static class IpNetworkHelper
{
    /// <summary>Berechnet aus "192.168.88.1/24" die Netzwerkadresse "192.168.88.0/24".</summary>
    public static string GetNetworkCidr(string addressCidr)
    {
        var parts = addressCidr.Split('/');
        var address = IPAddress.Parse(parts[0]);
        var prefixLength = int.Parse(parts[1]);

        var addressBytes = address.GetAddressBytes();
        var mask = CreateMask(prefixLength);
        var networkBytes = new byte[4];
        for (var i = 0; i < 4; i++)
        {
            networkBytes[i] = (byte)(addressBytes[i] & mask[i]);
        }

        return $"{new IPAddress(networkBytes)}/{prefixLength}";
    }

    private static byte[] CreateMask(int prefixLength)
    {
        var maskValue = prefixLength == 0 ? 0u : 0xFFFFFFFFu << (32 - prefixLength);
        return
        [
            (byte)((maskValue >> 24) & 0xFF),
            (byte)((maskValue >> 16) & 0xFF),
            (byte)((maskValue >> 8) & 0xFF),
            (byte)(maskValue & 0xFF),
        ];
    }
}
