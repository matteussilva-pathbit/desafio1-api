using API.Data;
using Api.Profiles;                  // ProductProfile
using Api.Repositories;
using Application.Interfaces;
using Application.Services;
using Microsoft.EntityFrameworkCore;

//  nao precisa de "using AutoMapper.Extensions.Microsoft.DependencyInjection;"
// o metodo de extensão está em Microsoft.Extensions.DependencyInjection (já incluso)

var builder = WebApplication.CreateBuilder(args);

// DbContext (ajuste para Sqlite se for o caso)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ AutoMapper: usa apenas a Action de configuração (sem Assembly)
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<ProductProfile>();
});

// DI
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
// app.UseHttpsRedirection();

app.MapControllers();
app.Run();
