using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Services;
using Common;
using Domain.Entities;
using Domain.Repositories;
using Moq;
using Xunit;

public class ProductServiceTests
{
    [Fact]
    public async Task UpdatePrice_Atualiza_Preco()
    {
        var id = Guid.NewGuid();
        var repo = new Mock<IProductRepository>();
        var uow = new Mock<IUnitOfWork>();
        var p = Product.Create("Item", 10m, 5);
        typeof(Product).GetProperty(nameof(Product.Id))!.SetValue(p, id);
        repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(p);

        var svc = new ProductService(repo.Object, uow.Object);
        await svc.UpdatePriceAsync(id, 25m, CancellationToken.None);

        Assert.Equal(25m, p.Preco);
        repo.Verify(r => r.Update(p), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePrice_PrecoInvalido_Dispara()
    {
        var svc = new ProductService(Mock.Of<IProductRepository>(), Mock.Of<IUnitOfWork>());
        await Assert.ThrowsAsync<DomainException>(() => svc.UpdatePriceAsync(Guid.NewGuid(), 0m, CancellationToken.None));
    }
}
