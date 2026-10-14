namespace PortfolioPro.Domain.Common;

// Türkçe ve İngilizce metni bir arada tutan küçük bir value object.
// NOT: EF Core'da ComplexProperty olarak ayarlandı, ayrı tablo açmıyor.
// Veritabanında Title_Tr ve Title_En diye iki kolon oluşuyor.
public class LocalizedText
{
    public string Tr { get; set; } = string.Empty;
    public string En { get; set; } = string.Empty;

    public static LocalizedText Create(string tr, string en) => new() { Tr = tr.Trim(), En = en.Trim() };

    public static LocalizedText Empty => new();
}
