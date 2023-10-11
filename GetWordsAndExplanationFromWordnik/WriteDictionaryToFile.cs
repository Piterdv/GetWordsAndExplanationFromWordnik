namespace GetWordsAndExplanationFromWordnik
{
    internal static class WriteDictionaryToFile
    {
        internal static void WriteDictionary(Dictionary<string, string> dictionary)
        {
            using (var writer = new StreamWriter("dictionary.csv",true))
            {
                foreach (var entry in dictionary)
                {
                    writer.WriteLine($"{entry.Key}|{entry.Value}");
                }
            }
        }
    }
}