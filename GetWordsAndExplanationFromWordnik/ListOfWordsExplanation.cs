using GetWordsAndExplanationFromWordnik.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;

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

#if DEBUG
                _log.LogInformation("Path: " + path);
#endif

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

                    _log.LogInformation($"{howManyRequests}.\t{explan.Word} - {explan.Text} | {explan.PartOfSpeech} | {explan.Citations[0].Cite} | {explan.ExampleUses[0]}");
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
                        explanations.Add(new Explanation()
                        {
                            Word = word,
                            Text = "Brak definicji w słowniku!("
                        });
                        if (words.Count > 1) continue;
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

        private Explanation Parse(string response, string word)
        {
            response = response.Replace("(", "").Replace(")", "").Replace(";", "").Replace("/**/", "").Replace("/**", "").Replace("*/", "").Replace("/*", "").Replace("/**/", "").Replace("/**", "").Replace("*/", "").Replace("/*", "").Replace("/**/", "").Replace("/**", "").Replace("*/", "").Replace("/*", "").Replace("/**/", "").Replace("/**", "").Replace("*/", "").Replace("/*", "").Replace("/**/", "").Replace("/**", "").Replace("*/", "").Replace("/*", "").Replace("/**/", "").Replace("/**", "").Replace("*/", "").Replace("/*", "").Replace("/**/", "").Replace("/**", "").Replace("*/", "").Replace("/*", "");
            response = Encoding.UTF8.GetString(Encoding.Default.GetBytes(response));

            List<Explanation>? explanation = new List<Explanation>();

            try
            {
                explanation = JsonConvert.DeserializeObject<List<Explanation>>(response, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
                _log.LogError(response);
                return new Explanation()
                {
                    Word = word,
                    Text = $"ERROR: {ex.Message}" //?
                };
            }

            if (explanation == null) //|| explanation.Count == 0
            {
                return new Explanation()
                {
                    Word = word,
                    Text = "Brak definicji - problem z pobieraniem definicji słowa:("
                };
            }

            List<string> eou = new List<string>();
            if (explanation[0].ExampleUses != null && explanation[0].ExampleUses.Count > 0)
            {
                foreach (var item in explanation[0].ExampleUses)
                {
                    eou.Add(Helpers.ParseStringFromHtml(item));
                }
            }
            else
            {
                eou.Add("No example of uses...");
            }

            string pos = explanation[0].PartOfSpeech != "" ? explanation[0].PartOfSpeech : "Unknown part of Speech...";

            List<Citation> cit = new List<Citation>();
            if (explanation[0].Citations != null && explanation[0].Citations.Count > 0)
            {
                foreach (var item in explanation[0].Citations)
                {
                    Citation c = new Citation();
                    c.Source = item.Source;
                    c.Cite = Helpers.ParseStringFromHtml(item.Cite);
                    cit.Add(c);
                }
            }
            else
            {
                Citation c = new Citation();
                c.Source = "?";
                c.Cite = "There's no cite...";
                cit.Add(c);
            }

            return new Explanation()
            {
                Word = word,
                Text = Helpers.ParseStringFromHtml(explanation[0].Text),
                TextProns = explanation[0].TextProns,
                SourceDictionary = explanation[0].SourceDictionary,
                AttributionText = explanation[0].AttributionText,
                PartOfSpeech = pos,//explanation[0].PartOfSpeech,
                Score = explanation[0].Score,
                SeqString = explanation[0].SeqString,
                Sequence = explanation[0].Sequence,
                ExampleUses = eou, //explanation[0].ExampleUses,
                RelatedWords = explanation[0].RelatedWords,
                Citations = cit,//explanation[0].Citations,
                                //Labels = explanation[0].Labels,
            };

        }

    }
}
