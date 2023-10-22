using GetWordsAndExplanationFromWordnik.Models;

namespace GetWordsAndExplanationFromWordnik
{
    public class GetWordAndExplanationClass
    {
        private readonly IListOfWords _oneWord;
        private readonly IListOfWordsExplanation _explanation;

        public GetWordAndExplanationClass(IListOfWords oneWord, IListOfWordsExplanation explanation)
        {
            _oneWord = oneWord;
            _explanation = explanation;
        }

        public Explanation GetWordAndExplanationOut()
        {
            List<string> l = _oneWord.GetWord(true).Result;
            //List<string> l = new List<string>() { "like" };
            Explanation exp = _explanation.GetExplanation(l).Result[0];
            return exp;
        }

        public void GetWordAndExplanationOutConsole()
        {
            //var la= await _oneWord.GetWord(true);
            //int x = 1;
            //List<string> l = la;
            //List<string> l =  _oneWord.GetWord(true).Result;
            List<string> l = new List<string>() { "like" };
            Explanation exp = _explanation.GetExplanation(l).Result[0];
            //Console.WriteLine(exp.text + "|" + exp.citations );
        }
    }
}
