using System;
using System.Threading.Tasks;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace UnitTests;

public class AppDbContext_Basic_Tests
{
    [Fact]
    public async Task DevePersistir_Customer_Product_Order()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"db_ctx_{Guid.NewGuid()}")
            .Options;

        using var db = new AppDbContext(options);

        var customer = new Customer { Name = "Cliente", Email = "c@c.com" };
        var product  = new Product  { Name = "Produto", Price = 99.9m, Quantity = 7 };

        db.Customers.Add(customer);
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var order = new Order
        {
            CustomerId = customer.Id,
            ProductId  = product.Id,
            Quantity   = 2,
            Price      = product.Price,
            Status     = OrderStatus.Enviado,
            DeliveryZipCode = "01001000",
            DeliveryAddress = "Rua X, 123"
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var loaded = await db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Product)
            .FirstAsync(o => o.Id == order.Id);

        loaded.Customer.Should().NotBeNull();
        loaded.Product.Should().NotBeNull();
        loaded.Quantity.Should().Be(2);
    }
}
