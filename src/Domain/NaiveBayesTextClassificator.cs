namespace Domain;

public sealed class NaiveBayesTextClassificator
{
    public required double PositiveSentimentTextProbability { get; init; }
    public required double NegativeSentimentTextProbability { get; init; }
    public required IReadOnlyDictionary<(string Word, Sentiment sentiment), double> WordProbabilities { get; init; }
    public required NaiveBayesTextClassificatorOptions Options { get; set; }

    private IEnumerable<string> Words => WordProbabilities.Select(w => w.Key.Word).Distinct();

    internal NaiveBayesTextClassificator()
    { }

    public Sentiment Predict(string text)
    {
        var processedText = text
            .ToLower()
            .RemoveHtmlTags(Options.Separator)
            .ReplaceInvalid(Options.ValidChars, Options.Separator);
        
        var words = Options.Tokenizer(processedText, Options.Separator)
            .Distinct()
            .ToList();

        if (Options.LemmitizationEnable)
        {
            words = words.Select(e => Options.Lemmitizer.Lemmatize(e)).Distinct().ToList();
        }
        
        if (Options.StemmingEnable)
        {
            words = words.Select(e => Options.Stemmer.Stem(e)).Distinct().ToList();
        }
        
        if (Options.ExcludeStopWordsEnable)
        {
            words = words.Except(Options.StopWords).ToList();
        }
        
        var positiveProbability = PositiveSentimentTextProbability;
        var negativeProbability = NegativeSentimentTextProbability;

        var notPresentedWords = Words.Except(words);

        foreach (var word in words)
        {
            if (WordProbabilities.TryGetValue((word, Sentiment.Positive), out var positive))
            {
                positiveProbability *= positive;
            }
            
            if (positiveProbability <= double.Epsilon)
            {
                return Sentiment.Negative;
            }
            
            if (WordProbabilities.TryGetValue((word, Sentiment.Negative), out var negative))
            {
                negativeProbability *= negative;
            }

            if (negativeProbability <= double.Epsilon)
            {
                return Sentiment.Positive;
            }
        }

        foreach (var word in notPresentedWords)
        {
            if (WordProbabilities.TryGetValue((word, Sentiment.Positive), out var positive))
            {
                positiveProbability *= 1 - positive;
            }
            
            if (positiveProbability <= double.Epsilon)
            {
                return Sentiment.Negative;
            }
            
            if (WordProbabilities.TryGetValue((word, Sentiment.Negative), out var negative))
            {
                negativeProbability *= 1 - negative;
            }
            
            if (negativeProbability <= double.Epsilon)
            {
                return Sentiment.Positive;
            }
        }
        
        return positiveProbability >= negativeProbability
            ? Sentiment.Positive
            : Sentiment.Negative;
    }
}