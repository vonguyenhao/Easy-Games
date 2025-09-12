using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EasyGames.Web.Models;

namespace EasyGames.Web.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var db = services.GetRequiredService<ApplicationDbContext>();

        // --- Ensure roles ---
        foreach (var role in new[] { "Owner", "Customer" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        const string DefaultPassword = "Easygames1@";

        // --- Helper: create user if missing and add to role ---
        async Task EnsureUserAsync(string email, string role)
        {
            var u = await userManager.FindByEmailAsync(email);
            if (u is null)
            {
                u = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };
                var create = await userManager.CreateAsync(u, DefaultPassword);
                if (!create.Succeeded)
                {
                    // fail fast to surface seeding issues
                    var msg = string.Join("; ", create.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create seed user {email}: {msg}");
                }
            }

            // ensure role membership
            if (!await userManager.IsInRoleAsync(u!, role))
                await userManager.AddToRoleAsync(u!, role);
        }

        // --- Seed demo accounts ---
        await EnsureUserAsync("admin@easygames.com", "Owner");
        await EnsureUserAsync("customer@easygames.com", "Customer");

        // --- Seed sample products (once) ---
        if (!await db.Products.AnyAsync())
        {
            db.Products.AddRange(
                new Product { Name = "Monopoly", Category = "Game", Price = 35m, StockQty = 20, CreatedAt = DateTime.UtcNow },
                new Product { Name = "Lego Starter", Category = "Toy", Price = 39.99m, StockQty = 15, CreatedAt = DateTime.UtcNow },
                new Product { Name = "Clean Code", Category = "Book", Price = 49.99m, StockQty = 10, CreatedAt = DateTime.UtcNow }
            );
            await db.SaveChangesAsync();
        }
    }
}
