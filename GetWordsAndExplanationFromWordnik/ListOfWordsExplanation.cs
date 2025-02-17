using GetWordsAndExplanationFromWordnik.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net;
using System.Text;

namespace GetWordsAndExplanationFromWordnik;

public class ListOfWordsExplanation : IListOfWordsExplanation
{
    private static readonly HttpClient client = new();
    private readonly ILogger<ListOfWordsExplanation> _log;
    private readonly IConfiguration _config;

    public ListOfWordsExplanation(ILogger<ListOfWordsExplanation> log, IConfiguration config)
    {
        _log = log;
        _config = config;
    }

    public async Task<List<Explanation>> GetExplanation(List<string>? inputWords = null)
    {
        List<string> words = GetInterestingWords(inputWords);

        var selectedWords = InitializeSelectedWords(words);

        List<Explanation> explanations = await GetAllExplanations(words, selectedWords);

        return explanations;
    }

    private List<string> GetInterestingWords(List<string>? inputWords)
    {
        List<string> words;
        if (inputWords != null)
            words = inputWords.Where(w => w.Length < _config.GetValue<int>("MaxWordLength")).ToList();
        else
            words = File.ReadAllLines("words.text").Where(word => word.Length < MyAppData.MaxWordLength).ToList();
        return words;
    }

    private static List<string> InitializeSelectedWords(List<string> words)
    {
        List<string> selectedWords = new();

        if (words.Count > 1)
        {
            var random = new Random();
            selectedWords = Enumerable.Range(0, MyAppData.HowManyWordsSelected)
                                          .Select(_ => words[random.Next(words.Count)]).ToList();
        }
        else
        {
            selectedWords.Add(words[0]);
        }

        return selectedWords;
    }

    private async Task<List<Explanation>> GetAllExplanations(List<string> words, List<string> selectedWords)
    {
        var explanations = new List<Explanation>();
        int howManyRequests = 0;

        foreach (var word in selectedWords)
        {
            string path = BuildApiPath(word);

#if DEBUG
            _log.LogInformation("Path: {Path}", path);
#endif

            var response = await client.GetAsync(path);
            howManyRequests++;

            if (response.IsSuccessStatusCode)
            {
                var explanation = await HandleSuccessResponse(response, word, howManyRequests);
                if (explanation != null)
                {
                    explanations.Add(explanation);
                }
            }
            else
            {
                if (HandleErrorResponse(response, word, howManyRequests, explanations))
                {
                    break;
                }
            }
        }

        return explanations;
    }

    private string BuildApiPath(string word)
    {
        string apiKey = _config.GetValue<string>("ApiKey") ?? MyAppData.ApiKey;
        return _config.GetValue<string>("BaseAddressOfWordnikExplanationApi")
               + "/" + word
               + _config.GetValue<string>("ApiPathForAskingOfExplanationFromWordnik")
               + apiKey;
    }

    private async Task<Explanation?> HandleSuccessResponse(HttpResponseMessage response, string word, int howManyRequests)
    {
        var responseString = await response.Content.ReadAsStringAsync();
        var explanation = ParseExplanation(responseString, word);
        if (!explanation.Text.Contains("ERROR:"))
        {
            _log.LogInformation("{RequestNumber}.\t{Word} - {Text} | {PartOfSpeech} | {Cite} | {ExampleUses}",
                howManyRequests, explanation.Word, Helpers.GetStringsAsOneString(explanation.Text), explanation.PartOfSpeech, explanation.Citations[0].Cite, Helpers.GetStringsAsOneString(explanation.ExampleUses));
            return explanation;
        }
        return null;
    }

