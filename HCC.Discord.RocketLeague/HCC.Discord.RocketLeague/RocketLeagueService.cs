using HtmlAgilityPack;

namespace HCC.Discord.RocketLeague;

internal class RocketLeagueService
{
    public async Task<List<TournamentInfo>> GetTouramentTimes()
    {
        using var client = new HttpClient();
        var uri = new Uri("https://rocket-league.com/tournaments");
        var httpResponse = await client.GetAsync(uri);
        httpResponse.EnsureSuccessStatusCode();
        var htmlContent = await httpResponse.Content.ReadAsStringAsync();

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlContent);
        var tournamentItemNodes = htmlDoc.DocumentNode.SelectNodes("//li[@class='rlg-tournament__item']");
        var tournamentInfoModels = tournamentItemNodes.Select(node =>
        {
            var itemInfoXPath = $"{node.XPath}//div[@class='rlg-tournament__item-info']";

            var matchTypeNode = node.SelectSingleNode($"{itemInfoXPath}/h2");
            var matchType = matchTypeNode.GetDirectInnerText();

            var startTimeNode = node.SelectSingleNode($"{itemInfoXPath}//span[@class='rlg-tournament__item-timer']");
            var startTimeString = startTimeNode.GetAttributeValue("data-time", "");
            var successfullyParsedTime = DateTime.TryParse(startTimeString, out var startTime);
            DateTime.SpecifyKind(startTime, DateTimeKind.Utc);

            return new TournamentInfo
            {
                MatchType = matchType,
                StartTime = successfullyParsedTime ? startTime : null,
            };
        }).ToList();

        return tournamentInfoModels;
    }
}

internal class TournamentInfo
{
    public string? MatchType { get; set; }
    public DateTime? StartTime { get; set; }
}