using Sample.Views;

namespace Sample;

public partial class App
{
    public App()
    {
        InitializeComponent();
    }
    
    protected override Window CreateWindow(IActivationState activationState)
    {
        return new Window(new AppShell());
    }
}