using System.Runtime.CompilerServices;

namespace Domain.Models;

public sealed class QualityMetrics
{
    public double Accuracy => _totalCorrect / (double) _total;

    public double BalancedAccuracy => 0.5 * (ConfusionMatrix.TruePositive / (double) (ConfusionMatrix.TruePositive + ConfusionMatrix.FalseNegative) +
                                             ConfusionMatrix.TrueNegative / (double) (ConfusionMatrix.TrueNegative + ConfusionMatrix.FalsePositive));

    public readonly ConfusionMatrix ConfusionMatrix;

    public double PositivePrecision => GetRecallOrPrecision(ConfusionMatrix.TruePositive, ConfusionMatrix.FalsePositive);
    public double NegativePrecision => GetRecallOrPrecision(ConfusionMatrix.TrueNegative, ConfusionMatrix.FalseNegative);
    public double PositiveRecall => GetRecallOrPrecision(ConfusionMatrix.TruePositive, ConfusionMatrix.FalseNegative);
    public double NegativeRecall => GetRecallOrPrecision(ConfusionMatrix.TrueNegative, ConfusionMatrix.FalsePositive);

    public double PositiveFMeasure => GetFMeasure(PositivePrecision, PositiveRecall);
    public double NegativeFMeasure => GetFMeasure(NegativePrecision, NegativeRecall);

    private readonly int _totalCorrect;
    private readonly int _total;

    public QualityMetrics(int correct, int total, int tp, int fp, int tn, int fn)
    {
        _totalCorrect = correct;
        _total = total;

        ConfusionMatrix = new ConfusionMatrix
        {
            TruePositive = ThrowIfNegative(tp),
            TrueNegative = ThrowIfNegative(tn),
            FalseNegative = ThrowIfNegative(fn),
            FalsePositive = ThrowIfNegative(fp)
        };
    }

    public override string ToString()
    {
        return 
            $"""
                Accuracy: {Accuracy}
                
                Balanced Accuracy: {BalancedAccuracy}
                
                ConfusionMatrix: 
                {ConfusionMatrix.TruePositive} {ConfusionMatrix.FalseNegative}
                {ConfusionMatrix.FalsePositive} {ConfusionMatrix.TrueNegative}
    
                +Precision: {PositivePrecision}
                +Recall: {PositiveRecall}
                
                -Precision: {NegativePrecision}
                -Recall: {NegativeRecall}
    
                +F-Measure: {PositiveFMeasure}
                -F-Measure: {NegativeFMeasure}
            """;
    }

    private static double GetFMeasure(double precision, double recall)
    {
        return 2 * (precision * recall) / (precision + recall);
    }

    private static double GetRecallOrPrecision(int correct, int incorrect)
    {
        return correct / (double) (correct + incorrect);
    }

    private static int ThrowIfNegative(int value, [CallerArgumentExpression(nameof(value))] string paramName = "")
    {
        if (value < 0)
        {
            throw new ArgumentException($"{paramName} can't be negative. Actual {value}.");
        }

        return value;
    }
}