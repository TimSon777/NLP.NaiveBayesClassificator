namespace Domain.Models;

public sealed class TextModel
{
    public required string Id { get; set; }
    
    public required Sentiment Sentiment { get; set; }
    
    public required string Text { get; set; }
}