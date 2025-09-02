using Common.Security;
using FluentAssertions;
using Xunit;

namespace UnitTests;

public class PasswordHasherTests
{
    [Theory]
    [InlineData("senha123")]
    [InlineData("PathBit@2025")]
    public void Hash_DeveGerarHex64_E_SerDeterministico(string plain)
    {
        var h1 = PasswordHasher.Hash(plain);
        var h2 = PasswordHasher.Hash(plain);

        h1.Should().NotBeNullOrWhiteSpace();
        h1.Length.Should().Be(64);        // SHA256 em hex = 64 chars
        h1.Should().Be(h2);               // determinístico
        h1.Should().NotContain(plain);    // não contém texto puro
    }
}
