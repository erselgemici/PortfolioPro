using FluentValidation;

namespace PortfolioPro.Application.Common.Validation;

public static class RuleExtensions
{
    // Sadece http ve https linklerine izin veriyoruz.
    // NOT: "javascript:alert(1)" gibi bir link kaydedilip sitede <a href> içine basılırsa XSS olur
    public static IRuleBuilderOptions<T, string?> HttpUrl<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(BeValidUrl).WithMessage("Geçerli bir http/https adresi giriniz.");

    // Yüklenen dosyalar "/uploads/..." diye relative geliyor, onlara da izin var
    public static IRuleBuilderOptions<T, string?> HttpUrlOrUpload<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(v => BeValidUrl(v) || (v != null && v.StartsWith("/uploads/") && !v.Contains("..")))
            .WithMessage("Geçerli bir adres giriniz.");

    private static bool BeValidUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return true; // boş bırakılabilir alanlar için
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
