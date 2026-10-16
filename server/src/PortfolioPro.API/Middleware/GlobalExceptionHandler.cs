using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace PortfolioPro.API.Middleware;

// Yakalanmayan bütün hatalar buraya düşüyor (.NET 8 ile gelen IExceptionHandler).
// DevLog'da bunun için kendi middleware'imi yazmıştım, burada .NET'in hazır yapısını kullandım.
// Kullanıcıya her zaman aynı formatta JSON dönüyoruz (ProblemDetails): { type, title, status, detail, errors }
internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        ProblemDetails problem;

        switch (exception)
        {
            case ValidationException validation:
                // ValidationBehavior'ın fırlattığı hata -> 400 ve alan bazında hata listesi
                var errors = validation.Errors
                    .GroupBy(e => ToCamelCase(e.PropertyName))
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

                problem = new HttpValidationProblemDetails(errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Doğrulama hatası",
                    Detail = errors.Values.FirstOrDefault()?.FirstOrDefault()
                };
                break;

            case BadHttpRequestException badRequest:
                // Bozuk JSON, yanlış tip gibi durumlar
                problem = new ProblemDetails
                {
                    Status = badRequest.StatusCode,
                    Title = "Geçersiz istek",
                    Detail = environment.IsDevelopment() ? badRequest.Message : null
                };
                break;

            default:
                // Beklenmeyen hata: logla ama detayı kullanıcıya GÖSTERME (stack trace bilgi sızdırır)
                logger.LogError(exception, "Beklenmeyen hata: {Path}", httpContext.Request.Path);
                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Sunucu hatası",
                    Detail = environment.IsDevelopment() ? exception.Message : "Beklenmeyen bir hata oluştu."
                };
                break;
        }

        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }

    // "Title.Tr" -> "title.tr" (Angular tarafındaki form alan isimleriyle eşleşsin)
    private static string ToCamelCase(string propertyName) =>
        string.Join('.', propertyName.Split('.').Select(p =>
            string.IsNullOrEmpty(p) ? p : char.ToLowerInvariant(p[0]) + p[1..]));
}
