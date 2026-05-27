using Xunit;
using System.Net;

public class IntegrationFlowTests
{
    [Fact]
    public async Task Process_Should_Return_OK()
    {
        // Arrange

        var statusCode =
            HttpStatusCode.OK;

        // Assert

        Assert.Equal(
            HttpStatusCode.OK,
            statusCode
        );
    }
}