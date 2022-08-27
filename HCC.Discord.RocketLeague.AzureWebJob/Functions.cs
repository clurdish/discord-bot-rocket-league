using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs;
using System.Configuration;
using HCC.Discord.RocketLeague.Domain;
using Microsoft.Extensions.Configuration;

namespace HCC.Discord.RocketLeague.AzureWebJob
{
    public class Functions
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly RocketLeagueService _rocketLeagueService;

        public Functions(ILogger<Functions> logger, IConfiguration config, RocketLeagueService rocketLeagueService)
        {
            _logger = logger;
            _config = config;
            _rocketLeagueService = rocketLeagueService;
        }

        [NoAutomaticTrigger]
        public async Task RunDiscordClient([TimerTrigger("0 */5 * * * *")] ILogger logger)
        {
            using var client = new DiscordSocketClient();
            var token = _config.GetValue<string>("Discord:Bot:Token");

            client.Log += HandleLog;

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            client.Ready += async () =>
            {
                Console.WriteLine("Ready");
                ListGuilds(client);
                await DoEventsWork(client);
            };
            client.JoinedGuild += async (guild) =>
            {
                Console.WriteLine("Joined Guild: " + guild.Name);
                ListGuilds(client);
                await DoEventsWork(client);
            };

            await Task.Delay(-1);
        }

        Task HandleLog(LogMessage msg)
        {
            switch (msg.Severity)
            {
                case LogSeverity.Debug:
                    _logger.LogDebug(msg.Message);
                    break;
                case LogSeverity.Info:
                    _logger.LogInformation(msg.Message);
                    break;
                case LogSeverity.Warning:
                    _logger.LogWarning(msg.Message);
                    break;
                case LogSeverity.Error:
                    if (msg.Exception is not null)
                    {
                        _logger.LogError(msg.Exception, msg.Message);
                    }
                    else
                    {
                        _logger.LogError(msg.Message);
                    }
                    break;
                case LogSeverity.Critical:
                    _logger.LogCritical(msg.Exception, msg.Message);
                    break;
            }

            return Task.CompletedTask;
        }

        static void ListGuilds(DiscordSocketClient client)
        {
            Console.WriteLine("Guilds:");
            foreach (var guild in client.Guilds)
            {
                Console.WriteLine(guild.Name);
            }
            Console.WriteLine();
        }

        async Task DoEventsWork(DiscordSocketClient client)
        {
            var tournamentTimes = await _rocketLeagueService.GetTouramentTimes();
            foreach (var guild in client.Guilds)
            {
                var events = await guild.GetEventsAsync();

                foreach (var tournament in tournamentTimes)
                {
                    if (tournament.StartTime.HasValue)
                    {
                        var eventName = $"Rocket League: {tournament.MatchType}";
                        var startTimeLocal = tournament.StartTime.Value.ToLocalTime();
                        var existingEvent = events.FirstOrDefault(x => x.Name == eventName && x.StartTime == startTimeLocal);
                        if (existingEvent is null)
                        {
                            Image? image = tournament.Image != null ? new Image(tournament.Image) : null;
                            await guild.CreateEventAsync(eventName, startTimeLocal, GuildScheduledEventType.External, GuildScheduledEventPrivacyLevel.Private, endTime: startTimeLocal.AddMinutes(60), location: "Rocket League", coverImage: image);
                        }
                    }
                }
            }
        }
    }
}
