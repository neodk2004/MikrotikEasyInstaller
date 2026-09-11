namespace MikrotikInstaller.Core.Connectivity;

/// <summary>
/// Fehler bei der Kommunikation mit einem RouterOS-Gerät. Die Message ist bewusst laientauglich formuliert
/// und darf direkt im UI angezeigt werden — technische Details stehen in <see cref="Exception.InnerException"/>.
/// </summary>
public sealed class RouterOsException : Exception
{
    public RouterOsException(string userFriendlyMessage, Exception? inner = null)
        : base(userFriendlyMessage, inner)
    {
    }
}
