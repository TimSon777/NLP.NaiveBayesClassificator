using Domain;
using Domain.Models;
using Domain.Validation;
using Serilog;
using Services;

namespace ConsoleApp;

public static class ApplicationRunner
{
    public static async Task RunAsync(string path)
    {
        Log.Information("Reading data");
        var texts = await ReadTextsAsync(path);

        Log.Information("Split the data into training and validation");
        var (validationData, trainData) = new Randomizer(Random.Shared).RandomDistribute(texts, 0.25);

        Log.Information("Train model");
        var classificator = TrainModel(trainData);

        Log.Information("Validation");
        var validator = new NaiveBayesValidator(options =>
        {
            options.TextOutputProbability = 0.01;
            options.LoggingStep = 100;
        });

        var qualityMetrics = validator.Validate(classificator, validationData);
        Log.Information("Quality Metrics\n{QualityMetrics}", qualityMetrics);
    }

    private static async Task<TextModel[]> ReadTextsAsync(string path)
    {
        var trainData = await File.ReadAllTextAsync(path);
        var formattedTrainData = trainData.Replace("\\\"", string.Empty);

        await File.WriteAllTextAsync(path, formattedTrainData);

        return new TcvTextsReader()
            .ReadTexts(path)
            .Select(TextModelMapper.Map)
            .ToArray();
    }

    private static NaiveBayesTextClassificator TrainModel(IEnumerable<TextModel> trainData)
    {
        var builder = NaiveBayesTextClassificatorBuilder.Create();

        var separators = new[] {' '};
        var validChars = new HashSet<char>("abcdefghijklmnopqrstuvwxyz'");

        foreach (var test in trainData)
        {
            builder.AddText(
                test.Text
                    .Replace("<br /><br />", string.Empty)
                    .ToLower()
                    .ReplaceInvalid(validChars, ' '),
                test.Sentiment);
        }

        return builder.Build(separators);
    }
}