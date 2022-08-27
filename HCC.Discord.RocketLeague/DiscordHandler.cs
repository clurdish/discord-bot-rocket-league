using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using HCC.Discord.RocketLeague.Domain;
using Microsoft.Extensions.Logging;

namespace HCC.Discord.RocketLeague.Console;

internal class DiscordHandler : DiscordClientService, IDisposable
{
    private Timer? _timer;
    private readonly ILogger _logger;
    private readonly RocketLeagueService _rocketLeagueService;

    public DiscordHandler(DiscordSocketClient client, ILogger<DiscordClientService> logger, RocketLeagueService rocketLeagueService) : base(client, logger)
    {
        _logger = logger;
        _rocketLeagueService = rocketLeagueService;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await Client.WaitForReadyAsync(cancellationToken);

        Client.GuildAvailable += guild => EvaluateRocketLeagueEventsForGuild(guild);

        _timer = new Timer(ScheduledWork, null, TimeSpan.Zero, TimeSpan.FromHours(3));
    }

    private async void ScheduledWork(object? state)
    {
        _logger.LogInformation("Running scheduled work...");
        await EvaluateRocketLeagueEvents();
        _logger.LogInformation("Scheduled work complete");
    }

    private async Task EvaluateRocketLeagueEvents()
    {
        var tournaments = await _rocketLeagueService.GetTouramentTimes();
        foreach (var guild in Client.Guilds)
        {
            await EvaluateRocketLeagueEventsForGuild(guild, tournaments);
        }
    }

    private async Task EvaluateRocketLeagueEventsForGuild(SocketGuild guild, List<TournamentInfo>? tournaments = null)
    {
        if (tournaments is null)
        {
            tournaments = await _rocketLeagueService.GetTouramentTimes();
        }

        _logger.LogInformation("Evaluating events for guild: {GuildName}", guild.Name);

        var events = await guild.GetEventsAsync();

        foreach (var tournament in tournaments)
        {
            if (tournament.StartTime.HasValue)
            {
                var eventName = $"Rocket League: {tournament.MatchType}";
                var startTimeLocal = tournament.StartTime.Value.ToLocalTime();
                var existingEvent = events.FirstOrDefault(x => x.Name == eventName && x.StartTime == startTimeLocal);
                if (existingEvent is null)
                {
                    Image? image = tournament.Image != null ? new Image(tournament.Image) : null;
                    await guild.CreateEventAsync(
                        eventName,
                        startTimeLocal,
                        GuildScheduledEventType.External,
                        GuildScheduledEventPrivacyLevel.Private,
                        endTime: startTimeLocal.AddMinutes(60),
                        location: "Rocket League",
                        coverImage: image
                    );
                }
            }
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        if (_timer is not null)
        {
            _timer.Dispose();
        }
    }
}
