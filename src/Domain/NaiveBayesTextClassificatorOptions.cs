using Iveonik.Stemmers;
using LemmaSharp;

namespace Domain;

public sealed class NaiveBayesTextClassificatorOptions
{
    public bool ExcludeStopWordsEnable { get; set; }
    public HashSet<string> StopWords { get; set; } = default!;
    public string Separator { get; set; } = " ";
    public Func<string, string, string[]> Tokenizer { get; set; } = default!;
    public IReadOnlySet<char> ValidChars { get; set; } = default!;
    
    public bool ExcludeRareWords { get; set; }
    public double MinProbability { get; set; }
    
    public bool ExcludeFrequentWords { get; set; }
    public double MaxProbability { get; set; }

    public bool ExcludePmiWords { get; set; }
    public double PercentExcludingPmiWords { get; set; }
    public bool ExcludeIdfWords { get; set; }
    public double PercentExcludingIdfWords { get; set; }
    public ILemmatizer Lemmitizer { get; set; } = default!;
    public bool LemmitizationEnable { get; set; }
    public bool StemmingEnable { get; set; }
    public IStemmer Stemmer { get; set; } = default!;

    public bool ToleranceEnable { get; set; }
    public int Tolerance { get; set; }
}