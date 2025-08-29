using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _ctx;
    public ProductRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<List<Product>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.Products.AsNoTracking().ToListAsync(ct);

    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _ctx.Products.FindAsync([id], ct);

    public async Task<Product> AddAsync(Product product, CancellationToken ct = default)
    {
        _ctx.Products.Add(product);
        await _ctx.SaveChangesAsync(ct);
        return product;
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        _ctx.Products.Update(product);
        await _ctx.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Product product, CancellationToken ct = default)
    {
        _ctx.Products.Remove(product);
        await _ctx.SaveChangesAsync(ct);
    }
}
