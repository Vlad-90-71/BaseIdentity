using System.ComponentModel.DataAnnotations;
using BaseIdentity.Data.Identity;

namespace BaseIdentity.Web.Models;

public class EditUserRoleViewModel
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;

    public Guid? CurrentRoleId { get; set; }

    [Required]
    public Guid NewRoleId { get; set; }

    public List<AppRole> Roles { get; set; } = [];
}
