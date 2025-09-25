using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Chirp.CLI;

public class ProgramTests
{
    [Fact]
    public async Task Main_WithCheepArgument_DoesNotThrow()
    {
        // Arrange
        string[] args = new[] { "cheep", "Hello, test!" };

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => Task.Run(() => Chirp.CLI.Program.Main(args)));
        Assert.Null(exception);
    }

    [Fact]
    public async Task MainNoArgs_HandlesExceptions()
    {
        // Arrange
        string[] args = Array.Empty<string>();

        using var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);
        int exitCode = 0;
        try
        {
            exitCode = await Chirp.CLI.Program.Main(args);
        }
        catch (Exception ex)
        {
            Assert.True(false, $"Unhandled exception occurred: {ex.Message}");
        }
        Assert.Equal(0, exitCode);
        string output = consoleOutput.ToString();
        Assert.DoesNotContain("Unhandled Exception", output);
    }

    [Fact]
    public async Task Main_WithCheepArgument_AndApiFails_PrintsFallbackMessage()
    {
        // Arrange
        string[] args = new[] { "cheep", "Hello, test!" };

        // Redirect Console output
        using var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);

        // Simulate API failure
        Environment.SetEnvironmentVariable("http_proxy", "http://127.0.0.1:0");

        // Act
        var exception = await Record.ExceptionAsync(() => Task.Run(() => Chirp.CLI.Program.Main(args)));
        Assert.Null(exception);

        string output = consoleOutput.ToString();
        Assert.Contains("API unreachable", output);
        Assert.Contains("Cheep stored in CSV database", output);
    }

    [Fact]
    public async Task MainCheepNoAuthor_UsesEnvironmentUsername()
    {
        // Arrange
        string[] args = new[] { "cheep", "Hello test!" };

        // Redirect Console output to silence test logs
        using var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);

        // Simulate API failure to force fallback to CSV
        Environment.SetEnvironmentVariable("http_proxy", "http://127.0.0.1:0");

        // Use the actual CSV database path
        var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var dataDir = Path.Combine(solutionRoot, "data");
        var csvPath = Path.Combine(dataDir, "chirp_cli_db.csv");

        try
        {
            // Act
            var exception = await Record.ExceptionAsync(() => Task.Run(() => Chirp.CLI.Program.Main(args)));
            Assert.Null(exception);

            var expectedAuthor = Environment.UserName;

            // Read the last line from the actual CSV database
            var lastLine = File.ReadAllLines(csvPath).LastOrDefault();
            Assert.NotNull(lastLine);
            Assert.Contains(expectedAuthor, lastLine);
            Assert.Contains("Hello test!", lastLine);
        }
        finally
        {
            // Cleanup: Remove the test cheep from the CSV database
            if (File.Exists(csvPath))
            {
                var lines = File.ReadAllLines(csvPath).ToList();
                if (lines.Any())
                {
                    lines.RemoveAt(lines.Count - 1); // Remove the last line (test cheep)
                    File.WriteAllLines(csvPath, lines);
                }
            }
        }
    }

    [Fact]
    public async Task Main_CheepWithSpecificAuthor_StoresCorrectAuthor()
    {
        // Arrange
        string[] args = new[] { "cheep", "Hello, test!", "--author=TestAuthor" };

        // Redirect Console output to silence test logs
        using var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);

        // Simulate API failure to force fallback to CSV
        Environment.SetEnvironmentVariable("http_proxy", "http://127.0.0.1:0");

        // Use the actual CSV database path
        var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var dataDir = Path.Combine(solutionRoot, "data");
        var csvPath = Path.Combine(dataDir, "chirp_cli_db.csv");

        try
        {
            // Act
            var exception = await Record.ExceptionAsync(() => Task.Run(() => Chirp.CLI.Program.Main(args)));
            Assert.Null(exception);

            // Read the last line from the actual CSV database
            var lastLine = File.ReadAllLines(csvPath).LastOrDefault();
            Assert.NotNull(lastLine);
            Assert.Contains("TestAuthor", lastLine);
            Assert.Contains("Hello, test!", lastLine);
        }
        finally
        {
            // Cleanup: Remove the test cheep from the CSV database
            if (File.Exists(csvPath))
            {
                var lines = File.ReadAllLines(csvPath).ToList();
                if (lines.Any())
                {
                    lines.RemoveAt(lines.Count - 1); // Remove the last line (test cheep)
                    File.WriteAllLines(csvPath, lines);
                }
            }
        }
    }

    [Fact]
    public async Task Main_CheepWithApiSuccess_StoresInCsv()
    {
        // Arrange
        string[] args = new[] { "cheep", "Hello, API!" };

        // Redirect Console output
        using var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);

        // Simulate API success
        Environment.SetEnvironmentVariable("http_proxy", null);

        // Use the actual CSV database path
        var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var dataDir = Path.Combine(solutionRoot, "data");
        var csvPath = Path.Combine(dataDir, "chirp_cli_db.csv");

        try
        {
            // Act
            var exception = await Record.ExceptionAsync(() => Task.Run(() => Chirp.CLI.Program.Main(args)));
            Assert.Null(exception);

            // Verify that the cheep was not written to the CSV database
            if (File.Exists(csvPath))
            {
                var lines = File.ReadAllLines(csvPath).ToList();
                Assert.Contains(lines, line => line.Contains("Hello, API!"));
            }
        }
        finally
        {
            // Cleanup: Ensure no test data remains in the CSV database
            if (File.Exists(csvPath))
            {
                var lines = File.ReadAllLines(csvPath).ToList();
                lines.RemoveAll(line => line.Contains("Hello, API!"));
                File.WriteAllLines(csvPath, lines);
            }
        }
    }

    [Fact]
    public async Task Main_RetrieveCheepsFromApi_PrintsCheeps()
    {
        // Arrange
        string[] args = Array.Empty<string>();

        // Redirect Console output
        using var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);

        // Simulate API success
        Environment.SetEnvironmentVariable("http_proxy", null);

        // Act
        var exception = await Record.ExceptionAsync(() => Task.Run(() => Chirp.CLI.Program.Main(args)));
        Assert.Null(exception);

        string output = consoleOutput.ToString();
        Assert.Contains("Displaying cheeps", output); // Adjust based on actual output format
    }
}