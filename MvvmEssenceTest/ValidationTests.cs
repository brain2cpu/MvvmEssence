namespace MvvmEssenceTest;

public class ValidationTests
{
    [Fact]
    public void StringLengthValidator_Empty_ReturnsError()
    {
        // Arrange
        var vm = new TestViewModel();

        // Act
        vm.LastName = "";

        // Assert
        Assert.True(vm.HasValidationErrors);
        Assert.Contains("Length must be between 2 and 50 characters", vm.GetValidationErrors(nameof(vm.LastName)));
    }
    
    [Fact]
    public void StringLengthValidator_TooLong_ReturnsError()
    {
        // Arrange
        var vm = new TestViewModel();

        // Act
        vm.LastName = new string('a', 51);

        // Assert
        Assert.True(vm.HasValidationErrors);
        Assert.Contains("Length must be between 2 and 50 characters", vm.GetValidationErrors(nameof(vm.LastName)));
    }

    [Fact]
    public void StringLengthValidator_AcceptedLength_NoError()
    {
        // Arrange
        var vm = new TestViewModel();

        // Act
        vm.LastName = new string('a', 5);

        // Assert
        Assert.Empty(vm.GetValidationErrors(nameof(vm.LastName)));
    }

    [Fact]
    public void RequiredValidator_EmptyString_ReturnsError()
    {
        // Arrange
        var vm = new TestViewModel();

        // Act
        vm.FirstName = "";

        // Assert
        Assert.True(vm.HasValidationErrors);
        Assert.Contains("This field is required", vm.GetValidationErrors(nameof(vm.FirstName)));
    }

    [Fact]
    public void RequiredValidator_Long_NoError()
    {
        // Arrange
        var vm = new TestViewModel();

        // Act
        vm.FirstName = new string('a', 51);

        // Assert
        Assert.Empty(vm.GetValidationErrors(nameof(vm.FirstName)));
    }

    [Fact]
    public void RequiredAndEmailValidator_EmptyEmail_ReturnsError()
    {
        // Arrange
        var vm = new TestViewModel();

        // Act
        vm.Email = "";

        // Assert
        Assert.True(vm.HasValidationErrors);
        Assert.Contains("This field is required", vm.GetValidationErrors(nameof(vm.Email)));
    }

    [Fact]
    public void EmailValidator_InvalidEmail_ReturnsError()
    {
        // Arrange
        var vm = new TestViewModel();

        // Act
        vm.Email = "invalid-email";

        // Assert
        Assert.True(vm.HasValidationErrors);
        Assert.Contains("Invalid email address", vm.GetValidationErrors(nameof(vm.Email)));
    }

    [Fact]
    public void EmailValidator_ValidEmail_NoError()
    {
        // Arrange
        var vm = new TestViewModel();

        // Act
        vm.Email = "test@example.com";

        // Assert
        Assert.Empty(vm.GetValidationErrors(nameof(vm.Email)));
    }

    [Fact]
    public void EmailValidator_EmptyEmail_NoError()
    {
        // Arrange
        var vm = new TestViewModel();

        // Act
        vm.SecondaryEmail = "";

        // Assert
        // these two are the same in the end
        Assert.True(vm.IsPropertyValid(nameof(vm.SecondaryEmail)));
        Assert.Empty(vm.GetValidationErrors(nameof(vm.SecondaryEmail)));
    }

    [Fact]
    public void RangeValidator_OutOfRange_ReturnsError()
    {
        // Arrange
        var vm = new TestViewModel();

        // Act
        vm.Age = 150;

        // Assert
        Assert.True(vm.HasValidationErrors);
        Assert.Contains("Value must be between 0 and 120", vm.GetValidationErrors(nameof(vm.Age)));
    }

    [Fact]
    public void RangeValidator_InRange_NoError()
    {
        // Arrange
        var vm = new TestViewModel();

        // Act
        vm.Age = 100;

        // Assert
        Assert.True(vm.IsPropertyValid(nameof(vm.Age)));
    }

    [Fact]
    public void RegexValidator_EmptyPattern_NoError()
    {
        // Arrange
        var vm = new TestViewModel();

        // Act
        vm.Website = "";

        // Assert
        Assert.Empty(vm.GetValidationErrors(nameof(vm.Website)));
    }

    [Fact]
    public void RegexValidator_InvalidPattern_ReturnsError()
    {
        // Arrange
        var vm = new TestViewModel();

        // Act
        vm.Website = "invalid-url";

        // Assert
        Assert.True(vm.HasValidationErrors);
        Assert.Contains("Value does not match the required pattern", vm.GetValidationErrors(nameof(vm.Website)));
    }

    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://example.com")]
    [InlineData("http://example.com/1")]
    public void RegexValidator_ValidPattern_NoError(string url)
    {
        // Arrange
        var vm = new TestViewModel();

        // Act
        vm.Website = url;

        // Assert
        Assert.Empty(vm.GetValidationErrors(nameof(vm.Website)));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("10")]
    [InlineData("10.5")]
    [InlineData("123,456.78")]
    public void NumberValidator_ValidString_NoError(string snr)
    {
        // Arrange
        var vm = new TestViewModel();

        // Act
        vm.Score = snr;

        // Assert
        Assert.True(vm.IsPropertyValid(nameof(vm.Website)));
    }

