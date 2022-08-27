using Discord;
using Discord.WebSocket;
using HCC.Discord.RocketLeague.Domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HCC.Discord.RocketLeague.AzureWebJob;

class Program
{
    static async Task Main()
    {
        var builder = new HostBuilder();
        builder.ConfigureWebJobs(b =>
        {
            b.AddAzureStorageCoreServices();
        });
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<RocketLeagueService>();
        });
        var host = builder.Build();
        using (host)
        {
            var jobHost = host.Services.GetService(typeof(IJobHost)) as JobHost;
            await host.StartAsync();
            await jobHost.CallAsync(nameof(Functions.RunDiscordClient));
            //var hostTask = host.RunAsync();
            //var discordClientTask = RunDiscordClient();

            //await Task.WhenAny(hostTask, discordClientTask);
        }
    }

    //static async Task RunDiscordClient()
    //{
    //    using var client = new DiscordSocketClient();
    //    var token = Environment.GetEnvironmentVariable("Discord:Bot:Token");

    //    client.Log += async msg =>
    //    {
    //        Console.WriteLine(msg);
    //    };

    //    await client.LoginAsync(TokenType.Bot, token);
    //    await client.StartAsync();

    //    client.Ready += async () =>
    //    {
    //        ListGuilds(client);
    //    };
    //    ListGuilds(client);

    //    await Task.Delay(-1);
    //}

    //static void ListGuilds(DiscordSocketClient client)
    //{
    //    Console.WriteLine("Guilds:");
    //    foreach(var guild in client.Guilds)
    //    {
    //        Console.WriteLine(guild.Name);
    //    }
    //    Console.WriteLine();
    //}
}