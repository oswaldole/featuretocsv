# Feature to CSV Converter for Zephyr

A .NET Core 8 console application that converts Gherkin `.feature` files into CSV format for import into the Zephyr plugin in Jira.

## Features

- Parses standard Gherkin format (Given, When, Then)
- Supports both regular scenarios and scenario outlines with examples
- Handles tags (converts to labels and priority)
- Extracts steps and expected results
- Generates CSV files compatible with Zephyr import

## Requirements

- .NET 8.0 SDK or later

## Installation

1. Clone or download this repository
2. Navigate to the project directory:
   ```
   cd featuretocsv\featuretocsv
   ```
3. Restore dependencies:
   ```
   dotnet restore
   ```
4. Build the project:
   ```
   dotnet build
   ```

## Usage

### Basic Usage

```bash
dotnet run <feature-file-path>
```

This will generate a CSV file with the same name as the feature file.

### Specify Output Path

```bash
dotnet run <feature-file-path> <output-csv-path>
```

### Examples

```bash
# Generate example.csv from example.feature
dotnet run example.feature

# Generate custom output file
dotnet run example.feature my-test-cases.csv

# From a different directory
dotnet run C:\path\to\login.feature C:\output\login-tests.csv
```

## Feature File Format

The application supports standard Gherkin syntax:

```gherkin
@tag1 @tag2
Feature: Feature Name
  Feature description

  @scenario-tag
  Scenario: Scenario name
    Given a precondition
    When an action is performed
    Then an expected result occurs

  Scenario Outline: Template scenario
    Given a user with "<username>"
    When they log in with "<password>"
    Then they should see "<result>"

    Examples:
      | username | password | result  |
      | user1    | pass1    | success |
      | user2    | pass2    | failure |
```

## Tag-Based Priority Mapping

The application automatically maps tags to priority levels:

- Tags containing `@critical` or `@high` → High priority
- Tags containing `@low` → Low priority
- All other scenarios → Medium priority

## CSV Output Format

The generated CSV includes the following columns:

- **Name**: Feature name + Scenario name
- **Description**: Scenario description
- **Steps**: All Given/When/Then steps numbered
- **Expected Result**: All "Then" steps
- **Priority**: High/Medium/Low (based on tags)
- **Status**: Always "Draft"
- **Labels**: Comma-separated list of tags

## Example Output

From the included `example.feature`, the application generates test cases for:
- Successful login with valid credentials
- Failed login with invalid password
- Login with empty credentials
- Login with different user roles (expanded from scenario outline)

## Project Structure

```
featuretocsv/
├── Models/
│   └── ZephyrTestCase.cs          # Data model for test cases
├── Services/
│   ├── GherkinParserService.cs    # Parses .feature files
│   └── CsvGeneratorService.cs     # Generates CSV output
├── Program.cs                      # Main entry point
├── featuretocsv.csproj            # Project configuration
└── example.feature                # Example Gherkin file
```

## Dependencies

- **Gherkin** (v29.0.0): Official Gherkin parser
- **CsvHelper** (v33.0.1): CSV generation library

## Importing to Zephyr

1. Run the application to generate a CSV file
2. In Jira, navigate to your project
3. Go to the Zephyr plugin
4. Select "Import Test Cases"
5. Choose CSV format
6. Upload the generated CSV file
7. Map the columns if necessary
8. Complete the import

## Notes

- All test cases are generated with "Draft" status
- Scenario outlines are expanded into individual test cases for each example row
- DocStrings and data tables in steps are preserved in the output
- The application handles special characters and line breaks properly

## License

This project is provided as-is for use with Zephyr test management in Jira.
