# MikroTik Installer

Ein Windows-Programm, mit dem sich MikroTik-Router/-Switches in wenigen, klar geführten Schritten einrichten lassen – auch ohne Netzwerk-Vorkenntnisse. Statt WinBox-Menüs und RouterOS-Fachbegriffen gibt es ein Interview: Verbindung herstellen, ein paar verständliche Fragen beantworten, fertig.

![Zusammenfassungs-Schritt des Assistenten](docs/screenshot.png)

## Was der Assistent einrichtet

Schritt für Schritt, mit Erklär-Tooltips (ⓘ) bei jedem Fachbegriff:

1. **Verbindung** – IP-Adresse, Benutzername, Passwort
2. **Geräte-Erkennung** – liest Modell, RouterOS-Version und Schnittstellen aus
3. **Internet-Zugang** – WAN-Schnittstelle, DHCP oder feste IP, NAT
4. **Heimnetzwerk (DHCP)** – Bridge, IP-Bereich, DHCP-Server, DNS
5. **VLANs** *(optional)* – zusätzliche, getrennte Netzwerke (z. B. Gäste, IoT)
6. **WLAN** *(übersprungen, wenn nicht vorhanden – oder bewusst abwählbar, z. B. bei einem reinen Switch)*
7. **Firewall** – sichere Basisregeln, optionale Portfreigaben
8. **Zusammenfassung** – alle geplanten Änderungen im Klartext, automatisches Backup auf dem Gerät vor dem Anwenden, **PDF-Export** mit allen Zugangsdaten inkl. WLAN-QR-Code zum Ausdrucken/Aufbewahren
9. **RouterOS-Update** *(optional)* – prüft auf eine neuere RouterOS-Version und kann sie mit deutlicher Warnung starten

Nichts wird am Gerät verändert, bevor der Nutzer in der Zusammenfassung ausdrücklich auf „Jetzt einrichten“ klickt.

## Für Nutzer

- Windows 10/11, keine Installation nötig
- Einfach `MikroTik-Installer.exe` herunterladen und starten – eine einzelne, eigenständige Datei (~67 MB), kein separates .NET muss installiert werden
- Portable: läuft von jedem Ordner/USB-Stick, schreibt nichts in Registry oder AppData

## Für Entwickler

### Voraussetzungen

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Bauen & Testen

```powershell
dotnet build
dotnet test src/MikrotikInstaller.Core.Tests/MikrotikInstaller.Core.Tests.csproj
dotnet run --project src/MikrotikInstaller.App
```

### Release-Build (eine einzelne .exe)

```powershell
.\build-release.ps1
```

Erzeugt `src/MikrotikInstaller.App/bin/Release/net8.0-windows/win-x64/publish/MikroTik-Installer.exe` – self-contained, single-file, ohne .NET-Runtime-Installation beim Nutzer.

Es gibt außerdem einen Gitea-Actions-Workflow (`.gitea/workflows/build.yml`), der das bei jedem Push automatisch übernimmt – vorausgesetzt, ein passender Runner ist auf der Gitea-Instanz registriert.

### Interner Testzugang (nur Debug-Builds)

Im Verbindungs-Schritt aktiviert die Kombination `127.127.127.127` / `admin` / `admin` einen komplett simulierten Router (kein echtes Gerät nötig, keine Netzwerkverbindung) – damit lässt sich der gesamte Assistent gefahrlos durchklicken. Dieser Zugang ist per `#if DEBUG` ausgeschlossen und existiert in veröffentlichten Release-Builds nicht.

### Projektstruktur

```
src/
  MikrotikInstaller.App/          WPF-UI (Views, ViewModels, Wizard-Shell)
  MikrotikInstaller.Core/         RouterOS-Clients, Konfigurationslogik, PDF-Export
  MikrotikInstaller.Core.Tests/   Unit-Tests für Core (xUnit)
```

**Core** kapselt die gesamte RouterOS-Kommunikation hinter `IRouterOsClient` – wahlweise über die REST-API (RouterOS 7+) oder die binäre API (Port 8728/8729, ältere Geräte), mit automatischem Fallback. Jede Konfigurations-Funktion (WAN, LAN, VLAN, WLAN, Firewall) erzeugt sowohl die auszuführenden RouterOS-Befehle als auch eine laienverständliche Klartext-Zusammenfassung.

**App** ist reines MVVM: Ein generischer Wizard-Shell (`MainViewModel`) navigiert durch eine Liste von Schritt-ViewModels; Schritte ohne Inhalt für das jeweilige Gerät (z. B. WLAN ohne WLAN-Hardware) werden automatisch übersprungen.

### Verwendete Bibliotheken

- [WPF-UI](https://github.com/lepoco/wpfui) – modernes Fluent-Design für WPF
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) – MVVM-Grundgerüst
- [PDFsharp](https://github.com/empira/PDFsharp) – PDF-Erzeugung
- [QRCoder](https://github.com/codebude/QRCoder) – WLAN-QR-Code

Die MikroTik-Markenfarbe (`#009245`) wurde aus der offiziellen Website übernommen; das App-Icon ist ein eigenes, neutrales Netzwerk-Symbol (kein MikroTik-Logo).
