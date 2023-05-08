namespace TestProject;

public class ViewModel : ViewModelBase
{
    public string Name
    {
        get => Get("");
        set => Set(value);
    }

    public RelayCommand<string> ChangeNameCommand =>
        GetTemplated<string>(ChangeName, () => !IsBusy && !string.IsNullOrEmpty(Name));

    private void ChangeName(string str)
    {
        Name = str ?? throw new NullReferenceException();
    }
}