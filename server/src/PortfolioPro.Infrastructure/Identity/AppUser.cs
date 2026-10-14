using Microsoft.AspNetCore.Identity;

namespace PortfolioPro.Infrastructure.Identity;

// NOT: IdentityUser<Guid> diyerek Id tipini string yerine Guid yaptık
public class AppUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
}
