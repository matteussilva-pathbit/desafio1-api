using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace API.Data;

public class AppDbContext : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        var config = new ConfigurationBuilder()
        .SetBasePath(basePath)
        .AddJsonFile("appsetting.json", optional: false, reloadOnChange: false)
        .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
        .AddEnviromentVariables()
        .Build();

        var connectionString = config.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' não encontrada. ");

        var optionBuilder = new DbContextOptionBuilder<AppDbContext>();

        optionBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);

    }



}

