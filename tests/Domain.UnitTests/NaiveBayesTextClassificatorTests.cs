namespace Domain.UnitTests;

public sealed class NaiveBayesTextClassificatorTests
{
    [Fact]
    public void Predict_ShouldReturnCorrectSentiment()
    {
        // Arrange
        var separators = new[] {' '};

        var classificator = new NaiveBayesTextClassificator
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
            WordSeparators = separators,
            NegativeSentimentTextProbability = 0.67,
            PositiveSentimentTextProbability = 0.33
        };

        // Act
        var actual = classificator.Predict("movie was not good");

        // Assert
        actual.Should().Be(Sentiment.Negative);
    }
}