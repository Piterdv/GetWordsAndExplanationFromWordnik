namespace GetWordsAndExplanationFromWordnik.Models
{
    public class Explanation
    {
        public string attributionText { get; set; } = string.Empty;
        public string attributionUrl { get; set; } = string.Empty;
        public List<Citation> citations { get; set; } = new List<Citation>();
        public List<string> exampleUses { get; set; } = new List<string>();
        public string extendedText { get; set; } = string.Empty;
        public List<string> labels { get; set; } = new List<string>();
        public List<string> notes { get; set; } = new List<string>();
        public string partOfSpeech { get; set; } = string.Empty;
        public List<string> relatedWords { get; set; } = new List<string>();
        public int score { get; set; } = 0;
        public string seqString { get; set; } = string.Empty;
        public string sequence { get; set; } = string.Empty;
        public string sourceDictionary { get; set; } = string.Empty;
        public string text { get; set; } = string.Empty;
        public List<string> textProns { get; set; } = new List<string>();
        public string word { get; set; } = string.Empty;
        //public string wordnikUrl { get; set; } = string.Empty;
    }
}
