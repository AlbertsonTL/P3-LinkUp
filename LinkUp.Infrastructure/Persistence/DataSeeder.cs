using LinkUp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LinkUp.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context, UserManager<AppUser> userManager)
    {
        if (await userManager.Users.AnyAsync()) return;

        var users = new[]
        {
            new AppUser { UserName = "pruebas", Email = "pruebas@linkup.com", FirstName = "Pruebas", LastName = "LinkUp", Phone = "809-000-0000", IsActive = true },
            new AppUser { UserName = "alb3rtsontl", Email = "alb3rtsontl@gmail.com", FirstName = "Albertson", LastName = "Terrero López", Phone = "809-111-1111", IsActive = true },
        };

        foreach (var user in users)
            await userManager.CreateAsync(user, "c-1234");
    }
}
