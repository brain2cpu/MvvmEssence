using System.Diagnostics;
using Brain2CPU.ApiClient;
using Brain2CPU.MvvmEssence;
using Sample.Interfaces;
using Sample.Models;
using Sample.ViewModels.Validators;

namespace Sample.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly ICalculationService _calc;
    private readonly ApiClient _apiClient;

    public MainViewModel(ICalculationService calc, ApiClient apiClient)
    {
        _calc = calc;
        _apiClient = apiClient;

        _apiClient.HttpClientName = "InsecureClient";

        ExceptionHandler += xcp => Debug.WriteLine(xcp.ToString());
    }

    [Required]
    [Number<double>]
    public string A
    {
        get => Get("0");
        set => Set(value, [ SquareRootCommand, DivideCommand ]);
    }

    [CustomValidation<NonZeroDoubleAsStringValidator>]
    public string B
    {
        get => Get("0");
        set => Set(value, DivideCommand);
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

    public RelayCommand SquareRootCommand => GetTemplated(() 
        => SquareRoot = _calc.Sqrt(A), () => !IsBusy && IsPropertyValid(nameof(A)));

    public RelayCommand DivideCommand 
        => Get(() => Quotient = ConvertAndDivide(A, B), () => !IsBusy && IsObjectValid);

    private static double ConvertAndDivide(string a, string b)
    {
        // simulate an exception, without any handling the app will crash
        if (b.EndsWith('0'))
            throw new ArgumentException("Simulated exception when B ends with 0", nameof(b));

        return double.Parse(a) / double.Parse(b);
    }

    public RelayCommandAsync LongRunningCommand => Get(LongRunningAsync, () => !IsBusy);

    // ApiClient doesn't throw, no exception handling is needed
    private async Task LongRunningAsync()
    {
        IsBusy = true;

        using var request = new ApiRequest("https://jsonplaceholder.typicode.com/posts")
        {
            Method = HttpMethod.Post,
        };
        request.Headers.Add("ApiKey", "1234567890");
        request.BuildJsonContent(new ApiPayload { Title = "foo", Body = "bar", UserId = 1 });
        
        var response = await _apiClient.SendRequestAsync<ApiPayload>(request);
        
        if(response.IsSuccess)
        {
            await Shell.Current.DisplayAlertAsync("Success", response.Data.ToString(), "OK");
        }
        else
        {
            await Shell.Current.DisplayAlertAsync("Failed", response.GetFullMessage(), "OK");
        }

        IsBusy = false;
    }
}
