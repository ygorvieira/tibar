using Microsoft.AspNetCore.Identity;

namespace Tibar.Infrastructure.Identity;

public class AppUser : IdentityUser<Guid>
{
    public string Name { get; set; } = null!;
}
