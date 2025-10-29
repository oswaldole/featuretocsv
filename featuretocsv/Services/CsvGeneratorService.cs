using CsvHelper;
using CsvHelper.Configuration;
using FeatureToCSV.Models;
using System.Globalization;

namespace FeatureToCSV.Services;

public class CsvGeneratorService
{
    public void GenerateCsv(List<ZephyrTestCase> testCases, string outputPath)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ",",
            Quote = '"',
            ShouldQuote = args => true // Quote all fields for safety
        };

        using var writer = new StreamWriter(outputPath);
        using var csv = new CsvWriter(writer, config);

        // Write header
        csv.WriteField("Name");
        csv.WriteField("Description");
        csv.WriteField("Steps");
        csv.WriteField("Expected Result");
        csv.WriteField("Priority");
        csv.WriteField("Status");
        csv.WriteField("Labels");
        csv.NextRecord();

        // Write test cases
        foreach (var testCase in testCases)
        {
            csv.WriteField(testCase.Name);
            csv.WriteField(testCase.Description);
            csv.WriteField(testCase.Steps);
            csv.WriteField(testCase.ExpectedResult);
            csv.WriteField(testCase.Priority);
            csv.WriteField(testCase.Status);
            csv.WriteField(testCase.Labels);
            csv.NextRecord();
        }

        Console.WriteLine($"CSV file generated successfully: {outputPath}");
        Console.WriteLine($"Total test cases: {testCases.Count}");
    }
}
