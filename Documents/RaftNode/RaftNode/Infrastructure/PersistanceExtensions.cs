using DotNext.Net.Cluster.Consensus.Raft;
using DotNext;
using RaftNode.Services;

namespace RaftNode.Infrastructure;

public static class PersistenceExtensions
{
    public static IServiceCollection ConfigurePersistence(this IServiceCollection services)
    {
        services.UsePersistenceEngine<ISupplier<long>, SimplePersistentState>()
                .AddSingleton<IHostedService, DataModifier>();
        return services;
    }
}
