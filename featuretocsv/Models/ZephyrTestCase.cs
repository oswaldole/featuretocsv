namespace FeatureToCSV.Models;

public class ZephyrTestCase
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Steps { get; set; } = string.Empty;
    public string ExpectedResult { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string Status { get; set; } = "Draft";
    public string Labels { get; set; } = string.Empty;
}
