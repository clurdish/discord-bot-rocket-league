using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using HCC.Discord.RocketLeague.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HCC.Discord.RocketLeague.Console;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(builder =>
            {
                builder.AddUserSecrets<Program>();
            })
            .ConfigureDiscordHost((context, config) =>
            {
                config.SocketConfig = new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Verbose,
                    AlwaysDownloadUsers = true,
                    MessageCacheSize = 200,
                };

                config.Token = context.Configuration.GetValue<string>("Discord:Bot:Token");
            })
            .ConfigureServices(ConfigureServices)
            .Build();

        await host.RunAsync();
    }

    public static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<RocketLeagueService>();
        services.AddHostedService<DiscordHandler>();
    }
}