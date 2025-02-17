using GetWordsAndExplanationFromWordnik.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Reflection;

namespace GetWordsAndExplanationFromWordnik;

public class WordnikRetriver
{
    public Explanation GetExplanation()
    {
        var builder = new ConfigurationBuilder();
        BuildConfig(builder);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Build())
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs\\log.txt")
            .CreateLogger();

        var assembly = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown version";
        Log.Logger.Information("----------->>>Starting LIB (R(" + assembly + "))");

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddTransient<IListOfWords, ListOfWords>();
                services.AddTransient<IListOfWordsExplanation, ListOfWordsExplanation>();
            })
            .UseSerilog()
            .Build();

        var wordnikService = ActivatorUtilities.CreateInstance<GetWordAndExplanation>(host.Services);
        var explanation = wordnikService.GetWordAndExplanationOut();
        return explanation;
    }

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