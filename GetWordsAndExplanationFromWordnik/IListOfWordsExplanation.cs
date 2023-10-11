using GetWordsAndExplanationFromWordnik.Models;

namespace GetWordsAndExplanationFromWordnik
{
    public interface IListOfWordsExplanation
    {
        Task<List<Explanation>> GetExplanation(List<string>? lstr = null);
    }
}