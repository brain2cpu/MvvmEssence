using System.Diagnostics;
using Brain2CPU.MvvmEssence;
using Sample.Services;

namespace Sample.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly CalculationService _calc;

    public MainViewModel(CalculationService calc)
    {
        _calc = calc;

        ExceptionHandler += xcp => Debug.WriteLine(xcp.ToString());
    }

    public string A
    {
        get => Get("0", o => double.TryParse((string)o, out _));
        set => Set(value, new[] { SquareRootCommand, DivideCommand });
    }

    public string B
    {
        get => Get("0", NonZeroDoubleAsStringValidator);
        set => Set(value, DivideCommand);
    }

    private bool NonZeroDoubleAsStringValidator(object o)
    {
        return double.TryParse((string)o, out var d) && Math.Abs(d) > 1E-6;
    }

    public double SquareRoot
    {
        get => Get(0.0);
        set => Set(value);
    }

    public double Quotient
    {
        get => Get(0.0);
        set => Set(value);
    }

    public RelayCommand SquareRootCommand => GetTemplated(() => SquareRoot = _calc.Sqrt(A), () => !IsBusy && IsPropertyValid(nameof(A)));

    public RelayCommand DivideCommand => Get(() => Quotient = ConvertAndDivide(A, B), () => !IsBusy && IsObjectValid);

    private static double ConvertAndDivide(string a, string b)
    {
        // simulate an exception, without any handling the app will crash
        if (b.EndsWith("0"))
            throw new ArgumentException("Simulated exception when B ends with 0", nameof(b));

        return double.Parse(a) / double.Parse(b);
    }

    public RelayCommandAsync LongRunningCommand => GetTemplated(LongRunningAsync, () => !IsBusy);

    private async Task LongRunningAsync()
    {
        await Task.Delay(2000);
    }
}