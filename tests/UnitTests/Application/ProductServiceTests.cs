using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Services;              // ProductService
using Common;
using Domain.Entities;
using Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    // use o tipo CONCRETO agora
    private readonly ProductService _svc;

    public ProductServiceTests()
    {
        _svc = new ProductService(_repo.Object, _uow.Object);
    }

    [Fact]
    public async Task UpdatePrice_Deve_falhar_quando_preco_invalido()
    {
        var act = () => _svc.UpdatePriceAsync(Guid.NewGuid(), 0m, CancellationToken.None);
        await act.Should().ThrowAsync<DomainException>().WithMessage("*Preço*");
    }

    [Fact]
    public async Task UpdatePrice_Deve_falhar_quando_produto_nao_encontrado()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Product?)null);

        var act = () => _svc.UpdatePriceAsync(Guid.NewGuid(), 10m, CancellationToken.None);
        await act.Should().ThrowAsync<DomainException>().WithMessage("*Produto*");
    }

    [Fact]
    public async Task UpdatePrice_Deve_atualizar_preco()
    {
        var product = Product.Create("P1", 5m, 10);
        _repo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        await _svc.UpdatePriceAsync(product.Id, 12.34m, CancellationToken.None);

        product.Preco.Should().Be(12.34m);
    }

    [Fact]
    public async Task AdjustInventory_Deve_ignorar_quando_delta_zero()
    {
        var product = Product.Create("P1", 5m, 10);
        _repo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        await _svc.AdjustInventoryAsync(product.Id, 0, CancellationToken.None);

        product.QuantityAvailable.Should().Be(10);
    }

    [Fact]
    public async Task AdjustInventory_Deve_incrementar_quando_delta_positivo()
    {
        var product = Product.Create("P1", 5m, 10);
        _repo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        await _svc.AdjustInventoryAsync(product.Id, 3, CancellationToken.None);

        product.QuantityAvailable.Should().Be(13);
    }

    [Fact]
    public async Task AdjustInventory_Deve_debitar_quando_delta_negativo()
    {
        var product = Product.Create("P1", 5m, 10);
        _repo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        await _svc.AdjustInventoryAsync(product.Id, -4, CancellationToken.None);

        product.QuantityAvailable.Should().Be(6);
    }
}
