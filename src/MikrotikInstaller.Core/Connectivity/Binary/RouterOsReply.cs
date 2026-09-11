namespace MikrotikInstaller.Core.Connectivity.Binary;

internal enum RouterOsReplyStatus
{
    /// <summary>"!re" — eine Ergebniszeile (z. B. ein Listeneintrag bei "print").</summary>
    Row,

    /// <summary>"!done" — der Befehl ist abgeschlossen.</summary>
    Done,

    /// <summary>"!trap" oder "!fatal" — der Befehl ist fehlgeschlagen.</summary>
    Trap,
}

internal sealed record RouterOsReply(RouterOsReplyStatus Status, IReadOnlyDictionary<string, string> Attributes);
