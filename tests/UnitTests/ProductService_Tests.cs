using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Product;
using Application.Interfaces;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace UnitTests;

public class ProductService_Tests
{
    private readonly Mock<IProductRepository> _repo = new();
    private readonly IMapper _mapper;

    public ProductService_Tests()
    {
        var cfg = new MapperConfiguration(c =>
        {
            c.CreateMap<ProductCreateDto, Product>();
            c.CreateMap<ProductUpdateDto, Product>();
            c.CreateMap<Product, ProductReadDto>();
        });
        _mapper = cfg.CreateMapper();
    }

    [Fact]
    public async Task CreateAsync_DeveCriarERetornarReadDto()
    {
        var svc = new ProductService(_repo.Object, _mapper);

        var dto = new ProductCreateDto
        {
            Name = "Teclado",
            Price = 150.0m,
            Quantity = 10 // remova se não existir no seu DTO
        };

        _repo
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns<Product, CancellationToken>((p, _) => Task.FromResult(p));

        var result = await svc.CreateAsync(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Teclado");
        result.Price.Should().Be(150.0m);
    }

    [Fact]
    public async Task GetAllAsync_DeveRetornarLista()
    {
        var svc = new ProductService(_repo.Object, _mapper);

        var list = new[]
        {
            new Product { Id = Guid.NewGuid(), Name = "A", Price = 10, Quantity = 1 },
            new Product { Id = Guid.NewGuid(), Name = "B", Price = 20, Quantity = 2 },
        };

        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(list);

        var result = await svc.GetAllAsync(CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAsync_DeveAtualizar()
    {
        var id = Guid.NewGuid();
        var existing = new Product { Id = id, Name = "Velho", Price = 10, Quantity = 1 };
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
             .ReturnsAsync(existing);

        var svc = new ProductService(_repo.Object, _mapper);

        var upd = new ProductUpdateDto { Name = "Novo", Price = 99, Quantity = 3 };
        await svc.UpdateAsync(id, upd, CancellationToken.None);

        _repo.Verify(r => r.UpdateAsync(It.Is<Product>(p => p.Name == "Novo" && p.Price == 99 && p.Quantity == 3),
                                        It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_DeveChamarRepositorio()
    {
        var id = Guid.NewGuid();
        var svc = new ProductService(_repo.Object, _mapper);

        await svc.DeleteAsync(id, CancellationToken.None);

        _repo.Verify(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
