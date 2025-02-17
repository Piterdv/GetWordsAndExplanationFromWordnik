using GetWordsAndExplanationFromWordnik;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

//Zasadniczo na potrzeby przełączenia exe <-> lib i szybkich testów
internal class Program
{
    private static void Main(string[] args)
    {
        //nowa configuracja z pliku appsettings.json dla Serilog
        var builder = new ConfigurationBuilder();
        BuildConfig(builder);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Build())
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs\\log.txt")
            .CreateLogger();

        Log.Logger.Information("----------->>>Starting");

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddTransient<IListOfWords, ListOfWords>();
                services.AddTransient<IListOfWordsExplanation, ListOfWordsExplanation>();
            })
            .UseSerilog()
            .Build();

        var svc = ActivatorUtilities.CreateInstance<GetWordAndExplanation>(host.Services);
        svc.GetWordAndExplanationOut();

#if DEBUG
        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
#endif

    }

    //appsettings.json + more
    static void BuildConfig(IConfigurationBuilder builder)
    {
        builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            //reloadOnChange: true - jeśli zmienimy plik appsettings.json to program przeładuje konfigurację
            .AddJsonFile(
                $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json"
                , optional: true)
            .AddEnvironmentVariables(); //
    }
}