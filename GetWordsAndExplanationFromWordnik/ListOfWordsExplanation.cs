using GetWordsAndExplanationFromWordnik.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GetWordsAndExplanationFromWordnik
{
    public class ListOfWordsExplanation : IListOfWordsExplanation
    {
        private static readonly HttpClient client = new HttpClient();

        private readonly ILogger<ListOfWordsExplanation> _log;
        private readonly IConfiguration _config;

        public ListOfWordsExplanation(ILogger<ListOfWordsExplanation> log, IConfiguration config)
        {
            _log = log;
            _config = config;
        }

        public async Task<List<Explanation>> GetExplanation(List<string>? lstr = null)
        {
            List<string> words = new List<string>();

            if (lstr != null)
                words = lstr.Where(w => w.Length < _config.GetValue<int>("MaxWordLength")).ToList();
            else
                words = File.ReadAllLines("words.txt").Where(word => word.Length < MyAppData.MaxWordLength).ToList();

            List<string> selectedWords = new List<string>();

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

            var explanations = new List<Explanation>();
            int howManyRequests = 0;

            foreach (var word in selectedWords)
            {

                string apiKey = _config.GetValue<string>("ApiKey") ?? MyAppData.ApiKey;

                string path =
                    _config.GetValue<string>("BaseAddressOfWordnikExplanationApi")
                    + "/" + word 
                    + _config.GetValue<string>("ApiPathForAskingOfExplanationFromWordnik")
                    + apiKey;

                var response = await client.GetAsync(path);

                howManyRequests++;

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var explan = Parse(responseString, word);
                    if (!explan.text.Contains("ERROR:"))
                    {
                        explanations.Add(explan);
                    }
                    _log.LogInformation($"{howManyRequests}.\t{word} - {explan.text}");
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        _log.LogError($"Too many requests: {howManyRequests}");
                        break;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        _log.LogWarning($"Brak definicji w słowniku!");
                        continue;
                    }
                    else
                    {
                        explanations.Add(new Explanation()
                        {
                            word = word,
                            text = "Problem z pobieraniem definicji słowa:("
                        });
                        _log.LogWarning("Problem z pobieraniem definicji słowa: ", word);
                    }
                }
            }

            return explanations;

        }

        private static Explanation Parse(string response, string word)
        {
            try
            {
                var explain = JsonConvert.DeserializeObject<List<Explanation>>(response);
                if (explain.Count == 0)
                {
                    return new Explanation()
                    {
                        word = word,
                        text = "Problem z pobieraniem definicji słowa:("
                    };
                }
                return new Explanation()
                {
                    word = word,
                    text = explain[0].text,
                    textProns = explain[0].textProns,
                    sourceDictionary = explain[0].sourceDictionary,
                    attributionText = explain[0].attributionText,
                    partOfSpeech = explain[0].partOfSpeech,
                    score = explain[0].score,
                    seqString = explain[0].seqString,
                    sequence = explain[0].sequence,
                    exampleUses = explain[0].exampleUses,
                    relatedWords = explain[0].relatedWords,
                };
            }
            catch (Exception ex)
            {
                return new Explanation()
                {
                    word = word,
                    text = $"ERROR: {ex.Message}" //?
                };
            }
        }

        private static string prepareString(string str)
        {
            return str.Replace("<xref>", "\"").Replace("</xref>", "\"");
        }
    }
}
