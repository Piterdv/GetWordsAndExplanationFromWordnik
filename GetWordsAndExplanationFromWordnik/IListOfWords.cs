namespace GetWordsAndExplanationFromWordnik;

public interface IListOfWords
{
    Task<List<string>> GetWord(bool onlyOneWord = false);
}