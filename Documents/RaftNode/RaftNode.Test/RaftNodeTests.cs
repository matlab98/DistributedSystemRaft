
    using System.Net;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Xunit;
    using DotNext.Net.Cluster.Consensus.Raft;
using Microsoft.AspNetCore.Connections;
using DotNext;
namespace RaftNode.Test;
    public class RaftNodeTests
    {
        [Fact]
        public async Task RedirectToLeaderAsync_ReturnsLeaderAddress()
        {
            // Arrange
            var mockCluster = new Mock<IRaftCluster>();
        Moq.Language.Flow.IReturnsResult<IRaftCluster> returnsResult = mockCluster.Setup(c => c.Leader).Returns(new UriEndPoint(new("http://localhost:3262")));

            var services = new ServiceCollection();
            services.AddSingleton(mockCluster.Object);
            var provider = services.BuildServiceProvider();

            var context = new DefaultHttpContext { RequestServices = provider };

            // Act
            await RedirectToLeaderAsync(context);

            // Assert
            Assert.Contains("Leader address is http://localhost:3262", context.Response.Body.ToString());
        }

        [Fact]
        public async Task GetValueAsync_ReturnsStoredValue()
        {
            // Arrange
            var mockCluster = new Mock<IRaftCluster>();
            var mockProvider = new Mock<ISupplier<long>>();
            mockProvider.Setup(p => p.Invoke()).Returns(42);

            var services = new ServiceCollection();
            services.AddSingleton(mockCluster.Object);
            services.AddSingleton(mockProvider.Object);
            var provider = services.BuildServiceProvider();

            var context = new DefaultHttpContext { RequestServices = provider };

            // Act
            await GetValueAsync(context);

            // Assert
            Assert.Contains("42", context.Response.Body.ToString());
        }
    }
