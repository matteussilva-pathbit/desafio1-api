using Xunit;

namespace UnitTests;

public class UnitTest1
{
    [Fact]
    public void Sanity_check()
    {
        Assert.True(true);
    }

    [Fact]
    public void Another_simple_check()
    {
        var sum = 2 + 2;
        Assert.Equal(4, sum);
    }
}
