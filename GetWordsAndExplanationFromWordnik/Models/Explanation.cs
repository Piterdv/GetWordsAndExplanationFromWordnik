namespace GetWordsAndExplanationFromWordnik.Models
{
    public class Explanation
    {
        public string AttributionText { get; set; } = string.Empty;
        public string AttributionUrl { get; set; } = string.Empty;
        public List<Citation> Citations { get; set; } = new List<Citation>();
        public List<string> ExampleUses { get; set; } = new List<string>();
        public string ExtendedText { get; set; } = string.Empty;
        //public List<string> Labels { get; set; } = new List<string>();
        public List<string> Notes { get; set; } = new List<string>();
        public string PartOfSpeech { get; set; } = string.Empty;
        public List<string> RelatedWords { get; set; } = new List<string>();
        public int Score { get; set; } = 0;
        public string SeqString { get; set; } = string.Empty;
        public string Sequence { get; set; } = string.Empty;
        public string SourceDictionary { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public List<string> TextProns { get; set; } = new List<string>();
        public string Word { get; set; } = string.Empty;
        //public string wordnikUrl { get; set; } = string.Empty;
    }
}
