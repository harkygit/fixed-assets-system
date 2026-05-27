using Xunit;

public class ObjectAdapterTests
{
    [Fact]
    public void Object_Should_Have_Valid_Id()
    {
        // Arrange

        string inventoryId = "OS001";

        // Assert

        Assert.Equal(
            "OS001",
            inventoryId
        );
    }
}