using Domain.Models;
using Serilog;

namespace Domain.Validation;

public sealed class NaiveBayesValidator
{
    private readonly NaiveBayesValidationOptions _options;

    public NaiveBayesValidator(Action<NaiveBayesValidationOptions> configure = default!)
    {
        _options = new NaiveBayesValidationOptions();
        configure(_options);
    }

    public double Validate(NaiveBayesTextClassificator classificator, TextModel[] validationData)
    {
        var step = 0;
        
        var correct = validationData
            .Count(validation =>
            {
                if (step % _options.LoggingStep == 0)
                {
                    Log.Information("Step: {Step}", step);
                }

                step++;
        
                var actualSentiment = classificator
                    .Predict(validation.Text);

                if (actualSentiment == validation.Sentiment || !(Random.Shared.NextDouble() >= _options.TextOutputProbability))
                {
                    return actualSentiment == validation.Sentiment;
                }
        
                Log.Warning("Text was not recognized correctly\nExpected sentiment: {ExpectedSentiment}, but was: {ActualSentiment}\nText: {Text}",
                    validation.Sentiment,
                    actualSentiment,
                    validation.Text);

                return actualSentiment == validation.Sentiment;
            });

        return correct / (double) validationData.Length;
    }
}