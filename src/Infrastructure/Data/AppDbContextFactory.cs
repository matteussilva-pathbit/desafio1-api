using System.IO;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Data
{
    // Usado pelo "dotnet ef" em design-time
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Carrega configuração de appsettings + env vars (prioridade para env)
            var env = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("src/API/appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile($"src/API/appsettings.{env}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();

            var config = builder.Build();

            var cs = config.GetConnectionString("Default")
                     ?? "Host=localhost;Port=5433;Database=desafio_db;Username=postgres;Password=postgres";

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(cs);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
