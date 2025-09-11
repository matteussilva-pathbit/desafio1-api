using System.Text;
using System.Text.Encodings.Web;
using Application.Interface;     // IOrderService (singular) - interface oficial do Order
using Application.Services;
using API.Auth; // BasicAuthenticationHandler
using Domain.Repositories;
using Infrastructure.Data;
using Infrastructure.Identity;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

// ===== Aliases para evitar ambiguidade =====
using IOrderServiceApp   = Application.Interface.IOrderService;      // existe e é a interface oficial de pedidos
// NÃO registre IProductService aqui; ProductService não implementa o contrato de Application.Interfaces

using OrderServiceImpl    = Application.Services.OrderService;
using ProductServiceImpl  = Application.Services.ProductService;
using CustomerServiceImpl = Application.Services.CustomerService;     // se precisar registrar o concreto

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------
// Serilog (logs em ./logs/application.log)
// -------------------------------------------
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.File(
        "./logs/application.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

builder.Host.UseSerilog();

// -------------------------------------------
// DbContext
// -------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// -------------------------------------------
// Repositórios + UoW
// -------------------------------------------
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// -------------------------------------------
// Services de aplicação
// -------------------------------------------
// Order: interface oficial -> implementação concreta
builder.Services.AddScoped<IOrderServiceApp, OrderServiceImpl>();

// Product: registrar o CONCRETO, pois a classe não implementa Application.Interfaces.IProductService
builder.Services.AddScoped<ProductServiceImpl>();

// (Opcional) Customer: registrar o concreto, caso algum controller injete a classe
builder.Services.AddScoped<CustomerServiceImpl>();

// -------------------------------------------
// Auth service
// -------------------------------------------
builder.Services.AddScoped<IAuthService, AuthService>();

// -------------------------------------------
// ViaCEP
// -------------------------------------------
builder.Services.AddSingleton<IViaCepService>(sp =>
{
    var http = new HttpClient { BaseAddress = new Uri("https://viacep.com.br/") };
    return new ViaCepService(http);
});

// -------------------------------------------
// Autenticação: JWT + Basic
// -------------------------------------------
var secret = builder.Configuration["Jwt:Secret"] ?? "dev-secret-change-me-please";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

builder.Services
    .AddAuthentication(options =>
    {
        // JWT como esquema padrão
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    })
    // habilita também Basic (para atender ao requisito do desafio)
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", null);

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("ADMIN", p => p.RequireRole("ADMIN"));
    o.AddPolicy("CLIENTE", p => p.RequireRole("CLIENTE"));
});

// -------------------------------------------
// Controllers + Swagger (JWT + Basic)
// -------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Desafio API",
        Version = "v1",
        Description = "API de clientes, produtos e pedidos com JWT e Basic Auth"
    });

    // JWT Bearer
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Cole APENAS o token (sem 'Bearer ')."
    });

    // Basic
    c.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        In = ParameterLocation.Header,
        Description = "Basic Auth (username:senha em Base64)."
    });

    // aplicar globalmente (ambos aceitos)
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Basic" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// -------------------------------------------
// Pipeline
// -------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opt =>
    {
        opt.SwaggerEndpoint("/swagger/v1/swagger.json", "Desafio API v1");
        opt.DisplayRequestDuration();
    });
}

// logs de requisição no Serilog
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// encerra com flush do Serilog
try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}
