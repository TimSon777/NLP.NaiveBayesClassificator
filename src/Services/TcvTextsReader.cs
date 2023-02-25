using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace Services;

public sealed class TcvTextsReader
{
    public IEnumerable<Review> ReadTexts(string path)
    {
        using var streamReader = new StreamReader(path);
        using var reader = new CsvReader(new CsvParser(streamReader,
            new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                Delimiter = "\t"
            }));

        return reader
            .GetRecords<Review>()
            .ToArray();
    }
}