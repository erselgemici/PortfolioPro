using FluentValidation;
using MediatR;

namespace PortfolioPro.Application.Common.Behaviors;

// Her MediatR isteği handler'a ulaşmadan önce buradan geçer.
// O isteğe ait validator varsa çalıştırır, hata varsa ValidationException fırlatır.
// API'deki GlobalExceptionHandler bunu yakalayıp 400 döner.
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
