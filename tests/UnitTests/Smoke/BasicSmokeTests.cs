using Xunit;

namespace UnitTests.Smoke;

public class BasicSmokeTests
{
    [Fact]
    public void Math_should_work()
    {
        Assert.Equal(2, 1 + 1);
    }
}
