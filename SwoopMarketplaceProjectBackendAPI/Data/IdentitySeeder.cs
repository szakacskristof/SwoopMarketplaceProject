using Microsoft.AspNetCore.Identity;
namespace SwoopMarketplaceProjectBackendAPI.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration cfg)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        // 1) Szerepkörök
        string[] roles = ["Admin", "User","Owner"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
        // 2) Admin user (ha van config)
        var adminEmail = cfg["SeedAdmin:Email"];
        var adminPassword = cfg["SeedAdmin:Password"];
        if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
        {
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin is null)
            {
                admin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var createResult = await userManager.CreateAsync(admin, adminPassword);
                if (!createResult.Succeeded)
                {
                    var msg = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    throw new Exception("Admin user létrehozása sikertelen: " + msg);
                }
            }



            // Admin szerepkör biztosítása
            if (!await userManager.IsInRoleAsync(admin, "Admin"))
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }

        var ownerEmail = cfg["SeedOwner:Email"];
        var ownerPassword = cfg["SeedOwner:Password"];
        if (!string.IsNullOrWhiteSpace(ownerEmail) && !string.IsNullOrWhiteSpace(ownerPassword))
        {
            var owner = await userManager.FindByEmailAsync(ownerEmail);
            if (owner is null)
            {
                owner = new IdentityUser
                {
                    UserName = ownerEmail,
                    Email = ownerEmail,
                    EmailConfirmed = true
                };
                var createResult = await userManager.CreateAsync(owner, ownerPassword);
                if (!createResult.Succeeded)
                {
                    var msg = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    throw new Exception("Owner user létrehozása sikertelen: " + msg);
                }
            }



            // Admin szerepkör biztosítása
            if (!await userManager.IsInRoleAsync(owner, "Owner"))
            {
                await userManager.AddToRoleAsync(owner, "Owner");
            }
        }




    }
}
