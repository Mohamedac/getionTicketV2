using gestionticket_v2.Data;
using gestionticket_v2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Ajouter les services au conteneur.

builder.Services.AddDbContext<gestionticket_v2Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("gestionticket_v2Context")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<gestionticket_v2Context>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configurer le pipeline de requête HTTP.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using (var scope = scopeFactory.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Ajouter vos rôles ici
    var roles = new[] { "Client", "MembreSupportTechnique", "Admin" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var identityRole = new IdentityRole(role);
            await roleManager.CreateAsync(identityRole);
        }
    }

    // Créer l'utilisateur administrateur
    var adminUser = new ApplicationUser
    {
        UserName = "admin@example.com",
        Email = "admin@example.com",
        // Ajouter d'autres propriétés ici au besoin
    };

    var adminPassword = "Admin@123"; // Utiliser un mot de passe plus fort en production !
    var adminUserExists = await userManager.FindByNameAsync(adminUser.UserName);
    if (adminUserExists == null)
    {
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

app.MapControllerRoute(
    name: "statistics",
    pattern: "{controller=Statistics}/{action=Index}/{id?}");

app.Run();
