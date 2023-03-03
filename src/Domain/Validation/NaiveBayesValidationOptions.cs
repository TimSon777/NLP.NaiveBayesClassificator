namespace Domain.Validation;

public sealed class NaiveBayesValidationOptions
{
    private double _textOutputProbability = 0.75;
    public double TextOutputProbability
    {
        get => _textOutputProbability;
        set
        {
            if (value is < 0 or > 1)
            {
                throw new ArgumentException($"TextOutputProbability must be between 0 and 1, but was {value}.");
            }
            
            _textOutputProbability = value;
        }
    }

    private int _loggingStep = 10;
    public int LoggingStep
    {
        get => _loggingStep;
        set
        {
            if (value < 0)
            {
                throw new ArgumentException($"LoggingStep must be greater than 0, but was {value}.");
            }
            
            _loggingStep = value;
        }
    }
}