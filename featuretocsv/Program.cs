using FeatureToCSV;
using FeatureToCSV.Services;

Console.WriteLine("=== Feature to CSV Converter for Zephyr ===");
Console.WriteLine();

if (args.Length == 0)
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  featuretocsv <feature-file-path> [output-csv-path]");
    Console.WriteLine("  featuretocsv --reformat <input-csv> <output-csv>");
    Console.WriteLine();
    Console.WriteLine("Commands:");
    Console.WriteLine("  Default mode:");
    Console.WriteLine("    <feature-file-path>  Path to the .feature file (required)");
    Console.WriteLine("    [output-csv-path]    Path for the output CSV file (optional)");
    Console.WriteLine("                         If not provided, uses the same name as the feature file with .csv extension");
    Console.WriteLine();
    Console.WriteLine("  --reformat mode:");
    Console.WriteLine("    Converts a CSV from comma-delimited single-row format to semicolon-delimited multi-row format");
    Console.WriteLine("    <input-csv>          Path to the input CSV file");
    Console.WriteLine("    <output-csv>         Path for the reformatted CSV file");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  featuretocsv login.feature");
    Console.WriteLine("  featuretocsv login.feature output.csv");
    Console.WriteLine("  featuretocsv --reformat example.csv reformatted.csv");
    return 1;
}

// Check if reformat mode
if (args[0] == "--reformat" || args[0] == "-r")
{
    if (args.Length < 3)
    {
        Console.WriteLine("Error: --reformat requires input and output file paths");
        Console.WriteLine("Usage: featuretocsv --reformat <input-csv> <output-csv>");
        return 1;
    }

    try
    {
        var inputCsv = args[1];
        var outputCsv = args[2];

        if (!File.Exists(inputCsv))
        {
            Console.WriteLine($"Error: Input file not found: {inputCsv}");
            return 1;
        }

        Console.WriteLine($"Input file:  {inputCsv}");
        Console.WriteLine($"Output file: {outputCsv}");
        Console.WriteLine();
        Console.WriteLine("Reformatting CSV...");

        CsvReformatter.ReformatCsv(inputCsv, outputCsv);

        Console.WriteLine();
        Console.WriteLine("Success! The CSV file has been reformatted.");
        return 0;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        Console.WriteLine($"Details: {ex}");
        return 1;
    }
}

try
{
    var featureFilePath = args[0];

    // Determine output path
    string outputPath;
    if (args.Length > 1)
    {
        outputPath = args[1];
    }
    else
    {
        outputPath = Path.ChangeExtension(featureFilePath, ".csv");
    }

    Console.WriteLine($"Input file:  {featureFilePath}");
    Console.WriteLine($"Output file: {outputPath}");
    Console.WriteLine();

    // Parse the feature file
    Console.WriteLine("Parsing feature file...");
    var parser = new GherkinParserService();
    var testCases = parser.ParseFeatureFile(featureFilePath);

    if (testCases.Count == 0)
    {
        Console.WriteLine("Warning: No test cases found in the feature file.");
        return 1;
    }

    Console.WriteLine($"Found {testCases.Count} test case(s)");
    Console.WriteLine();

    // Generate CSV
    Console.WriteLine("Generating CSV file...");
    var csvGenerator = new CsvGeneratorService();
    csvGenerator.GenerateCsv(testCases, outputPath);

    Console.WriteLine();
    Console.WriteLine("Success! The CSV file is ready for import into Zephyr.");

    return 0;
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    return 1;
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Details: {ex}");
    return 1;
}
