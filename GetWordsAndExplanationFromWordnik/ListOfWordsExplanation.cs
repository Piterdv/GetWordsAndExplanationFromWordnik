using GetWordsAndExplanationFromWordnik.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
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
                    var explan = ParseExplanation(responseString, word);
                    if (!explan.Text.Contains("ERROR:"))
                    {
                        explanations.Add(explan);
                    }

                    _log.LogInformation($"{howManyRequests}.\t{explan.Word} - {Helpers.GetListAsString(explan.Text)} | {explan.PartOfSpeech} | {explan.Citations[0].Cite} | {Helpers.GetListAsString(explan.ExampleUses)}");
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
                            Text = new List<string> { "Brak definicji w słowniku!(" }
                        });
                        if (words.Count > 1) continue;
                    }
                    else
                    {
                        explanations.Add(new Explanation()
                        {
                            Word = word,
                            Text = new List<string> { "Problem z pobieraniem definicji słowa:(" }
                        });
                        _log.LogWarning("Problem z pobieraniem definicji słowa: ", word);
                    }
                }
            }

            return explanations;

        }

        private Explanation ParseExplanation(string response, string word)
        {
            response = response.Replace("(", "").Replace(")", "").Replace(";", "").Replace("/**/", "").Replace("/**", "").Replace("*/", "").Replace("/*", "").Replace("/**/", "").Replace("/**", "").Replace("*/", "").Replace("/*", "").Replace("/**/", "").Replace("/**", "").Replace("*/", "").Replace("/*", "").Replace("/**/", "").Replace("/**", "").Replace("*/", "").Replace("/*", "").Replace("/**/", "").Replace("/**", "").Replace("*/", "").Replace("/*", "").Replace("/**/", "").Replace("/**", "").Replace("*/", "").Replace("/*", "").Replace("/**/", "").Replace("/**", "").Replace("*/", "").Replace("/*", "");
            response = Encoding.UTF8.GetString(Encoding.Default.GetBytes(response));

            List<Explanation>? explanation = new List<Explanation>();

            try
            {
                //uno paranoja z labels - wycinam bo za dużo problemów - różne typy w API
                if (response.Contains("\"labels\":[{"))
                {
                    int start = response.IndexOf("\"labels\":");
                    int end = response.IndexOf("\"word\":");
                    response = response.Remove(start, end - start);
                }

                if (response.Contains("\"sequence\":")) //??"sequence":"1","score":0
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

                //duo paranoja - raz text w API jest jako string a innym razem jako List<string> - dlatego takie "obejście"
                if (!response.Contains("\"text\":["))
                {
                    response = response.Replace("\"text\":", "\"text\":[");
                    response = response.Replace(",\"word\":", "],\"word\":");
                }

                explanation = JsonConvert.DeserializeObject<List<Explanation>>(response, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            }
            catch (Exception ex)
            {
                _log.LogError(word);
                _log.LogError("({0}): " + ex.Message, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
                _log.LogError(response);
                return new Explanation()
                {
                    Word = word,
                    Text = new List<string> { $"ERROR: {ex.Message}" }
                };
            }

            if (explanation == null)
            {
                return new Explanation()
                {
                    Word = word,
                    Text = new List<string> { "Brak definicji - problem z pobieraniem definicji słowa:(" }
                };
            }

            List<string> txt = new List<string>();
            if (explanation[0].Text != null && explanation[0].Text.Count > 0)
            {
                foreach (var item in explanation[0].Text)
                {
                    txt.Add(Helpers.ParseStringFromHtml(item));
                }
            }
            else
            {
                txt.Add("There's no explanation...");
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
                Text = txt,
                TextProns = explanation[0].TextProns,
                SourceDictionary = explanation[0].SourceDictionary,
                AttributionText = explanation[0].AttributionText,
                PartOfSpeech = pos,
                Score = explanation[0].Score,
                SeqString = explanation[0].SeqString,
                Sequence = explanation[0].Sequence,
                ExampleUses = eou,
                RelatedWords = explanation[0].RelatedWords,
                Citations = cit,
            };

        }
    }
}
