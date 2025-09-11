using Common.Security;
using FluentAssertions;
using Xunit;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_Deve_retornar_SHA256_em_hex_minusculo_para_senha123()
    {
        var hash = PasswordHasher.Hash("senha123");
        // SHA256("senha123")
        hash.Should().Be("55a5e9e78207b4df8699d60886fa070079463547b095d1a05bc719bb4e6cd251");
        hash.Should().HaveLength(64).And.MatchRegex("^[0-9a-f]{64}$");
    }

    [Fact]
    public void Hash_Deve_ser_deterministico_para_string_vazia()
    {
        var h1 = PasswordHasher.Hash(string.Empty);
        var h2 = PasswordHasher.Hash(string.Empty);
        h1.Should().Be(h2);
        h1.Should().HaveLength(64).And.MatchRegex("^[0-9a-f]{64}$");
    }
}