    private bool HandleErrorResponse(HttpResponseMessage response, string word, int howManyRequests, List<Explanation> explanations)
    {
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            _log.LogError("Too many requests: {RequestNumber}", howManyRequests);
            return true;
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _log.LogWarning("No definition found in the dictionary!");
            explanations.Add(new Explanation()
            {
                Word = word,
                Text = new List<string> { "No definition found in the dictionary!" }
            });
        }
        else
        {
            explanations.Add(new Explanation()
            {
                Word = word,
                Text = new List<string> { "Problem fetching the word definition:(" }
            });
            _log.LogWarning("Problem fetching the word definition: {Word}", word);
        }
        return false;
    }

    private Explanation ParseExplanation(string response, string word)
    {
        response = CleanResponseString(response);
        response = Encoding.UTF8.GetString(Encoding.Default.GetBytes(response));

        List<Explanation>? explanation;

        try
        {
            explanation = DeserializeExplanation(response);
        }
        catch (Exception ex)
        {
            LogError(word, ex, response);
            return CreateErrorExplanation(word, ex.Message);
        }

        if (explanation == null)
        {
            return CreateErrorExplanation(word, "There are some problems with getting deffinition of the word;(");
        }

        return CreateExplanation(word, explanation[0]);
    }

    private static string CleanResponseString(string response)
    {
        response = response.Replace("(", "").Replace(")", "").Replace(";", "").Replace("/**/", "").Replace("/**", "").Replace("*/", "").Replace("/*", "");
        if (response.Contains("\"labels\":[{"))
        {
            int start = response.IndexOf("\"labels\":");
            int end = response.IndexOf("\"word\":");
            response = response.Remove(start, end - start);
        }

        if (response.Contains("\"sequence\":"))
        {
            int start = response.IndexOf("\"sequence\":");
            int end = response.IndexOf("\"word\":");
            response = response.Remove(start, end - start);
        }

        if (response.Contains("\"exampleUses\":[{"))
        {
            int start = response.IndexOf("\"exampleUses\":");
            int end = response.IndexOf("\"attributionUrl\":");
            response = response.Remove(start, end - start);
        }

        if (!response.Contains("\"text\":["))
        {
            response = response.Replace("\"text\":", "\"text\":[");
            response = response.Replace(",\"word\":", "],\"word\":");
        }

        return response;
    }

    private static List<Explanation>? DeserializeExplanation(string response)
    {
        return JsonConvert.DeserializeObject<List<Explanation>>
            (response, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
    }

    private void LogError(string word, Exception ex, string response)
    {
        _log.LogError("{word}", word);
        _log.LogError("{TypeName}: {error}", Helpers.GetCurrentMethodName(), ex.Message);
        _log.LogError("{response}", response);
    }



    private static Explanation CreateErrorExplanation(string word, string errorMessage)
    {
        return new Explanation()
        {
            Word = word,
            Text = new List<string> { $"ERROR: {errorMessage}" }
        };
    }

    private static Explanation CreateExplanation(string word, Explanation explanation)
    {
        List<string> text = explanation.Text != null && explanation.Text.Count > 0
            ? explanation.Text.Select(Helpers.ParseStringInOddWordnikHtml).ToList()
            : new List<string> { "There's no explanation..." };

        List<string> examleOfUses = explanation.ExampleUses != null && explanation.ExampleUses.Count > 0
            ? explanation.ExampleUses.Select(Helpers.ParseStringInOddWordnikHtml).ToList()
            : new List<string> { "No example of uses..." };

        string pos = !string.IsNullOrEmpty(explanation.PartOfSpeech) ? explanation.PartOfSpeech : "Unknown part of Speech...";

        List<Citation> citations = explanation.Citations != null && explanation.Citations.Count > 0
            ? explanation.Citations.Select(item => new Citation { Source = item.Source, Cite = Helpers.ParseStringInOddWordnikHtml(item.Cite) }).ToList()
            : new List<Citation> { new() { Source = "?", Cite = "There's no cite..." } };

        return new Explanation()
        {
            Word = word,
            Text = text,
            TextProns = explanation.TextProns,
            SourceDictionary = explanation.SourceDictionary,
            AttributionText = explanation.AttributionText,
            PartOfSpeech = pos,
            Score = explanation.Score,
            SeqString = explanation.SeqString,
            Sequence = explanation.Sequence,
            ExampleUses = examleOfUses,
            RelatedWords = explanation.RelatedWords,
            Citations = citations,
        };
    }
}
