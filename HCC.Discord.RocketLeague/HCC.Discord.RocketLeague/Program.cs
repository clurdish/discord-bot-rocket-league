using HCC.Discord.RocketLeague;

Console.WriteLine("Rocket League tournament times. Press any key to stop.\n");

var cts = new CancellationTokenSource();
var readLineTask = ReadLine();
var printTimesTask = PrintTimes(cts.Token);

await Task.WhenAny(readLineTask, printTimesTask);
cts.Cancel();


async Task ReadLine()
{
    while (!Console.KeyAvailable)
    {
        await Task.Delay(100);
    }
}

async Task PrintTimes(CancellationToken cancellationToken)
{
    var rlgService = new RocketLeagueService();
    while (!cancellationToken.IsCancellationRequested)
    {
        var tournamentInfoList = await rlgService.GetTouramentTimes();
        
        foreach (var t in tournamentInfoList)
        {
            Console.WriteLine($"{t.MatchType}: {t.StartTime?.ToLocalTime()}");
        }
        Console.WriteLine();

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        await Task.Delay(5000);
    }
}
