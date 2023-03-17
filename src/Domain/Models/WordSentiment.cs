namespace Domain.Models;

public record WordSentiment(string Word, Sentiment Sentiment)
{
    public static implicit operator WordSentiment((string, Sentiment) tuple)
    {
        return new WordSentiment(tuple.Item1, tuple.Item2);
    }
};