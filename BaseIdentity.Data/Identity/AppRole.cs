using Microsoft.AspNetCore.Identity;

namespace BaseIdentity.Data.Identity;

public class AppRole : IdentityRole<Guid>
{
    public ICollection<AppUser> Users { get; set; } = [];
}

public static class Role
{
    public static RoleName Admin => new("Admin", Guid.Parse("778A580F-B535-4CF5-BCB5-D20363FEC893"));
    public static RoleName Manager => new("Manager", Guid.Parse("780C09D9-99A8-4B7F-BF52-1B1F223AF38C"));
    public static RoleName User => new("User", Guid.Parse("3BF04E3E-6CB5-481A-9881-1B1A277B476E"));
}

public readonly struct RoleName(string value, Guid id)
{
    public Guid Id { get; } = id;
    private readonly string _value = value;

    public override string ToString() => _value;

    public static implicit operator string(RoleName constant) => constant._value;
}
