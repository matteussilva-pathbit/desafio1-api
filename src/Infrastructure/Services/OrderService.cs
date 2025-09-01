using Application.DTOs.Orders;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.External;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _ctx;
    private readonly IViaCepService _viaCep;

    public OrderService(AppDbContext ctx, IViaCepService viaCep)
    {
        _ctx = ctx;
        _viaCep = viaCep;
    }

    public async Task<Guid?> CreateAsync(OrderCreateDto dto, Guid customerId, CancellationToken ct = default)
    {
        var addr = await _viaCep.GetAsync(dto.Cep, ct);
        if (addr is null || addr.erro == true)
            return null; // endereço inválido

        var product = await _ctx.Products.FirstOrDefaultAsync(p => p.Id == dto.ProductId, ct);
        if (product is null) return null;
        if (product.QuantityAvailable < dto.Quantity) return null;

        using var tx = await _ctx.Database.BeginTransactionAsync(ct);
        try
        {
            product.QuantityAvailable -= dto.Quantity;

            var order = new Order
            {
                CustomerId = customerId,
                Cep = dto.Cep,
                Address = addr.ToString()
            };
            _ctx.Orders.Add(order);

            var item = new OrderItem
            {
                Order = order,
                ProductId = product.Id,
                Quantity = dto.Quantity,
                UnitPrice = product.Price
            };
            _ctx.OrderItems.Add(item);

            await _ctx.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return order.Id;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}
