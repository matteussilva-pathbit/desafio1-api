using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var cfg = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var cs = cfg.GetConnectionString("DefaultConnection")
                 ?? "Host=localhost;Port=5432;Database=desafio_db;Username=postgres;Password=postgres";

        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(cs) // estamos com PostgreSQL
            .Options;

        return new AppDbContext(opts);
    }
}
