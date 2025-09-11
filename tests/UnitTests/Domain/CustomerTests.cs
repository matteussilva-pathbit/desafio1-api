using Common;
using Domain.Entities;
using FluentAssertions;
using Xunit;

public class CustomerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Create_Deve_invalidar_nome(string nome)
    {
        FluentActions.Invoking(() => Customer.Create(nome!, "a@a.com"))
            .Should().Throw<DomainException>().WithMessage("*Nome*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Create_Deve_invalidar_email(string email)
    {
        FluentActions.Invoking(() => Customer.Create("Fulano", email!))
            .Should().Throw<DomainException>().WithMessage("*Email*");
    }
}
