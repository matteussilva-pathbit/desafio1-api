using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.External; // <-- IMPORTANTE: usa a pasta External
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace UnitTests;

public class OrderService_InMemory_Tests
{
    private class FakeViaCep : IViaCepService
    {
        public Task<ViaCepAddress?> GetAddressAsync(string cep, CancellationToken ct = default)
            => Task.FromResult<ViaCepAddress?>(new ViaCepAddress
            {
                Cep = cep,
                Street = "Rua Teste",
                City = "Sao Paulo",
                Uf = "SP"
            });
    }

    [Fact]
    public async Task CreateAsync_ComEstoqueOk_GravaPedidoEDebitaEstoque()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"db_order_{Guid.NewGuid()}")
            .Options;

        var mapperCfg = new MapperConfiguration(cfg =>
        {
            // Se seu OrderService retorna DTO, mapeie aqui. Se retorna entidade, não precisa.
            // Exemplo (ajuste se necessário):
            // cfg.CreateMap<Order, OrderReadDto>();
        });
        var mapper = mapperCfg.CreateMapper();

        using var db = new AppDbContext(options);

        var customer = new Customer { Name = "Cliente", Email = "c@c.com" };
        var product  = new Product  { Name = "Produto", Price = 100m, Quantity = 5 };

        db.Customers.Add(customer);
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var viaCep  = new FakeViaCep();
        var service = new OrderService(db, viaCep /*, mapper se o ctor pedir */);

        // Assinatura comum que você vinha usando:
        // CreateAsync(Guid customerId, Guid productId, int quantity, string cep, CancellationToken)
        var created = await service.CreateAsync(customer.Id, product.Id, 3, "01001000", CancellationToken.None);

        created.Should().NotBeNull();

        var prodDb = await db.Products.AsNoTracking().FirstAsync(p => p.Id == product.Id);
        prodDb.Quantity.Should().Be(2); // 5 - 3
    }
}
