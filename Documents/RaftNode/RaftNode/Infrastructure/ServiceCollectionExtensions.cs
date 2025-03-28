using DotNext.Net.Cluster.Consensus.Raft;
using RaftNode.Cluster;
using RaftNode.Services;

namespace RaftNode.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureClusterServices(this IServiceCollection services)
    {
        services.UseInMemoryConfigurationStorage(ClusterMembership.AddClusterMembers)
                .ConfigureCluster<ClusterConfigurator>()
                .AddSingleton<IHttpMessageHandlerFactory, RaftClientHandlerFactory>()
                .AddRouting();
        return services;
    }
}
