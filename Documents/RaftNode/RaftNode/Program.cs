using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using DotNext;
using DotNext.Net.Cluster.Consensus.Raft;
using DotNext.Net.Cluster.Consensus.Raft.Http;
using DotNext.Net.Cluster.Consensus.Raft.Membership;
using Microsoft.AspNetCore.Connections;
using RaftNode;
using static System.Globalization.CultureInfo;
using SslOptions = DotNext.Net.Security.SslOptions;


await StartNode(int.Parse(args[0]));

static async Task UseAspNetCoreHost(int port, string? persistentStorage = null)
{
    var configuration = new Dictionary<string, string?>
    {
        { "partitioning", "false" },
        { "lowerElectionTimeout", "500" }, 
        { "upperElectionTimeout", "1000" }, 
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
        .UseInMemoryConfigurationStorage(AddClusterMembers)
        .ConfigureCluster<ClusterConfigurator>()
        .AddSingleton<IHttpMessageHandlerFactory, RaftClientHandlerFactory>()
        .AddOptions()
        .AddRouting();

    if (!string.IsNullOrWhiteSpace(persistentStorage))
    {
        builder.Services.UsePersistenceEngine<ISupplier<long>, SimplePersistentState>()
            .AddSingleton<IHostedService, DataModifier>();
    }

    ConfigureLogging(builder.Logging);
    builder.JoinCluster();
    builder.WebHost.UseKestrelHttpsConfiguration();

    await using var app = builder.Build();

    const string leaderResource = "/leader";
    const string valueResource = "/value";
    app.UseConsensusProtocolHandler()
        .RedirectToLeader(leaderResource)
        .UseRouting()
        .UseEndpoints(static endpoints =>
        {
            endpoints.MapGet(leaderResource, RedirectToLeaderAsync);
            endpoints.MapGet(valueResource, GetValueAsync);
        });


    await app.RunAsync();

    static Task RedirectToLeaderAsync(HttpContext context)
    {
        var cluster = context.RequestServices.GetRequiredService<IRaftCluster>();
        return context.Response.WriteAsync($"Leader address is {cluster.Leader?.EndPoint}. Current address is {context.Connection.LocalIpAddress}:{context.Connection.LocalPort}", context.RequestAborted);
    }

    static async Task GetValueAsync(HttpContext context)
    {
        var cluster = context.RequestServices.GetRequiredService<IRaftCluster>();
        var provider = context.RequestServices.GetRequiredService<ISupplier<long>>();

        await cluster.ApplyReadBarrierAsync(context.RequestAborted);
        await context.Response.WriteAsync(provider.Invoke().ToString(InvariantCulture), context.RequestAborted);
    }

    // NOTE: this way of adding members to the cluster is not recommended in production code
    static void AddClusterMembers(ICollection<UriEndPoint> members)
    {
        members.Add(new UriEndPoint(new("http://localhost:3262", UriKind.Absolute)));
        members.Add(new UriEndPoint(new("http://localhost:3263", UriKind.Absolute)));
        members.Add(new UriEndPoint(new("http://localhost:3264", UriKind.Absolute)));
    }
}

static async Task UseConfiguration(RaftCluster.NodeConfiguration config, string? persistentStorage)
{
    var loggerFactory = LoggerFactory.Create(ConfigureLogging);
    config.LoggerFactory = loggerFactory;
    using var cluster = new RaftCluster(config);
    cluster.LeaderChanged += ClusterConfigurator.LeaderChanged;
    var modifier = default(DataModifier?);
    if (!string.IsNullOrEmpty(persistentStorage))
    {
        var state = new SimplePersistentState(persistentStorage);
        cluster.AuditTrail = state;
        modifier = new DataModifier(cluster, state);
    }
    await cluster.StartAsync(CancellationToken.None);
    await (modifier?.StartAsync(CancellationToken.None) ?? Task.CompletedTask);

    await (modifier?.StopAsync(CancellationToken.None) ?? Task.CompletedTask);
    await cluster.StopAsync(CancellationToken.None);
}

static void ConfigureLogging(ILoggingBuilder builder)
    => builder.AddConsole().SetMinimumLevel(LogLevel.Debug);

static Task UseTcpTransport(int port, string? persistentStorage, bool useSsl)
{
    var configuration = new RaftCluster.TcpConfiguration(new IPEndPoint(IPAddress.Loopback, port))
    {
        RequestTimeout = TimeSpan.FromSeconds(140), //  5 segundos
        LowerElectionTimeout = 500, //  500 ms
        UpperElectionTimeout = 1000, //  1000 ms
        TransmissionBlockSize = 4096,
        ColdStart = false,
        SslOptions = null
    };

    return UseConfiguration(configuration, persistentStorage);

}

static Task StartNode(int port, string? persistentStorage = null)
{

            return UseAspNetCoreHost(port, persistentStorage);

}

