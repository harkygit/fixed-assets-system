using Xunit;

public class DepreciationAdapterTests
{
    [Fact]
    public void Depreciation_Should_Be_Calculated()
    {
        // Arrange

        decimal cost = 120000;

        int usefulLife = 5;

        // Act

        decimal depreciation =
            cost / usefulLife;

        // Assert

        Assert.Equal(
            24000,
            depreciation
        );
    }
}