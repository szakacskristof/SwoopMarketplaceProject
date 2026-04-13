using Microsoft.AspNetCore.Identity;
using SwoopMarketplaceProject.Models;
namespace SwoopMarketplaceProjectBackendAPI.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration cfg)
    {
        using var scope = services.CreateScope();
        SwoopContext _sdbc = new SwoopContext();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        // 1) Szerepkörök
        string[] roles = ["Admin", "User", "Owner"];
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
                User newAdmin = new User()
                {


                    Email = adminEmail,
                    Id = 2,
                    Username = "SwoopAdmin1",
                    CreatedAt = DateTime.UtcNow,
                    Bio = "SwoopMarketplace Admin felhasználója",
                    Phone = "Admin,telefonszámt titkosítva",
                    ProfileImageUrl = "https://localhost:7000/images/profilepictures/adminpfp.png"

                };

                await _sdbc.Users.AddAsync(newAdmin);
                await _sdbc.SaveChangesAsync();

            }



            // Admin szerepkör biztosítása
            if (!await userManager.IsInRoleAsync(admin, "Admin"))
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
        // 2) Owner user (ha van config)
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


                User newUser = new User()
                {


                    Email = ownerEmail,
                    Id = 1,
                    Username = "SwoopOwner",
                    CreatedAt = DateTime.UtcNow,
                    Bio = "SwoopMarketplace tulajdonosa",
                    Phone = "Tulajdonos,telefonszámt titkosítva",
                    ProfileImageUrl = "https://localhost:7000/images/profilepictures/ownerpfp.png"

                };

                await _sdbc.Users.AddAsync(newUser);
                await _sdbc.SaveChangesAsync();
            }



            // Admin szerepkör biztosítása
            if (!await userManager.IsInRoleAsync(owner, "Owner"))
            {
                await userManager.AddToRoleAsync(owner, "Owner");
            }
        }




        // 3) User1
        var user1Email = cfg["SeedUser1:Email"];
        var user1Password = cfg["SeedUser1:Password"];
        if (!string.IsNullOrWhiteSpace(user1Email) && !string.IsNullOrWhiteSpace(user1Password))
        {
            var user1 = await userManager.FindByEmailAsync(user1Email);
            if (user1 is null)
            {
                user1 = new IdentityUser
                {
                    UserName = user1Email,
                    Email = user1Email,
                    EmailConfirmed = true
                };
                var createResult = await userManager.CreateAsync(user1, user1Password);
                if (!createResult.Succeeded)
                {
                    var msg = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    throw new Exception("User létrehozása sikertelen: " + msg);
                }
                User newUser1 = new User()
                {


                    Email = user1Email,
                    Id = 3,
                    Username = "Belavagyok",
                    CreatedAt = DateTime.UtcNow,
                    Bio = "Üdv! Nagy Béla vagyok!",
                    Phone = "+36 20 252 2525",
                    ProfileImageUrl = "https://localhost:7000/images/profilepictures/defaultprofilepicture.png"

                };

                await _sdbc.Users.AddAsync(newUser1);
                await _sdbc.SaveChangesAsync();

            }



            // Admin szerepkör biztosítása
            if (!await userManager.IsInRoleAsync(user1, "User"))
            {
                await userManager.AddToRoleAsync(user1, "User");




            }
        }
        // 4) User2
        var user2Email = cfg["SeedUser2:Email"];
        var user2Password = cfg["SeedUser2:Password"];
        if (!string.IsNullOrWhiteSpace(user2Email) && !string.IsNullOrWhiteSpace(user2Password))
        {
            var user2 = await userManager.FindByEmailAsync(user2Email);
            if (user2 is null)
            {
                user2 = new IdentityUser
                {
                    UserName = user2Email,
                    Email = user2Email,
                    EmailConfirmed = true
                };
                var createResult = await userManager.CreateAsync(user2, user2Password);
                if (!createResult.Succeeded)
                {
                    var msg = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    throw new Exception("User létrehozása sikertelen: " + msg);
                }
                User newUser2 = new User()
                {


                    Email = user2Email,
                    Id = 4,
                    Username = "Gyulavagyok",
                    CreatedAt = DateTime.UtcNow,
                    Bio = "Üdv! Kis Gyula vagyok!",
                    Phone = "+36 20 646 3221",
                    ProfileImageUrl = "https://localhost:7000/images/profilepictures/defaultprofilepicture.png"

                };
                await _sdbc.Users.AddAsync(newUser2);
                await _sdbc.SaveChangesAsync();

            }

            if (!await userManager.IsInRoleAsync(user2, "User"))
            {
                await userManager.AddToRoleAsync(user2, "User");
            }
        }
        // 5) User3
        var user3Email = cfg["SeedUser3:Email"];
        var user3Password = cfg["SeedUser3:Password"];
        if (!string.IsNullOrWhiteSpace(user3Email) && !string.IsNullOrWhiteSpace(user3Password))
        {
            var user3 = await userManager.FindByEmailAsync(user3Email);
            if (user3 is null)
            {
                user3 = new IdentityUser
                {
                    UserName = user3Email,
                    Email = user3Email,
                    EmailConfirmed = true
                };
                var createResult = await userManager.CreateAsync(user3, user3Password);
                if (!createResult.Succeeded)
                {
                    var msg = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    throw new Exception("User létrehozása sikertelen: " + msg);
                }
                User newUser3 = new User()
                {


                    Email = user3Email,
                    Id = 5,
                    Username = "Magyar Kata",
                    CreatedAt = DateTime.UtcNow,
                    Bio = "Üdv! Magyar Kata vagyok!",
                    Phone = "+36 20 777 3532",
                    ProfileImageUrl = "https://localhost:7000/images/profilepictures/defaultprofilepicture.png"

                };
                await _sdbc.Users.AddAsync(newUser3);
                await _sdbc.SaveChangesAsync();

            }

            if (!await userManager.IsInRoleAsync(user3, "User"))
            {
                await userManager.AddToRoleAsync(user3, "User");
            }
        }
        // 6) User4
        var user4Email = cfg["SeedUser4:Email"];
        var user4Password = cfg["SeedUser4:Password"];
        if (!string.IsNullOrWhiteSpace(user4Email) && !string.IsNullOrWhiteSpace(user4Password))
        {
            var user4 = await userManager.FindByEmailAsync(user4Email);
            if (user4 is null)
            {
                user4 = new IdentityUser
                {
                    UserName = user4Email,
                    Email = user4Email,
                    EmailConfirmed = true
                };
                var createResult = await userManager.CreateAsync(user4, user4Password);
                if (!createResult.Succeeded)
                {
                    var msg = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    throw new Exception("User létrehozása sikertelen: " + msg);
                }
                User newUser4 = new User()
                {


                    Email = user4Email,
                    Id = 6,
                    Username = "Földes Mária",
                    CreatedAt = DateTime.UtcNow,
                    Bio = "Üdv! Földes Mária vagyok!",
                    Phone = "+36 20 9999 2525",
                    ProfileImageUrl = "https://localhost:7000/images/profilepictures/defaultprofilepicture.png"

                };
                await _sdbc.Users.AddAsync(newUser4);
                await _sdbc.SaveChangesAsync();


            }

            if (!await userManager.IsInRoleAsync(user4, "User"))
            {
                await userManager.AddToRoleAsync(user4, "User");
            }
        }
        // 7) User5
        var user5Email = cfg["SeedUser5:Email"];
        var user5Password = cfg["SeedUser5:Password"];
        if (!string.IsNullOrWhiteSpace(user5Email) && !string.IsNullOrWhiteSpace(user5Password))
        {
            var user5 = await userManager.FindByEmailAsync(user5Email);
            if (user5 is null)
            {
                user5 = new IdentityUser
                {
                    UserName = user5Email,
                    Email = user5Email,
                    EmailConfirmed = true
                };
                var createResult = await userManager.CreateAsync(user5, user5Password);
                if (!createResult.Succeeded)
                {
                    var msg = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    throw new Exception("User létrehozása sikertelen: " + msg);
                }
                User newUser5 = new User()
                {


                    Email = user5Email,

                    Id = 7,
                    Username = "Tejfölös Ferenc",
                    CreatedAt = DateTime.UtcNow,
                    Bio = "Üdv! Tejfölös Ferenc vagyok!",
                    Phone = "+36 20 654 1111",
                    ProfileImageUrl = "https://localhost:7000/images/profilepictures/defaultprofilepicture.png"

                };

                await _sdbc.Users.AddAsync(newUser5);
                await _sdbc.SaveChangesAsync();

            }

            if (!await userManager.IsInRoleAsync(user5, "User"))
            {
                await userManager.AddToRoleAsync(user5, "User");
            }
        }

    }
}