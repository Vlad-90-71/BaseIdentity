using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BaseIdentity.Data;
using BaseIdentity.Data.Identity;
using BaseIdentity.Services.Services;
using BaseIdentity.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = false; //true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole(Role.Admin));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// === Авто-миграции и инициализация (создание БД, применение миграций, сидинг админа) ===
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync(); // создаст БД при отсутствии и применит миграции

    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    // Роли сидируются миграцией (HasData). На всякий случай проверим их наличие:
    async Task EnsureRoleAsync(Guid id, string name)
    {
        var role = await roleMgr.FindByNameAsync(name);
        if (role == null)
        {
            role = new AppRole { Id = id, Name = name, NormalizedName = name.ToUpperInvariant() };
            await roleMgr.CreateAsync(role);
        }
    }
    await EnsureRoleAsync(Role.Admin.Id, Role.Admin);
    await EnsureRoleAsync(Role.Manager.Id, Role.Manager);
    await EnsureRoleAsync(Role.User.Id, Role.User);

    // Администратор по умолчанию
    var adminEmail = "admin@local";
    var admin = await userMgr.FindByEmailAsync(adminEmail);
    if (admin == null)
    {
        admin = new AppUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        var created = await userMgr.CreateAsync(admin, "Admin123!");
        if (created.Succeeded)
        {
            await userMgr.AddToRoleAsync(admin, Role.Admin);
        }
    }
}

app.Run();
