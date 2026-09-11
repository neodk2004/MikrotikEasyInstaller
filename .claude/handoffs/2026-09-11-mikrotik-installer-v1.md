# Session Handoff: MikroTik Installer — MVP fertiggestellt

**Datum:** 2026-09-11
**Projekt:** T:\CC-Projekte\Mikrotik-Installer (Gitea: `admin/Mikrotik-Installer`, `ssh://git@192.168.1.222:2222`)
**Sitzungsdauer:** ~1 Tag, sehr umfangreich (Neuaufbau von Null bis MVP + Distribution)

## Aktueller Stand

**Aufgabe:** Windows-Programm, das MikroTik-Router/-Switches per geführtem Interview (Wizard) für Laien einrichtet — Grundeinrichtung (WAN, DHCP, VLAN, WLAN, Firewall), ohne RouterOS-Kenntnisse.
**Phase:** MVP fertig, alle 8 ursprünglich geplanten Phasen abgeschlossen + mehrere Erweiterungen unterwegs.
**Fortschritt:** Voll funktionsfähiger 9-Schritte-Assistent, als portable Single-File-exe baubar, mit Icon und README. Läuft nachweislich gegen ein simuliertes Gerät (per UI-Automation von mir mehrfach durchgetestet) und wurde vom Nutzer teilweise gegen ein echtes Gerät getestet (siehe „Offene Fragen").

## Was wir gemacht haben

Von Null gestartet (leeres Verzeichnis): .NET-8/WPF-Projekt aufgesetzt, RouterOS-Verbindungsschicht (REST + Binary-API mit Fallback) gebaut, 9-Schritte-Wizard implementiert (Verbindung → Geräte-Erkennung → WAN → LAN → VLAN → WLAN → Firewall → Zusammenfassung → RouterOS-Update), komplett auf modernes Fluent-Design (WPF-UI) mit MikroTik-Markenfarbe umgestellt, PDF-Export mit WLAN-QR-Code ergänzt, Tooltips für alle Fachbegriffe eingebaut, und als eigenständige Windows-exe (67 MB, self-contained) mit eigenem Icon veröffentlicht. Laufend Bugs behoben, die beim echten Testen durch den Nutzer auftraten.

## Entscheidungen (mit Begründung)

- **.NET 8 / WPF statt Electron/Python** — native Windows-Optik, einfachste Distribution als eine exe.
- **REST-API (RouterOS 7+) mit Fallback auf Binary-API (Port 8728/8729)** — maximale Gerätekompatibilität; Binary-API nutzt nur den modernen einstufigen Login (RouterOS ≥ 6.43), ältere Geräte (EOL seit 2018) werden bewusst nicht unterstützt.
- **Backup nur auf dem Gerät selbst, kein SFTP-Download aufs PC** — Nutzer hat sich bewusst gegen den Mehraufwand (SSH.NET-Abhängigkeit) entschieden, als das zur Sprache kam. Bleibt eine mögliche spätere Erweiterung.
- **Simulierter Testzugang (`127.127.127.127` / `admin` / `admin`)** — auf Wunsch des Nutzers eingebaut, damit der komplette Wizard ohne echtes Gerät durchgeklickt werden kann. Nur in Debug-Builds aktiv (`#if DEBUG` in `RouterOsClientFactory`), in Release-Builds nicht erreichbar — bewusst so gebaut, damit kein Test-Zugang in der an Endnutzer verteilten Version landet.
- **Self-contained Single-File-exe statt framework-dependent** — Nutzer hat sich explizit für "garantiert lauffähig ohne Vorbedingung" (67 MB) statt "klein, aber .NET-Runtime-Installation nötig" (5-10 MB) entschieden.
- **Kein Trimming (`PublishTrimmed`)** — bei WPF wegen starker Reflection-Nutzung riskant (kann zur Laufzeit an ungetesteten Stellen brechen). Stattdessen nur `SatelliteResourceLanguages=de;en` (sicher, spart trotzdem ~5 MB komprimiert).
- **Eigenes Icon statt MikroTik-Logo** — Markenrechte; stattdessen ein neutrales, selbst gezeichnetes Netzwerk-Symbol in MikroTik-Grün.
- **Gitea-Actions-Workflow bleibt liegen, obwohl kein Runner konfiguriert ist** — schadet nicht, lässt sich bei Bedarf später ohne Änderung nutzen. Stattdessen `build-release.ps1` als lokale Alternative.

## Code-Struktur

```
src/
  MikrotikInstaller.App/          WPF-UI, MVVM (Views/ViewModels je Wizard-Schritt)
  MikrotikInstaller.Core/         RouterOS-Clients, Konfigurationslogik, PDF-Export, Updates
  MikrotikInstaller.Core.Tests/   xUnit, 38 Tests, alle grün
```

**Wichtigste Dateien:**
- `src/MikrotikInstaller.Core/Connectivity/RouterOsClientFactory.cs` — Verbindungsaufbau (REST → Binary-Fallback → Demo bei Test-Zugangsdaten)
- `src/MikrotikInstaller.Core/Connectivity/Demo/DemoRouterOsClient.cs` — Simulator, Debug-only
- `src/MikrotikInstaller.Core/Configuration/*Configurator.cs` — je ein Configurator pro Feature (Wan/Lan/Vlan/Wireless/Firewall), erzeugt sowohl die technischen RouterOS-Befehle als auch `BuildFriendlySummary(...)` für die laientaugliche Anzeige
- `src/MikrotikInstaller.Core/Updates/RouterOsUpdateService.cs` — RouterOS-Update-Prüfung/-Start
- `src/MikrotikInstaller.Core/Export/SummaryPdfExporter.cs` — PDF-Export (PdfSharp + QRCoder)
- `src/MikrotikInstaller.App/ViewModels/MainViewModel.cs` — Wizard-Navigation, `ShouldSkip`-Mechanismus für überspringbare Schritte
- `src/MikrotikInstaller.App/ViewModels/WizardSession.cs` — über alle Schritte geteilter Zustand (Client, Device, Settings je Schritt)
- `src/MikrotikInstaller.App/Views/Controls/InfoHint.xaml(.cs)` — wiederverwendbares Tooltip-„?"-Symbol
- `build-release.ps1` — lokaler Release-Build ohne CI

## Bekannte Stolperfallen (bereits gelöst, aber gut zu wissen)

- **`Wpf.Ui.Controls.PasswordBox.Password`** bindet nicht automatisch zweiseitig — immer `Mode=TwoWay` explizit angeben, sonst bleibt der gebundene Wert leer, ohne Fehlermeldung.
- **WPF-`ToolTip`-Inhalte hängen nicht im normalen visuellen Baum** — `ElementName`-Bindungen darin laufen ins Leere. Lösung: Wert auf `Tag` des sichtbaren Elements binden, im ToolTip über `PlacementTarget.Tag` zurücklesen (siehe `InfoHint.xaml`).
- **PDFsharp 6.x löst Schriften nicht mehr automatisch über GDI auf** — eigener `IFontResolver` nötig (`WindowsSegoeUiFontResolver.cs`, liest Segoe UI direkt aus dem Windows-Fonts-Ordner).
- **QRCoder-PNGs wurden von PDFsharps PNG-Decoder abgelehnt** — Workaround: über GDI+ nach BMP normalisieren (`SummaryPdfExporter.ConvertToBitmapStream`).
- **Beim Bauen/Publishen schlägt der Build fehl, wenn die App gerade läuft** (`MSB3027`, Datei gesperrt) — laufende Instanz vorher beenden (`taskkill //IM MikroTik-Installer.exe //F` bzw. `//IM MikrotikInstaller.App.exe //F` je nach Build).
- **UI-Automation-Skripte in dieser Session** brauchten öfter einen zweiten `Weiter`-Klick bei Timing-Rennen (Feld noch nicht neu gerendert) — kein App-Bug, nur ein Automatisierungs-Artefakt.

## Offene Fragen

- [ ] **Noch nicht bestätigt: kompletter Durchlauf inkl. „Jetzt einrichten" (Apply) gegen ein echtes physisches Gerät.** Der Nutzer hat reale UI-Bugs beim Klicken durch den Wizard auf einem echten Switch gefunden (Passwort-Feld, Tooltip, WLAN-Überspringen — alle behoben), aber ob der tatsächliche Anwenden-Schritt (RouterOS-Befehle schreiben, Backup, PDF-Export, Update-Prüfung) auf einem echten Gerät sauber durchläuft, ist noch nicht verifiziert. Das wäre der wichtigste nächste Test.
- [ ] Kein Gitea-Actions-Runner konfiguriert — Workflow-Datei liegt bereit, wird aber nie ausgelöst. Falls später ein Runner eingerichtet wird: Label `runs-on: ubuntu-latest` in `.gitea/workflows/build.yml` ggf. anpassen.
- [ ] Phase 7 „Feinschliff" ist nur teilweise abgeschlossen: Tooltips ✅, aber **bessere Eingabevalidierung mit klareren Fehlermeldungen** (z. B. Live-Hinweis bei ungültigem IP-Format) war ursprünglich mit angedacht und wurde nicht mehr angegangen.
- [ ] Backup-Download aufs PC (SFTP) bewusst zurückgestellt — falls das später gewünscht wird, siehe „Entscheidungen" oben.

## Kontext zum Merken

- **Gitea-Zugang:** SSH-Key `~/.ssh/id_ed25519` (Kommentar „onlypets@gitea", authentifiziert aber tatsächlich als Konto **`admin`**). Host `192.168.1.222`, SSH-Port `2222`. Remote: `ssh://git@192.168.1.222:2222/admin/Mikrotik-Installer.git`. „Push-to-create" ist auf der Instanz deaktiviert — neue Repos müssen im Web-UI vorher angelegt werden.
- **MikroTik-Markenfarben** (aus mikrotik.com-CSS extrahiert): Grün `#009245` (Hauptakzent), Schwarz `#0e0e10`, Blau `#1961bc`, Grauskala `#f1f1f2`…`#2c2c2e`. Als `MikrotikAccentBrush` etc. in `App.xaml` hinterlegt.
- **Theme:** Folgt automatisch dem Windows-Hell/Dunkel-Modus (`ApplicationThemeManager` + `SystemThemeWatcher`), MikroTik-Grün als Akzent statt der Windows-Systemfarbe.
- **Nutzer testet gerne selbst live** — mehrfach in dieser Sitzung hat er reale Bugs gefunden, die ich nur per UI-Automation-Screenshots nicht aufgedeckt hätte (z. B. Tooltip-Binding, PasswordBox-Binding). Für zukünftige Änderungen: nach jedem funktionalen Fix aktiv zum eigenen Testen einladen, nicht nur meine eigene Automation als ausreichend betrachten.
- **Paketversionen:** CommunityToolkit.Mvvm 8.4.2, WPF-UI 4.3.0, PDFsharp 6.2.4, QRCoder 1.8.0, .NET 8 SDK (9.0.318 lokal installiert, Zielframework bleibt net8.0/net8.0-windows).

## Nächste Schritte

1. [ ] Nutzer bittet, einmal den kompletten Assistenten inkl. „Jetzt einrichten" gegen ein echtes (Test-)Gerät durchzuspielen — größter verbleibender Validierungs-Gap.
2. [ ] Falls dabei Probleme auftreten: gezielt nachfragen, was genau schiefging (wie bisher — die konkreten Bug-Reports des Nutzers waren jedes Mal sehr hilfreich).
3. [ ] Bei Bedarf: Eingabevalidierung mit Live-Fehlermeldungen ergänzen (Rest von Phase 7).
4. [ ] Bei Bedarf: SFTP-Backup-Download aufs PC nachrüsten.
5. [ ] Falls ein Gitea-Actions-Runner eingerichtet wird: Workflow testen und `runs-on`-Label anpassen.

## Dateien zum Wiedereinstieg

- `README.md` — Projektüberblick, Bau-Anleitung, Architektur
- `src/MikrotikInstaller.App/ViewModels/MainViewModel.cs` — Einstieg in die Wizard-Logik
- `src/MikrotikInstaller.App/ViewModels/WizardSession.cs` — zentraler Zustand über alle Schritte
- `src/MikrotikInstaller.Core/Connectivity/RouterOsClientFactory.cs` — Verbindungsaufbau, inkl. Demo-Fallback
- `build-release.ps1` — Release-Build lokal auslösen
