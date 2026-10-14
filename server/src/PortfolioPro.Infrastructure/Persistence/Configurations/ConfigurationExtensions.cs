using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortfolioPro.Domain.Common;

namespace PortfolioPro.Infrastructure.Persistence.Configurations;

internal static class ConfigurationExtensions
{
    // İki dilli alanları ComplexProperty olarak ayarlıyor.
    // Sonuç: Projects tablosunda Title_Tr ve Title_En kolonları (ayrı tablo yok, JOIN yok)
    public static void Localized<T>(
        this EntityTypeBuilder<T> builder,
        Expression<Func<T, LocalizedText?>> property,
        int? maxLength = null) where T : class
    {
        builder.ComplexProperty(property, c =>
        {
            var tr = c.Property(x => x.Tr).IsRequired();
            var en = c.Property(x => x.En).IsRequired();

            if (maxLength.HasValue)
            {
                tr.HasMaxLength(maxLength.Value);
                en.HasMaxLength(maxLength.Value);
            }
        });
    }
}
