using Sample.ViewModels;

namespace Sample.Views;

public partial class MainPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();

        viewModel.ExceptionHandler += ExceptionHandler;
        BindingContext = viewModel;
    }

    private void ExceptionHandler(Exception xcp)
    {
        Shell.Current.DisplayAlert("Error", xcp.Message, "OK");
    }
}