using Domain.Models;
using Services;

namespace ConsoleApp;

public static class TextModelMapper
{
    public static TextModel Map(this Review review) => new()
    {
        Id = review.Id,
        Sentiment = review.Sentiment,
        Text = review.Text
    };
}