using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.External;

public class OrderService : IOrderService
{
    private readonly AppDbContext _ctx;
    private readonly IViaCepService _viaCep;

    public OrderService(AppDbContext ctx, IViaCepService viaCep)
    {
        _ctx = ctx;
        _viaCep = viaCep;
    }

    public async Task<Order> CreateAsync(Guid customerId, Guid productId, int quantity, string cep, CancellationToken ct = default)
    {
        if (quantity <= 0) throw new ArgumentException("Quantidade inválida.", nameof(quantity));

        var product = await _ctx.Products.FirstOrDefaultAsync(p => p.Id == productId, ct)
                      ?? throw new InvalidOperationException("Produto não existe.");

        if (product.QuantityAvailable < quantity)
            throw new InvalidOperationException("Estoque insuficiente.");

        var addr = await _viaCep.GetAddressAsync(cep, ct);
        if (addr is null || addr.Erro)
            throw new InvalidOperationException("CEP inválido ou não encontrado.");

        await using var tx = await _ctx.Database.BeginTransactionAsync(ct);

        var order = new Order
        {
            CustomerId = customerId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.ENVIADO,
            DeliveryZipCode = cep,
            DeliveryAddress = $"{addr.Logradouro}, {addr.Bairro}, {addr.Localidade}-{addr.Uf}"
        };

        var item = new OrderItem
        {
            OrderId = order.Id,
            ProductId = product.Id,
            Quantity = quantity,
            UnitPrice = product.Price
        };

        order.Items = new List<OrderItem> { item };

        _ctx.Orders.Add(order);
        product.QuantityAvailable -= quantity;

        await _ctx.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return order;
    }
}
