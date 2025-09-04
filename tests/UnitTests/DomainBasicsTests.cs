using System;
using Domain;
using Domain.Entities;
using Xunit;

namespace UnitTests.Domain
{
    public class DomainBasicsTests
    {
        [Fact]
        public void Product_NaoPermitePrecoZero()
        {
            Assert.Throws<DomainException>(() => Product.Create("Produto X", 0m, 10));
        }

        [Fact]
        public void Order_NaoPermiteQuantidadeZero()
        {
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            Assert.Throws<DomainException>(() => Order.Create(customerId, productId, 0, 100m, "01001-000", "Rua A, 123"));
        }
    }
}
