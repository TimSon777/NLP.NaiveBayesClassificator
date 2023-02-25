namespace Domain;

public sealed class NaiveBayesTextClassificator
{
    public required double PositiveSentimentTextProbability { get; init; }
    public required double NegativeSentimentTextProbability { get; init; }
    public required IReadOnlyCollection<char> WordSeparators { get; init; }
    public required HashSet<string> UniqueWords { get; init; }
    public required IReadOnlyDictionary<(string Word, Sentiment sentiment), double> WordProbabilities { get; init; }

    private IEnumerable<string> Words => WordProbabilities.Select(w => w.Key.Word);

    internal NaiveBayesTextClassificator()
    { }

    public Sentiment Predict(string text)
    {
        var words = text
            .Split(WordSeparators)
            .Distinct()
            .ToArray();

        var positiveProbability = PositiveSentimentTextProbability;
        var negativeProbability = NegativeSentimentTextProbability;

        var notPresentedWords = Words.Except(words);

        foreach (var word in words)
        {
            if (!UniqueWords.Contains(word))
            {
                continue;
            }
            
            positiveProbability *= WordProbabilities[(word, Sentiment.Positive)];
            negativeProbability *= WordProbabilities[(word, Sentiment.Negative)];
        }

        foreach (var word in notPresentedWords)
        {
            positiveProbability *= 1 - WordProbabilities[(word, Sentiment.Positive)];
            negativeProbability *= 1 - WordProbabilities[(word, Sentiment.Negative)];
        }
        
        return positiveProbability >= negativeProbability
            ? Sentiment.Positive
            : Sentiment.Negative;
    }
}