    [Fact]
    public void NumberValidator_InvalidString_ReturnsError()
    {
        // Arrange
        var vm = new TestViewModel();

        // Act
        vm.Score = "qwerty";

        // Assert
        Assert.True(vm.HasValidationErrors);
        Assert.Contains("Value must be a number", vm.GetValidationErrors(nameof(vm.Score)));
    }

    [Theory]
    [InlineData("2000-01-01")]
    [InlineData("2000/01/01")]
    [InlineData("2000.01.01")]
    public void DateValidator_ValidString_NoError(string snr)
    {
        // Arrange
        var vm = new TestViewModel();

        // Act
        vm.HireDate = snr;

        // Assert
        Assert.True(vm.IsPropertyValid(nameof(vm.HireDate)));
    }

    [Fact]
    public void DateValidator_InvalidString_ReturnsError()
    {
        // Arrange
        var vm = new TestViewModel();

        // Act
        vm.HireDate = "qwerty";

        // Assert
        Assert.True(vm.HasValidationErrors);
        Assert.Contains("Value must be a valid date", vm.GetValidationErrors(nameof(vm.HireDate)));
    }


    [Theory]
    [InlineData("A")]
    [InlineData("B")]
    [InlineData("C")]
    [InlineData("D")]
    public void BetweenValidator_ValidString_NoError(string snr)
    {
        // Arrange
        var vm = new TestViewModel();

        // Act
        vm.Grade = snr;

        // Assert
        Assert.True(vm.IsPropertyValid(nameof(vm.Grade)));
    }

    [Fact]
    public void BetweenValidator_InvalidString_ReturnsError()
    {
        // Arrange
        var vm = new TestViewModel();

        // Act
        vm.Grade = "qwerty";

        // Assert
        Assert.True(vm.HasValidationErrors);
        Assert.Contains("Value must be between A and D", vm.GetValidationErrors(nameof(vm.Grade)));
    }

    [Theory]
    [InlineData("abcd")]
    [InlineData("qwertyuiopasdfgh")]
    [InlineData("asdfghjkl")]
    public void CustomValidator_ValidString_NoError(string un)
    {
        // Arrange
        var vm = new TestViewModel();

        // Act
        vm.Username = un;

        // Assert
        Assert.True(vm.IsPropertyValid(nameof(vm.Username)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("qwertyuiopasdfghj")]
    [InlineData("qwerty.uiop")]
    [InlineData("qwerty4uiop")]
    [InlineData("qwertyAuiop")]
    public void CustomValidator_InvalidString_ReturnsError(string un)
    {
        // Arrange
        var vm = new TestViewModel();

        // Act
        vm.Username = un;

        // Assert
        Assert.True(vm.HasValidationErrors);
        Assert.Contains("Validation failed", vm.GetValidationErrors(nameof(vm.Username)));
    }

    [Theory]
    [InlineData("None")]
    [InlineData("none")]
    [InlineData("NONE")]
    [InlineData("Single")]
    [InlineData("Double")]
    [InlineData("Multiple")]
    public void EnumValidator_ValidString_NoError(string snr)
    {
        // Arrange
        var vm = new TestViewModel();

        // Act
        vm.Cardinality = snr;

        // Assert
        Assert.True(vm.IsPropertyValid(nameof(vm.Cardinality)));
    }

    [Fact]
    public void EnumValidator_InvalidString_ReturnsError()
    {
        // Arrange
        var vm = new TestViewModel();

        // Act
        vm.Cardinality = "qwerty";

        // Assert
        Assert.True(vm.HasValidationErrors);
        Assert.Contains($"Value must be one of {nameof(MyEnum)} values", vm.GetValidationErrors(nameof(vm.Cardinality)));
    }

    [Fact]
    public void ValidateAllProperties_AllRequired_NoErrors()
    {
        // Arrange
        var vm = new TestViewModel();

        // Act
        vm.LastName = "Doe";
        vm.FirstName = "John";
        vm.Email = "john@example.com";
        vm.Age = 25;
        vm.Score = "2.5";
        vm.Website = "https://example.com";
        vm.Username = "qwertyu";
        vm.ValidateAllProperties();

        // Assert
        Assert.False(vm.HasValidationErrors);
    }
    
    [Fact]
    public void ValidateAllProperties_MissingRequired_ReturnsErrors()
    {
        // Arrange
        var vm = new TestViewModel();

        // Act
        vm.LastName = "Doe";
        vm.Email = "john@example.com";
        vm.Age = 25;
        vm.Score = "";
        vm.Website = "https://example.com";
        vm.Username = "qwertyu";
        vm.ValidateAllProperties();

        // Assert
        Assert.True(vm.HasValidationErrors);
        Assert.Equal(2, vm.ValidationErrors.Count);
    }
}

public enum MyEnum
{
    None,
    Single,
    Double,
    Multiple
}

public class UsernameValidator : IValidator
{
    public bool IsValid(object? value)
    {
        return value is string n && n.Length is >= 4 and <= 16 && n.All(char.IsAsciiLetterLower);
    }
}
