using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PortfolioPro.Application.Common.Behaviors;
using PortfolioPro.Application.Common.Validation;

namespace PortfolioPro.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            // NOT: Her istek handler'a gitmeden önce ValidationBehavior'dan geçiyor.
            // AddOpenBehavior ile bunu tek tek değil, bütün istekler için bir kerede kaydediyoruz
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // AbstractValidator'dan türeyen bütün sınıfları bulup DI'a ekliyor.
        // NOT: LocalizedDtoValidator oluşturulurken bir sayı (maxLength) istiyor, DI bu sayıyı bilemez.
        // Onu zaten diğer validator'ların içinde "new" ile kullanıyoruz, o yüzden otomatik taramadan çıkardım.
        services.AddValidatorsFromAssembly(
            assembly,
            filter: r => r.ValidatorType != typeof(LocalizedDtoValidator),
            includeInternalTypes: true);

        return services;
    }
}
