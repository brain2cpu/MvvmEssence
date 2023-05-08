namespace TestProject;

public class NamespaceInclusionCheckerTests
{
    [Theory]
    [InlineData("a", false)]
    [InlineData("a.a", false)]
    [InlineData("a.b", true)]
    [InlineData("a.c", false)]
    [InlineData("a.c.d", true)]
    [InlineData("a.c.d.e", false)]
    [InlineData("a.e", true)]
    [InlineData("a.e.x1", true)]
    [InlineData("a.e.x2.x3", true)]
    public void Includes_StateUnderTest_ExpectedBehavior(string ns, bool expected)
    {
        // Arrange
        var namespaceInclusionChecker = new NamespaceInclusionChecker(new [] {"a.b", "a.c.d", "a.e.*", "a.b.*"});

        // Act
        var result = namespaceInclusionChecker.Includes(ns);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Includes_ReturnsFalseOnEmptyNamespace()
    {
        // Arrange
        var namespaceInclusionChecker = new NamespaceInclusionChecker(new[] { "a.b", "a.c.d", "a.e.*", "a.b.*" });

        // Act
        var result = namespaceInclusionChecker.Includes(null);

        // Assert
        Assert.False(result);
    }
}