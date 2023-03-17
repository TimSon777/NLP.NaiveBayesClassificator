using Iveonik.Stemmers;
using LemmaSharp;
using Serilog;

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
}

public sealed class NaiveBayesTextClassificatorBuilder
{
    private readonly Dictionary<string, Sentiment> _textTonalities = new();
    private readonly NaiveBayesTextClassificatorOptions _options = new();
    public bool IsBuild { get; private set; }

    public static NaiveBayesTextClassificatorBuilder Create(Action<NaiveBayesTextClassificatorOptions> configure)
    {
        return new NaiveBayesTextClassificatorBuilder(configure);
    }

    private NaiveBayesTextClassificatorBuilder(Action<NaiveBayesTextClassificatorOptions> configure)
    {
        configure(_options);
    }

    public NaiveBayesTextClassificatorBuilder AddText(string text, Sentiment sentiment)
    {
        var processedText = text
            .ToLower()
            .RemoveHtmlTags(_options.Separator)
            .ReplaceInvalid(_options.ValidChars, _options.Separator);
        
        _textTonalities[processedText] = sentiment;
        return this;
    }

    public NaiveBayesTextClassificator Build(int tolerance = 10000)
    {
        if (IsBuild)
        {
            throw new InvalidOperationException("Already build.");
        }

        IsBuild = true;

        return InternalBuild(tolerance);
    }

    private NaiveBayesTextClassificator InternalBuild(int tolerance)
    {
        var positiveSentimentTextCount = (double) GetTextSentimentCounts(Sentiment.Positive);
        var negativeSentimentTextCount = (double) GetTextSentimentCounts(Sentiment.Negative);

        var wordTonalityCounts = new Dictionary<(string Word, Sentiment Sentiment), int>();

        foreach (var textTonality in _textTonalities)
        {
            var words = _options.Tokenizer(textTonality.Key, _options.Separator)
                .Distinct()
                .ToList();

            if (_options.LemmitizationEnable)
            {
                words = words.Select(e => _options.Lemmitizer.Lemmatize(e)).Distinct().ToList();
            }
            
            if (_options.StemmingEnable)
            {
                words = words.Select(e => _options.Stemmer.Stem(e)).Distinct().ToList();
            }
            
            if (_options.ExcludeStopWordsEnable)
            {
                words = words.Except(_options.StopWords).ToList();
            }
            
            foreach (var word in words)
            {
                wordTonalityCounts[(word, textTonality.Value)] = wordTonalityCounts.GetValueOrDefault((word, textTonality.Value)) + 1;
            }
        }

        var wordCounts = wordTonalityCounts
            .GroupBy(e => e.Key.Word)
            .ToDictionary(e => e.Key, e => e.Sum(x => x.Value));

        var wordSentimentProbabilities = new Dictionary<(string Word, Sentiment), double>();

        foreach (var wordTonalityCount in wordTonalityCounts)
        {
            var probability = wordTonalityCount.Value / (
                wordTonalityCount.Key.Sentiment == Sentiment.Positive 
                    ? positiveSentimentTextCount 
                    : negativeSentimentTextCount);

            if ((!_options.ExcludeFrequentWords || probability <= _options.MaxProbability)
                && (!_options.ExcludeRareWords || probability >= _options.MinProbability))
            {
                wordSentimentProbabilities[wordTonalityCount.Key] = probability;
            }
        }
        
        if (_options.ExcludePmiWords)
        {
            var dict = new Dictionary<(string Word, Sentiment), double>();
            foreach (var wordSentiment in wordSentimentProbabilities)
            {
                var wordProbability = wordCounts[wordSentiment.Key.Word] / (double) _textTonalities.Count;
                var pmi = MathExtensions.Pmi(wordSentiment.Value, wordProbability);
                dict.Add(wordSentiment.Key, pmi);
            }

            var take = (int) (wordCounts.Count * _options.PercentExcludingPmiWords);
        
            var goodWordSentiments = dict
                .OrderBy(e => e.Value)
                .Take(take)
                .Select(e => e.Key);

            foreach (var goodWordSentiment in goodWordSentiments)
            {
                wordSentimentProbabilities.Remove(goodWordSentiment);
            }
        }
        
        if (_options.ExcludeIdfWords)
        {
            var dict = new Dictionary<string, (double Idf, int Count)>();
        
            foreach (var frequency in wordCounts)
            {
                dict[frequency.Key] = (_textTonalities.Count / (double) frequency.Value, frequency.Value);
            }
        
            var take = (int) _options.PercentExcludingIdfWords * _textTonalities.Count;
        
            var bad = dict
                .OrderByDescending(e => e.Value)
                .Take(take)
                .Select(e => e.Key);

            foreach (var bads in bad)
            {
                if (wordSentimentProbabilities.ContainsKey((bads, Sentiment.Negative)))
                {
                    wordSentimentProbabilities.Remove((bads, Sentiment.Negative));
                }
                
                if (wordSentimentProbabilities.ContainsKey((bads, Sentiment.Positive)))
                {
                    wordSentimentProbabilities.Remove((bads, Sentiment.Positive));
                }
            }
        }
        
        return new NaiveBayesTextClassificator
        {
            Options = _options,
            WordProbabilities = wordSentimentProbabilities.OrderBy(e => e.Key.Item1).ThenBy(e => e.Value).ToDictionary(e => e.Key, e => e.Value),
            NegativeSentimentTextProbability = negativeSentimentTextCount / _textTonalities.Count,
            PositiveSentimentTextProbability = positiveSentimentTextCount / _textTonalities.Count
        };
    }

    private int GetTextSentimentCounts(Sentiment sentiment)
    {
        return _textTonalities.Count(t => t.Value == sentiment);
    }
}