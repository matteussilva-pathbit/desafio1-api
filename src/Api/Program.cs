using Application.Interfaces;
using Infrastructure.Data;
using Application.Services;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Domain.Repositories;


var builder = WebApplication.CreateBuilder(args);

// DbContext (Postgres)
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// DI — repositórios e serviços
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>(); 

// Se existirem, registre também:
// builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
// builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// builder.Services.AddScoped<IProductService, ProductService>();
// builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddHttpClient<Application.Interfaces.IViaCepService, Infrastructure.Services.ViaCepService>();

// AutoMapper (você tem Profiles/ProductProfile.cs)
builder.Services.AddAutoMapper(cfg => { }, typeof(Api.Profiles.ProductProfile).Assembly);


// JWT
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero // 1h exata
        };
    });

// Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin",    p => p.RequireRole("ADMINISTRADOR"));
    options.AddPolicy("Customer", p => p.RequireRole("CLIENTE"));
});

builder.Services.AddControllers();

// Swagger (apenas Bearer)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Desafio API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "JWT Bearer",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer"} }, Array.Empty<string>() }
    });
});

var app = builder.Build();

// aplica migrations e (opcional) seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    // await DbSeeder.SeedAsync(db, scope.ServiceProvider); // se você tiver um seeder async
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
