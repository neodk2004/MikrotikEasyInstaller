<#
    Baut MikroTik-Installer.exe als eigenständige Single-File-Anwendung für Windows x64.
    Kein Gitea-Actions-Runner nötig - einfach lokal ausführen:

        .\build-release.ps1
#>

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

Write-Host "Baue MikroTik Installer (Release, win-x64, self-contained) ..." -ForegroundColor Cyan

dotnet publish "$root\src\MikrotikInstaller.App\MikrotikInstaller.App.csproj" `
    --configuration Release `
    --runtime win-x64

$exePath = Join-Path $root "src\MikrotikInstaller.App\bin\Release\net8.0-windows\win-x64\publish\MikroTik-Installer.exe"

if (Test-Path $exePath) {
    $sizeMb = [math]::Round((Get-Item $exePath).Length / 1MB, 1)
    Write-Host ""
    Write-Host "Fertig: $exePath ($sizeMb MB)" -ForegroundColor Green
    Write-Host "Diese eine Datei kann direkt weitergegeben werden - keine Installation notwendig." -ForegroundColor Green
}
else {
    Write-Host "Build fehlgeschlagen - exe wurde nicht gefunden." -ForegroundColor Red
    exit 1
}
