using CsvHelper.Configuration.Attributes;
using Domain;

namespace Services;

public sealed class Review
{
    [Name("id")]
    public required string Id { get; set; }
    
    [Name("sentiment")]
    public required Sentiment Sentiment { get; set; }
    
    [Name("review")]
    public required string Text { get; set; }
}