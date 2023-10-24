using GetWordsAndExplanationFromWordnik.Models;
using Microsoft.Extensions.Logging;

namespace GetWordsAndExplanationFromWordnik
{
    public class GetWordAndExplanationClass
    {
        private readonly IListOfWords _oneWord;
        private readonly IListOfWordsExplanation _explanation;
        private readonly ILogger<GetWordAndExplanationClass> _logger;

        public GetWordAndExplanationClass(IListOfWords oneWord, IListOfWordsExplanation explanation, ILogger<GetWordAndExplanationClass> logger)
        {
            _oneWord = oneWord;
            _explanation = explanation;
            _logger = logger;
        }

        public Explanation GetWordAndExplanationOut()
        {
            List<string> l =new List<string>();

            try
            {
                l = _oneWord.GetWord(true).Result;
                //l = new List<string>() { "like" };        //Dużo informacji
                //l = new List<string>() { "nonstatic" };   //problem z Deserialize response, ok z DeserializeObject
                Explanation exp = _explanation.GetExplanation(l).Result[0];
                _logger.LogInformation(">>>OK:)");
                return exp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new Explanation() { Text = "Error", Word = l[0] ?? "Error" };
            }
        }

    }
}
