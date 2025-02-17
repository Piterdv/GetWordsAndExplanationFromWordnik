using GetWordsAndExplanationFromWordnik.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text.RegularExpressions;

namespace GetWordsAndExplanationFromWordnik;

public class ListOfWords : IListOfWords
{
    // Reference: https://stackoverflow.com/questions/40187153/httpclient-getasync-never-returns-when-using-await-async
    // Use a single instance of HttpClient to avoid socket exhaustion and improve performance.
    private static readonly HttpClient client = new();

    private readonly ILogger<ListOfWords> _log;
    private readonly IConfiguration _config;

    public ListOfWords(ILogger<ListOfWords> log, IConfiguration config)
    {
        _log = log;
        _config = config;
    }

    public async Task<List<string>> GetWord(bool onlyOneWord = false)
    {
        var wordList = new List<string>();

        try
        {
            string path = BuildApiPath();
#if DEBUG
            _log.LogInformation("Path: {Path}", path);
#endif

            int howManyWords = onlyOneWord ? 1 : _config.GetValue<int>("HowManyWordsGet");
            int notToMuchBadWords = 0;

            for (int i = 0; i < howManyWords; i++)
            {
                var response = await client.GetAsync(path);

                if (response.IsSuccessStatusCode)
                {
                    var result = await HandleSuccessResponse(response, notToMuchBadWords, i);
                    string word = result.Item1;
                    notToMuchBadWords = result.Item2;
                    i = result.Item3;

                    if (!string.IsNullOrEmpty(word))
                    {
                        wordList.Add(word);
                    }
                }
                else
                {
                    HandleErrorResponse(response, wordList);
                }
            }
        }
        catch (HttpRequestException ex)
        {
            _log.LogError("{Error}", ex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError("{Error}", ex.Message);
        }

        return wordList;
    }

    private string BuildApiPath()
    {
        string apiKey = _config.GetValue<string>("ApiKey") ?? MyAppData.ApiKey;
        return _config.GetValue<string>("BaseAddressOfWordnikWordsApi")
               + _config.GetValue<string>("ApiPathForAskingOfWordFromWordnik")
               + apiKey;
    }

    private async Task<Tuple<string, int, int>> HandleSuccessResponse(HttpResponseMessage response, int notToMuchBadWords, int i)
    {
        string responseString = await response.Content.ReadAsStringAsync();
        string word = ParseWord(responseString);

        if (word.Contains(":BAD:WORD:") && notToMuchBadWords < 5)
        {
            notToMuchBadWords++;
            i--;
            return new Tuple<string, int, int>(string.Empty, notToMuchBadWords, i);
        }

        if (!string.IsNullOrEmpty(word))
        {
            _log.LogInformation("Word: {Word}", word);
        }

        return new Tuple<string, int, int>(word, notToMuchBadWords, i);
    }

    private void HandleErrorResponse(HttpResponseMessage response, List<string> wordList)
    {
        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            _log.LogError("ErrorTMR: {Response}", response.StatusCode);
            wordList.Add("ErrorTMR: " + response.StatusCode);
        }
        else
        {
            _log.LogError("Error: {Response}", response.StatusCode);
            wordList.Add("Error: " + response.StatusCode);
        }
    }

    private string ParseWord(string response)
    {
        WordInfo? word;

        try
        {
            word = JsonConvert.DeserializeObject<WordInfo>(response, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
        }
        catch (Exception ex)
        {
            _log.LogError("{Error} {Type}", ex.Message, Helpers.GetCurrentMethodName());
            return ":BAD:WORD:";
        }

        word ??= new WordInfo()
        {
            Word = ":BAD:WORD:"
        };

        if (word.Word == ":BAD:WORD:" || !Regex.IsMatch(word.Word, @"^[a-zA-Z-' ]*$"))
        {
            string badWord = "BAD WORD: " + word.Word + ". I'll try next word to take.";
            _log.LogError("{BadWord}", badWord);
            return ":BAD:WORD:";
        }

        return word.Word;
    }
}
