using Common.Security;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext ctx)
    {
        await ctx.Database.MigrateAsync();

        if (!await ctx.Users.AnyAsync(u => u.Type == UserType.ADMINISTRADOR))
        {
            var adminCustomer = new Customer { Name = "Admin", Email = "admin@local" };
            ctx.Customers.Add(adminCustomer);

            var admin = new User {
                Email = "admin@local",
                UserName = "admin",
                PasswordHash = PasswordHasher.Sha256("Admin@123"),
                Type = UserType.ADMINISTRADOR,
                CustomerId = adminCustomer.Id
            };
            ctx.Users.Add(admin);
            await ctx.SaveChangesAsync();
        }
    }
}
