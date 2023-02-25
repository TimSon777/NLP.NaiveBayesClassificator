using FluentAssertions.Equivalency;

namespace Domain.UnitTests;

public sealed class NaiveBayesTextClassificatorBuilderTests
{
    private readonly NaiveBayesTextClassificatorBuilder _builder;
    private static readonly IReadOnlyCollection<char> Separators = new[] {' '};

    public NaiveBayesTextClassificatorBuilderTests()
    {
        var builder = NaiveBayesTextClassificatorBuilder.Create()
            .AddText("good movie", Sentiment.Positive)
            .AddText("not a good movie", Sentiment.Negative)
            .AddText("did not like", Sentiment.Negative);
            
        _builder = builder;
    }

    [Fact]
    public void Build_ShouldReturnCorrectBagOfWords()
    {
        // Arrange
        var expected = new NaiveBayesTextClassificator
        {
            UniqueWords = new HashSet<string>
            {
                "good",
                "movie",
                "not",
                "a",
                "did",
                "like"
            },
            WordProbabilities = new Dictionary<(string Word, Sentiment sentiment), double>
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
            },
            WordSeparators = Separators,
            NegativeSentimentTextProbability = 0.67,
            PositiveSentimentTextProbability = 0.33
        };

        // Act
        var classificator = _builder.Build(Separators);

        // Assert
        classificator.Should().BeEquivalentTo(expected,
            options => options.WithPrecision(2));
    }

    [Fact]
    public void Build_ShouldThrowInvalidOperationException_WhenAlreadyBuild()
    {
        // Arrange

        // Act
        _builder.Build(Separators);

        var act = () => _builder.Build(Separators);

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
        _builder.Build(Separators);

        // Assert
        _builder.IsBuild.Should().BeTrue();
    }
}