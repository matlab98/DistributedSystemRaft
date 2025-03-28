using RaftNode.Cluster;
using RaftNode.Services;
using DotNext.Net.Cluster.Consensus.Raft.Http;
using DotNext.Net.Cluster.Consensus.Raft;
using DotNext;
using RaftNode;
using RaftNode.Infrastructure;
using RaftNode.Configuration;

var port = args.Length > 0 ? int.Parse(args[0]) : 5000;
var persistentStorage = args.Length > 1 ? args[1] : null;

await StartNode(port, persistentStorage);

static async Task StartNode(int port, string? persistentStorage = null)
{
    var configuration = new Dictionary<string, string?>
    {
        { "partitioning", "false" },
        { "lowerElectionTimeout", "1500" },
        { "upperElectionTimeout", "3000" },
        { "requestTimeout", "00:00:10" },
        { "publicEndPoint", $"http://localhost:{port}" },
        { "coldStart", "false" },
        { "requestJournal:memoryLimit", "5" },
        { "requestJournal:expiration", "00:02:00" },
        { SimplePersistentState.LogLocation, persistentStorage },
    };

    var builder = WebApplication.CreateSlimBuilder();
    builder.Configuration.AddInMemoryCollection(configuration);
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenLocalhost(port);
    });

    builder.Services
        .UseInMemoryConfigurationStorage(ClusterMembership.AddClusterMembers)
        .ConfigureCluster<ClusterConfigurator>()
        .AddSingleton<IHttpMessageHandlerFactory, RaftClientHandlerFactory>()
        .ConfigureLogging()
        .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.IncludeFields = true; // Permite serializar todos los campos
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; // Ignora mayúsculas y minúsculas en nombres de propiedades
    });

    if (!string.IsNullOrWhiteSpace(persistentStorage))
    {
        builder.Services.ConfigurePersistence();
    }
    builder.Services.AddSingleton<ISupplier<long>>(provider =>
    {
        var state = provider.GetRequiredService<SimplePersistentState>();
        return state; // Asegura que se use la misma instancia del estado persistente
    });
    builder.Services.AddSingleton<SimplePersistentState>(provider =>
    {
        var config = provider.GetRequiredService<IConfiguration>();
        var storagePath = config[SimplePersistentState.LogLocation] ?? $"raft_{port}state.log";
        return new SimplePersistentState(storagePath);
    });

    builder.JoinCluster();
    builder.WebHost.UseKestrelHttpsConfiguration();

    await using var app = builder.Build();

    app.UseConsensusProtocolHandler();
    app.UseRouting();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers(); // Mapeamos los controladores
    });

    await app.RunAsync();
}
