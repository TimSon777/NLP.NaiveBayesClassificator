using Domain.Models;

namespace Domain;

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

    public NaiveBayesTextClassificator Build()
    {
        if (IsBuild)
        {
            throw new InvalidOperationException("Already build.");
        }

        IsBuild = true;

        return InternalBuild();
    }

    private NaiveBayesTextClassificator InternalBuild()
    {
        var positiveSentimentTextCount = (double) GetTextSentimentCounts(Sentiment.Positive);
        var negativeSentimentTextCount = _textTonalities.Count - positiveSentimentTextCount;

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
                var oppositeSentiment = textTonality.Value == Sentiment.Positive
                    ? Sentiment.Negative
                    : Sentiment.Positive;
                
                wordTonalityCounts[(word, textTonality.Value)] = wordTonalityCounts.GetValueOrDefault((word, textTonality.Value)) + 1;
                wordTonalityCounts.TryAdd((word, oppositeSentiment), 0);
            }
        }

        var wordCounts = wordTonalityCounts
            .GroupBy(e => e.Key.Word)
            .ToDictionary(e => e.Key, e => e.Sum(x => x.Value));

        var wordSentimentProbabilities = new Dictionary<WordSentiment, double>();

        foreach (var wordTonalityCount in wordTonalityCounts)
        {
            var probability = wordTonalityCount.Value / (
                wordTonalityCount.Key.Sentiment == Sentiment.Positive 
                    ? positiveSentimentTextCount 
                    : negativeSentimentTextCount);

            if ((_options.ExcludeFrequentWords && probability > _options.MaxProbability)
                || (_options.ExcludeRareWords && probability < _options.MinProbability))
            {
                continue;
            }
            
            if (_options.ToleranceEnable)
            {
                if (probability <= double.Epsilon)
                {
                    probability = 1 / (double) _options.Tolerance;
                }
                else if (Math.Abs(probability - 1) <= double.Epsilon)
                {
                    probability = 1 - 1 / (double) _options.Tolerance;
                }
            }
                
            wordSentimentProbabilities[wordTonalityCount.Key] = probability;
        }
        
        if (_options.ExcludePmiWords)
        {
            var dict = new Dictionary<WordSentiment, double>();
            foreach (var wordSentiment in wordSentimentProbabilities)
            {
                var wordProbability = wordCounts[wordSentiment.Key.Word] / (double) _textTonalities.Count;
                var pmi = MathExtensions.Pmi(wordSentiment.Value, wordProbability);
                dict.Add(wordSentiment.Key, pmi);
            }

            var take = (int) (wordCounts.Count * _options.PercentExcludingPmiWords);
        
            var elements = dict
                .OrderBy(e => e.Value)
                .Take(take)
                .Select(e => e.Key);

            foreach (var e in elements)
            {
                wordSentimentProbabilities.Remove(e);
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
        
            var elements = dict
                .OrderByDescending(e => e.Value)
                .Take(take)
                .Select(e => e.Key);

            foreach (var e in elements)
            {
                if (wordSentimentProbabilities.ContainsKey((e, Sentiment.Negative)))
                {
                    wordSentimentProbabilities.Remove((e, Sentiment.Negative));
                }
                
                if (wordSentimentProbabilities.ContainsKey((e, Sentiment.Positive)))
                {
                    wordSentimentProbabilities.Remove((e, Sentiment.Positive));
                }
            }
        }
        
        return new NaiveBayesTextClassificator
        {
            Options = _options,
            WordProbabilities = wordSentimentProbabilities,
            PositiveSentimentTextProbability = positiveSentimentTextCount / _textTonalities.Count
        };
    }

    private int GetTextSentimentCounts(Sentiment sentiment)
    {
        return _textTonalities.Count(t => t.Value == sentiment);
    }
}