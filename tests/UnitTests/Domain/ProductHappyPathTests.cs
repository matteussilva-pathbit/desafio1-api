using Domain.Entities;
using FluentAssertions;
using Xunit;

public class ProductHappyPathTests
{
    [Fact]
    public void Create_Deve_criar_produto_valido()
    {
        var p = Product.Create("P", 9.99m, 2);
        p.Nome.Should().Be("P");
        p.Preco.Should().Be(9.99m);
        p.QuantityAvailable.Should().Be(2);
    }
}
