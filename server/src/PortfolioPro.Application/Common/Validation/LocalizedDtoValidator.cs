using FluentValidation;
using PortfolioPro.Application.Common.Models;

namespace PortfolioPro.Application.Common.Validation;

// İki dilli alanların hepsinde aynı kuralları tekrar yazmamak için ortak validator
public sealed class LocalizedDtoValidator : AbstractValidator<LocalizedDto>
{
    public LocalizedDtoValidator(int maxLength, bool required = true)
    {
        if (required)
        {
            RuleFor(x => x.Tr).NotEmpty().WithMessage("Türkçe alan zorunludur.");
            RuleFor(x => x.En).NotEmpty().WithMessage("İngilizce alan zorunludur.");
        }

        RuleFor(x => x.Tr).MaximumLength(maxLength);
        RuleFor(x => x.En).MaximumLength(maxLength);
    }
}
