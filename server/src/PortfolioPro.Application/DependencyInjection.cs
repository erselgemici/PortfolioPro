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
            // AddOpenBehavior generic tipi (<,>) tüm istekler için kaydediyor
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // AbstractValidator'dan türeyen bütün sınıfları bulup DI'a ekliyor.
        // NOT: LocalizedDtoValidator constructor'da parametre (maxLength) istiyor, DI bunu oluşturamaz.
        // Zaten onu diğer validator'ların içinde "new" ile kullanıyoruz, bu yüzden taramadan hariç tutuyoruz.
        services.AddValidatorsFromAssembly(
            assembly,
            filter: r => r.ValidatorType != typeof(LocalizedDtoValidator),
            includeInternalTypes: true);

        return services;
    }
}
