using Microsoft.Extensions.Logging;

namespace HCC.Discord.RocketLeague.Domain
{
    public class DiscordService
    {
        private readonly ILogger _logger;
        //private readonly HttpClient _httpClient;

        public DiscordService(ILogger<DiscordService> logger)
        {
            _logger = logger;
            //_httpClient = httpClient;
        }

        public async Task ListGuilds()
        {

        }

        public async Task CreateEvent()
        {

        }
    }
}
