using PdfSharp.Fonts;

namespace MikrotikInstaller.Core.Export;

/// <summary>
/// Liest "Segoe UI" (Regular/Bold) direkt aus dem Windows-Schriftartenordner. PDFsharp 6 löst
/// Schriftarten nicht mehr automatisch über GDI auf, daher braucht es einen eigenen Resolver.
/// Segoe UI ist auf jedem unterstützten Windows vorinstalliert — es werden keine Schriftdateien
/// mitgeliefert (Lizenz von Microsoft), nur zur Laufzeit vom System gelesen.
/// </summary>
public sealed class WindowsSegoeUiFontResolver : IFontResolver
{
    private const string RegularFace = "SegoeUI#Regular";
    private const string BoldFace = "SegoeUI#Bold";

    public byte[]? GetFont(string faceName)
    {
        var fileName = faceName == BoldFace ? "segoeuib.ttf" : "segoeui.ttf";
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), fileName);
        return File.Exists(path) ? File.ReadAllBytes(path) : null;
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic) =>
        new(isBold ? BoldFace : RegularFace);
}
