using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using DotNext.Net.Cluster.Consensus.Raft;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotNext;
using DotNext.Net.Cluster;

public class RaftControllerTests
{
    private readonly Mock<IRaftCluster> _clusterMock;
    private readonly Mock<ISupplier<long>> _providerMock;
    private readonly RaftController _controller;

    public RaftControllerTests()
    {
        _clusterMock = new Mock<IRaftCluster>();
        _providerMock = new Mock<ISupplier<long>>();
        _controller = new RaftController(_clusterMock.Object, _providerMock.Object);
    }

    [Fact]
    public void GetLeader_ShouldReturnLeaderAddress()
    {
        // Arrange
        var testLeader = Mock.Of<IClusterMember>(m => m.EndPoint == new IPEndPoint(IPAddress.Loopback, 5000));
        _clusterMock.Setup(c => c.Leader).Returns(testLeader);

        // Act
        var result = _controller.GetLeader() as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Contains("Leader address is", result.Value.ToString());
    }

    [Fact]
    public async Task GetValue_ShouldReturn503IfNoLeader()
    {
        // Arrange
        _clusterMock.Setup(c => c.Leader).Returns((IClusterMember?)null);

        // Act
        var result = await _controller.GetValue(CancellationToken.None) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(503, result.StatusCode);
        Assert.Equal("Leader node is not yet elected. Please try again later.", result.Value);
    }

}
