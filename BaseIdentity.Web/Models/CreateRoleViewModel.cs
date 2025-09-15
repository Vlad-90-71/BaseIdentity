using System.ComponentModel.DataAnnotations;

namespace BaseIdentity.Web.Models;

public class CreateRoleViewModel
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
}
