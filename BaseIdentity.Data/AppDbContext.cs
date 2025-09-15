using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using BaseIdentity.Data.Entity;
using BaseIdentity.Data.Identity;

namespace BaseIdentity.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<
    AppUser,
    AppRole,
    Guid,
    IdentityUserClaim<Guid>,
    IdentityUserRole<Guid>,
    IdentityUserLogin<Guid>,
    IdentityRoleClaim<Guid>,
    IdentityUserToken<Guid>>(options)
{
    public DbSet<AdminActionLog> AdminActionLogs => Set<AdminActionLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Имена таблиц
        builder.Entity<AppUser>().ToTable("Users");
        builder.Entity<AppRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

        // Many-to-many через UserRoles, одна роль на пользователя
        builder.Entity<AppUser>()
            .HasMany(u => u.Roles)
            .WithMany(r => r.Users)
            .UsingEntity<IdentityUserRole<Guid>>(
                j => j.HasOne<AppRole>().WithMany().HasForeignKey(ur => ur.RoleId).OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<AppUser>().WithMany().HasForeignKey(ur => ur.UserId).OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey(ur => new { ur.UserId, ur.RoleId });
                    j.HasIndex(ur => ur.UserId).IsUnique(); // ключевой инвариант
                });

        //builder.Entity<AppUser>().Navigation(u => u.Roles).AutoInclude();

        // Аудит-лог
        builder.Entity<AdminActionLog>().ToTable("AdminActionLogs");
        builder.Entity<AdminActionLog>().HasKey(x => x.Id);
        builder.Entity<AdminActionLog>().Property(x => x.AdminEmail).HasMaxLength(320);
        builder.Entity<AdminActionLog>().Property(x => x.ActionType).HasMaxLength(64);
        builder.Entity<AdminActionLog>().Property(x => x.Target).HasMaxLength(512);

        // Начальные роли (HasData — попадают в миграции)
        builder.Entity<AppRole>().HasData(
            new AppRole { Id = Role.Admin.Id, Name = Role.Admin, NormalizedName = Role.Admin.ToString().ToUpperInvariant() },
            new AppRole { Id = Role.Manager.Id, Name = Role.Manager, NormalizedName = Role.Manager.ToString().ToUpperInvariant() },
            new AppRole { Id = Role.User.Id, Name = Role.User, NormalizedName = Role.User.ToString().ToUpperInvariant() }
        );
    }
}
