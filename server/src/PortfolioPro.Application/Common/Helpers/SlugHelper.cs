using System.Text;
using System.Text.RegularExpressions;

namespace PortfolioPro.Application.Common.Helpers;

public static partial class SlugHelper
{
    // "101 Çetele: Skor Takip" -> "101-cetele-skor-takip"
    public static string Generate(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var sb = new StringBuilder(text.Length);
        foreach (var ch in text.Trim().ToLowerInvariant())
        {
            sb.Append(ch switch
            {
                // NOT: ToLowerInvariant büyük "İ" harfini küçültmüyor (Türkçeye özel harf), elle çeviriyoruz
                'İ' => 'i',
                'ç' => 'c',
                'ğ' => 'g',
                'ı' => 'i',
                'ö' => 'o',
                'ş' => 's',
                'ü' => 'u',
                _ => ch
            });
        }

        // harf ve rakam dışındaki her şey tire oluyor, art arda tireler teke iniyor
        var slug = NonAlphaNumeric().Replace(sb.ToString(), "-");
        slug = MultipleDash().Replace(slug, "-").Trim('-');

        return slug.Length > 80 ? slug[..80].TrimEnd('-') : slug;
    }

    // NOT: [GeneratedRegex] ile regex uygulama derlenirken hazırlanıyor. Her çağrıda baştan oluşturulmadığı için daha hızlı.
    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphaNumeric();

    [GeneratedRegex("-{2,}")]
    private static partial Regex MultipleDash();
}
