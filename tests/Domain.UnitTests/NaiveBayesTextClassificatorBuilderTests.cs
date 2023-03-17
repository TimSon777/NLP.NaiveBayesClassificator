using Domain.Models;
using Domain.UnitTests.Builders;
using FluentAssertions.Equivalency;

namespace Domain.UnitTests;

public sealed class NaiveBayesTextClassificatorBuilderTests
{
    private readonly NaiveBayesTextClassificatorBuilder _builder;

    public NaiveBayesTextClassificatorBuilderTests()
    {
        var builder = NaiveBayesTextClassificatorBuilder.Create(options =>
            {
                options.ValidChars = "abcdefghijklmnopqrstuvwxyz".ToHashSet();
                options.Separator = " ";
                options.Tokenizer = (text, separator) => text
                    .Split(separator, StringSplitOptions.RemoveEmptyEntries)
                    .Distinct()
                    .ToArray();

                options.Tolerance = 100;
                options.ToleranceEnable = true;
            })
            .AddText("good movie", Sentiment.Positive)
            .AddText("not a good movie", Sentiment.Negative)
            .AddText("did not like", Sentiment.Negative);
            
        _builder = builder;
    }

    [Fact]
    public void Build_ShouldReturnCorrectBagOfWords()
    {
        // Arrange
        var expected = NaiveBayesTextClassificatorTestsBuilder.Create()
            .WithDefaultOptions()
            .WithPositiveSentimentTextProbability(0.33)
            .WithWordProbabilities(new Dictionary<WordSentiment, double>
            {
                {("good", Sentiment.Positive), 0.99},
                {("movie", Sentiment.Positive), 0.99},
                {("not", Sentiment.Positive), 0.01},
                {("a", Sentiment.Positive), 0.01},
                {("did", Sentiment.Positive), 0.01},
                {("like", Sentiment.Positive), 0.01},

                {("good", Sentiment.Negative), 0.5},
                {("movie", Sentiment.Negative), 0.5},
                {("not", Sentiment.Negative), 0.99},
                {("a", Sentiment.Negative), 0.5},
                {("did", Sentiment.Negative), 0.5},
                {("like", Sentiment.Negative), 0.5}
            })
            .Build();

        // Act
        var classificator = _builder.Build();

        // Assert
        classificator.Should().BeEquivalentTo(expected,
            options => options.Excluding(x => x.Options.Lemmitizer)
                .Excluding(x => x.Options.Stemmer)
                .Excluding(x => x.Options.LemmitizationEnable)
                .Excluding(x => x.Options.MaxProbability)
                .Excluding(x => x.Options.MinProbability)
                .Excluding(x => x.Options.StemmingEnable)
                .Excluding(x => x.Options.StopWords)
                .Excluding(x => x.Options.ExcludeFrequentWords)
                .Excluding(x => x.Options.ExcludeIdfWords)
                .Excluding(x => x.Options.ExcludePmiWords)
                .Excluding(x => x.Options.ExcludeRareWords)
                .Excluding(x => x.Options.Tokenizer)
                .WithPrecision(2));
    }

    [Fact]
    public void Build_ShouldThrowInvalidOperationException_WhenAlreadyBuild()
    {
        // Arrange

        // Act
        _builder.Build();

        var act = () => _builder.Build();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void IsBuild_ShouldBeFalse_WhenNotBuild()
    {
        // Arrange

        // Act

        // Assert
        _builder.IsBuild.Should().BeFalse();
    }

    [Fact]
    public void IsBuild_ShouldBeTrue_WhenBuild()
    {
        // Arrange

        // Act
        _builder.Build();

        // Assert
        _builder.IsBuild.Should().BeTrue();
    }
}