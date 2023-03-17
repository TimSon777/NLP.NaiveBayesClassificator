using Domain.Models;

namespace Domain.UnitTests.Builders;

public sealed class NaiveBayesTextClassificatorTestsBuilder
{
    private double PositiveSentimentTextProbability { get; set; }
    private IReadOnlyDictionary<WordSentiment, double> WordProbabilities { get; set; } = default!;
    private NaiveBayesTextClassificatorOptions Options { get; set; } = default!;

    public static NaiveBayesTextClassificatorTestsBuilder Create()
    {
        return new NaiveBayesTextClassificatorTestsBuilder();
    }

    private NaiveBayesTextClassificatorTestsBuilder()
    { }

    public NaiveBayesTextClassificatorTestsBuilder WithDefaultOptions()
    {
        Options = new NaiveBayesTextClassificatorOptions
        {
            ValidChars = "abcdefghijklmnopqrstuvwxyz".ToHashSet(),
            Separator = " ",
            Tokenizer = (text, separator) => text
                .Split(separator, StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToArray(),
            
            ToleranceEnable = true,
            Tolerance = 100
        };

        return this;
    }

    public NaiveBayesTextClassificatorTestsBuilder WithPositiveSentimentTextProbability(double probability)
    {
        PositiveSentimentTextProbability = probability;
        return this;
    }

    public NaiveBayesTextClassificatorTestsBuilder WithWordProbabilities(IReadOnlyDictionary<WordSentiment, double> probabilities)
    {
        WordProbabilities = probabilities;
        return this;
    }
    
    public NaiveBayesTextClassificator Build()
    {
        return new NaiveBayesTextClassificator
        {
            WordProbabilities = WordProbabilities,
            Options = Options,
            PositiveSentimentTextProbability = PositiveSentimentTextProbability,
        };
    }
}