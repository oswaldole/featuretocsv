using Gherkin;
using Gherkin.Ast;
using FeatureToCSV.Models;
using System.Text;

namespace FeatureToCSV.Services;

public class GherkinParserService
{
    public List<ZephyrTestCase> ParseFeatureFile(string featureFilePath)
    {
        if (!File.Exists(featureFilePath))
        {
            throw new FileNotFoundException($"Feature file not found: {featureFilePath}");
        }

        var parser = new Parser();
        var testCases = new List<ZephyrTestCase>();

        using var reader = new StreamReader(featureFilePath);
        var gherkinDocument = parser.Parse(reader);

        if (gherkinDocument.Feature == null)
        {
            throw new InvalidOperationException("No feature found in the file");
        }

        var feature = gherkinDocument.Feature;

        foreach (var child in feature.Children)
        {
            if (child is Scenario scenario)
            {
                // Check if it's a scenario outline (has examples)
                if (scenario.Examples != null && scenario.Examples.Any())
                {
                    testCases.AddRange(ConvertScenarioOutlineToTestCases(scenario, feature));
                }
                else
                {
                    testCases.Add(ConvertScenarioToTestCase(scenario, feature));
                }
            }
        }

        return testCases;
    }

    private ZephyrTestCase ConvertScenarioToTestCase(Scenario scenario, Feature feature)
    {
        var steps = new StringBuilder();
        var expectedResults = new StringBuilder();
        int stepNumber = 1;

        foreach (var step in scenario.Steps)
        {
            steps.AppendLine($"{stepNumber}. {step.Keyword.Trim()} {step.Text}");

            if (step.Argument is DocString docString)
            {
                steps.AppendLine($"   {docString.Content}");
            }
            else if (step.Argument is DataTable dataTable)
            {
                foreach (var row in dataTable.Rows)
                {
                    var cells = string.Join(" | ", row.Cells.Select(c => c.Value));
                    steps.AppendLine($"   | {cells} |");
                }
            }

            // Expected results are typically in "Then" steps
            if (step.Keyword.Trim().Equals("Then", StringComparison.OrdinalIgnoreCase))
            {
                expectedResults.AppendLine($"{stepNumber}. {step.Text}");

                if (step.Argument is DocString docString2)
                {
                    expectedResults.AppendLine($"   {docString2.Content}");
                }
            }

            stepNumber++;
        }

        var tags = string.Join(", ", scenario.Tags.Select(t => t.Name.TrimStart('@')));

        return new ZephyrTestCase
        {
            Name = $"{feature.Name} - {scenario.Name}",
            Description = scenario.Description ?? string.Empty,
            Steps = steps.ToString().TrimEnd(),
            ExpectedResult = expectedResults.Length > 0 ? expectedResults.ToString().TrimEnd() : "Verify all steps complete successfully",
            Labels = tags,
            Priority = DeterminePriority(scenario.Tags),
            Status = "Draft"
        };
    }

    private List<ZephyrTestCase> ConvertScenarioOutlineToTestCases(Scenario scenarioOutline, Feature feature)
    {
        var testCases = new List<ZephyrTestCase>();

        foreach (var examples in scenarioOutline.Examples)
        {
            if (examples.TableHeader == null) continue;

            var headers = examples.TableHeader.Cells.Select(c => c.Value).ToList();

            foreach (var row in examples.TableBody)
            {
                var values = row.Cells.Select(c => c.Value).ToList();
                var parameterMap = headers.Zip(values, (h, v) => new { Header = h, Value = v })
                                         .ToDictionary(x => $"<{x.Header}>", x => x.Value);

                var steps = new StringBuilder();
                var expectedResults = new StringBuilder();
                int stepNumber = 1;

                foreach (var step in scenarioOutline.Steps)
                {
                    var stepText = ReplaceParameters(step.Text, parameterMap);
                    steps.AppendLine($"{stepNumber}. {step.Keyword.Trim()} {stepText}");

                    if (step.Keyword.Trim().Equals("Then", StringComparison.OrdinalIgnoreCase))
                    {
                        expectedResults.AppendLine($"{stepNumber}. {stepText}");
                    }

                    stepNumber++;
                }

                var exampleDescription = string.Join(", ", parameterMap.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                var tags = string.Join(", ", scenarioOutline.Tags.Select(t => t.Name.TrimStart('@')));

                testCases.Add(new ZephyrTestCase
                {
                    Name = $"{feature.Name} - {scenarioOutline.Name} ({exampleDescription})",
                    Description = scenarioOutline.Description ?? string.Empty,
                    Steps = steps.ToString().TrimEnd(),
                    ExpectedResult = expectedResults.Length > 0 ? expectedResults.ToString().TrimEnd() : "Verify all steps complete successfully",
                    Labels = tags,
                    Priority = DeterminePriority(scenarioOutline.Tags),
                    Status = "Draft"
                });
            }
        }

        return testCases;
    }

    private string ReplaceParameters(string text, Dictionary<string, string> parameterMap)
    {
        var result = text;
        foreach (var kvp in parameterMap)
        {
            result = result.Replace(kvp.Key, kvp.Value);
        }
        return result;
    }

    private string DeterminePriority(IEnumerable<Tag> tags)
    {
        var tagNames = tags.Select(t => t.Name.ToLower()).ToList();

        if (tagNames.Any(t => t.Contains("critical") || t.Contains("high")))
            return "High";
        if (tagNames.Any(t => t.Contains("low")))
            return "Low";

        return "Medium";
    }
}
