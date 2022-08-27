using HtmlAgilityPack;

namespace HCC.Discord.RocketLeague.Domain;

public class RocketLeagueService
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
        var tournamentInfoModels = new List<TournamentInfo>();
        foreach (var node in tournamentItemNodes)
        {
            var itemInfoXPath = $"{node.XPath}//div[@class='rlg-tournament__item-info']";

            var matchTypeNode = node.SelectSingleNode($"{itemInfoXPath}/h2");
            var matchType = matchTypeNode.GetDirectInnerText();

            var startTimeNode = node.SelectSingleNode($"{itemInfoXPath}//span[@class='rlg-tournament__item-timer']");
            var startTimeString = startTimeNode.GetAttributeValue("data-time", "");
            var successfullyParsedTime = DateTime.TryParse(startTimeString, out var startTime);
            DateTime.SpecifyKind(startTime, DateTimeKind.Utc);

            var imageNode = node.SelectSingleNode($"{node.XPath}//img[@class='rlg-tournament__item-img']");
            var imageUrl = imageNode.GetAttributeValue("src", "");
            Stream? image = null;
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                var imageResponse = await client.GetAsync(imageUrl);
                if (imageResponse.IsSuccessStatusCode)
                {
                    image = await imageResponse.Content.ReadAsStreamAsync();
                }
            }

            var info = new TournamentInfo
            {
                MatchType = matchType,
                StartTime = successfullyParsedTime ? startTime : null,
                Image = image,
            };
            tournamentInfoModels.Add(info);
        }

        return tournamentInfoModels;
    }
}

public class TournamentInfo
{
    public string? MatchType { get; set; }
    public DateTime? StartTime { get; set; }
    public Stream? Image { get; set; }
}