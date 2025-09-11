using Common;
using Domain.Entities;
using FluentAssertions;
using Xunit;

public class ProductTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Create_Deve_invalidar_nome(string nome)
    {
        FluentActions.Invoking(() => Product.Create(nome!, 10m, 1))
            .Should().Throw<DomainException>().WithMessage("*Nome*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Create_Deve_invalidar_preco(decimal preco)
    {
        FluentActions.Invoking(() => Product.Create("P", preco, 1))
            .Should().Throw<DomainException>().WithMessage("*Preço*");
    }

    [Fact]
    public void IncreaseInventory_Deve_exigir_delta_positivo()
    {
        var p = Product.Create("P", 10m, 1);
        FluentActions.Invoking(() => p.IncreaseInventory(0))
            .Should().Throw<DomainException>().WithMessage("*Delta*");
    }

    [Fact]
    public void DebitInventory_Nao_pode_ficar_negativo()
    {
        var p = Product.Create("P", 10m, 1);
        FluentActions.Invoking(() => p.DebitInventory(2))
            .Should().Throw<DomainException>().WithMessage("*Estoque*");
    }
}
