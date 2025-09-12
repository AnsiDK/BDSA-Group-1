using System;
using Xunit;

public class ProgramTests
{
    [Fact]
    public void Main_WithCheepArgument_DoesNotThrow()
    {
        // Arrange
        string[] args = new[] { "cheep", "Hello, test!" };

        // Act & Assert
        var exception = Record.Exception(() => Program.Main(args));
        Assert.Null(exception);
    }

    [Fact]
    public void Main_WithNoArguments_DoesNotThrow()
    {
        // Arrange
        string[] args = Array.Empty<string>();

        // Act & Assert
        var exception = Record.Exception(() => Program.Main(args));
        Assert.Null(exception);
    }
}