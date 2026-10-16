namespace PortfolioPro.Domain.Common;

// Türkçe ve İngilizce metni bir arada tutan küçük bir sınıf.
// NOT: EF Core'da ComplexProperty olarak ayarladım, bunun için ayrı tablo açılmıyor.
// Veritabanında Title_Tr ve Title_En diye iki kolon oluşuyor.
public class LocalizedText
{
    public string Tr { get; set; } = string.Empty;
    public string En { get; set; } = string.Empty;

    public static LocalizedText Create(string tr, string en) => new() { Tr = tr.Trim(), En = en.Trim() };

    public static LocalizedText Empty => new();
}
