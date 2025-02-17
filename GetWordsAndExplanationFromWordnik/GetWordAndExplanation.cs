using GetWordsAndExplanationFromWordnik.Models;
using Microsoft.Extensions.Logging;

namespace GetWordsAndExplanationFromWordnik;

public class GetWordAndExplanation
{
    private readonly IListOfWords _oneWord;
    private readonly IListOfWordsExplanation _explanation;
    private readonly ILogger<GetWordAndExplanation> _logger;

    public GetWordAndExplanation(IListOfWords oneWord, IListOfWordsExplanation explanation, ILogger<GetWordAndExplanation> logger)
    {
        _oneWord = oneWord;
        _explanation = explanation;
        _logger = logger;
    }

    public Explanation GetWordAndExplanationOut()
    {
        List<string> word = new();

        try
        {
            word = _oneWord.GetWord(true).Result;
            if(word.Contains("Error:"))
                return new Explanation() { Text = new List<string> { word[0] }, Word = word[0] ?? "Error" };
            if (word.Contains("ErrorTMR:"))
                return new Explanation() { Text = new List<string> { "Wait a moment before you try new game again, too may request, sorry:)" }, Word = "ErrorTMR" };

            Explanation exp = _explanation.GetExplanation(word).Result[0];
            
            _logger.LogInformation(">>>OK:)");
            
            return exp;
        }
        catch (Exception ex)
        {
            _logger.LogError("{error}", ex.Message);
            return new Explanation() { Text = new List<string> { "Error" }, Word = word[0] ?? "Error" };
        }
    }

}
