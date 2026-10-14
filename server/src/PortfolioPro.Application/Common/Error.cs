namespace PortfolioPro.Application.Common;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden
}

// Hata bilgisini exception fırlatmadan taşımak için.
// NOT: Exception pahalı bir işlem ve "beklenen" durumlar (kayıt bulunamadı, slug kullanılıyor)
// için exception kullanmak yerine Result + Error dönüyoruz. API katmanı bunu HTTP koduna çeviriyor.
public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);
    public static Error Unauthorized(string code, string message) => new(code, message, ErrorType.Unauthorized);
}
