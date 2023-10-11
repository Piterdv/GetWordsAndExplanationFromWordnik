//DI, Serilog, Settings :)
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class GreetingsService : IGreetingsService
{
    private readonly ILogger<GreetingsService> _log;
    private readonly IConfiguration _config;

    public GreetingsService(ILogger<GreetingsService> log, IConfiguration config)
    {
        _log = log;
        _config = config;
    }

    public void Run()
    {
        for (int i = 0; i < _config.GetValue<int>("LoopTimes"); i++)
        {
            //_log.LogInformation("Run number {runNumber}", i); //nie stosować $"{}" bo to jest wolne
            _log.LogError("Run number {runNumber}", i); //nie stosować $"{}" bo to jest wolne
            Thread.Sleep(1000);
        }
    }

}



/*using GetWordsAndExplanationFromWordnik;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

var service = new ServiceCollection();
service.AddSingleton<IListOfWords, ListOfWords>();
var provider = service.BuildServiceProvider();
provider.GetService<IListOfWords>();


//GetWord();
GetExplanation();


void GetWord()
{
    MoreThan9Words mt9w = new MoreThan9Words(new ListOfWords());

    var result = mt9w.GetMoreThen9Words().Result;

    foreach (var item in result)
    {
        Console.WriteLine(item);
    }
}

void GetExplanation()
{
    ListOfWordsExplanation.GetExplanation(null).Wait();
}*/
