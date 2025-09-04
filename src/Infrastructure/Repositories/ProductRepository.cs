using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public sealed class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _ctx;
        public ProductRepository(AppDbContext ctx) => _ctx = ctx;

        public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct)
            => _ctx.Products.FirstOrDefaultAsync(p => p.Id == id, ct);

        public async Task AddAsync(Product product, CancellationToken ct)
            => await _ctx.Products.AddAsync(product, ct);

        public void Update(Product product)
            => _ctx.Products.Update(product);
    }
}
