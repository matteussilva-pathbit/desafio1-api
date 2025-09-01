using Common.Security;
using Xunit;

public class PasswordHasherTests
{
    [Fact]
    public void Sha256_ReturnsHex()
    {
        var h = PasswordHasher.Sha256("abc");
        Assert.Equal(64, h.Length);
        Assert.True(System.Text.RegularExpressions.Regex.IsMatch(h, "^[A-F0-9]{64}$"));
    }
}
