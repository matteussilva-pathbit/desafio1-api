using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Services;
using Application.Interface;
using Application.Models;
using Common;
using Domain.Entities;
using Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

public class OrderServiceAdapterTests
{
    [Fact]
    public async Task CreateAsync_Deve_criar_pedido_via_adapter()
    {
        var orders     = new Mock<IOrderRepository>();
        var products   = new Mock<IProductRepository>();
        var customers  = new Mock<ICustomerRepository>();
        var uow        = new Mock<IUnitOfWork>();
        var viaCep     = new Mock<IViaCepService>();

        var svc = new OrderService(orders.Object, products.Object, customers.Object, uow.Object, viaCep.Object);

        var customer = Customer.Create("Fulano", "f@f.com");
        var product  = Product.Create("Produto X", 10m, 5);

        customers.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        products .Setup(r => r.GetByIdAsync(product.Id,  It.IsAny<CancellationToken>())).ReturnsAsync(product);

        viaCep.Setup(v => v.BuscarEnderecoAsync("01001000", It.IsAny<CancellationToken>()))
              .ReturnsAsync(new ViaCepResponse { Logradouro = "Rua A", Bairro = "B", Localidade = "C", Uf = "SP" });

        uow.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        uow.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        orders.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var id = await svc.CreateAsync(customer.Id, product.Id, 2, "01001000", CancellationToken.None);

        id.Should().NotBeEmpty();
        product.QuantityAvailable.Should().Be(3);
    }
}
