using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetWordsAndExplanationFromWordnik.Models
{
    public class Word
    {
        public string canonicalForm { get; set; } = string.Empty;
        public int id { get; set; }
        public string originalWord { get; set; } = string.Empty;
        public List<string> suggestions { get; set; } = new List<string>();
        public string vulgar { get; set; } = string.Empty;
        public string word { get; set; } = string.Empty;

    }
}
