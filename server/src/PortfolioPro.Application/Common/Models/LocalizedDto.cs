using PortfolioPro.Domain.Common;

namespace PortfolioPro.Application.Common.Models;

// API'ye giden ve gelen iki dilli metin. Angular seçili dile göre Tr ya da En'i gösteriyor,
// dil değişince tekrar istek atmaya gerek kalmıyor.
public sealed record LocalizedDto(string Tr, string En)
{
    public LocalizedText ToEntity() => LocalizedText.Create(Tr ?? string.Empty, En ?? string.Empty);
}
