using Domain;
using Domain.Models;
using Domain.Validation;
using Iveonik.Stemmers;
using LemmaSharp;
using Serilog;
using Services;

namespace ConsoleApp;

public static class ApplicationRunner
{
    public static async Task RunAsync(string pathToTexts, string pathToStopWords, string pathToLemmitization)
    {
        var stream = File.OpenRead(pathToLemmitization);
        var lemmitizer = new Lemmatizer(stream);
        
        var stopWords = await File.ReadAllLinesAsync(pathToStopWords);
        Log.Information("Reading data");
        var texts = await ReadTextsAsync(pathToTexts);

        Log.Information("Split the data into training and validation");
        var (validationData, trainData) = new Randomizer(Random.Shared).RandomDistribute(texts, 0.25);

        Log.Information("Train model");
        var classificator = TrainModel(trainData, stopWords.ToHashSet(), lemmitizer);

        Log.Information("Dictionary size: {Size}", classificator.WordProbabilities.Count);
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

    private static NaiveBayesTextClassificator TrainModel(IEnumerable<TextModel> trainData, HashSet<string> stopWords, ILemmatizer lemmitizer)
    {
        var builder = NaiveBayesTextClassificatorBuilder.Create(options =>
        {
            options.ValidChars = "abcdefghijklmnopqrstuvwxyz".ToHashSet();
            options.Separator = " ";
            options.Tokenizer = (text, separator) => text.Split(separator, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
            
            options.ExcludeStopWordsEnable = true;
            options.StopWords = stopWords;
            
            options.ExcludeFrequentWords = true;
            options.MaxProbability = 0.9;
            
            options.ExcludeRareWords = true;
            options.MinProbability = 0.00001;

            options.ExcludePmiWords = false;
            options.PercentExcludingPmiWords = 0.3;

            options.ExcludeIdfWords = false;
            options.PercentExcludingIdfWords = 0.9;

            options.LemmitizationEnable = false;
            options.Lemmitizer = lemmitizer;

            options.StemmingEnable = true;
            options.Stemmer = new EnglishStemmer();
        });

        foreach (var test in trainData)
        {
            builder.AddText(test.Text, test.Sentiment);
        }

        return builder.Build();
    }
}