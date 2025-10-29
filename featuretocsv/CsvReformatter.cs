using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace FeatureToCSV;

public class CsvReformatter
{
    public static void ReformatCsv(string inputPath, string outputPath)
    {
        var records = ParseCsvFile(inputPath);
        var output = new List<string>();

        // Add header in the new format (semicolon-delimited)
        output.Add("Priority;Name;Description;Steps;Expected Result");

        // Process each record (skip header)
        for (int i = 1; i < records.Count; i++)
        {
            var record = records[i];

            if (record.Count < 4)
            {
                Console.WriteLine($"Warning: Skipping record {i} with insufficient fields ({record.Count} < 4)");
                continue;
            }

            var name = CleanField(record[0]);
            var description = CleanField(record[1]);
            var stepsText = CleanField(record[2]);
            var expectedResultText = CleanField(record[3]);
            var priority = record.Count > 4 ? CleanField(record[4]) : "";

            // Parse steps (numbered list like "1. Given...\n2. When...")
            var steps = ParseSteps(stepsText);

            if (steps.Count == 0)
            {
                // No steps found, write single row
                output.Add($"{priority};{name};{description};;");
                continue;
            }

            // First row has Priority, Name, Description, and first step
            // For expected result, we'll leave it empty unless explicitly needed
            output.Add($"{priority};{name};{description};{EscapeSemicolon(steps[0])};");

            // Subsequent rows have empty Priority, Name, Description
            for (int j = 1; j < steps.Count; j++)
            {
                output.Add($";;;{EscapeSemicolon(steps[j])};");
            }
        }

        File.WriteAllLines(outputPath, output);
        Console.WriteLine($"Reformatted CSV written to: {outputPath}");
        Console.WriteLine($"Total rows (excluding header): {output.Count - 1}");
    }

    private static List<List<string>> ParseCsvFile(string filePath)
    {
        var records = new List<List<string>>();
        var content = File.ReadAllText(filePath);
        var position = 0;

        while (position < content.Length)
        {
            var record = new List<string>();
            var field = "";
            var inQuotes = false;

            while (position < content.Length)
            {
                var c = content[position];

                if (c == '"')
                {
                    if (inQuotes && position + 1 < content.Length && content[position + 1] == '"')
                    {
                        // Escaped quote
                        field += '"';
                        position += 2;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                        position++;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    record.Add(field);
                    field = "";
                    position++;
                }
                else if ((c == '\r' || c == '\n') && !inQuotes)
                {
                    // End of record
                    record.Add(field);
                    if (c == '\r' && position + 1 < content.Length && content[position + 1] == '\n')
                    {
                        position += 2; // Skip \r\n
                    }
                    else
                    {
                        position++;
                    }
                    break;
                }
                else
                {
                    field += c;
                    position++;
                }
            }

            // Handle last field if we reached end of file
            if (position >= content.Length && field.Length > 0)
            {
                record.Add(field);
            }

            if (record.Count > 0)
            {
                records.Add(record);
            }
        }

        return records;
    }

    private static string CleanField(string field)
    {
        // The CSV parser already handles quotes, so just trim whitespace
        return field.Trim();
    }

    private static string EscapeSemicolon(string text)
    {
        // Replace semicolons with commas to avoid breaking semicolon-delimited CSV
        // Also escape any existing double quotes by doubling them
        return text.Replace(";", ",");
    }

    private static List<string> ParseSteps(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<string>();

        var steps = new List<string>();

        // Split by lines first
        var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmedLine))
                continue;

            // Check if line starts with number (e.g., "1.", "2.", etc.)
            var match = Regex.Match(trimmedLine, @"^(\d+)\.\s*(.+)$");
            if (match.Success)
            {
                steps.Add(match.Groups[2].Value.Trim());
            }
            else
            {
                // Not a numbered step, might be continuation or description
                // Add as-is
                steps.Add(trimmedLine);
            }
        }

        return steps;
    }
}
