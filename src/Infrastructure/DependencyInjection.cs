using System;
using System.Net.Http;
using Application.Interface;
using Application.Services;
using Domain.Repositories;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            // DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(config.GetConnectionString("Default")));

            // Repositórios
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICustomerService, CustomerService>();

            // Services de aplicação
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IProductService, ProductService>();

            // ViaCEP – registramos sem AddHttpClient para evitar pacote extra
            services.AddSingleton<IViaCepService>(sp =>
            {
                var http = new HttpClient { BaseAddress = new Uri("https://viacep.com.br/") };
                return new ViaCepService(http);
            });

            return services;
        }
    }
}
