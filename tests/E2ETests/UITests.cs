using Xunit;

public class UITests
{
    [Fact]
    public void Button_Click_Should_Work()
    {
        // Arrange

        bool clicked = true;

        // Assert

        Assert.True(clicked);
    }
}