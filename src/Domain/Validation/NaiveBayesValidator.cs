using Domain.Models;
using Serilog;

namespace Domain.Validation;

public sealed class NaiveBayesValidator
{
    private readonly NaiveBayesValidationOptions _options;

    public NaiveBayesValidator(Action<NaiveBayesValidationOptions>? configure = default!)
    {
        _options = new NaiveBayesValidationOptions();

        configure?.Invoke(_options);
    }

    public QualityMetrics Validate(NaiveBayesTextClassificator classificator, TextModel[] testData)
    {
        var correct = 0;
        var total = testData.Length;
        var tp = 0;
        var tn = 0;
        var fp = 0;
        var fn = 0;

        for (var index = 0; index < testData.Length; index++)
        {
            var test = testData[index];
            if (index % _options.LoggingStep == 0)
            {
                Log.Information("Step: {Step}", index);
            }

            var actualSentiment = classificator.Predict(test.Text);

            if (actualSentiment != test.Sentiment && Random.Shared.NextDouble() <= _options.TextOutputProbability)
            {
                Log.Warning(
                    "Text was not recognized correctly\nExpected sentiment: {ExpectedSentiment}, but was: {ActualSentiment}\nText: {Text}",
                    test.Sentiment,
                    actualSentiment,
                    test.Text);
            }

            if (actualSentiment == test.Sentiment)
            {
                correct++;

                if (actualSentiment == Sentiment.Positive)
                {
                    tp++;
                }
                else
                {
                    tn++;
                }
            }
            else
            {
                if (actualSentiment == Sentiment.Positive)
                {
                    fp++;
                }
                else
                {
                    fn++;
                }
            }
        }

        return new QualityMetrics(correct, total, tp, fp, tn, fn);
    }
}