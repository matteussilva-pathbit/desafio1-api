using Microsoft.Extensions.DependencyInjection;

// aliases para evitar ambiguidade
using IOrderServiceApp    = Application.Interface.IOrderService; // interface OFICIAL de pedidos
using OrderServiceImpl    = Application.Services.OrderService;
using ProductServiceImpl  = Application.Services.ProductService; // serviço CONCRETO de produtos
using CustomerServiceImpl = Application.Services.CustomerService;

using Domain.Repositories;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            // Repositórios + UoW
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Services de aplicação
            services.AddScoped<IOrderServiceApp, OrderServiceImpl>(); // interface -> implementação
            services.AddScoped<ProductServiceImpl>();                 // registra o CONCRETO (sem interface)
            services.AddScoped<CustomerServiceImpl>();                // se houver uso do concreto

            // ViaCEP (se quiser registrar aqui também)
            // services.AddSingleton<IViaCepService>(sp =>
            // {
            //     var http = new HttpClient { BaseAddress = new Uri("https://viacep.com.br/") };
            //     return new ViaCepService(http);
            // });

            return services;
        }
    }
}
