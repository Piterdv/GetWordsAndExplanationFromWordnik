using GetWordsAndExplanationFromWordnik.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GetWordsAndExplanationFromWordnik
{
    public class ListOfWords : IListOfWords
    {
        //muszę inicjować HttpClienta bo muszę odświeżać połączenie co minutę?
        //https://stackoverflow.com/questions/40187153/httpclient-getasync-never-returns-when-using-await-async
        //użyj tylko raz, bo inaczej dostaniesz wyjątek?
        private static HttpClient client = new HttpClient();

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
                string apiKey = _config.GetValue<string>("ApiKey") ?? MyAppData.ApiKey;

                string path =
                    _config.GetValue<string>("BaseAddressOfWordnikWordsApi")
                    + _config.GetValue<string>("ApiPathForAskingOfWordFromWordnik")
                    + apiKey;

                _log.LogInformation("Path: " + path);
                
                int howManyWords = onlyOneWord ? 1 : _config.GetValue<int>("HowManyWordsGet");

                for (int i = 0; i < howManyWords; i++) //max 9 in free version of API of wordnik
                {
                    var response = await client.GetAsync(path);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseString = await response.Content.ReadAsStringAsync();
                        string word = ParseWord(responseString);
                        if (word != null)
                        {
                            wordList.Add(word);
                            _log.LogInformation("Word: " + word);
                        }
                    }
                    else
                    {
                        _log.LogError("Error: " + response.StatusCode);
                    }
                }
            }
            catch (HttpRequestException e)
            {
                _log.LogError(e.Message);
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
            }

            return wordList;
        }

        private string ParseWord(string response)
        {
            var wordr = JsonConvert.DeserializeObject<Word>(response);
            return wordr.word;
        }
    }
}
