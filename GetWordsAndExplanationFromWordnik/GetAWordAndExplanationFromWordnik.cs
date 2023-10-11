using GetWordsAndExplanationFromWordnik.Models;

namespace GetWordsAndExplanationFromWordnik
{
    public class GetAnOneWordAndExplanationFromWordnik
    {
        private readonly IListOfWords _oneWord;
        private readonly IListOfWordsExplanation _explanation;

        public GetAnOneWordAndExplanationFromWordnik(IListOfWords oneWord, IListOfWordsExplanation explanation)
        {
            _oneWord = oneWord;
            _explanation = explanation;
        }

        public Explanation GetWordAndExplanation()
        {
            List<string> l = _oneWord.GetWord(true).Result;
            //List<string> l = new List<string>() { "hello" };
            Explanation exp = _explanation.GetExplanation(l).Result[0];
            return exp;
        }
    }
}
