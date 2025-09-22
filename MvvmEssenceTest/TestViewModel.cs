
namespace MvvmEssenceTest;

public class TestViewModel : ViewModelBase
{
    protected override Task InitializeAsync()
    {
        Status = "starting...";

        return base.InitializeAsync();
    }

    public string? Status
    {
        get => Get<string?>();
        set => Set(value);
    }

    [Between<string>("A", "D")]
    public string Grade
    {
        get => Get("A");
        set => Set(value);
    }

    [Date]
    public string HireDate
    {
        get => Get("2000-01-01");
        set => Set(value);
    }

    [StringLength(2, 50)]
    public string LastName
    {
        get => Get("");
        set => Set(value);
    }

    [Required]
    public string FirstName
    {
        get => Get("");
        set => Set(value);
    }

    [Required]
    [Email]
    public string Email
    {
        get => Get<string>();
        set => Set(value);
    }

    [Email]
    public string SecondaryEmail
    {
        get => Get<string>();
        set => Set(value);
    }

    [Range<int>(0, 120)]
    public int Age
    {
        get => Get<int>();
        set => Set(value);
    }

    [Required]
    [Number<decimal>]
    public string Score
    {
        get => Get("0");
        set => Set(value);
    }

    [Regex(@"^https?://.*")]
    public string Website
    {
        get => Get<string>();
        set => Set(value);
    }

    [CustomValidation<UsernameValidator>()]
    public string Username
    {
        get => Get<string>();
        set => Set(value);
    }

    [Enum<MyEnum>()]
    public string Cardinality
    {
        get => Get<string>();
        set => Set(value);
    }

    public RelayCommand<string?> ChangeNameCommand =>
        GetTemplated<string?>(ChangeName, () => !IsBusy && !string.IsNullOrEmpty(LastName));

    private void ChangeName(string? str)
    {
        LastName = str ?? throw new NullReferenceException();
    }
}