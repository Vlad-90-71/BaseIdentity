using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseIdentity.Data.Identity;

public class AppUser : IdentityUser<Guid>
{
    public ICollection<AppRole> Roles { get; set; } = [];

    [NotMapped]
    public bool IsAdmin => Roles.Any(r => r.Name == Role.Admin || r.Id == Role.Admin.Id);
}
