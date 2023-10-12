﻿using GetWordsAndExplanationFromWordnik.Models;
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

                _log.LogInformation("Path: " + path);

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

                    //TODO: wrzuć to do parse
                    string eou = explan.exampleUses != null && explan.exampleUses.Count > 0 ?
                        explan.exampleUses[0] : "No example of uses...";
                    string pos = explan.partOfSpeech != "" ? explan.partOfSpeech : "Unknown part of Speech...";
                    string cit = explan.citations != null && explan.citations.Count > 0 ?
                        explan.citations[0].cite : "There's no citation...";

                    _log.LogInformation($"{howManyRequests}.\t{explan.word} - {explan.text} | {explan.partOfSpeech} | {eou} | {cit}");
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
                            word = word,
                            text = "Brak definicji w słowniku!("
                        });
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

        //Domyślnie przerób to na pobieranie listy, chociaż to nie ma sensu, bo i tak pobieramy jedno słowo
        //ponieważ API wordnika nie pozwala na pobranie więcej niż kilku definicji na raz dla darmowej wersji
        private static Explanation Parse(string response, string word)
        {
            try
            {
                var explanation = JsonConvert.DeserializeObject<List<Explanation>>(response);
                if (explanation.Count == 0)
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
                    text = Helpers.ParseStringFromHtml(explanation[0].text),
                    textProns = explanation[0].textProns,
                    sourceDictionary = explanation[0].sourceDictionary,
                    attributionText = explanation[0].attributionText,
                    partOfSpeech = explanation[0].partOfSpeech,
                    score = explanation[0].score,
                    seqString = explanation[0].seqString,
                    sequence = explanation[0].sequence,
                    exampleUses = explanation[0].exampleUses,
                    relatedWords = explanation[0].relatedWords,
                    citations = explanation[0].citations,
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

    }
}
