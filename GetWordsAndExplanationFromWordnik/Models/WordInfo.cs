using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetWordsAndExplanationFromWordnik.Models;

public class WordInfo
{
    public string CanonicalForm { get; set; } = string.Empty;
    public int Id { get; set; }
    public string OriginalWord { get; set; } = string.Empty;
    public List<string> Suggestions { get; set; } = new List<string>();
    public string Vulgar { get; set; } = string.Empty;
    public string Word { get; set; } = string.Empty;

}
