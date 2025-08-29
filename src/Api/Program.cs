using Application.Interfaces;
using Application.Services;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Api.Profiles; // se seus Profiles estão em Api (ajuste o namespace após renomear)

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Se usar SQLite: opt.UseSqlite(...)

builder.Services.AddAutoMapper(typeof(ProductProfile)); // ou o seu profile principal

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
