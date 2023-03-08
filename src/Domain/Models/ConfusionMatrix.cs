namespace Domain.Models;

public sealed class ConfusionMatrix
{
    public required int TruePositive { get; set; }
    public required int FalsePositive { get; set; }
    public required int TrueNegative { get; set; }
    public required int FalseNegative { get; set; }
}