namespace RaftNode.Configuration;

public static class LoggingExtensions
{
    public static IServiceCollection ConfigureLogging(this IServiceCollection services)
    {
        services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Debug);
        });
        return services;
    }
}
