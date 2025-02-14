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
        List<string> l = new List<string>();

        try
        {
            l = _oneWord.GetWord(true).Result;
            if(l.Contains("Error:"))
                return new Explanation() { Text = new List<string> { l[0] }, Word = l[0] ?? "Error" };
            if (l.Contains("ErrorTMR:"))
                return new Explanation() { Text = new List<string> { "Wait a moment before you try new game again, too may request, sorry:)" }, Word = "ErrorTMR" };

            Explanation exp = _explanation.GetExplanation(l).Result[0];
            
            _logger.LogInformation(">>>OK:)");
            
            return exp;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return new Explanation() { Text = new List<string> { "Error" }, Word = l[0] ?? "Error" };
        }
    }

}
