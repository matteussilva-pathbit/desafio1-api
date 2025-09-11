using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Services;            // OrderService, CreateOrderRequest
using Common;                          // DomainException
using Domain.Entities;                 // Customer, Product, Order
using Domain.Repositories;             // IOrderRepository, IProductRepository, ICustomerRepository, IUnitOfWork
using FluentAssertions;
using Moq;
using Xunit;
using Application.Models;              // ViaCepResponse
using Application.Interface;           // IViaCepService (singular)
using SCreateOrderRequest = Application.Services.CreateOrderRequest; // alias do request

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orders = new();
    private readonly Mock<IProductRepository> _products = new();
    private readonly Mock<ICustomerRepository> _customers = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IViaCepService> _viaCep = new();

    private readonly OrderService _svc;

    public OrderServiceTests()
    {
        _svc = new OrderService(
            _orders.Object,
            _products.Object,
            _customers.Object,
            _uow.Object,
            _viaCep.Object
        );
    }

    [Fact]
    public async Task Deve_criar_pedido_quando_tudo_valido()
    {
        var customer = Customer.Create("Fulano", "f@f.com");
        var product  = Product.Create("Produto X", 10m, 5);

        _customers.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _products.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        _viaCep.Setup(v => v.BuscarEnderecoAsync("01001000", It.IsAny<CancellationToken>()))
               .ReturnsAsync(new ViaCepResponse { Logradouro = "Praça da Sé", Bairro = "Sé", Localidade = "São Paulo", Uf = "SP" });

        _orders.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        _uow.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _uow.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var req = new SCreateOrderRequest { ProductId = product.Id, Quantity = 2, Cep = "01001000" };

        var id = await _svc.CreateOrderAsync(req, customer.Id, CancellationToken.None);

        id.Should().NotBeEmpty();
        product.QuantityAvailable.Should().Be(3); // 5 - 2
    }

    [Fact]
    public async Task Nao_deve_aceitar_quantidade_menor_ou_igual_a_zero()
    {
        var req = new SCreateOrderRequest { ProductId = Guid.NewGuid(), Quantity = 0, Cep = "01001000" };
        var act = () => _svc.CreateOrderAsync(req, Guid.NewGuid(), CancellationToken.None);
        await act.Should().ThrowAsync<DomainException>().WithMessage("*Quantidade*");
    }

    [Fact]
    public async Task Deve_lancar_erro_se_cliente_invalido()
    {
        _customers.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync((Customer?)null);

        var req = new SCreateOrderRequest { ProductId = Guid.NewGuid(), Quantity = 1, Cep = "01001000" };
        var act = () => _svc.CreateOrderAsync(req, Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*Cliente*");
    }

    [Fact]
    public async Task Deve_lancar_erro_se_produto_nao_encontrado()
    {
        var customer = Customer.Create("Fulano", "f@f.com");
        _customers.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        _products.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Product?)null);

        var req = new SCreateOrderRequest { ProductId = Guid.NewGuid(), Quantity = 1, Cep = "01001000" };
        var act = () => _svc.CreateOrderAsync(req, customer.Id, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*Produto*");
    }

    [Fact]
    public async Task Deve_lancar_erro_se_estoque_insuficiente()
    {
        var customer = Customer.Create("Fulano", "f@f.com");
        var product  = Product.Create("Produto X", 10m, 1);

        _customers.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _products.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var req = new SCreateOrderRequest { ProductId = product.Id, Quantity = 2, Cep = "01001000" };
        var act = () => _svc.CreateOrderAsync(req, customer.Id, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*Estoque*");
    }

    [Fact]
    public async Task Deve_negar_se_cep_invalido()
    {
        var customer = Customer.Create("Fulano", "f@f.com");
        var product  = Product.Create("Produto X", 10m, 10);

        _customers.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _products.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        _viaCep.Setup(v => v.BuscarEnderecoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((ViaCepResponse?)null);

        var req = new SCreateOrderRequest { ProductId = product.Id, Quantity = 1, Cep = "00000000" };
        var act = () => _svc.CreateOrderAsync(req, customer.Id, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*CEP*");
    }

    [Fact]
    public async Task Deve_usar_AddressOverride_sem_chamar_ViaCep()
    {
        var customer = Customer.Create("Fulano", "f@f.com");
        var product  = Product.Create("Produto X", 10m, 5);

        _customers.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _products.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var req = new SCreateOrderRequest { ProductId = product.Id, Quantity = 1, Cep = "qualquer", AddressOverride = "Rua X, 123" };

        await _svc.CreateOrderAsync(req, customer.Id, CancellationToken.None);

        _viaCep.Verify(v => v.BuscarEnderecoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        product.QuantityAvailable.Should().Be(4);
    }

    [Fact]
    public async Task Deve_fazer_Rollback_se_AddAsync_lancar_excecao()
    {
        var customer = Customer.Create("Fulano", "f@f.com");
        var product  = Product.Create("Produto X", 10m, 5);

        _customers.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _products.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        _uow.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _orders.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new Exception("falha X"));

        var req = new SCreateOrderRequest { ProductId = product.Id, Quantity = 1, Cep = "01001000", AddressOverride = "Rua Y, 999" };

        Func<Task> act = () => _svc.CreateOrderAsync(req, customer.Id, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>().WithMessage("falha X");

        _uow.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
