using System.Globalization;
using System.Numerics;

namespace Brain2CPU.MvvmEssence;

// the StringLengthAttribute implies required if minimumLength > 0
// CustomValidationAttribute handles the required / optional separately
// all other validators consider the corresponding property as optional, add the RequiredAttribute if needed

public interface IValidator
{
    bool IsValid(object? value);
}

public interface IPropertyValidator : IValidator
{
    string? ErrorMessage { get; }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public abstract class ValidationAttribute : Attribute, IPropertyValidator
{
    protected string DefaultErrorMessage = "Validation failed";

    public string? ErrorMessage { get; protected set; }

    protected abstract bool IsObjectValid(object? value);

    public bool IsValid(object? value)
    {
        try
        {
            var isValid = IsObjectValid(value);
            if(!isValid)
                ErrorMessage = DefaultErrorMessage;
            return isValid;
        }
        catch(Exception xcp)
        {
            ErrorMessage = xcp.Message;
            return false; 
        }
    }
}

// as a general rule if a property doesn't have RequiredAttribute then null or empty values will be accepted
public class RequiredAttribute : ValidationAttribute
{
    public RequiredAttribute()
    {
        DefaultErrorMessage = "This field is required";
    }

    protected override bool IsObjectValid(object? value)
    {
        if (value == null) 
            return false;

        if (value is string str) 
            return !string.IsNullOrWhiteSpace(str);

        return true;
    }
}

// it can be applied to a string that could be empty but if it is not then must hold a number of the given type
public class NumberAttribute<T>: ValidationAttribute where T : INumber<T> 
{
    public NumberAttribute()
    {
        DefaultErrorMessage = "Value must be a number";
    }

    protected override bool IsObjectValid(object? value)
    {
        if (value is null or T)
            return true;

        try
        {
            _ = (T)Convert.ChangeType(value, typeof(T));
            return true;
        }
        catch
        {
            // if the type can be successfully converted will return true, otherwise it must continue
        }
        
        var str = value as string ?? value.ToString();

        if (string.IsNullOrEmpty(str)) 
            return true;

        return T.TryParse(str, NumberStyles.Number, CultureInfo.CurrentCulture, out _);
    }
}

public class DateAttribute: ValidationAttribute 
{
    public DateAttribute()
    {
        DefaultErrorMessage = "Value must be a valid date";
    }

    protected override bool IsObjectValid(object? value)
    {
        if (value is null or DateOnly or DateTime)
            return true;

        var str = value as string ?? value.ToString();

        if (string.IsNullOrEmpty(str)) 
            return true;

        return DateOnly.TryParse(str, out _) || DateTime.TryParse(str, out _);
    }
}

public class DateTimeAttribute: ValidationAttribute 
{
    public DateTimeAttribute()
    {
        DefaultErrorMessage = "Value must be a valid date-time";
    }

    protected override bool IsObjectValid(object? value)
    {
        if (value is null or DateTime)
            return true;

        var str = value as string ?? value.ToString();

        if (string.IsNullOrEmpty(str)) 
            return true;

        return DateTime.TryParse(str, out _);
    }
}

public class RangeAttribute<T>: ValidationAttribute where T : INumber<T> 
{
    private readonly T _minimum;
    private readonly T _maximum;

    public RangeAttribute(T minimum, T maximum)
    {
        _minimum = minimum;
        _maximum = maximum;

        DefaultErrorMessage = $"Value must be between {minimum} and {maximum}";
    }

    protected override bool IsObjectValid(object? value)
    {
        if (value is null)
            return true;
        
        if (value is T number)
            return number >= _minimum && number <= _maximum;

        try
        {
            number = (T)Convert.ChangeType(value, typeof(T));
            return number >= _minimum && number <= _maximum;
        }
        catch
        {
            // ignore, see above
        }
        
        var str = value as string ?? value.ToString();

        if (string.IsNullOrEmpty(str)) 
            return true;
        
        if (T.TryParse(str, NumberStyles.Number, CultureInfo.CurrentCulture, out var numericValue))
        {
            return numericValue >= _minimum && numericValue <= _maximum;
        }
        
        return false;
    }
}

public class BetweenAttribute<T>: ValidationAttribute where T : IComparable<T> 
{
    private readonly T _minimum;
    private readonly T _maximum;

    public BetweenAttribute(T minimum, T maximum)
    {
        _minimum = minimum;
        _maximum = maximum;

        DefaultErrorMessage = $"Value must be between {minimum} and {maximum}";
    }

    protected override bool IsObjectValid(object? value)
    {
        if (value is null)
            return true;

        var c = (IComparable<T>)value;
        return c.CompareTo(_minimum) >= 0 && c.CompareTo(_maximum) <= 0;
    }
}

public class EmailAttribute : ValidationAttribute
{
    public EmailAttribute()
    {
        DefaultErrorMessage = "Invalid email address";
    }

    protected override bool IsObjectValid(object? value)
    {
        var email = value as string ?? value?.ToString();

        if (string.IsNullOrWhiteSpace(email))
            return true;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

public class StringLengthAttribute : ValidationAttribute
{
    private readonly int _minimumLength;
    private readonly int _maximumLength;

    public StringLengthAttribute(int minimumLength, int maximumLength)
    {
        _minimumLength = minimumLength;
        _maximumLength = maximumLength;

        DefaultErrorMessage = $"Length must be between {minimumLength} and {maximumLength} characters";
    }

    protected override bool IsObjectValid(object? value)
    {
        var str = value as string ?? value?.ToString();
        if (string.IsNullOrEmpty(str))
            return _minimumLength == 0;
        
        var length = str.Length;
        return length >= _minimumLength && length <= _maximumLength;
    }
}

public class RegexAttribute : ValidationAttribute
{
    private readonly System.Text.RegularExpressions.Regex _regex;

    public RegexAttribute(string pattern)
    {
        _regex = new System.Text.RegularExpressions.Regex(pattern);

        DefaultErrorMessage = "Value does not match the required pattern";
    }

    protected override bool IsObjectValid(object? value)
    {
        var str = value as string ?? value?.ToString();
        if(string.IsNullOrEmpty(str))
            return true;

        return _regex.IsMatch(str);
    }
}

public class EnumAttribute<T> : ValidationAttribute where T : struct, Enum
{
    public EnumAttribute()
    {
        DefaultErrorMessage = $"Value must be one of {typeof(T).Name} values";
    }

    protected override bool IsObjectValid(object? value)
    {
        if(value is T)
            return true;
        
        var str = value as string ?? value?.ToString();

        if (string.IsNullOrEmpty(str))
            return true;

        return Enum.TryParse<T>(str, true, out _);
    }
}

public class CustomValidationAttribute<T> : ValidationAttribute where T : IValidator, new()
{
    private readonly T _validator = new ();

    protected override bool IsObjectValid(object? value)
    {
        return _validator.IsValid(value);
    }
}
