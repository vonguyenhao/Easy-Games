using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EasyGames.Web.Data;

public static class StartupTasks
{
    public static async Task ApplyMigrationsAndSeedAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;

        var db = services.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();          // apply pending migrations
        await SeedData.InitializeAsync(services);  // seed roles + demo users
    }
}
