namespace Domain;

public sealed class NaiveBayesTextClassificatorBuilder
{
    private readonly Dictionary<string, Sentiment> _textTonalities = new();
    public bool IsBuild { get; private set; }

    public static NaiveBayesTextClassificatorBuilder Create()
    {
        return new NaiveBayesTextClassificatorBuilder();
    }

    private NaiveBayesTextClassificatorBuilder()
    { }

    public NaiveBayesTextClassificatorBuilder AddText(string text, Sentiment sentiment)
    {
        _textTonalities[text] = sentiment;
        return this;
    }

    public NaiveBayesTextClassificator Build(IReadOnlyCollection<char> separators, int tolerance = 1000000)
    {
        if (IsBuild)
        {
            throw new InvalidOperationException("Already build.");
        }

        IsBuild = true;

        return InternalBuild(separators, tolerance);
    }

    private NaiveBayesTextClassificator InternalBuild(IReadOnlyCollection<char> separators, int tolerance)
    {
        var positiveSentimentTextCount = (double) GetTextSentimentCounts(Sentiment.Positive);
        var negativeSentimentTextCount = (double) GetTextSentimentCounts(Sentiment.Negative);

        var wordTonalityCounts = new Dictionary<(string Word, Sentiment Sentiment), int>();

        var uniqueWords = _textTonalities
            .Select(t => t.Key)
            .Select(t => t.Split(separators))
            .SelectMany(t => t)
            .ToHashSet();

        foreach (var word in uniqueWords)
        {
            wordTonalityCounts[(word, Sentiment.Negative)] = 0;
            wordTonalityCounts[(word, Sentiment.Positive)] = 0;
        }

        foreach (var textTonality in _textTonalities)
        {
            foreach (var word in textTonality.Key.Split(separators).Distinct())
            {
                wordTonalityCounts[(word, textTonality.Value)] += 1;
            }
        }

        var wordProbabilities = new Dictionary<(string, Sentiment), double>();

        foreach (var wordTonalityCount in wordTonalityCounts)
        {
            var probability = wordTonalityCount.Value / (
                wordTonalityCount.Key.Sentiment == Sentiment.Positive 
                    ? positiveSentimentTextCount 
                    : negativeSentimentTextCount);

            if (probability <= double.Epsilon)
            {
                probability = 1 / (double) tolerance;
            }
            else if (Math.Abs(probability - 1) <= double.Epsilon)
            {
                probability = 1 - 1 / (double) tolerance;
            }
            
            wordProbabilities[wordTonalityCount.Key] = probability;
        }
        
        return new NaiveBayesTextClassificator
        {
            UniqueWords = uniqueWords,
            WordProbabilities = wordProbabilities,
            WordSeparators = separators,
            NegativeSentimentTextProbability = negativeSentimentTextCount / _textTonalities.Count,
            PositiveSentimentTextProbability = positiveSentimentTextCount / _textTonalities.Count
        };
    }

    private int GetTextSentimentCounts(Sentiment sentiment)
    {
        return _textTonalities.Count(t => t.Value == sentiment);
    }
}