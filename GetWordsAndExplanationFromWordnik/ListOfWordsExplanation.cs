using GetWordsAndExplanationFromWordnik.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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

                _log.LogInformation("Path: " + path);

                var response = await client.GetAsync(path);

                howManyRequests++;

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var explan = Parse(responseString, word);
                    if (!explan.Text.Contains("ERROR:"))
                    {
                        explanations.Add(explan);
                    }

                    //TODO: wrzuć to do parse
                    string eou = explan.ExampleUses != null && explan.ExampleUses.Count > 0 ?
                        explan.ExampleUses[0] : "No example of uses...";
                    string pos = explan.PartOfSpeech != "" ? explan.PartOfSpeech : "Unknown part of Speech...";
                    string cit = explan.Citations != null && explan.Citations.Count > 0 ?
                        explan.Citations[0].Cite : "There's no citation...";

                    _log.LogInformation($"{howManyRequests}.\t{explan.Word} - {explan.Text} | {explan.PartOfSpeech} | {eou} | {cit}");
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
                        if (words.Count > 1) continue;
                        explanations.Add(new Explanation()
                        {
                            Word = word,
                            Text = "Brak definicji w słowniku!("
                        });
                    }
                    else
                    {
                        explanations.Add(new Explanation()
                        {
                            Word = word,
                            Text = "Problem z pobieraniem definicji słowa:("
                        });
                        _log.LogWarning("Problem z pobieraniem definicji słowa: ", word);
                    }
                }
            }

            return explanations;

        }

        //Domyślnie przerób to na pobieranie listy, chociaż to nie ma sensu, bo i tak pobieramy jedno słowo
        //ponieważ API wordnika nie pozwala na pobranie więcej niż kilku definicji na raz dla darmowej wersji
        private static Explanation Parse(string response, string word)
        {
            try
            {
                var explanation = JsonConvert.DeserializeObject<List<Explanation>>(response, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                if (explanation.Count == 0)
                {
                    return new Explanation()
                    {
                        Word = word,
                        Text = "Problem z pobieraniem definicji słowa:("
                    };
                }

                return new Explanation()
                {
                    Word = word,
                    Text = Helpers.ParseStringFromHtml(explanation[0].Text),
                    TextProns = explanation[0].TextProns,
                    SourceDictionary = explanation[0].SourceDictionary,
                    AttributionText = explanation[0].AttributionText,
                    PartOfSpeech = explanation[0].PartOfSpeech,
                    Score = explanation[0].Score,
                    SeqString = explanation[0].SeqString,
                    Sequence = explanation[0].Sequence,
                    ExampleUses = explanation[0].ExampleUses,
                    RelatedWords = explanation[0].RelatedWords,
                    Citations = explanation[0].Citations,
                };
            }
            catch (Exception ex)
            {
                return new Explanation()
                {
                    Word = word,
                    Text = $"ERROR: {ex.Message}" //?
                };
            }
        }

    }
}
