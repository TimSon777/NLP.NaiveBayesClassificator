using Domain.Models;
using Domain.UnitTests.Builders;

namespace Domain.UnitTests;

public sealed class NaiveBayesTextClassificatorTests
{
    [Fact]
    public void Predict_ShouldReturnCorrectSentiment()
    {
        // Arrange
        var classificator = NaiveBayesTextClassificatorTestsBuilder.Create()
            .WithDefaultOptions()
            .WithPositiveSentimentTextProbability(0.33)
            .WithDefaultOptions()
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
        var actual = classificator.Predict("movie was not good");

        // Assert
        actual.Should().Be(Sentiment.Negative);
    }
}