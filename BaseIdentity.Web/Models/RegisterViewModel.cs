using System.ComponentModel.DataAnnotations;

namespace BaseIdentity.Web.Models;

public class RegisterViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public Guid RoleId { get; set; }
}
